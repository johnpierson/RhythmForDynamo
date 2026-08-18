// The 3d spatial tagging node, and the planning it is built on, are Revit 2025 and up.
// The tag family it places is saved in Revit 2025, and Revit loads families forward but never
// backward, so there is nothing for an earlier version to place.
#if R25_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Rhythm.Revit.Tools.SpatialTagging;
using RevitServices.Persistence;
using RevitServices.Transactions;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;

namespace Rhythm.Revit.Tools
{
    public partial class Tools
    {
        /// <summary>
        /// The family this node discovers and, failing that, loads. Named by a contains match rather
        /// than an exact one, so a copy somebody has renamed to "3dSpatialElementTag-old" is still
        /// found and still matched against on later runs.
        /// </summary>
        private const string TagFamilyName = "3dSpatialElementTag";

        /// <summary>
        /// The parameter a tag stores its source element's id in. This is the whole basis of
        /// matching a tag back to its room on a later run, so it is named once here rather than
        /// spelled out at each of the places that read and write it.
        /// </summary>
        private const string SpatialElementIdParameter = "SpatialElementId";

        /// <summary>
        /// Where the tag records its source in a form Revit can schedule and filter on.
        ///
        /// Written when the family carries them, and read in preference to the composite value
        /// above. They are deliberately not required: a family without them still works, it just
        /// cannot be scheduled by source, and rejecting one would turn a reporting convenience into
        /// a hard incompatibility.
        /// </summary>
        private const string SourceDocumentIdParameter = "SourceDocumentId";

        private const string SourceLinkInstanceIdParameter = "SourceLinkInstanceId";

        private const string NameParameter = "Name";
        private const string NumberParameter = "Number";

        /// <summary>The parameter the text height is written to, on the family type.</summary>
        private const string TextHeightParameter = "Text Height";

        /// <summary>The parameters a tag family has to carry for this node to write to it.</summary>
        private static readonly string[] RequiredTagParameters = { NameParameter, NumberParameter, SpatialElementIdParameter };

        /// <summary>
        /// Place or update 3d tags on every room or space in the model, in one go.
        ///
        /// This is the whole of the 3d Spatial Tags add-in as a single node. It reads the rooms or
        /// spaces out of this document or a link, works out which of them already have a tag, moves
        /// and rewrites those and creates the rest, and reports what it decided.
        ///
        /// Re-running follows the model rather than piling up duplicates: a tag records the element
        /// it belongs to, and the link instance it was read through, in the family's
        /// SpatialElementId parameter. Rooms with a blank name or number are skipped, because there
        /// would be nothing to put in the tag, and so are unplaced ones, because there is nowhere to
        /// put it. Unbounded and redundant rooms still have a marker, so they are still tagged, and
        /// counted in the message so you know. In a workshared model a tag owned by somebody else is
        /// reported, never overwritten. Tags whose room has been deleted are counted and left alone,
        /// because deleting elements out of somebody's model is not this node's business.
        /// </summary>
        /// <param name="runIt">Set to true to write to the model. False reports nothing and changes nothing.</param>
        /// <param name="target">What to tag: "Rooms" or "Spaces".</param>
        /// <param name="phase">The phase to tag. Matched to the source document by name. Leave this empty to tag every phase.</param>
        /// <param name="linkInstance">A Revit link to read the rooms or spaces out of. Leave this empty to use the current document. The tags are always placed in the current document, transformed into place.</param>
        /// <param name="tagType">The 3d tag family type to use. Leave this empty and the node uses a loaded 3dSpatialElementTag, loading the one from Rhythm's extra folder if the model has none. Whatever you give it has to carry Name, Number and SpatialElementId instance parameters.</param>
        /// <param name="updateExisting">Whether to move and rewrite the tags that are already placed. False places a fresh set on top of them every run.</param>
        /// <param name="textHeightInches">The text height to apply to the tag family type, in inches. Zero or less leaves the family's own height alone.</param>
        /// <returns name="tags">The tags this run created or updated.</returns>
        /// <returns name="created">How many tags were placed.</returns>
        /// <returns name="updated">How many existing tags were moved and rewritten.</returns>
        /// <returns name="skipped">How many rooms or spaces could not be tagged.</returns>
        /// <returns name="orphanedTags">How many tags in the model name a room that has been deleted.</returns>
        /// <returns name="message">What the run did, in words.</returns>
        /// <search>
        /// 3d room tags, 3d space tags, spatial tags, rhythm
        /// </search>
        [MultiReturn(new[] { "tags", "created", "updated", "skipped", "orphanedTags", "message" })]
        [NodeCategory("Actions")]
        public static Dictionary<string, object> ThreeDeeSpatialTags(
            bool runIt,
            string target = "Rooms",
            global::Revit.Elements.Element phase = null,
            global::Revit.Elements.Element linkInstance = null,
            global::Revit.Elements.FamilyType tagType = null,
            bool updateExisting = true,
            double textHeightInches = 0.0)
        {
            var wantsSpaces = ReadTarget(target);

            if (!runIt)
            {
                return Report(new List<object>(), 0, 0, 0, 0, "runIt is false, so nothing was written to the model.");
            }

            var doc = DocumentManager.Instance.CurrentDBDocument;

            // The link is resolved first because everything else is asked of the document it names:
            // the phase, the rooms, and whether a tag's room still exists.
            RevitLinkInstance link = null;
            Transform transform = null;
            string linkInstanceId = null;
            var sourceDoc = doc;

            if (linkInstance != null)
            {
                link = linkInstance.InternalElement as RevitLinkInstance;

                if (link == null)
                {
                    throw new ArgumentException("linkInstance is not a Revit link instance.", "linkInstance");
                }

                sourceDoc = link.GetLinkDocument();

                if (sourceDoc == null)
                {
                    throw new InvalidOperationException("That link is not loaded, so it has no rooms or spaces to read.");
                }

                // GetTotalTransform, not GetTransform: a link nested inside another link is placed by
                // the composition of both, and its own transform alone would put the tags in the
                // wrong place. The two are identical for a directly placed link.
                transform = link.GetTotalTransform();
                linkInstanceId = link.UniqueId;
            }

            var resolvedPhase = ResolvePhase(sourceDoc, phase);
            var spatialElements = CollectSpatialElements(sourceDoc, resolvedPhase, wantsSpaces);

            if (spatialElements.Count == 0)
            {
                return Report(new List<object>(), 0, 0, 0, 0,
                    "There are no " + (wantsSpaces ? "spaces" : "rooms") + " in that scope, so nothing was tagged.");
            }

            TransactionManager.Instance.ForceCloseTransaction();

            var symbol = ResolveTagSymbol(doc, tagType);

            // The family type's text height and the tags are one thing. Two committed transactions
            // with nothing tying them together means a run that reports "nothing was placed" has
            // already resized the family type and left it resized, and a successful run arrives as
            // two undo entries where the first Ctrl+Z takes back only the tags.
            using (var group = new TransactionGroup(doc, "Create / update 3d spatial tags"))
            {
                group.Start();

                try
                {
                    ApplyTextHeight(doc, symbol, textHeightInches);

                    var result = CreateOrUpdateTags(doc, symbol, spatialElements, updateExisting, link, sourceDoc, transform, linkInstanceId);

                    // The family could not hold what this node writes, so the tagging transaction
                    // rolled itself back. The height change goes with it: reporting that nothing was
                    // placed while having quietly resized the family type is the partial mutation
                    // this group exists to prevent.
                    if (result.MissingParameter != null)
                    {
                        group.RollBack();

                        throw new InvalidOperationException(
                            "The tag family type " + symbol.Family.Name + " : " + symbol.Name +
                            " has no " + result.MissingParameter + " instance parameter, so nothing was placed. " +
                            "Add it to the family, or leave tagType empty and let the node load the one Rhythm ships.");
                    }

                    group.Assimilate();

                    return Report(
                        result.Tags.Select(t => (object)t.ToDSType(true)).ToList(),
                        result.Created,
                        result.Updated,
                        result.SkippedNotPlaced + result.SkippedMissingNameOrNumber + result.SkippedNotEditable,
                        result.OrphanedTags,
                        Describe(result, wantsSpaces, updateExisting));
                }
                catch (Exception)
                {
                    // Only ever rolls back a group that is still open. Assimilate above closes it,
                    // and the missing-parameter path rolled it back before throwing.
                    if (group.GetStatus() == TransactionStatus.Started) group.RollBack();

                    throw;
                }
            }
        }

        /// <summary>Rooms or spaces, however the user spelled it.</summary>
        private static bool ReadTarget(string target)
        {
            var trimmed = (target ?? string.Empty).Trim();

            if (trimmed.Length == 0 || trimmed.Equals("Rooms", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Room", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (trimmed.Equals("Spaces", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Space", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            throw new ArgumentException("target has to be \"Rooms\" or \"Spaces\". It was \"" + target + "\".", "target");
        }

        /// <summary>
        /// The phase in the document the rooms are read from.
        ///
        /// Matched by name rather than by id on purpose. When the rooms come from a link, a phase
        /// picked out of the host document is a different element with a different id, and comparing
        /// the two matches nothing at all. Names are what a person picks a phase by anyway.
        /// </summary>
        private static Phase ResolvePhase(Document sourceDoc, global::Revit.Elements.Element phase)
        {
            if (phase == null) return null;

            var internalPhase = phase.InternalElement;

            if (internalPhase == null)
            {
                throw new ArgumentException("phase is empty.", "phase");
            }

            var phaseName = internalPhase.Name;

            var match = new FilteredElementCollector(sourceDoc).OfClass(typeof(Phase)).Cast<Phase>()
                .FirstOrDefault(p => string.Equals(p.Name, phaseName, StringComparison.Ordinal));

            if (match == null)
            {
                throw new ArgumentException(
                    "There is no phase called \"" + phaseName + "\" in " + sourceDoc.Title + ".", "phase");
            }

            return match;
        }

        private static List<SpatialElement> CollectSpatialElements(Document sourceDoc, Phase phase, bool wantsSpaces)
        {
            var category = wantsSpaces ? BuiltInCategory.OST_MEPSpaces : BuiltInCategory.OST_Rooms;

            return new FilteredElementCollector(sourceDoc)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .OfType<SpatialElement>()

                // Null-guarded, and compared from the phase's own id so a missing parameter is a
                // non-match rather than a null reference: an element in the category without a phase
                // parameter is not a room or space this node understands.
                .Where(s => phase == null || phase.Id.Equals(PhaseIdOf(s)))
                .ToList();
        }

        private static ElementId PhaseIdOf(SpatialElement spatialElement)
        {
            var param = spatialElement.get_Parameter(BuiltInParameter.ROOM_PHASE);

            return param == null ? null : param.AsElementId();
        }

        /// <summary>
        /// The tag type to place: the one asked for, or the bundled family, loading it if the model
        /// has not got it yet.
        /// </summary>
        private static FamilySymbol ResolveTagSymbol(Document doc, global::Revit.Elements.FamilyType tagType)
        {
            if (tagType != null)
            {
                var chosen = tagType.InternalElement as FamilySymbol;

                if (chosen == null)
                {
                    throw new ArgumentException("tagType is not a family type.", "tagType");
                }

                return chosen;
            }

            var symbol = FindTagSymbols(doc).FirstOrDefault();

            if (symbol != null) return symbol;

            LoadBundledFamily(doc);

            symbol = FindTagSymbols(doc).FirstOrDefault();

            if (symbol == null)
            {
                throw new InvalidOperationException(
                    "No " + TagFamilyName + " family is loaded and the copy in Rhythm's extra folder could not be " +
                    "loaded either. Load a 3d tag family into the model and feed it to tagType.");
            }

            return symbol;
        }

        private static List<FamilySymbol> FindTagSymbols(Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
                .Where(f => f.Family.Name.IndexOf(TagFamilyName, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(f => f.Name)
                .ToList();
        }

        /// <summary>
        /// Loads the tag family that ships in the package's extra folder.
        ///
        /// A failure here is not worth throwing over. The caller looks for the family again either
        /// way, and it has a better message to give than anything about a file path: what the user
        /// can do about it is load a family and pass it in.
        /// </summary>
        private static void LoadBundledFamily(Document doc)
        {
            var path = BundledFamilyPath();

            if (path == null || !File.Exists(path)) return;

            try
            {
                using (var t = new Transaction(doc, "Loading the 3d spatial tag family"))
                {
                    t.Start();

                    if (doc.LoadFamily(path)) t.Commit();
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
        /// Families load forward but never backward, so one saved in 2025 serves every version this
        /// node runs on and none of the ones it does not. It sits in its own folder rather than
        /// beside the family Rhythm has always shipped in extra/, which is saved in Revit 2021,
        /// predates the SpatialElementId parameter, and is still what the older ThreeDeeRoomTags and
        /// ThreeDeeSpaceTags nodes point people at.
        ///
        /// Both files keep the same name on purpose. Revit names a loaded family after the file it
        /// came from, so telling the two apart by filename would put the year in the family name in
        /// every model that loaded it.
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

        /// <summary>
        /// Pushes the requested text height onto the tag family type, if one was asked for and the
        /// family can take it. A family without the parameter is not a failure worth stopping the
        /// run over: the tags are still correct, they are just the size the family already was.
        /// </summary>
        private static void ApplyTextHeight(Document doc, FamilySymbol symbol, double inches)
        {
            if (inches <= 0) return;

            var param = symbol.LookupParameter(TextHeightParameter);

            if (param == null || param.IsReadOnly || param.StorageType != StorageType.Double) return;

            var feet = inches / 12;

            if (Math.Abs(param.AsDouble() - feet) < 1e-9) return;

            using (var t = new Transaction(doc, "Setting the tag text height"))
            {
                t.Start();
                param.Set(feet);
                t.Commit();
            }
        }

        private static TaggingResult CreateOrUpdateTags(
            Document doc,
            FamilySymbol symbol,
            List<SpatialElement> spatialElements,
            bool updateExisting,
            RevitLinkInstance link,
            Document sourceDoc,
            Transform transform,
            string linkInstanceId)
        {
            var result = new TaggingResult();

            var existingTags = updateExisting ? CollectExistingTags(doc, symbol) : new List<FamilyInstance>();

            // Which file the rooms were read out of, recorded so a schedule can say so. The path is
            // what identifies it to a person; an unsaved document has only a title.
            var sourceDocumentId = string.IsNullOrWhiteSpace(sourceDoc.PathName) ? sourceDoc.Title : sourceDoc.PathName;

            // Decided in full before the transaction opens, so that what the run intends to do is a
            // value that can be inspected rather than a shape that only exists while a document is
            // being written to.
            var tagsById = new Dictionary<string, FamilyInstance>(StringComparer.Ordinal);
            foreach (var t in existingTags) tagsById[t.UniqueId] = t;

            var plan = TagPlanner.Plan(
                spatialElements.Select(s => Snapshot(s, transform, linkInstanceId)).ToList(),
                existingTags.Select(t => Snapshot(t, sourceDoc, linkInstanceId)).ToList(),
                updateExisting);

            result.OrphanedTags = plan.OrphanedTagCount;

            using (var t = new Transaction(doc, "Placing 3d spatial element tags"))
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

                    // An unbounded or redundant room still has a marker to hang a tag on, so it gets
                    // one, exactly as the add-in gives it one. It is worth saying out loud though:
                    // the tag will sit wherever the marker was dropped rather than in a room.
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
                        tag = doc.Create.NewFamilyInstance(point, symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        result.Created++;
                    }

                    // Checked once, on the first tag, and the whole run is abandoned if the family
                    // cannot hold what this node writes. Carrying on would leave a model full of
                    // tags with no room name in them and no way to match them up again.
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
                    // where Revit can schedule and filter on it. Set only if the family has them: an
                    // older family is still perfectly usable without.
                    SetIfPresent(tag, SourceLinkInstanceIdParameter, source.LinkInstanceId ?? string.Empty);
                    SetIfPresent(tag, SourceDocumentIdParameter, sourceDocumentId);

                    result.Tags.Add(tag);
                }

                // Suppress the duplicate-instance warning for tags this run stacked on its own
                // previous ones, which is what updateExisting off means, and nothing else. The
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
        private static List<FamilyInstance> CollectExistingTags(Document doc, FamilySymbol symbol)
        {
            var chosenFamily = symbol.Family.Name;

            return new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance))
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

                // A point location and nothing more. An unbounded or redundant room has no area but
                // still has a marker, and the add-in tags it rather than skipping it; the area is
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
        /// Whether this user may write to the element. Everything in a model that is not workshared
        /// is fair game; in one that is, a tag checked out by somebody else is not.
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
        /// Deliberately narrow. Only tags belonging to the scope being tagged are considered, because
        /// a tag for a room in another phase, or read through a different link instance, has not been
        /// orphaned just because this run is not about it. Calling it one would report a number that
        /// frightens people about nothing.
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

        /// <summary>What the run did, in the words the add-in's status card uses.</summary>
        private static string Describe(TaggingResult result, bool wantsSpaces, bool updateExisting)
        {
            var noun = wantsSpaces ? "space" : "room";
            var message = new StringBuilder();

            message.Append(result.Created).Append(result.Created == 1 ? " tag placed, " : " tags placed, ");
            message.Append(result.Updated).Append(" updated.");

            if (result.SkippedNotPlaced > 0)
            {
                message.Append(" ").Append(result.SkippedNotPlaced).Append(" ").Append(noun)
                    .Append(result.SkippedNotPlaced == 1 ? " is" : "s are")
                    .Append(" unplaced, so there is nowhere to put a tag.");
            }

            if (result.SkippedMissingNameOrNumber > 0)
            {
                message.Append(" ").Append(result.SkippedMissingNameOrNumber).Append(" ").Append(noun)
                    .Append(result.SkippedMissingNameOrNumber == 1 ? " has" : "s have")
                    .Append(" a blank name or number, so there would be nothing in the tag.");
            }

            if (result.Unbounded > 0)
            {
                message.Append(" ").Append(result.Unbounded).Append(" tagged ").Append(noun)
                    .Append(result.Unbounded == 1 ? " is" : "s are")
                    .Append(" unbounded or redundant, so the tag sits on the marker rather than in a room.");
            }

            if (result.SkippedNotEditable > 0)
            {
                message.Append(" ").Append(result.SkippedNotEditable)
                    .Append(result.SkippedNotEditable == 1 ? " tag is" : " tags are")
                    .Append(" owned by another user and was left alone.");
            }

            if (result.MigratedLegacyTags > 0)
            {
                message.Append(" ").Append(result.MigratedLegacyTags).Append(result.MigratedLegacyTags == 1
                    ? " tag was written before link instances were part of a tag's identity, and has been rewritten with it."
                    : " tags were written before link instances were part of a tag's identity, and have been rewritten with it.");
                message.Append(" That only happens once.");
            }

            if (result.OrphanedTags > 0)
            {
                message.Append(" ").Append(result.OrphanedTags).Append(result.OrphanedTags == 1
                    ? " tag names a " + noun + " that has been deleted, so its text is now wrong. It was counted, not removed."
                    : " tags name " + noun + "s that have been deleted, so their text is now wrong. They were counted, not removed.");
            }

            if (result.SuppressedDuplicateWarnings > 0)
            {
                message.Append(" ").Append(result.SuppressedDuplicateWarnings)
                    .Append(" duplicate-instance warning")
                    .Append(result.SuppressedDuplicateWarnings == 1 ? " was" : "s were")
                    .Append(" hidden, because this run stacked tags on its own previous ones.");
            }

            if (!updateExisting)
            {
                message.Append(" updateExisting is false, so the tags already in the model were left where they were.");
            }

            return message.ToString();
        }

        private static Dictionary<string, object> Report(
            List<object> tags, int created, int updated, int skipped, int orphaned, string message)
        {
            return new Dictionary<string, object>
            {
                { "tags", tags },
                { "created", created },
                { "updated", updated },
                { "skipped", skipped },
                { "orphanedTags", orphaned },
                { "message", message }
            };
        }

        /// <summary>What a tagging run actually did, so the node can say so.</summary>
        private class TaggingResult
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

            /// <summary>
            /// Tagged rooms with no area: unbounded or redundant. They keep a marker, so they get a
            /// tag, but it sits wherever the marker is rather than in a room. Reported for the same
            /// reason the add-in's dialog reports it, which is that it usually means a modelling
            /// problem somebody wants to know about.
            /// </summary>
            public int Unbounded { get; set; }

            public int SkippedMissingNameOrNumber { get; set; }

            /// <summary>
            /// Existing tags that could not be touched, owned by another user or changed in central.
            /// Counted and reported rather than replaced: placing a second tag on top of one somebody
            /// else owns leaves two tags in the model and a suppressed warning saying so.
            /// </summary>
            public int SkippedNotEditable { get; set; }

            /// <summary>
            /// Set when the chosen family cannot hold a value this node writes. The run is rolled
            /// back whole rather than leaving half a model tagged with blanks.
            /// </summary>
            public string MissingParameter { get; set; }

            /// <summary>
            /// Tags written before the link instance was part of a tag's identity, adopted by this
            /// run and rewritten with it. Reported because it explains a one-off run where tags were
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
            /// previous ones. Only ever non-zero with updateExisting off.
            /// </summary>
            public int SuppressedDuplicateWarnings { get; set; }
        }
    }
}
#endif
