
using System.Windows.Forms;
using System = global::System;
namespace Rhythm
{
    internal partial class FamilyUpgradeForm
    {

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private global::System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            global::System.ComponentModel.ComponentResourceManager resources = new global::System.ComponentModel.ComponentResourceManager(typeof(FamilyUpgradeForm));
            this.progressBar1 = new global::System.Windows.Forms.ProgressBar();
            this.label1 = new global::System.Windows.Forms.Label();
            this.label3 = new global::System.Windows.Forms.Label();
            this.linkLabel1 = new global::System.Windows.Forms.LinkLabel();
            this.label4 = new global::System.Windows.Forms.Label();
            this.pictureBox1 = new global::System.Windows.Forms.PictureBox();
            this.button1 = new global::System.Windows.Forms.Button();
            ((global::System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new global::System.Drawing.Point(14, 75);
            this.progressBar1.Margin = new global::System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new global::System.Drawing.Size(432, 29);
            this.progressBar1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new global::System.Drawing.Point(10, 52);
            this.label1.Margin = new global::System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new global::System.Drawing.Size(84, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Processing...";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new global::System.Drawing.Font("Open Sans", 9.75F, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new global::System.Drawing.Point(10, 159);
            this.label3.Margin = new global::System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new global::System.Drawing.Size(259, 23);
            this.label3.TabIndex = 4;
            this.label3.Text = "This free tool brought to you by:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new global::System.Drawing.Font("Open Sans", 9.75F, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.Location = new global::System.Drawing.Point(42, 181);
            this.linkLabel1.Margin = new global::System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new global::System.Drawing.Size(276, 23);
            this.linkLabel1.TabIndex = 6;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://designtechunraveled.com/";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new global::System.Drawing.Font("Open Sans", 14.25F, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = global::System.Drawing.SystemColors.Highlight;
            this.label4.Location = new global::System.Drawing.Point(8, 11);
            this.label4.Margin = new global::System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new global::System.Drawing.Size(187, 33);
            this.label4.TabIndex = 7;
            this.label4.Text = "Tool Title Here";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((global::System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((global::System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new global::System.Drawing.Point(326, 137);
            this.pictureBox1.Margin = new global::System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new global::System.Drawing.Size(130, 94);
            this.pictureBox1.SizeMode = global::System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Font = new global::System.Drawing.Font("Open Sans", 9.75F, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new global::System.Drawing.Point(311, 112);
            this.button1.Margin = new global::System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new global::System.Drawing.Size(135, 29);
            this.button1.TabIndex = 8;
            this.button1.Text = "Cancel ";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new global::System.EventHandler(this.Button1_Click);
            // 
            // FamilyUpgradeForm
            // 
            this.AutoScaleDimensions = new global::System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new global::System.Drawing.Size(459, 235);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new global::System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FamilyUpgradeForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Working...";
            this.TopMost = true;
            ((global::System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private global::System.Windows.Forms.Label label1;
        private global::System.Windows.Forms.ProgressBar progressBar1;
        private global::System.Windows.Forms.PictureBox pictureBox1;
        private Label label3;
        private LinkLabel linkLabel1;
        private Label label4;
        private Button button1;
    }
}
