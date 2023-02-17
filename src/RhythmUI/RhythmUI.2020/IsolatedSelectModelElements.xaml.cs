using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CoreNodeModels;
using RevitServices.Persistence;
using RhythmUI.Utilities;
using ComboBox = Autodesk.Revit.UI.ComboBox;

namespace RhythmUI
{
    /// <summary>
    /// Interaction logic for IsolatedSelectModelElements.xaml
    /// </summary>
    public partial class IsolatedSelectModelElements : UserControl
    {
        public class CustomCat
        {
            public string Name { get; set; }
            public Category cat { get; set; }
        }

        public static ISelectionFilter selFilter;

        public IsolatedSelectModelElements()
        {
            InitializeComponent();
            PackLists();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            UIDocument uidoc = DocumentManager.Instance.CurrentUIDocument;
            try
            {
                var selection = uidoc.Selection.PickObjects(ObjectType.Element, selFilter);
                IsolatedSelectModelElementsUI.results = selection.Select(s => doc.GetElement(s.ElementId)).ToList();
            }
            catch (Exception)
            {
                // testing
            }
           
        }

        private void PackLists()
        {
            List<CustomCat> categories = new List<CustomCat>();
            Document document = DocumentManager.Instance.CurrentDBDocument;

            foreach (Category category in document.Settings.Categories)
            {
                try
                {
                    string name = getFullName(category);
                    categories.Add(new CustomCat{Name = name, cat = category});
                }
                catch
                {
                    // We get here for internal/deprecated categories
                    continue;
                }
            }

            this.CategoriesComboBox.ItemsSource = categories.OrderBy(c => c.Name);
            this.CategoriesComboBox.DisplayMemberPath = "Name";
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox cBox = sender as System.Windows.Controls.ComboBox;
            CustomCat ddItem = cBox.SelectedItem as CustomCat;

            Category cat = ddItem.cat;

            selFilter =
                SelFilter.GetElementFilter(
                    new ElementCategoryFilter(cat.Id));
            this.SelectButton.IsEnabled = true;
        }
        private static string getFullName(Category category)
        {
            string name = String.Empty;
            if (category != null)
            {
                var parent = category.Parent;
                if (parent == null)
                {
                    // Top level category
                    // For example "Cable Trays"
                    name = category.Name.ToString();
                }
                else
                {
                    // Sub-category
                    // For example "Cable Tray - Center Lines"
                    name = parent.Name.ToString() + " - " + category.Name.ToString();
                }
            }
            return name;
        }
    }
}
