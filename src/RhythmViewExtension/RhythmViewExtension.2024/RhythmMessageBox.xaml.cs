using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            Process.Start("https://designtechunraveled.com/rhythm-for-revit-loading-errors/");
        }
    }
}
