using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace MwtWinDllTest
{
    public partial class frmTextbrowser : Form
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

        public frmTextbrowser()
        {
            InitializeComponent();
            InitializeForm();
        }

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
