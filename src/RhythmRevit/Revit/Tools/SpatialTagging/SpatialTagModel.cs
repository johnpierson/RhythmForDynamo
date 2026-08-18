// The 3d spatial tagging node, and the planning it is built on, are Revit 2025 and up.
// The tag family it places is saved in Revit 2025, and Revit loads families forward but never
// backward, so there is nothing for an earlier version to place.
#if R25_OR_GREATER
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;

namespace Rhythm.Revit.Tools.SpatialTagging
{
    /// <summary>What a tagging run actually did, so the dialog and the node can say so.</summary>
    internal class TaggingResult
    {
        public List<FamilyInstance> Tags { get; private set; }

        public TaggingResult()
        {
            Tags = new List<FamilyInstance>();
        }

        public int Created { get; set; }
        public int Updated { get; set; }

        /// <summary>Rooms with no point location at all. There is nowhere to put a tag.</summary>
        public int SkippedNotPlaced { get; set; }

        public int SkippedMissingNameOrNumber { get; set; }

        /// <summary>
        /// Tagged rooms with no area: unbounded or redundant. They keep a marker, so they get a
        /// tag, but it sits wherever the marker is rather than in a room. Reported because it
        /// usually means a modelling problem somebody wants to know about.
        /// </summary>
        public int Unbounded { get; set; }

        /// <summary>
        /// Existing tags that could not be touched, owned by another user or changed in central.
        /// Counted and reported rather than replaced: placing a second tag on top of one somebody
        /// else owns leaves two tags in the model and a suppressed warning saying so.
        /// </summary>
        public int SkippedNotEditable { get; set; }

        /// <summary>
        /// Set when the chosen family cannot hold a value this writes. The run is rolled back
        /// whole rather than leaving half a model tagged with blanks.
        /// </summary>
        public string MissingParameter { get; set; }

        /// <summary>
        /// Tags written before the link instance was part of a tag's identity, adopted by this run
        /// and rewritten with it. Reported because it explains a one-off run where tags were
        /// updated rather than created, and because it only ever happens once.
        /// </summary>
        public int MigratedLegacyTags { get; set; }

        /// <summary>
        /// Tags whose spatial element has been deleted. Reported rather than removed: the tag is
        /// still model geometry in somebody's project.
        /// </summary>
        public int OrphanedTags { get; set; }

        /// <summary>
        /// Duplicate-instance warnings hidden because this run placed tags on top of its own
        /// previous ones. Only ever non-zero with updating turned off.
        /// </summary>
        public int SuppressedDuplicateWarnings { get; set; }

        public int Skipped
        {
            get { return SkippedNotPlaced + SkippedMissingNameOrNumber + SkippedNotEditable; }
        }
    }

    /// <summary>
    /// Everything the 3d spatial tags dialog asks of the model, and the run itself.
    ///
    /// Shaped to match the add-in's own model class so the dialog's view model could be ported
    /// across largely as it was written. The node uses it through the dialog rather than directly.
    /// </summary>
    internal class SpatialTagModel
    {
        /// <summary>
        /// The family this discovers and, failing that, loads. Named by a contains match rather
        /// than an exact one, so a copy somebody has renamed to "3dSpatialElementTag-old" is still
        /// found and still matched against on later runs.
        /// </summary>
        internal const string TagFamilyName = "3dSpatialElementTag";

        /// <summary>
        /// The parameter a tag stores its source element's id in. This is the whole basis of
        /// matching a tag back to its room on a later run, so it is named once here rather than
        /// spelled out at each of the places that read and write it.
        /// </summary>
        internal const string SpatialElementIdParameter = "SpatialElementId";

        /// <summary>
        /// Where the tag records its source in a form Revit can schedule and filter on.
        ///
        /// Written when the family carries them, and read in preference to the composite value
        /// above. They are deliberately not required: a family without them still works, it just
        /// cannot be scheduled by source, and rejecting one would turn a reporting convenience
        /// into a hard incompatibility.
        /// </summary>
        internal const string SourceDocumentIdParameter = "SourceDocumentId";

        internal const string SourceLinkInstanceIdParameter = "SourceLinkInstanceId";

        internal const string NameParameter = "Name";
        internal const string NumberParameter = "Number";

        /// <summary>The parameter the text height is written to, on the family type.</summary>
        internal const string TextHeightParameter = "Text Height";

        /// <summary>The parameters a tag family has to carry for this to write to it.</summary>
        private static readonly string[] RequiredTagParameters = { NameParameter, NumberParameter, SpatialElementIdParameter };

        public Document Doc { get; private set; }

        public SpatialTagModel()
        {
            Doc = DocumentManager.Instance.CurrentDBDocument;
        }

        // =====================================================================================
        // Collecting, for the dialog to offer
        // =====================================================================================

        public ObservableCollection<Phase> CollectPhases()
        {
            return CollectPhases(Doc);
        }

        public ObservableCollection<Phase> CollectPhases(RevitLinkInstance linkInstance)
        {
            // A link can be unloaded between the dialog opening and a selection being made, and an
            // unloaded link has no document at all.
            var doc = linkInstance == null ? null : linkInstance.GetLinkDocument();

            return doc == null ? new ObservableCollection<Phase>() : CollectPhases(doc);
        }

        private static ObservableCollection<Phase> CollectPhases(Document doc)
        {
            var phases = new FilteredElementCollector(doc).OfClass(typeof(Phase))
                .WhereElementIsNotElementType().Cast<Phase>().ToList();

            return new ObservableCollection<Phase>(phases);
        }

        public ObservableCollection<SpatialElement> CollectSpatialElements(Phase phase, int targetIndex)
        {
            return CollectSpatialElements(Doc, phase, targetIndex);
        }

        public ObservableCollection<SpatialElement> CollectSpatialElements(Phase phase, RevitLinkInstance linkInstance, int targetIndex)
        {
            var doc = linkInstance == null ? null : linkInstance.GetLinkDocument();

            return doc == null
                ? new ObservableCollection<SpatialElement>()
                : CollectSpatialElements(doc, phase, targetIndex);
        }

        private static ObservableCollection<SpatialElement> CollectSpatialElements(Document doc, Phase phase, int targetIndex)
        {
            if (phase == null) return new ObservableCollection<SpatialElement>();

            var category = targetIndex == 0 ? BuiltInCategory.OST_Rooms : BuiltInCategory.OST_MEPSpaces;

            var elements = new FilteredElementCollector(doc)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .OfType<SpatialElement>()

                // Null-guarded, and compared from the phase's own id so a missing parameter is a
                // non-match rather than a null reference: an element in the category without a
                // phase parameter is not a room or space this understands.
                .Where(s => phase.Id.Equals(PhaseIdOf(s)))
                .ToList();

            return new ObservableCollection<SpatialElement>(elements);
        }

        private static ElementId PhaseIdOf(SpatialElement spatialElement)
        {
            var param = spatialElement.get_Parameter(BuiltInParameter.ROOM_PHASE);

            return param == null ? null : param.AsElementId();
        }

        /// <summary>
        /// The tag family types on offer, loading the one Rhythm ships if the model has none.
        /// </summary>
        public ObservableCollection<FamilySymbol> CollectTagFamilySymbols()
        {
            var tags = FindTagSymbols();

            if (!tags.Any())
            {
                LoadBundledFamily();
                tags = FindTagSymbols();
            }

            return new ObservableCollection<FamilySymbol>(tags);
        }

        private List<FamilySymbol> FindTagSymbols()
        {
            return new FilteredElementCollector(Doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
                .Where(f => f.Family.Name.IndexOf(TagFamilyName, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(f => f.Name)
                .ToList();
        }

        public ObservableCollection<RevitLinkInstance> GetRevitLinks()
        {
            // Unloaded links are left out: they have no document to read rooms from, and offering
            // one only leads to an empty phase list and no explanation.
            var links = new FilteredElementCollector(Doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>()
                .Where(l => l.GetLinkDocument() != null)
                .OrderBy(l => l.Name).ToList();

            return new ObservableCollection<RevitLinkInstance>(links);
        }

        /// <summary>
        /// The link the user already had selected in Revit, so the dialog opens on it.
        /// </summary>
        public RevitLinkInstance IsLinkSelected()
        {
            try
            {
                var uiDoc = DocumentManager.Instance.CurrentUIDocument;

                if (uiDoc == null) return null;

                var id = uiDoc.Selection.GetElementIds().FirstOrDefault();

                return id == null ? null : Doc.GetElement(id) as RevitLinkInstance;
            }
            catch (Exception)
            {
                // A selection this cannot read is not worth failing the dialog over; it just means
                // no link is pre-picked.
                return null;
            }
        }

        // =====================================================================================
        // Loading the bundled family
        // =====================================================================================

        /// <summary>
        /// Loads the tag family that ships in the package's extra folder.
        ///
        /// A failure here is not worth throwing over. The caller looks for the family again either
        /// way, and the dialog has a better thing to say than anything about a file path: that no
        /// family is loaded, next to the field where one is chosen.
        /// </summary>
        private void LoadBundledFamily()
        {
            var path = BundledFamilyPath();

            if (path == null || !File.Exists(path)) return;

            TransactionManager.Instance.ForceCloseTransaction();

            try
            {
                using (var t = new Transaction(Doc, "Loading the 3d spatial tag family"))
                {
                    t.Start();

                    if (t.GetStatus() != TransactionStatus.Started) return;

                    if (Doc.LoadFamily(path)) t.Commit();
                    else t.RollBack();
                }
            }
            catch (Exception)
            {
                // Left to the caller to report as a missing family.
            }
        }

        /// <summary>
        /// The family the package ships for this node, in extra/2025.
        ///
        /// The year in the folder is the Revit the family was saved in, not the Revit it is for.
        /// Families load forward but never backward, so one saved in 2025 serves every version
        /// this node runs on and none of the ones it does not. It sits in its own folder rather
        /// than beside the family Rhythm has always shipped in extra/, which is saved in Revit
        /// 2021, predates the SpatialElementId parameter, and is still what the older
        /// ThreeDeeRoomTags and ThreeDeeSpaceTags nodes point people at.
        ///
        /// Both files keep the same name on purpose. Revit names a loaded family after the file it
        /// came from, so telling the two apart by filename would put the year in the family name
        /// in every model that loaded it.
        /// </summary>
        private static string BundledFamilyPath()
        {
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(binFolder)) return null;

            // The extra folder is found by walking up one level rather than by replacing "bin" in
            // the path, which is what Rhythm does elsewhere and which rewrites any other "bin" a
            // user happens to have in their profile path.
            var packageRoot = Path.GetDirectoryName(binFolder);

            if (string.IsNullOrEmpty(packageRoot)) return null;

            return Path.Combine(packageRoot, "extra", "2025", TagFamilyName + ".rfa");
        }

        // =====================================================================================
        // The run
        // =====================================================================================

        /// <summary>
        /// A whole run: the family type's text height, and the tags, as one thing.
        ///
        /// Two committed transactions with nothing tying them together means a run that reports
        /// "nothing was placed" has already resized the family type and left it resized, and a
        /// successful run arrives as two undo entries where the first Ctrl+Z takes back only the
        /// tags. A transaction group makes the pair atomic and, assimilated, a single undo step.
        /// </summary>
        /// <param name="spatialElementTag">The tag family type to place.</param>
        /// <param name="spatialElements">The rooms or spaces in scope.</param>
        /// <param name="updateExisting">Whether to adopt the tags already placed rather than add to them.</param>
        /// <param name="linkInstance">The link the elements were read through, or null for this document.</param>
        /// <param name="textHeightInches">
        /// The height to apply, or zero or less to leave the family's own alone.
        /// </param>
        public TaggingResult RunTagging(
            FamilySymbol spatialElementTag,
            ObservableCollection<SpatialElement> spatialElements,
            bool updateExisting,
            RevitLinkInstance linkInstance,
            double textHeightInches)
        {
            // Dynamo has its own transaction open around graph evaluation. It has to be closed
            // before a group can be started, or the group nests inside it and the undo step this
            // is arranging for never appears.
            TransactionManager.Instance.ForceCloseTransaction();

            using (var group = new TransactionGroup(Doc, "Create / update 3d spatial tags"))
            {
                group.Start();

                try
                {
                    ApplyTextHeight(spatialElementTag, textHeightInches);

                    var result = CreateOrUpdateTags(spatialElementTag, spatialElements, updateExisting, linkInstance);

                    // The family could not hold what this writes, so the tagging transaction rolled
                    // itself back. The height change goes with it: reporting that nothing was
                    // placed while having quietly resized the family type is the partial mutation
                    // this group exists to prevent.
                    if (result.MissingParameter != null)
                    {
                        group.RollBack();
                        return result;
                    }

                    group.Assimilate();

                    return result;
                }
                catch (Exception)
                {
                    if (group.GetStatus() == TransactionStatus.Started) group.RollBack();

                    throw;
                }
            }
        }

        /// <summary>
        /// Pushes the requested text height onto the tag family type, if one was asked for and the
        /// family can take it. A family without the parameter is not a failure worth stopping the
        /// run over: the tags are still correct, they are just the size the family already was.
        /// </summary>
        private void ApplyTextHeight(FamilySymbol symbol, double inches)
        {
            if (inches <= 0) return;

            var param = symbol.LookupParameter(TextHeightParameter);

            if (param == null || param.IsReadOnly || param.StorageType != StorageType.Double) return;

            var feet = inches / 12;

            if (Math.Abs(param.AsDouble() - feet) < 1e-9) return;

            using (var t = new Transaction(Doc, "Setting the tag text height"))
            {
                t.Start();
                param.Set(feet);
                t.Commit();
            }
        }

        private TaggingResult CreateOrUpdateTags(
            FamilySymbol symbol,
            ObservableCollection<SpatialElement> spatialElements,
            bool updateExisting,
            RevitLinkInstance link)
        {
            var result = new TaggingResult();

            var existingTags = updateExisting ? CollectExistingTags(symbol) : new List<FamilyInstance>();

            // GetTotalTransform, not GetTransform: a link nested inside another link is placed by
            // the composition of both, and its own transform alone would put the tags in the wrong
            // place. The two are identical for a directly placed link.
            var transform = link == null ? null : link.GetTotalTransform();
            var linkInstanceId = link == null ? null : link.UniqueId;
            var sourceDoc = link == null ? Doc : link.GetLinkDocument();

            // Which file the rooms were read out of, recorded so a schedule can say so. The path is
            // what identifies it to a person; an unsaved document has only a title.
            var sourceDocumentId = sourceDoc == null || string.IsNullOrWhiteSpace(sourceDoc.PathName)
                ? (sourceDoc == null ? string.Empty : sourceDoc.Title)
                : sourceDoc.PathName;

            var tagsById = new Dictionary<string, FamilyInstance>(StringComparer.Ordinal);
            foreach (var t in existingTags) tagsById[t.UniqueId] = t;

            // Decided in full before the transaction opens, so that what the run intends to do is a
            // value that can be inspected rather than a shape that only exists while a document is
            // being written to.
            var plan = TagPlanner.Plan(
                spatialElements.Select(s => Snapshot(s, transform, linkInstanceId)).ToList(),
                existingTags.Select(t => Snapshot(t, sourceDoc, linkInstanceId)).ToList(),
                updateExisting);

            result.OrphanedTags = plan.OrphanedTagCount;

            using (var t = new Transaction(Doc, "Placing 3d spatial element tags"))
            {
                t.Start();

                if (!symbol.IsActive) symbol.Activate();

                foreach (var operation in plan.Operations)
                {
                    if (operation.Kind == TagOperationKind.Skip)
                    {
                        if (operation.Reason == SkipReason.NotEditable) result.SkippedNotEditable++;
                        else if (operation.Reason == SkipReason.NotPlaced) result.SkippedNotPlaced++;
                        else result.SkippedMissingNameOrNumber++;

                        continue;
                    }

                    var source = operation.Source;
                    var point = new XYZ(source.Point.X, source.Point.Y, source.Point.Z);

                    // An unbounded or redundant room still has a marker to hang a tag on, so it
                    // gets one. It is worth saying out loud though: the tag will sit wherever the
                    // marker was dropped rather than in a room.
                    if (source.Area <= 0) result.Unbounded++;

                    FamilyInstance tag;

                    if (operation.Kind == TagOperationKind.Update)
                    {
                        tag = tagsById[operation.ExistingTagId];
                        tag.Symbol = symbol;

                        var tagLocation = tag.Location as LocationPoint;

                        if (tagLocation != null) tagLocation.Point = point;

                        if (operation.IsLegacyMigration) result.MigratedLegacyTags++;

                        result.Updated++;
                    }
                    else
                    {
                        tag = Doc.Create.NewFamilyInstance(point, symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        result.Created++;
                    }

                    // Checked once, on the first tag, and the whole run is abandoned if the family
                    // cannot hold what this writes. Carrying on would leave a model full of tags
                    // with no room name in them and no way to match them up again.
                    if (result.Tags.Count == 0)
                    {
                        result.MissingParameter = FirstMissingParameter(tag);

                        if (result.MissingParameter != null)
                        {
                            t.RollBack();
                            result.Tags.Clear();
                            return result;
                        }
                    }

                    tag.LookupParameter(NameParameter).Set(source.Name);
                    tag.LookupParameter(NumberParameter).Set(source.Number);

                    // The element AND the link instance it came from. A tag adopted from the old
                    // bare-id format is rewritten here, so the next run matches it exactly and a
                    // second placement of the same link no longer finds it to steal.
                    tag.LookupParameter(SpatialElementIdParameter).Set(source.Identity.ToStoredValue());

                    // The same identity again, split into the parameters the family carries for it,
                    // where Revit can schedule and filter on it. Set only if the family has them:
                    // an older family is still perfectly usable without.
                    SetIfPresent(tag, SourceLinkInstanceIdParameter, source.LinkInstanceId ?? string.Empty);
                    SetIfPresent(tag, SourceDocumentIdParameter, sourceDocumentId);

                    result.Tags.Add(tag);
                }

                // Suppress the duplicate-instance warning for tags this run stacked on its own
                // previous ones, which is what updating turned off means, and nothing else. The
                // preprocessor is given the ids it is allowed to silence.
                var preprocessor = new HideOverlappingElementWarning(result.Tags.Select(tag => tag.Id));

                var failureOptions = t.GetFailureHandlingOptions();
                failureOptions.SetFailuresPreprocessor(preprocessor);
                t.Commit(failureOptions);

                result.SuppressedDuplicateWarnings = preprocessor.SuppressedDuplicateWarnings;
            }

            return result;
        }

        private static void SetIfPresent(FamilyInstance tag, string parameterName, string value)
        {
            var param = tag.LookupParameter(parameterName);

            if (param == null || param.IsReadOnly || param.StorageType != StorageType.String) return;

            param.Set(value);
        }

        /// <summary>
        /// The first parameter this tag instance is missing, or null if it carries all of them.
        ///
        /// Asked of a placed instance rather than of the symbol, because these are instance
        /// parameters and a family symbol does not carry them.
        /// </summary>
        private static string FirstMissingParameter(FamilyInstance tag)
        {
            return RequiredTagParameters.FirstOrDefault(name => tag.LookupParameter(name) == null);
        }

        /// <summary>
        /// The tags already in the model: the bundled family, plus whatever family the run was
        /// pointed at, so a custom tag family still matches itself on the next run.
        /// </summary>
        private List<FamilyInstance> CollectExistingTags(FamilySymbol symbol)
        {
            var chosenFamily = symbol.Family.Name;

            return new FilteredElementCollector(Doc).OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType().Cast<FamilyInstance>()
                .Where(f => f.Symbol.Family.Name.IndexOf(TagFamilyName, StringComparison.OrdinalIgnoreCase) >= 0
                            || string.Equals(f.Symbol.Family.Name, chosenFamily, StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>
        /// Reads a spatial element into the plain data the planner works on, applying the link
        /// transform on the way so that everything downstream is in host coordinates.
        /// </summary>
        private static SpatialElementSnapshot Snapshot(SpatialElement spatialElement, Transform transform, string linkInstanceId)
        {
            var location = spatialElement.Location as LocationPoint;
            var point = location == null ? null : location.Point;

            if (point != null && transform != null) point = transform.OfPoint(point);

            var name = spatialElement.get_Parameter(BuiltInParameter.ROOM_NAME);
            var number = spatialElement.get_Parameter(BuiltInParameter.ROOM_NUMBER);

            return new SpatialElementSnapshot
            {
                SourceId = spatialElement.UniqueId,
                LinkInstanceId = linkInstanceId,
                Name = name == null ? null : name.AsString(),
                Number = number == null ? null : number.AsString(),

                // A point location and nothing more. An unbounded or redundant room has no area
                // but still has a marker, and it is tagged rather than skipped; the area is
                // carried below so the run can say how many of those there were.
                IsPlaced = location != null,
                Point = point == null ? default(TagPoint) : new TagPoint(point.X, point.Y, point.Z),
                Area = spatialElement.Area
            };
        }

        /// <summary>
        /// Reads an existing tag into plain data. The editability question is asked here, once per
        /// tag, rather than in the middle of placing things.
        /// </summary>
        /// <param name="tag">The tag already in the model.</param>
        /// <param name="sourceDoc">
        /// The document the run reads elements from, used to notice tags whose element has been
        /// deleted.
        /// </param>
        /// <param name="linkInstanceId">The link instance being tagged, or null for the host.</param>
        private static ExistingTagSnapshot Snapshot(FamilyInstance tag, Document sourceDoc, string linkInstanceId)
        {
            // Null-safe: a tag whose id parameter was removed or never filled in reads back as null.
            var storedParam = tag.LookupParameter(SpatialElementIdParameter);
            var stored = storedParam == null ? null : storedParam.AsString();

            var storedLinkParam = tag.LookupParameter(SourceLinkInstanceIdParameter);
            var storedLink = storedLinkParam == null ? null : storedLinkParam.AsString();

            return new ExistingTagSnapshot
            {
                TagId = tag.UniqueId,
                StoredSourceId = stored,
                SourceLinkInstanceId = storedLink,
                IsEditable = IsEditable(tag),
                SourceMissing = IsSourceMissing(stored, storedLink, sourceDoc, linkInstanceId)
            };
        }

        /// <summary>
        /// Whether this user may write to the element. Everything in a model that is not
        /// workshared is fair game; in one that is, a tag checked out by somebody else is not.
        /// </summary>
        private static bool IsEditable(Autodesk.Revit.DB.Element element)
        {
            var doc = element.Document;

            if (doc == null || !doc.IsWorkshared) return true;

            try
            {
                return WorksharingUtils.GetCheckoutStatus(doc, element.Id) != CheckoutStatus.OwnedByOtherUser;
            }
            catch (Exception)
            {
                // Not being able to ask is not proof that it is editable, but refusing to touch the
                // whole model over it is worse. Revit will still stop the write if it has to.
                return true;
            }
        }

        /// <summary>
        /// Whether a tag names an element that is no longer in the document it came from.
        ///
        /// Deliberately narrow. Only tags belonging to the scope being tagged are considered,
        /// because a tag for a room in another phase, or read through a different link instance,
        /// has not been orphaned just because this run is not about it. Calling it one would report
        /// a number that frightens people about nothing.
        /// </summary>
        private static bool IsSourceMissing(string stored, string storedLink, Document sourceDoc, string linkInstanceId)
        {
            if (sourceDoc == null) return false;

            TagSourceIdentity identity;
            bool isLegacy;
            if (!TagSourceIdentity.TryResolve(stored, storedLink, out identity, out isLegacy)) return false;

            // A legacy tag carries no link instance, so there is no way to tell which scope it
            // belongs to. Left alone rather than guessed at.
            if (isLegacy && linkInstanceId != null) return false;

            if (!string.Equals(identity.LinkInstanceId, linkInstanceId, StringComparison.Ordinal)) return false;

            try
            {
                return sourceDoc.GetElement(identity.SourceId) == null;
            }
            catch (Exception)
            {
                // A malformed id is not proof of a deleted element.
                return false;
            }
        }
    }
}
#endif
