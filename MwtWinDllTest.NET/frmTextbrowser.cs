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

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
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
                if (components is object)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.
        // Do not modify it using the code editor.
        internal TextBox txtData;
        internal MainMenu MainMenuControl;
        internal MenuItem mnuFile;
        private MenuItem _mnuFileExit;

        internal MenuItem mnuFileExit
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuFileExit;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuFileExit != null)
                {
                    /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
                    /* TODO ERROR: Skipped RegionDirectiveTrivia */
                    _mnuFileExit.Click -= mnuFileExit_Click;
                }

                _mnuFileExit = value;
                if (_mnuFileExit != null)
                {
                    _mnuFileExit.Click += mnuFileExit_Click;
                }
            }
        }

        internal MenuItem mnuEdit;
        private MenuItem _mnuEditCut;

        internal MenuItem mnuEditCut
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditCut;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditCut != null)
                {
                    _mnuEditCut.Click -= mnuEditCut_Click;
                }

                _mnuEditCut = value;
                if (_mnuEditCut != null)
                {
                    _mnuEditCut.Click += mnuEditCut_Click;
                }
            }
        }

        private MenuItem _mnuEditCopy;

        internal MenuItem mnuEditCopy
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditCopy;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditCopy != null)
                {
                    _mnuEditCopy.Click -= mnuEditCopy_Click;
                }

                _mnuEditCopy = value;
                if (_mnuEditCopy != null)
                {
                    _mnuEditCopy.Click += mnuEditCopy_Click;
                }
            }
        }

        private MenuItem _mnuEditPaste;

        internal MenuItem mnuEditPaste
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditPaste;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditPaste != null)
                {
                    _mnuEditPaste.Click -= mnuEditPaste_Click;
                }

                _mnuEditPaste = value;
                if (_mnuEditPaste != null)
                {
                    _mnuEditPaste.Click += mnuEditPaste_Click;
                }
            }
        }

        internal MenuItem mnuEditSep1;
        private MenuItem _mnuEditFontSizeDecrease;

        internal MenuItem mnuEditFontSizeDecrease
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditFontSizeDecrease;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditFontSizeDecrease != null)
                {
                    _mnuEditFontSizeDecrease.Click -= mnuEditFontSizeDecrease_Click;
                }

                _mnuEditFontSizeDecrease = value;
                if (_mnuEditFontSizeDecrease != null)
                {
                    _mnuEditFontSizeDecrease.Click += mnuEditFontSizeDecrease_Click;
                }
            }
        }

        private MenuItem _mnuEditFontSizeIncrease;

        internal MenuItem mnuEditFontSizeIncrease
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditFontSizeIncrease;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditFontSizeIncrease != null)
                {
                    _mnuEditFontSizeIncrease.Click -= mnuEditFontSizeIncrease_Click;
                }

                _mnuEditFontSizeIncrease = value;
                if (_mnuEditFontSizeIncrease != null)
                {
                    _mnuEditFontSizeIncrease.Click += mnuEditFontSizeIncrease_Click;
                }
            }
        }

        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            txtData = new TextBox();
            MainMenuControl = new MainMenu();
            mnuFile = new MenuItem();
            _mnuFileExit = new MenuItem();
            _mnuFileExit.Click += new EventHandler(mnuFileExit_Click);
            mnuEdit = new MenuItem();
            _mnuEditCut = new MenuItem();
            _mnuEditCut.Click += new EventHandler(mnuEditCut_Click);
            _mnuEditCopy = new MenuItem();
            _mnuEditCopy.Click += new EventHandler(mnuEditCopy_Click);
            _mnuEditPaste = new MenuItem();
            _mnuEditPaste.Click += new EventHandler(mnuEditPaste_Click);
            mnuEditSep1 = new MenuItem();
            _mnuEditFontSizeDecrease = new MenuItem();
            _mnuEditFontSizeDecrease.Click += new EventHandler(mnuEditFontSizeDecrease_Click);
            _mnuEditFontSizeIncrease = new MenuItem();
            _mnuEditFontSizeIncrease.Click += new EventHandler(mnuEditFontSizeIncrease_Click);
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
            mnuFile.MenuItems.AddRange(new MenuItem[] { _mnuFileExit });
            mnuFile.Text = "&File";
            // 
            // mnuFileExit
            // 
            _mnuFileExit.Index = 0;
            _mnuFileExit.Text = "E&xit";
            // 
            // mnuEdit
            // 
            mnuEdit.Index = 1;
            mnuEdit.MenuItems.AddRange(new MenuItem[] { _mnuEditCut, _mnuEditCopy, _mnuEditPaste, mnuEditSep1, _mnuEditFontSizeDecrease, _mnuEditFontSizeIncrease });
            mnuEdit.Text = "&Edit";
            // 
            // mnuEditCut
            // 
            _mnuEditCut.Index = 0;
            _mnuEditCut.Text = "Cu&t";
            // 
            // mnuEditCopy
            // 
            _mnuEditCopy.Index = 1;
            _mnuEditCopy.Text = "&Copy";
            // 
            // mnuEditPaste
            // 
            _mnuEditPaste.Index = 2;
            _mnuEditPaste.Text = "&Paste";
            // 
            // mnuEditSep1
            // 
            mnuEditSep1.Index = 3;
            mnuEditSep1.Text = "-";
            // 
            // mnuEditFontSizeDecrease
            // 
            _mnuEditFontSizeDecrease.Index = 4;
            _mnuEditFontSizeDecrease.Shortcut = Shortcut.F3;
            _mnuEditFontSizeDecrease.Text = "Decrease Font Size";
            // 
            // mnuEditFontSizeIncrease
            // 
            _mnuEditFontSizeIncrease.Index = 5;
            _mnuEditFontSizeIncrease.Shortcut = Shortcut.F4;
            _mnuEditFontSizeIncrease.Text = "Increase Font Size";
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

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public bool ReadOnlyText
        {
            get
            {
                return txtData.ReadOnly;
            }

            set
            {
                txtData.ReadOnly = value;
            }
        }

        public string GetText
        {
            get
            {
                return txtData.Text;
            }
        }

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
            get
            {
                return txtData.Font.SizeInPoints;
            }

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
                catch (Exception ex)
                {
                    // Ignore errors here
                }
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
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

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}