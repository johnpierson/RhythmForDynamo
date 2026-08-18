// The 3d spatial tags dialog is Revit 2025 and up, with the node it belongs to.
#if R25_OR_GREATER
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autodesk.Revit.DB;
using Rhythm.Revit.Tools.SpatialTagging;

namespace Rhythm.SpatialTagsUi
{
    /// <summary>
    /// The 3d Spatial Tags dialog, ported from the add-in of the same name.
    ///
    /// Kept close to the original on purpose: the point of the node is that the dialog behaves the
    /// way the add-in's does. What changed is the plumbing rather than the behaviour. The base
    /// class and the command are Rhythm's own two-class MVVM instead of CommunityToolkit.Mvvm,
    /// the remembered choices live in memory for the Dynamo session instead of in user settings,
    /// and there is no logger: what would have gone to a log file goes into the error line the
    /// user is already reading, and the node reports it as well.
    /// </summary>
    internal class SpatialTagDialogViewModel : ObservableBase
    {
        private SpatialTagModel Model { get; set; }

        public RelayCommand<Window> Close { get; set; }
        public RelayCommand<Window> Run { get; set; }

        /// <summary>
        /// What the last run in this dialog did, for the node to report after the dialog closes.
        /// Null until a run completes, which is how "the user closed without running" is told
        /// apart from "a run happened and placed nothing".
        /// </summary>
        public TaggingResult LastResult { get; private set; }

        /// <summary>Whatever stopped the last run, if something did.</summary>
        public string LastFailure { get; private set; }

        public string PluginVersion
        {
            get { return "3d Spatial Tags — Rhythm " + Assembly.GetExecutingAssembly().GetName().Version; }
        }

        private bool _flyOutVisibility;
        public bool FlyOutVisibility
        {
            get { return _flyOutVisibility; }
            set { _flyOutVisibility = value; OnPropertyChanged("FlyOutVisibility"); OnPropertyChanged("HasStatus"); }
        }

        private bool _unboundedFlyOutVisibility;
        public bool UnboundedFlyOutVisibility
        {
            get { return _unboundedFlyOutVisibility; }
            set { _unboundedFlyOutVisibility = value; OnPropertyChanged("UnboundedFlyOutVisibility"); OnPropertyChanged("HasStatus"); }
        }

        private ObservableCollection<SpatialElement> _spatialElements;
        public ObservableCollection<SpatialElement> SpatialElements
        {
            get { return _spatialElements; }
            set { _spatialElements = value; OnPropertyChanged("SpatialElements"); OnPropertyChanged("CanRun"); }
        }

        private ObservableCollection<Phase> _phases;
        public ObservableCollection<Phase> Phases
        {
            get { return _phases; }
            set { _phases = value; OnPropertyChanged("Phases"); }
        }

        private ObservableCollection<FamilySymbol> _roomTagFamilySymbols;
        public ObservableCollection<FamilySymbol> RoomTagFamilySymbols
        {
            get { return _roomTagFamilySymbols; }
            set
            {
                _roomTagFamilySymbols = value;
                OnPropertyChanged("RoomTagFamilySymbols");
                OnPropertyChanged("HasNoFamilySymbols");
                OnPropertyChanged("CanRun");
            }
        }

        private ObservableCollection<RevitLinkInstance> _links;
        public ObservableCollection<RevitLinkInstance> Links
        {
            get { return _links; }
            set
            {
                _links = value;
                OnPropertyChanged("Links");
                OnPropertyChanged("HasLinks");
                OnPropertyChanged("FromLinkCaption");
            }
        }

        private int _familySymbolIndex;
        public int FamilySymbolIndex
        {
            get { return _familySymbolIndex; }
            set { _familySymbolIndex = value; OnPropertyChanged("FamilySymbolIndex"); OnPropertyChanged("CanRun"); }
        }

        private int _targetIndex;
        public int TargetIndex
        {
            get { return _targetIndex; }
            set { _targetIndex = value; OnPropertyChanged("TargetIndex"); }
        }

        private bool _updateExisting;
        public bool UpdateExisting
        {
            get { return _updateExisting; }
            set { _updateExisting = value; OnPropertyChanged("UpdateExisting"); }
        }

        private string _flyOutText;
        public string FlyOutText
        {
            get { return _flyOutText; }
            set { _flyOutText = value; OnPropertyChanged("FlyOutText"); }
        }

        private string _unboundedFlyOutText;
        public string UnboundedFlyOutText
        {
            get { return _unboundedFlyOutText; }
            set { _unboundedFlyOutText = value; OnPropertyChanged("UnboundedFlyOutText"); }
        }

        private string _errorText;

        /// <summary>Whatever went wrong on the last attempt, in words the user can act on.</summary>
        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                OnPropertyChanged("ErrorText");
                OnPropertyChanged("HasError");
                OnPropertyChanged("HasStatus");
            }
        }

        private string _titleText;
        public string TitleText
        {
            get { return _titleText; }
            set { _titleText = value; OnPropertyChanged("TitleText"); }
        }

        private bool _inProgress;
        public bool InProgress
        {
            get { return _inProgress; }
            set { _inProgress = value; OnPropertyChanged("InProgress"); OnPropertyChanged("CanRun"); }
        }

        private bool _fromLink;
        public bool FromLink
        {
            get { return _fromLink; }
            set { _fromLink = value; OnPropertyChanged("FromLink"); OnPropertyChanged("CanRun"); }
        }

        private int _linkIndex;
        public int LinkIndex
        {
            get { return _linkIndex; }
            set { _linkIndex = value; OnPropertyChanged("LinkIndex"); OnPropertyChanged("CanRun"); }
        }

        private string _textHeightString;
        public string TextHeightString
        {
            get { return _textHeightString; }
            set
            {
                _textHeightString = value;
                OnPropertyChanged("TextHeightString");
                OnPropertyChanged("HasTextHeightError");
            }
        }

        private double _textHeight;
        public double TextHeight
        {
            get { return _textHeight; }
            set { _textHeight = value; OnPropertyChanged("TextHeight"); }
        }

        /// <summary>Whether this document has any link to read spatial elements from.</summary>
        public bool HasLinks
        {
            get { return Links != null && Links.Any(); }
        }

        public string FromLinkCaption
        {
            get { return HasLinks ? "Use a linked model" : "No loaded links in this document"; }
        }

        public bool HasNoFamilySymbols
        {
            get { return RoomTagFamilySymbols == null || !RoomTagFamilySymbols.Any(); }
        }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(ErrorText); }
        }

        /// <summary>Whether the status card has anything to show.</summary>
        public bool HasStatus
        {
            get { return FlyOutVisibility || UnboundedFlyOutVisibility || HasError; }
        }

        /// <summary>
        /// Whether the text height box holds something this cannot read. Blank is not an error: it
        /// means leave the family's own height alone.
        /// </summary>
        public bool HasTextHeightError
        {
            get
            {
                return !string.IsNullOrWhiteSpace(TextHeightString)
                       && LengthText.ParseFeetAndInches(TextHeightString) <= 0;
            }
        }

        /// <summary>Whether the current selection is complete enough to place tags.</summary>
        public bool CanRun
        {
            get
            {
                return !InProgress
                       && SelectedFamilySymbol != null
                       && SpatialElements != null && SpatialElements.Any()
                       && (!FromLink || SelectedLink != null);
            }
        }

        private FamilySymbol SelectedFamilySymbol
        {
            get
            {
                return RoomTagFamilySymbols != null
                       && FamilySymbolIndex >= 0
                       && FamilySymbolIndex < RoomTagFamilySymbols.Count
                    ? RoomTagFamilySymbols[FamilySymbolIndex]
                    : null;
            }
        }

        private RevitLinkInstance SelectedLink
        {
            get
            {
                return Links != null && LinkIndex >= 0 && LinkIndex < Links.Count
                    ? Links[LinkIndex]
                    : null;
            }
        }

        public SpatialTagDialogViewModel(SpatialTagModel model)
        {
            Close = new RelayCommand<Window>(OnClose);
            Run = new RelayCommand<Window>(OnRun);

            Model = model;
            FlyOutText = string.Empty;
            UnboundedFlyOutText = string.Empty;
            ErrorText = string.Empty;
            TextHeightString = SpatialTagSession.TextHeight;

            TargetIndex = SpatialTagSession.TargetIndex;
            TitleText = TitleFor(TargetIndex);
            UpdateExisting = true;
            InProgress = false;
            FromLink = false;

            Links = Model.GetRevitLinks();
            GetLinkIndex();

            // After GetLinkIndex, not before: a link picked in the model means the dialog opens on
            // that link, and it should open showing that link's phases rather than the host's.
            RefreshPhasesForCurrentSource();

            RoomTagFamilySymbols = Model.CollectTagFamilySymbols();
            SpatialElements = new ObservableCollection<SpatialElement>();

            // The remembered index belongs to whichever document was open last, and this one may
            // have fewer tag types, or none. Restoring it blind is an index out of range on the
            // first run in a new model. A document with no tag family says so beside the family
            // field rather than here, so the status card stays about what the last action did.
            FamilySymbolIndex = RoomTagFamilySymbols.Any()
                ? Math.Min(Math.Max(SpatialTagSession.FamilySymbolIndex, 0), RoomTagFamilySymbols.Count - 1)
                : -1;
        }

        private static string TitleFor(int targetIndex)
        {
            return targetIndex == 0 ? "3d Room Tags" : "3d Space Tags";
        }

        /// <summary>
        /// Records which target the user chose, and everything that follows from it.
        ///
        /// Rooms and spaces are different elements in different categories, so whatever was
        /// collected for the old target says nothing about the new one and is dropped.
        /// </summary>
        public void ChangeTarget(int index)
        {
            if (index < 0) return;

            TargetIndex = index;
            TitleText = TitleFor(index);

            SpatialTagSession.TargetIndex = index;

            ClearCollectedElements();
        }

        /// <summary>
        /// Records the chosen tag family type so the next opening starts on it. -1 is "nothing
        /// chosen", not a position worth remembering.
        /// </summary>
        public void ChangeFamilySymbol(int index)
        {
            if (index < 0) return;

            FamilySymbolIndex = index;

            SpatialTagSession.FamilySymbolIndex = index;
        }

        public void GetLinkIndex()
        {
            if (Links.Any())
            {
                var linkInstance = Model.IsLinkSelected();

                if (linkInstance != null)
                {
                    // Matched on id rather than on name: two instances of the same linked file
                    // share a name, and the first one found is not necessarily the one selected.
                    LinkIndex = Links.ToList().FindIndex(l => linkInstance.Id.Equals(l.Id));

                    if (LinkIndex >= 0)
                    {
                        FromLink = true;
                        return;
                    }
                }
            }

            LinkIndex = -1;
        }

        /// <summary>
        /// Whether this element would actually get a tag.
        ///
        /// The same two conditions the planner applies: a point location, and something to write
        /// in the tag. Kept here rather than duplicated in words so the count the dialog shows and
        /// the count the run produces cannot drift apart.
        /// </summary>
        private static bool IsTaggable(SpatialElement element)
        {
            var name = element.get_Parameter(BuiltInParameter.ROOM_NAME);
            var number = element.get_Parameter(BuiltInParameter.ROOM_NUMBER);

            return element.Location is LocationPoint
                   && name != null && !string.IsNullOrWhiteSpace(name.AsString())
                   && number != null && !string.IsNullOrWhiteSpace(number.AsString());
        }

        public void RefreshRooms(Phase phase)
        {
            if (phase == null)
            {
                ClearCollectedElements();
                return;
            }

            var link = FromLink ? SelectedLink : null;

            if (FromLink && link == null)
            {
                ClearCollectedElements();
                return;
            }

            SpatialElements = link == null
                ? Model.CollectSpatialElements(phase, TargetIndex)
                : Model.CollectSpatialElements(phase, link, TargetIndex);

            var spatialElementType = TargetIndex == 0 ? "rooms" : "spaces";

            // Counted with the same rules the run itself uses, rather than counting everything
            // collected, so the number before a run and the number after it agree.
            var taggable = SpatialElements.Count(IsTaggable);

            FlyOutText = taggable + " taggable " + spatialElementType + " found in the selected phase.";
            FlyOutVisibility = true;
            ErrorText = string.Empty;

            var untaggable = SpatialElements.Count - taggable;
            var unbounded = SpatialElements.Count(s => s.Area <= 0 && IsTaggable(s));

            // Cleared rather than left standing, or a warning from a previous phase stays on screen
            // describing a count that no longer exists.
            if (untaggable == 0 && unbounded == 0)
            {
                UnboundedFlyOutText = string.Empty;
                UnboundedFlyOutVisibility = false;
                return;
            }

            var warnings = new List<string>();

            if (untaggable > 0)
            {
                warnings.Add(untaggable + " " + spatialElementType + " are unplaced or have no name or number, so they cannot be tagged.");
            }

            // Said accurately: unbounded and redundant elements are placed, they carry the name and
            // number Revit gave them, and they get tags like anything else. What is worth saying is
            // that the tag lands at a point in a room with no boundary.
            if (unbounded > 0)
            {
                warnings.Add(unbounded + " " + spatialElementType + " are unbounded or redundant. They will still be tagged, at their placement point.");
            }

            UnboundedFlyOutText = "Warning: " + string.Join(" ", warnings);
            UnboundedFlyOutVisibility = true;
        }

        public void RefreshPhases(RevitLinkInstance linkInstance)
        {
            Phases = linkInstance == null ? Model.CollectPhases() : Model.CollectPhases(linkInstance);

            ClearCollectedElements();
        }

        /// <summary>
        /// Reloads the phase list for whatever source is currently chosen. Called when the link
        /// checkbox is toggled: ticking it without a link chosen has nothing to collect from, and
        /// showing the host document's phases there invited a run against the wrong model.
        /// </summary>
        public void RefreshPhasesForCurrentSource()
        {
            if (!FromLink)
            {
                RefreshPhases(null);
                return;
            }

            var link = SelectedLink;

            if (link == null)
            {
                Phases = new ObservableCollection<Phase>();
                ClearCollectedElements();
                return;
            }

            RefreshPhases(link);
        }

        /// <summary>
        /// Drops the collected elements and the counts describing them. Anything that changes what
        /// would be tagged has to come through here, or the status card keeps describing a
        /// selection the user has already moved on from.
        /// </summary>
        public void ClearCollectedElements()
        {
            SpatialElements = new ObservableCollection<SpatialElement>();
            FlyOutText = string.Empty;
            FlyOutVisibility = false;
            UnboundedFlyOutText = string.Empty;
            UnboundedFlyOutVisibility = false;
        }

        private void OnRun(Window win)
        {
            var spatialElementTag = SelectedFamilySymbol;

            if (spatialElementTag == null || SpatialElements == null || !SpatialElements.Any()) return;

            InProgress = true;

            try
            {
                var link = FromLink ? SelectedLink : null;

                TextHeight = LengthText.ParseFeetAndInches(TextHeightString);

                // Remembered for next time only when it is a height this could read. Remembering an
                // unreadable one would hand the same error back on every opening.
                if (TextHeight > 0) SpatialTagSession.TextHeight = TextHeightString;

                var result = Model.RunTagging(spatialElementTag, SpatialElements, UpdateExisting, link, TextHeight);

                LastResult = result;
                LastFailure = null;

                if (result.MissingParameter != null)
                {
                    LastFailure = "The selected tag family type has no \"" + result.MissingParameter + "\" parameter, so nothing was placed.";
                    ErrorText = "The selected tag family type cannot be used: it has no \"" + result.MissingParameter + "\" parameter. Nothing was placed.";
                    FlyOutVisibility = false;
                    return;
                }

                var summary = result.Tags.Count + " tags created or updated.";

                if (result.MigratedLegacyTags > 0)
                {
                    summary += " " + result.MigratedLegacyTags + " of them were tags from an earlier version, "
                               + "now recorded against the link they came from.";
                }

                FlyOutText = summary;
                FlyOutVisibility = true;

                // Both are worth saying, and a run can produce both at once, so neither is allowed
                // to hide the other.
                var notes = new List<string>();

                if (result.SkippedNotEditable > 0)
                {
                    notes.Add(result.SkippedNotEditable + " existing tags are owned by another user or out of date, so they were left alone.");
                }

                if (result.OrphanedTags > 0)
                {
                    notes.Add(result.OrphanedTags + " tags are for elements that no longer exist. They still say what they said, "
                              + "so they are now wrong; delete them yourself if you no longer want them.");
                }

                ErrorText = string.Join(" ", notes);
            }
            catch (Exception ex)
            {
                // A Revit API failure here would otherwise escape into Revit's own error dialog
                // with a stack trace. The user can act on a sentence; they cannot act on that. The
                // node reports the same sentence when the dialog closes.
                LastFailure = ex.Message;
                ErrorText = "Tags could not be created: " + ex.Message;
                FlyOutVisibility = false;
            }
            finally
            {
                InProgress = false;
            }
        }

        private static void OnClose(Window win)
        {
            try
            {
                if (win != null) win.Close();
            }
            catch (Exception)
            {
                // A window already closing. Nothing to do about it and nothing worth saying.
            }
        }
    }
}
#endif
