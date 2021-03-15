using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace MwtWinDllTest
{
    public class frmTextbrowser : Form
    {
        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
        // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
        // Website: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL and https://omics.pnl.gov/
        // -------------------------------------------------------------------------------
        //
        // Licensed under the Apache License, Version 2.0; you may not use this file except
        // in compliance with the License.  You may obtain a copy of the License at
        // http://www.apache.org/licenses/LICENSE-2.0
        //
        // Notice: This computer software was prepared by Battelle Memorial Institute,
        // hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
        // Department of Energy (DOE).  All rights in the computer software are reserved
        // by DOE on behalf of the United States Government and the Contractor as
        // provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY
        // WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS
        // SOFTWARE.  This notice including this sentence must appear on any copies of
        // this computer software.

        #region "Windows Form Designer generated code"
        public frmTextbrowser() : base()
        {

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call
            InitializeForm();
        }

        // Form overrides dispose to clean up the component list.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        // Required by the Windows Form Designer
        private readonly System.ComponentModel.IContainer components = null;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.
        // Do not modify it using the code editor.
        internal TextBox txtData;
        internal MainMenu MainMenuControl;
        internal MenuItem mnuFile;
        internal MenuItem mnuFileExit;
        internal MenuItem mnuEdit;
        internal MenuItem mnuEditCut;
        internal MenuItem mnuEditCopy;
        internal MenuItem mnuEditPaste;
        internal MenuItem mnuEditSep1;
        internal MenuItem mnuEditFontSizeDecrease;
        internal MenuItem mnuEditFontSizeIncrease;

        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
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
            ClientSize = new Size(488, 314);
            Controls.Add(txtData);
            Menu = MainMenuControl;
            Name = "frmTextbrowser";
            Text = "frmTextbrowser";
            ResumeLayout(false);
        }

        #endregion

        #region "Processing Options Interface Functions"
        public bool ReadOnlyText
        {
            get => txtData.ReadOnly;
            set => txtData.ReadOnly = value;
        }

        public string GetText => txtData.Text;

        public string SetText
        {
            set
            {
                txtData.Text = value;
                txtData.SelectionStart = 1;
                txtData.ScrollToCaret();
            }
        }

        public float TextFontSize
        {
            get => txtData.Font.SizeInPoints;
            set
            {
                if (value < 6f)
                {
                    value = 6f;
                }
                else if (value > 72f)
                {
                    value = 72f;
                }

                try
                {
                    txtData.Font = new Font(txtData.Font.FontFamily, value);
                }
                catch
                {
                    // Ignore errors here
                }
            }
        }
        #endregion

        #region "Procedures"

        public void AppendText(string Value)
        {
            txtData.Text += Value + ControlChars.NewLine;
            txtData.SelectionStart = txtData.TextLength;
            txtData.ScrollToCaret();
        }

        private void CopyText()
        {
            txtData.Copy();
        }

        private void CutText()
        {
            if (txtData.ReadOnly)
            {
                CopyText();
            }
            else
            {
                txtData.Cut();
            }
        }

        private void InitializeForm()
        {
            txtData.ReadOnly = true;
            TextFontSize = 11f;
        }

        private void PasteText()
        {
            if (txtData.ReadOnly)
                return;
            txtData.Paste();
        }
        #endregion

        #region "Menu Handlers"

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuEditCut_Click(object sender, EventArgs e)
        {
            CutText();
        }

        private void mnuEditCopy_Click(object sender, EventArgs e)
        {
            CopyText();
        }

        private void mnuEditPaste_Click(object sender, EventArgs e)
        {
            PasteText();
        }

        private void mnuEditFontSizeDecrease_Click(object sender, EventArgs e)
        {
            if (TextFontSize > 14f)
            {
                TextFontSize -= 2f;
            }
            else
            {
                TextFontSize -= 1f;
            }
        }

        private void mnuEditFontSizeIncrease_Click(object sender, EventArgs e)
        {
            if (TextFontSize >= 14f)
            {
                TextFontSize += 2f;
            }
            else
            {
                TextFontSize += 1f;
            }
        }

        #endregion
    }
}