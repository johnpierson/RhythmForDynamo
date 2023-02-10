#region Header
// Revit MEP API sample application
//
// Copyright (C) 2007-2010 by Jeremy Tammik, Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software
// for any purpose and without fee is hereby granted, provided
// that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  
// AUTODESK, INC. DOES NOT WARRANT THAT THE OPERATION OF THE 
// PROGRAM WILL BE UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject
// to restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
#endregion // Header

#region Namespaces
using System;
using System.Drawing;
using System.Windows.Forms;
#endregion // Namespaces

namespace Rhythm
{
    internal partial class DefaultProgressForm : Form
    {


        string _format;
        string _toolTitle;

        /// <summary>
        /// Set up progress bar form and immediately display it modelessly.
        /// </summary>
        /// <param name="caption">Form caption</param>
        /// <param name="format">Progress message string</param>
        /// <param name="toolTitle">Name of the tool.</param>
        /// <param name="max">Number of elements to process</param>
        internal DefaultProgressForm(string caption, string format, string toolTitle, int max)
        {

            _format = format;
            _toolTitle = toolTitle;
            InitializeComponent();
            Text = caption;
            label1.Text = (null == format) ? caption : string.Format(format, 1);
            label4.Text = toolTitle;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = max;
            progressBar1.Value = 0;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            Show();
            Application.DoEvents();

        }

        internal void Increment()
        {
            ++progressBar1.Value;
            if (null != _format)
            {
                label1.Text = string.Format(_format, progressBar1.Value);
            }
            Application.DoEvents();
        }

    }
}
