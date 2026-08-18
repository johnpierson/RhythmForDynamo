// The 3d spatial tags dialog is Revit 2025 and up, with the node it belongs to.
#if R25_OR_GREATER
using System;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Rhythm.SpatialTagsUi
{
    /// <summary>
    /// The 3d Spatial Tags dialog, as the add-in draws it.
    ///
    /// The handlers here are the view's own business: which control changed, and what the view
    /// model should be told about it. Everything that follows from a change lives in the view
    /// model, so there is one owner for each piece of state.
    /// </summary>
    internal sealed partial class SpatialTagDialog : Window
    {
        public SpatialTagDialog()
        {
            InitializeComponent();

            // The title bar is Windows' own, tinted to the same ground the dialog is drawn on, so
            // the window reads as one surface rather than a cream form wearing a white hat. The
            // colours are read out of the theme rather than written here, or the two drift the
            // first time the palette is touched.
            WindowChromeUtils.ApplyCaptionColors(
                this,
                ThemeColor("Interlude.Background", Colors.White),
                ThemeColor("Interlude.Foreground", Colors.Black),
                ThemeColor("Interlude.Border", Colors.Black));
        }

        /// <summary>
        /// A colour from the merged Interlude dictionary, or the fallback if the key is missing or
        /// is not a solid colour brush.
        /// </summary>
        private Color ThemeColor(string key, Color fallback)
        {
            var brush = TryFindResource(key) as SolidColorBrush;

            return brush == null ? fallback : brush.Color;
        }

        /// <summary>
        /// The view model, or null while the window is being built.
        ///
        /// Every handler below goes through this. The data context is assigned after construction,
        /// and the combo boxes raise SelectionChanged as their bindings first resolve, so a
        /// straight cast here would be a null reference waiting for the right ordering.
        /// </summary>
        private SpatialTagDialogViewModel ViewModel
        {
            get { return DataContext as SpatialTagDialogViewModel; }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var vm = ViewModel;
            if (vm == null) return;

            // A cleared phase clears what would be tagged with it. Returning early instead would
            // leave the previous phase's element count on screen and arm the run button with it.
            vm.RefreshRooms(PhaseComboBox.SelectedItem as Phase);
        }

        private void Link_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var vm = ViewModel;
            if (vm == null) return;

            vm.RefreshPhasesForCurrentSource();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var vm = ViewModel;
            if (vm == null) return;

            vm.RefreshPhasesForCurrentSource();
        }

        private void LicenceLink_OnClick(object sender, RoutedEventArgs e)
        {
            const string url = "https://github.com/johnpierson/3dSpatialTags/blob/main/LICENSE";

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                // No browser, or a policy that blocks launching one. Not worth taking the dialog
                // down over a credit link.
            }
        }

        private void TargetChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var vm = ViewModel;
            if (vm == null) return;

            // Clearing the phase is a view concern: it is this control's own selection. What that
            // means for the remembered choice, the window title and the collected elements is the
            // view model's, and lives there.
            PhaseComboBox.SelectedIndex = -1;

            vm.ChangeTarget(TargetComboBox.SelectedIndex);
        }

        private void FamilySymbolSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var vm = ViewModel;
            if (vm == null) return;

            vm.ChangeFamilySymbol(FamilySymbolComboBox.SelectedIndex);
        }
    }
}
#endif
