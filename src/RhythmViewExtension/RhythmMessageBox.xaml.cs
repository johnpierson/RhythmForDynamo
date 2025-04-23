using System.Diagnostics;
using System.Windows;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RhythmViewExtension
{
    public sealed partial class RhythmMessageBox : Window
    {
       
        public RhythmMessageBox()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Process openWeblink = new Process();
            openWeblink.StartInfo.FileName = "https://designtechunraveled.com/rhythm-for-revit-loading-errors/";
            openWeblink.StartInfo.UseShellExecute = true;
            openWeblink.Start();
        }
    }
}
