using System;
using System.Drawing;
using System.Windows.Forms;

namespace MwtWinDllTest
{
    partial class frmTextbrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            txtData = new TextBox();
            MainMenuControl = new MainMenu();
            mnuFile = new MenuItem();
            mnuFileExit = new MenuItem();
            mnuFileExit.Click += new EventHandler(mnuFileExit_Click);
            mnuEdit = new MenuItem();
            mnuEditCut = new MenuItem();
            mnuEditCut.Click += new EventHandler(mnuEditCut_Click);
            mnuEditCopy = new MenuItem();
            mnuEditCopy.Click += new EventHandler(mnuEditCopy_Click);
            mnuEditPaste = new MenuItem();
            mnuEditPaste.Click += new EventHandler(mnuEditPaste_Click);
            mnuEditSep1 = new MenuItem();
            mnuEditFontSizeDecrease = new MenuItem();
            mnuEditFontSizeDecrease.Click += new EventHandler(mnuEditFontSizeDecrease_Click);
            mnuEditFontSizeIncrease = new MenuItem();
            mnuEditFontSizeIncrease.Click += new EventHandler(mnuEditFontSizeIncrease_Click);
            SuspendLayout();
            //
            // txtData
            //
            txtData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtData.Location = new Point(0, 0);
            txtData.Multiline = true;
            txtData.Name = "txtData";
            txtData.ScrollBars = ScrollBars.Both;
            txtData.Size = new Size(488, 316);
            txtData.TabIndex = 0;
            txtData.Text = "";
            txtData.WordWrap = false;
            //
            // MainMenuControl
            //
            MainMenuControl.MenuItems.AddRange(new MenuItem[] { mnuFile, mnuEdit });
            //
            // mnuFile
            //
            mnuFile.Index = 0;
            mnuFile.MenuItems.AddRange(new MenuItem[] { mnuFileExit });
            mnuFile.Text = "&File";
            //
            // mnuFileExit
            //
            mnuFileExit.Index = 0;
            mnuFileExit.Text = "E&xit";
            //
            // mnuEdit
            //
            mnuEdit.Index = 1;
            mnuEdit.MenuItems.AddRange(new MenuItem[] { mnuEditCut, mnuEditCopy, mnuEditPaste, mnuEditSep1, mnuEditFontSizeDecrease, mnuEditFontSizeIncrease });
            mnuEdit.Text = "&Edit";
            //
            // mnuEditCut
            //
            mnuEditCut.Index = 0;
            mnuEditCut.Text = "Cu&t";
            //
            // mnuEditCopy
            //
            mnuEditCopy.Index = 1;
            mnuEditCopy.Text = "&Copy";
            //
            // mnuEditPaste
            //
            mnuEditPaste.Index = 2;
            mnuEditPaste.Text = "&Paste";
            //
            // mnuEditSep1
            //
            mnuEditSep1.Index = 3;
            mnuEditSep1.Text = "-";
            //
            // mnuEditFontSizeDecrease
            //
            mnuEditFontSizeDecrease.Index = 4;
            mnuEditFontSizeDecrease.Shortcut = Shortcut.F3;
            mnuEditFontSizeDecrease.Text = "Decrease Font Size";
            //
            // mnuEditFontSizeIncrease
            //
            mnuEditFontSizeIncrease.Index = 5;
            mnuEditFontSizeIncrease.Shortcut = Shortcut.F4;
            mnuEditFontSizeIncrease.Text = "Increase Font Size";
            //
            // frmTextbrowser
            //
            AutoScaleBaseSize = new Size(5, 13);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new Size(488, 314);
            Controls.Add(txtData);
            Menu = MainMenuControl;
            Name = "frmTextbrowser";
            Text = "frmTextbrowser";
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtData;
        private MainMenu MainMenuControl;
        private MenuItem mnuFile;
        private MenuItem mnuFileExit;
        private MenuItem mnuEdit;
        private MenuItem mnuEditCut;
        private MenuItem mnuEditCopy;
        private MenuItem mnuEditPaste;
        private MenuItem mnuEditSep1;
        private MenuItem mnuEditFontSizeDecrease;
        private MenuItem mnuEditFontSizeIncrease;
    }
}