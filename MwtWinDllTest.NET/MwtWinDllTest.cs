﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using MwtWinDll;

namespace MwtWinDllTest
{
    // Molecular Weight Calculator Dll test program

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

    internal class frmMwtWinDllTest : Form
    {
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public frmMwtWinDllTest()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            InitializeControls();
        }
        // Form overrides dispose to clean up the component list.
        protected override void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                if (components is object)
                {
                    components.Dispose();
                }
            }

            base.Dispose(Disposing);
        }
        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;
        public ToolTip ToolTip1;
        private Button _cmdTestGetTrypticName;

        public Button cmdTestGetTrypticName
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdTestGetTrypticName;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdTestGetTrypticName != null)
                {
                    _cmdTestGetTrypticName.Click -= cmdTestGetTrypticName_Click;
                }

                _cmdTestGetTrypticName = value;
                if (_cmdTestGetTrypticName != null)
                {
                    _cmdTestGetTrypticName.Click += cmdTestGetTrypticName_Click;
                }
            }
        }
        // Public WithEvents grdFlexGrid As AxMSFlexGridLib.AxMSFlexGrid
        private Button _cmdExpandAbbreviations;

        public Button cmdExpandAbbreviations
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdExpandAbbreviations;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdExpandAbbreviations != null)
                {
                    _cmdExpandAbbreviations.Click -= cmdExpandAbbreviations_Click;
                }

                _cmdExpandAbbreviations = value;
                if (_cmdExpandAbbreviations != null)
                {
                    _cmdExpandAbbreviations.Click += cmdExpandAbbreviations_Click;
                }
            }
        }

        private Button _cmdTestFunctions;

        public Button cmdTestFunctions
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdTestFunctions;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdTestFunctions != null)
                {
                    _cmdTestFunctions.Click -= cmdTestFunctions_Click;
                }

                _cmdTestFunctions = value;
                if (_cmdTestFunctions != null)
                {
                    _cmdTestFunctions.Click += cmdTestFunctions_Click;
                }
            }
        }

        private ComboBox _cboStdDevMode;

        public ComboBox cboStdDevMode
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cboStdDevMode;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cboStdDevMode != null)
                {
                    _cboStdDevMode.SelectedIndexChanged -= cboStdDevMode_SelectedIndexChanged;
                }

                _cboStdDevMode = value;
                if (_cboStdDevMode != null)
                {
                    _cboStdDevMode.SelectedIndexChanged += cboStdDevMode_SelectedIndexChanged;
                }
            }
        }

        private ComboBox _cboWeightMode;

        public ComboBox cboWeightMode
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cboWeightMode;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cboWeightMode != null)
                {
                    _cboWeightMode.SelectedIndexChanged -= cboWeightMode_SelectedIndexChanged;
                }

                _cboWeightMode = value;
                if (_cboWeightMode != null)
                {
                    _cboWeightMode.SelectedIndexChanged += cboWeightMode_SelectedIndexChanged;
                }
            }
        }

        private Button _cmdConvertToEmpirical;

        public Button cmdConvertToEmpirical
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdConvertToEmpirical;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdConvertToEmpirical != null)
                {
                    _cmdConvertToEmpirical.Click -= cmdConvertToEmpirical_Click;
                }

                _cmdConvertToEmpirical = value;
                if (_cmdConvertToEmpirical != null)
                {
                    _cmdConvertToEmpirical.Click += cmdConvertToEmpirical_Click;
                }
            }
        }

        private Button _cmdFindMass;

        public Button cmdFindMass
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdFindMass;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdFindMass != null)
                {
                    _cmdFindMass.Click -= cmdFindMass_Click;
                }

                _cmdFindMass = value;
                if (_cmdFindMass != null)
                {
                    _cmdFindMass.Click += cmdFindMass_Click;
                }
            }
        }

        public TextBox txtFormula;
        private Button _cmdClose;

        public Button cmdClose
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdClose;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdClose != null)
                {
                    _cmdClose.Click -= cmdClose_Click;
                }

                _cmdClose = value;
                if (_cmdClose != null)
                {
                    _cmdClose.Click += cmdClose_Click;
                }
            }
        }

        public Label lblStdDevMode;
        public Label lblWeightMode;
        public Label lblStatusLabel;
        public Label lblMassAndStdDevLabel;
        public Label lblMassAndStdDev;
        public Label lblStatus;
        public Label lblMass;
        public Label lblMassLabel;
        public Label lblFormula;
        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.
        // Do not modify it using the code editor.
        internal DataGrid dgDataGrid;
        private RichTextBox _rtfFormula;

        internal RichTextBox rtfFormula
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _rtfFormula;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_rtfFormula != null)
                {
                    _rtfFormula.TextChanged -= rtfFormula_TextChanged;
                }

                _rtfFormula = value;
                if (_rtfFormula != null)
                {
                    _rtfFormula.TextChanged += rtfFormula_TextChanged;
                }
            }
        }

        private CheckBox _chkShowRTFSource;

        internal CheckBox chkShowRTFSource
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkShowRTFSource;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkShowRTFSource != null)
                {
                    _chkShowRTFSource.CheckedChanged -= chkShowRTFSource_CheckedChanged;
                }

                _chkShowRTFSource = value;
                if (_chkShowRTFSource != null)
                {
                    _chkShowRTFSource.CheckedChanged += chkShowRTFSource_CheckedChanged;
                }
            }
        }

        public TextBox txtRTFSource;
        public Label lblProgress;
        private Button _cmdTestFormulaFinder;

        public Button cmdTestFormulaFinder
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdTestFormulaFinder;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdTestFormulaFinder != null)
                {
                    _cmdTestFormulaFinder.Click -= cmdTestFormulaFinder_Click;
                }

                _cmdTestFormulaFinder = value;
                if (_cmdTestFormulaFinder != null)
                {
                    _cmdTestFormulaFinder.Click += cmdTestFormulaFinder_Click;
                }
            }
        }

        public ComboBox cboFormulaFinderTestMode;
        public Label lblDLLVersion;

        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ToolTip1 = new ToolTip(components);
            _cmdTestGetTrypticName = new Button();
            _cmdTestGetTrypticName.Click += new EventHandler(cmdTestGetTrypticName_Click);
            _cmdExpandAbbreviations = new Button();
            _cmdExpandAbbreviations.Click += new EventHandler(cmdExpandAbbreviations_Click);
            _cmdTestFunctions = new Button();
            _cmdTestFunctions.Click += new EventHandler(cmdTestFunctions_Click);
            _cboStdDevMode = new ComboBox();
            _cboStdDevMode.SelectedIndexChanged += new EventHandler(cboStdDevMode_SelectedIndexChanged);
            _cboWeightMode = new ComboBox();
            _cboWeightMode.SelectedIndexChanged += new EventHandler(cboWeightMode_SelectedIndexChanged);
            _cmdConvertToEmpirical = new Button();
            _cmdConvertToEmpirical.Click += new EventHandler(cmdConvertToEmpirical_Click);
            _cmdFindMass = new Button();
            _cmdFindMass.Click += new EventHandler(cmdFindMass_Click);
            txtFormula = new TextBox();
            _cmdClose = new Button();
            _cmdClose.Click += new EventHandler(cmdClose_Click);
            lblStdDevMode = new Label();
            lblWeightMode = new Label();
            lblStatusLabel = new Label();
            lblMassAndStdDevLabel = new Label();
            lblMassAndStdDev = new Label();
            lblStatus = new Label();
            lblMass = new Label();
            lblMassLabel = new Label();
            lblFormula = new Label();
            dgDataGrid = new DataGrid();
            _rtfFormula = new RichTextBox();
            _rtfFormula.TextChanged += new EventHandler(rtfFormula_TextChanged);
            _chkShowRTFSource = new CheckBox();
            _chkShowRTFSource.CheckedChanged += new EventHandler(chkShowRTFSource_CheckedChanged);
            txtRTFSource = new TextBox();
            lblDLLVersion = new Label();
            lblProgress = new Label();
            _cmdTestFormulaFinder = new Button();
            _cmdTestFormulaFinder.Click += new EventHandler(cmdTestFormulaFinder_Click);
            cboFormulaFinderTestMode = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dgDataGrid).BeginInit();
            SuspendLayout();
            // 
            // cmdTestGetTrypticName
            // 
            _cmdTestGetTrypticName.BackColor = SystemColors.Control;
            _cmdTestGetTrypticName.Cursor = Cursors.Default;
            _cmdTestGetTrypticName.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cmdTestGetTrypticName.ForeColor = SystemColors.ControlText;
            _cmdTestGetTrypticName.Location = new Point(307, 396);
            _cmdTestGetTrypticName.Name = "_cmdTestGetTrypticName";
            _cmdTestGetTrypticName.RightToLeft = RightToLeft.No;
            _cmdTestGetTrypticName.Size = new Size(107, 51);
            _cmdTestGetTrypticName.TabIndex = 19;
            _cmdTestGetTrypticName.Text = "Test Get Tryptic Name";
            _cmdTestGetTrypticName.UseVisualStyleBackColor = false;
            // 
            // cmdExpandAbbreviations
            // 
            _cmdExpandAbbreviations.BackColor = SystemColors.Control;
            _cmdExpandAbbreviations.Cursor = Cursors.Default;
            _cmdExpandAbbreviations.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cmdExpandAbbreviations.ForeColor = SystemColors.ControlText;
            _cmdExpandAbbreviations.Location = new Point(346, 10);
            _cmdExpandAbbreviations.Name = "_cmdExpandAbbreviations";
            _cmdExpandAbbreviations.RightToLeft = RightToLeft.No;
            _cmdExpandAbbreviations.Size = new Size(106, 50);
            _cmdExpandAbbreviations.TabIndex = 4;
            _cmdExpandAbbreviations.Text = "Expand Abbreviations";
            _cmdExpandAbbreviations.UseVisualStyleBackColor = false;
            // 
            // cmdTestFunctions
            // 
            _cmdTestFunctions.BackColor = SystemColors.Control;
            _cmdTestFunctions.Cursor = Cursors.Default;
            _cmdTestFunctions.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cmdTestFunctions.ForeColor = SystemColors.ControlText;
            _cmdTestFunctions.Location = new Point(614, 10);
            _cmdTestFunctions.Name = "_cmdTestFunctions";
            _cmdTestFunctions.RightToLeft = RightToLeft.No;
            _cmdTestFunctions.Size = new Size(107, 50);
            _cmdTestFunctions.TabIndex = 6;
            _cmdTestFunctions.Text = "Test Functions";
            _cmdTestFunctions.UseVisualStyleBackColor = false;
            // 
            // cboStdDevMode
            // 
            _cboStdDevMode.BackColor = SystemColors.Window;
            _cboStdDevMode.Cursor = Cursors.Default;
            _cboStdDevMode.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboStdDevMode.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cboStdDevMode.ForeColor = SystemColors.WindowText;
            _cboStdDevMode.Location = new Point(125, 318);
            _cboStdDevMode.Name = "_cboStdDevMode";
            _cboStdDevMode.RightToLeft = RightToLeft.No;
            _cboStdDevMode.Size = new Size(174, 24);
            _cboStdDevMode.TabIndex = 3;
            // 
            // cboWeightMode
            // 
            _cboWeightMode.BackColor = SystemColors.Window;
            _cboWeightMode.Cursor = Cursors.Default;
            _cboWeightMode.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboWeightMode.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cboWeightMode.ForeColor = SystemColors.WindowText;
            _cboWeightMode.Location = new Point(125, 288);
            _cboWeightMode.Name = "_cboWeightMode";
            _cboWeightMode.RightToLeft = RightToLeft.No;
            _cboWeightMode.Size = new Size(174, 24);
            _cboWeightMode.TabIndex = 2;
            // 
            // cmdConvertToEmpirical
            // 
            _cmdConvertToEmpirical.BackColor = SystemColors.Control;
            _cmdConvertToEmpirical.Cursor = Cursors.Default;
            _cmdConvertToEmpirical.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cmdConvertToEmpirical.ForeColor = SystemColors.ControlText;
            _cmdConvertToEmpirical.Location = new Point(480, 10);
            _cmdConvertToEmpirical.Name = "_cmdConvertToEmpirical";
            _cmdConvertToEmpirical.RightToLeft = RightToLeft.No;
            _cmdConvertToEmpirical.Size = new Size(107, 50);
            _cmdConvertToEmpirical.TabIndex = 5;
            _cmdConvertToEmpirical.Text = "Convert to &Empirical";
            _cmdConvertToEmpirical.UseVisualStyleBackColor = false;
            // 
            // cmdFindMass
            // 
            _cmdFindMass.BackColor = SystemColors.Control;
            _cmdFindMass.Cursor = Cursors.Default;
            _cmdFindMass.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cmdFindMass.ForeColor = SystemColors.ControlText;
            _cmdFindMass.Location = new Point(10, 406);
            _cmdFindMass.Name = "_cmdFindMass";
            _cmdFindMass.RightToLeft = RightToLeft.No;
            _cmdFindMass.Size = new Size(106, 41);
            _cmdFindMass.TabIndex = 8;
            _cmdFindMass.Text = "&Calculate";
            _cmdFindMass.UseVisualStyleBackColor = false;
            // 
            // txtFormula
            // 
            txtFormula.AcceptsReturn = true;
            txtFormula.BackColor = SystemColors.Window;
            txtFormula.Cursor = Cursors.IBeam;
            txtFormula.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            txtFormula.ForeColor = SystemColors.WindowText;
            txtFormula.Location = new Point(125, 20);
            txtFormula.MaxLength = 0;
            txtFormula.Name = "txtFormula";
            txtFormula.RightToLeft = RightToLeft.No;
            txtFormula.Size = new Size(155, 23);
            txtFormula.TabIndex = 0;
            txtFormula.Text = "Cl2PhH4OH";
            // 
            // cmdClose
            // 
            _cmdClose.BackColor = SystemColors.Control;
            _cmdClose.Cursor = Cursors.Default;
            _cmdClose.DialogResult = DialogResult.Cancel;
            _cmdClose.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cmdClose.ForeColor = SystemColors.ControlText;
            _cmdClose.Location = new Point(125, 406);
            _cmdClose.Name = "_cmdClose";
            _cmdClose.RightToLeft = RightToLeft.No;
            _cmdClose.Size = new Size(107, 41);
            _cmdClose.TabIndex = 9;
            _cmdClose.Text = "Cl&ose";
            _cmdClose.UseVisualStyleBackColor = false;
            // 
            // lblStdDevMode
            // 
            lblStdDevMode.BackColor = SystemColors.Control;
            lblStdDevMode.Cursor = Cursors.Default;
            lblStdDevMode.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblStdDevMode.ForeColor = SystemColors.ControlText;
            lblStdDevMode.Location = new Point(10, 318);
            lblStdDevMode.Name = "lblStdDevMode";
            lblStdDevMode.RightToLeft = RightToLeft.No;
            lblStdDevMode.Size = new Size(87, 39);
            lblStdDevMode.TabIndex = 18;
            lblStdDevMode.Text = "Std Dev Mode";
            // 
            // lblWeightMode
            // 
            lblWeightMode.BackColor = SystemColors.Control;
            lblWeightMode.Cursor = Cursors.Default;
            lblWeightMode.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblWeightMode.ForeColor = SystemColors.ControlText;
            lblWeightMode.Location = new Point(10, 288);
            lblWeightMode.Name = "lblWeightMode";
            lblWeightMode.RightToLeft = RightToLeft.No;
            lblWeightMode.Size = new Size(87, 21);
            lblWeightMode.TabIndex = 17;
            lblWeightMode.Text = "Weight Mode";
            // 
            // lblStatusLabel
            // 
            lblStatusLabel.BackColor = SystemColors.Control;
            lblStatusLabel.Cursor = Cursors.Default;
            lblStatusLabel.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblStatusLabel.ForeColor = SystemColors.ControlText;
            lblStatusLabel.Location = new Point(10, 167);
            lblStatusLabel.Name = "lblStatusLabel";
            lblStatusLabel.RightToLeft = RightToLeft.No;
            lblStatusLabel.Size = new Size(87, 21);
            lblStatusLabel.TabIndex = 16;
            lblStatusLabel.Text = "Status:";
            // 
            // lblMassAndStdDevLabel
            // 
            lblMassAndStdDevLabel.BackColor = SystemColors.Control;
            lblMassAndStdDevLabel.Cursor = Cursors.Default;
            lblMassAndStdDevLabel.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblMassAndStdDevLabel.ForeColor = SystemColors.ControlText;
            lblMassAndStdDevLabel.Location = new Point(10, 79);
            lblMassAndStdDevLabel.Name = "lblMassAndStdDevLabel";
            lblMassAndStdDevLabel.RightToLeft = RightToLeft.No;
            lblMassAndStdDevLabel.Size = new Size(106, 49);
            lblMassAndStdDevLabel.TabIndex = 15;
            lblMassAndStdDevLabel.Text = "Mass and StdDev:";
            // 
            // lblMassAndStdDev
            // 
            lblMassAndStdDev.BackColor = SystemColors.Control;
            lblMassAndStdDev.Cursor = Cursors.Default;
            lblMassAndStdDev.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblMassAndStdDev.ForeColor = SystemColors.ControlText;
            lblMassAndStdDev.Location = new Point(125, 79);
            lblMassAndStdDev.Name = "lblMassAndStdDev";
            lblMassAndStdDev.RightToLeft = RightToLeft.No;
            lblMassAndStdDev.Size = new Size(251, 21);
            lblMassAndStdDev.TabIndex = 14;
            lblMassAndStdDev.Text = "0";
            // 
            // lblStatus
            // 
            lblStatus.BackColor = SystemColors.Control;
            lblStatus.Cursor = Cursors.Default;
            lblStatus.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblStatus.ForeColor = SystemColors.ControlText;
            lblStatus.Location = new Point(125, 171);
            lblStatus.Name = "lblStatus";
            lblStatus.RightToLeft = RightToLeft.No;
            lblStatus.Size = new Size(270, 41);
            lblStatus.TabIndex = 13;
            // 
            // lblMass
            // 
            lblMass.BackColor = SystemColors.Control;
            lblMass.Cursor = Cursors.Default;
            lblMass.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblMass.ForeColor = SystemColors.ControlText;
            lblMass.Location = new Point(125, 49);
            lblMass.Name = "lblMass";
            lblMass.RightToLeft = RightToLeft.No;
            lblMass.Size = new Size(155, 21);
            lblMass.TabIndex = 12;
            lblMass.Text = "0";
            // 
            // lblMassLabel
            // 
            lblMassLabel.BackColor = SystemColors.Control;
            lblMassLabel.Cursor = Cursors.Default;
            lblMassLabel.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblMassLabel.ForeColor = SystemColors.ControlText;
            lblMassLabel.Location = new Point(10, 49);
            lblMassLabel.Name = "lblMassLabel";
            lblMassLabel.RightToLeft = RightToLeft.No;
            lblMassLabel.Size = new Size(87, 21);
            lblMassLabel.TabIndex = 11;
            lblMassLabel.Text = "Mass:";
            // 
            // lblFormula
            // 
            lblFormula.BackColor = SystemColors.Control;
            lblFormula.Cursor = Cursors.Default;
            lblFormula.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblFormula.ForeColor = SystemColors.ControlText;
            lblFormula.Location = new Point(10, 20);
            lblFormula.Name = "lblFormula";
            lblFormula.RightToLeft = RightToLeft.No;
            lblFormula.Size = new Size(106, 21);
            lblFormula.TabIndex = 10;
            lblFormula.Text = "Formula:";
            // 
            // dgDataGrid
            // 
            dgDataGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            dgDataGrid.DataMember = "";
            dgDataGrid.HeaderForeColor = SystemColors.ControlText;
            dgDataGrid.Location = new Point(432, 89);
            dgDataGrid.Name = "dgDataGrid";
            dgDataGrid.Size = new Size(540, 396);
            dgDataGrid.TabIndex = 20;
            // 
            // rtfFormula
            // 
            _rtfFormula.Location = new Point(125, 108);
            _rtfFormula.Name = "_rtfFormula";
            _rtfFormula.ReadOnly = true;
            _rtfFormula.Size = new Size(278, 59);
            _rtfFormula.TabIndex = 21;
            _rtfFormula.Text = "";
            // 
            // chkShowRTFSource
            // 
            _chkShowRTFSource.Location = new Point(10, 465);
            _chkShowRTFSource.Name = "_chkShowRTFSource";
            _chkShowRTFSource.Size = new Size(105, 30);
            _chkShowRTFSource.TabIndex = 22;
            _chkShowRTFSource.Text = "Show RTF Source";
            // 
            // txtRTFSource
            // 
            txtRTFSource.AcceptsReturn = true;
            txtRTFSource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtRTFSource.BackColor = SystemColors.Window;
            txtRTFSource.Cursor = Cursors.IBeam;
            txtRTFSource.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            txtRTFSource.ForeColor = SystemColors.WindowText;
            txtRTFSource.Location = new Point(134, 455);
            txtRTFSource.MaxLength = 0;
            txtRTFSource.Multiline = true;
            txtRTFSource.Name = "txtRTFSource";
            txtRTFSource.RightToLeft = RightToLeft.No;
            txtRTFSource.ScrollBars = ScrollBars.Both;
            txtRTFSource.Size = new Size(288, 52);
            txtRTFSource.TabIndex = 23;
            txtRTFSource.Visible = false;
            // 
            // lblDLLVersion
            // 
            lblDLLVersion.BackColor = SystemColors.Control;
            lblDLLVersion.Cursor = Cursors.Default;
            lblDLLVersion.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblDLLVersion.ForeColor = SystemColors.ControlText;
            lblDLLVersion.Location = new Point(10, 367);
            lblDLLVersion.Name = "lblDLLVersion";
            lblDLLVersion.RightToLeft = RightToLeft.No;
            lblDLLVersion.Size = new Size(403, 19);
            lblDLLVersion.TabIndex = 24;
            lblDLLVersion.Text = "DLL Version";
            // 
            // lblProgress
            // 
            lblProgress.BackColor = SystemColors.Control;
            lblProgress.Cursor = Cursors.Default;
            lblProgress.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            lblProgress.ForeColor = SystemColors.ControlText;
            lblProgress.Location = new Point(125, 224);
            lblProgress.Name = "lblProgress";
            lblProgress.RightToLeft = RightToLeft.No;
            lblProgress.Size = new Size(270, 48);
            lblProgress.TabIndex = 25;
            // 
            // cmdTestFormulaFinder
            // 
            _cmdTestFormulaFinder.BackColor = SystemColors.Control;
            _cmdTestFormulaFinder.Cursor = Cursors.Default;
            _cmdTestFormulaFinder.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            _cmdTestFormulaFinder.ForeColor = SystemColors.ControlText;
            _cmdTestFormulaFinder.Location = new Point(743, 10);
            _cmdTestFormulaFinder.Name = "_cmdTestFormulaFinder";
            _cmdTestFormulaFinder.RightToLeft = RightToLeft.No;
            _cmdTestFormulaFinder.Size = new Size(161, 33);
            _cmdTestFormulaFinder.TabIndex = 26;
            _cmdTestFormulaFinder.Text = "Test Formula Finder";
            _cmdTestFormulaFinder.UseVisualStyleBackColor = false;
            // 
            // cboFormulaFinderTestMode
            // 
            cboFormulaFinderTestMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboFormulaFinderTestMode.BackColor = SystemColors.Window;
            cboFormulaFinderTestMode.Cursor = Cursors.Default;
            cboFormulaFinderTestMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFormulaFinderTestMode.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cboFormulaFinderTestMode.ForeColor = SystemColors.WindowText;
            cboFormulaFinderTestMode.Location = new Point(743, 46);
            cboFormulaFinderTestMode.Name = "cboFormulaFinderTestMode";
            cboFormulaFinderTestMode.RightToLeft = RightToLeft.No;
            cboFormulaFinderTestMode.Size = new Size(239, 24);
            cboFormulaFinderTestMode.TabIndex = 27;
            // 
            // frmMwtWinDllTest
            // 
            AcceptButton = _cmdFindMass;
            AutoScaleBaseSize = new Size(6, 16);
            BackColor = SystemColors.Control;
            CancelButton = _cmdClose;
            ClientSize = new Size(995, 505);
            Controls.Add(cboFormulaFinderTestMode);
            Controls.Add(_cmdTestFormulaFinder);
            Controls.Add(lblProgress);
            Controls.Add(lblDLLVersion);
            Controls.Add(txtRTFSource);
            Controls.Add(txtFormula);
            Controls.Add(_chkShowRTFSource);
            Controls.Add(_rtfFormula);
            Controls.Add(dgDataGrid);
            Controls.Add(_cmdTestGetTrypticName);
            Controls.Add(_cmdExpandAbbreviations);
            Controls.Add(_cmdTestFunctions);
            Controls.Add(_cboStdDevMode);
            Controls.Add(_cboWeightMode);
            Controls.Add(_cmdConvertToEmpirical);
            Controls.Add(_cmdFindMass);
            Controls.Add(_cmdClose);
            Controls.Add(lblStdDevMode);
            Controls.Add(lblWeightMode);
            Controls.Add(lblStatusLabel);
            Controls.Add(lblMassAndStdDevLabel);
            Controls.Add(lblMassAndStdDev);
            Controls.Add(lblStatus);
            Controls.Add(lblMass);
            Controls.Add(lblMassLabel);
            Controls.Add(lblFormula);
            Cursor = Cursors.Default;
            Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            Location = new Point(4, 25);
            Name = "frmMwtWinDllTest";
            RightToLeft = RightToLeft.No;
            Text = "Mwt Win Dll Test";
            ((System.ComponentModel.ISupportInitialize)dgDataGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        private MolecularWeightTool _mMwtWin;

        private MolecularWeightTool mMwtWin
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mMwtWin;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mMwtWin != null)
                {
                    _mMwtWin.ProgressChanged -= mMwtWin_ProgressChanged;
                    _mMwtWin.ProgressComplete -= mMwtWin_ProgressComplete;
                    _mMwtWin.ProgressReset -= mMwtWin_ProgressReset;
                }

                _mMwtWin = value;
                if (_mMwtWin != null)
                {
                    _mMwtWin.ProgressChanged += mMwtWin_ProgressChanged;
                    _mMwtWin.ProgressComplete += mMwtWin_ProgressComplete;
                    _mMwtWin.ProgressReset += mMwtWin_ProgressReset;
                }
            }
        }

        private DataView mMDIDListDataView;
        private DataSet myDataSet;

        private void AppendColumnToTableStyle(ref DataGridTableStyle tsTableStyle, string strMappingName, string strHeaderText, int intWidth = 75, bool blnIsReadOnly = false, bool blnIsDateTime = false, int intDecimalPlaces = -1)
        {
            // If intDecimalPlaces is >=0, then a format string is constructed to show the specified number of decimal places
            var TextCol = new DataGridTextBoxColumn();
            int i;
            TextCol.MappingName = strMappingName;
            TextCol.HeaderText = strHeaderText;
            TextCol.Width = intWidth;
            TextCol.ReadOnly = blnIsReadOnly;
            if (blnIsDateTime)
            {
                TextCol.Format = "g";
            }
            else if (intDecimalPlaces >= 0)
            {
                TextCol.Format = "0.";
                var loopTo = intDecimalPlaces - 1;
                for (i = 0; i <= loopTo; i++)
                    TextCol.Format += "0";
            }

            tsTableStyle.GridColumnStyles.Add(TextCol);
        }

        private void AppendBoolColumnToTableStyle(ref DataGridTableStyle tsTableStyle, string strMappingName, string strHeaderText, int intWidth = 75, bool blnIsReadOnly = false, bool blnSourceIsTrueFalse = true)
        {
            // If intDecimalPlaces is >=0, then a format string is constructed to show the specified number of decimal places
            var BoolCol = new DataGridBoolColumn();
            BoolCol.MappingName = strMappingName;
            BoolCol.HeaderText = strHeaderText;
            BoolCol.Width = intWidth;
            BoolCol.ReadOnly = blnIsReadOnly;
            if (blnSourceIsTrueFalse)
            {
                BoolCol.FalseValue = false;
                BoolCol.TrueValue = true;
            }
            else
            {
                BoolCol.FalseValue = 0;
                BoolCol.TrueValue = 1;
            }

            BoolCol.AllowNull = false;
            BoolCol.NullValue = Convert.DBNull;
            tsTableStyle.GridColumnStyles.Add(BoolCol);
        }

        private void FindPercentComposition()
        {
            mMwtWin.Compound.Formula = txtFormula.Text;
            double dblPctCompForCarbon = mMwtWin.Compound.GetPercentCompositionForElement(6);
            string strPctCompForCarbon = mMwtWin.Compound.GetPercentCompositionForElementAsString(6);
            var percentCompositionByElement = mMwtWin.Compound.GetPercentCompositionForAllElements();
            MakePercentCompositionDataSet(percentCompositionByElement);
            dgDataGrid.SetDataBinding(myDataSet, "DataTable1");
        }

        private void FindMass()
        {
            lblProgress.Text = string.Empty;

            // Can simply compute the mass of a formula using ComputeMass
            lblMass.Text = mMwtWin.ComputeMass(txtFormula.Text).ToString();

            // If we want to do more complex operations, need to fill mMwtWin.Compound with valid info
            // Then, can read out values from it
            {
                var withBlock = mMwtWin.Compound;
                withBlock.Formula = txtFormula.Text;
                if (string.IsNullOrEmpty(withBlock.ErrorDescription))
                {
                    lblMass.Text = withBlock.Mass.ToString();
                    lblStatus.Text = withBlock.CautionDescription;
                    txtFormula.Text = withBlock.FormulaCapitalized;
                    rtfFormula.Rtf = withBlock.FormulaRTF;
                    lblMassAndStdDev.Text = withBlock.MassAndStdDevString;
                }
                else
                {
                    lblStatus.Text = withBlock.ErrorDescription;
                }
            }
        }

        private void InitializeControls()
        {
            mMwtWin = new MolecularWeightTool() { ShowErrorDialogs = true };
            lblDLLVersion.Text = "DLL Info: " + mMwtWin.AppDate + ", Version " + mMwtWin.AppVersion;
            PopulateComboBoxes();
        }

        private void MakeDataSet(int lngIonCount, Peptide.udtFragmentationSpectrumDataType[] udtFragSpectrum)
        {
            // Create a DataSet.
            myDataSet = new DataSet("myDataSet");

            // Create a DataTable.
            var tDataTable = new DataTable("DataTable1");

            // Create three columns, and add them to the table.
            var cCMass = new DataColumn("Mass", typeof(double));
            var cIntensity = new DataColumn("Intensity", typeof(double));
            var cSymbol = new DataColumn("Symbol", typeof(string));
            tDataTable.Columns.Add(cCMass);
            tDataTable.Columns.Add(cIntensity);
            tDataTable.Columns.Add(cSymbol);

            // Add the table to the DataSet.
            myDataSet.Tables.Add(tDataTable);

            // Populates the table.
            DataRow newRow;

            // Append rows to the table.
            int lngIndex;
            var loopTo = lngIonCount - 1;
            for (lngIndex = 0; lngIndex <= loopTo; lngIndex++)
            {
                newRow = tDataTable.NewRow();
                newRow["Mass"] = udtFragSpectrum[lngIndex].Mass;
                newRow["Intensity"] = udtFragSpectrum[lngIndex].Intensity;
                newRow["Symbol"] = udtFragSpectrum[lngIndex].Symbol;
                tDataTable.Rows.Add(newRow);
            }
        }

        private void MakePercentCompositionDataSet(Dictionary<string, string> percentCompositionByElement)
        {

            // Create a DataSet.
            myDataSet = new DataSet("myDataSet");

            // Create a DataTable.
            var tDataTable = new DataTable("DataTable1");

            // Create three columns, and add them to the table.
            var cElement = new DataColumn("Element", typeof(string));
            var cPctComp = new DataColumn("Pct Comp", typeof(string));
            tDataTable.Columns.Add(cElement);
            tDataTable.Columns.Add(cPctComp);


            // Add the table to the DataSet.
            myDataSet.Tables.Add(tDataTable);

            // Populates the table

            // Append rows to the table.
            foreach (var item in percentCompositionByElement)
            {
                var newRow = tDataTable.NewRow();
                newRow["Element"] = item.Key;
                newRow["Pct Comp"] = item.Value;
                tDataTable.Rows.Add(newRow);
            }
        }

        private void PopulateComboBoxes()
        {
            {
                var withBlock = cboWeightMode;
                withBlock.Items.Clear();
                withBlock.Items.Add("Average mass");
                withBlock.Items.Add("Isotopic mass");
                withBlock.Items.Add("Integer mass");
                withBlock.SelectedIndex = 0;
            }

            {
                var withBlock1 = cboStdDevMode;
                withBlock1.Items.Clear();
                withBlock1.Items.Add("Short");
                withBlock1.Items.Add("Scientific");
                withBlock1.Items.Add("Decimal");
                withBlock1.SelectedIndex = 0;
            }

            {
                var withBlock2 = cboFormulaFinderTestMode;
                withBlock2.Items.Clear();
                withBlock2.Items.Add("Match 200 Da, +/- 0.05 Da");
                withBlock2.Items.Add("Match 200 Da, +/- 250 ppm");
                withBlock2.Items.Add("Match 200 Da, +/- 250 ppm, limit charge range");
                withBlock2.Items.Add("Match 100 m/z, +/- 250 ppm");
                withBlock2.Items.Add("Match percent composition values");
                withBlock2.Items.Add("Match 200 Da, +/- 250 ppm, Bounded search");
                withBlock2.SelectedIndex = 0;
            }
        }

        public void TestAccessFunctions()
        {
            int intResult;
            int lngIndex;
            int lngItemCount;
            string strSymbol = string.Empty;
            string strFormula = string.Empty;
            var sngCharge = default(float);
            var blnIsAminoAcid = default(bool);
            string strOneLetterSymbol = string.Empty;
            string strComment = string.Empty;
            string strStatement = string.Empty;
            var dblMass = default(double);
            var dblUncertainty = default(double);
            short intIsotopeCount = default, intIsotopeCount2 = default;
            double[] dblIsotopeMasses;
            float[] sngIsotopeAbundances;
            double dblNewPressure;
            var objResults = new frmTextbrowser();
            lblProgress.Text = string.Empty;
            objResults.Show();
            objResults.SetText = string.Empty;
            {
                var withBlock = mMwtWin;
                // Test Abbreviations
                lngItemCount = withBlock.GetAbbreviationCount();
                for (int intIndex = 1, loopTo = lngItemCount; intIndex <= loopTo; intIndex++)
                {
                    intResult = withBlock.GetAbbreviation(intIndex, ref strSymbol, ref strFormula, ref sngCharge, ref blnIsAminoAcid, ref strOneLetterSymbol, ref strComment);
                    Debug.Assert(intResult == 0, "");
                    Debug.Assert(withBlock.GetAbbreviationID(strSymbol) == intIndex, "");
                    intResult = withBlock.SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment);
                    Debug.Assert(intResult == 0, "");
                }

                // Test Caution statements
                lngItemCount = withBlock.GetCautionStatementCount();
                for (int intIndex = 1, loopTo1 = lngItemCount; intIndex <= loopTo1; intIndex++)
                {
                    intResult = withBlock.GetCautionStatement(intIndex, ref strSymbol, ref strStatement);
                    Debug.Assert(intResult == 0, "");
                    Debug.Assert(withBlock.GetCautionStatementID(strSymbol) == intIndex, "");
                    intResult = withBlock.SetCautionStatement(strSymbol, strStatement);
                    Debug.Assert(intResult == 0, "");
                }

                // Test Element access
                lngItemCount = withBlock.GetElementCount();
                for (int intIndex = 1, loopTo2 = lngItemCount; intIndex <= loopTo2; intIndex++)
                {
                    intResult = withBlock.GetElement((short)intIndex, ref strSymbol, ref dblMass, ref dblUncertainty, ref sngCharge, ref intIsotopeCount);
                    Debug.Assert(intResult == 0, "");
                    Debug.Assert(withBlock.GetElementID(strSymbol) == intIndex, "");
                    intResult = withBlock.SetElement(strSymbol, dblMass, dblUncertainty, sngCharge, false);
                    Debug.Assert(intResult == 0, "");
                    dblIsotopeMasses = new double[intIsotopeCount + 1 + 1];
                    sngIsotopeAbundances = new float[intIsotopeCount + 1 + 1];
                    intResult = withBlock.GetElementIsotopes((short)intIndex, ref intIsotopeCount2, ref dblIsotopeMasses, ref sngIsotopeAbundances);
                    Debug.Assert(intIsotopeCount == intIsotopeCount2, "");
                    Debug.Assert(intResult == 0, "");
                    intResult = withBlock.SetElementIsotopes(strSymbol, intIsotopeCount, ref dblIsotopeMasses, ref sngIsotopeAbundances);
                    Debug.Assert(intResult == 0, "");
                }

                // Test Message Statements access
                lngItemCount = withBlock.GetMessageStatementCount();
                var loopTo3 = lngItemCount;
                for (lngIndex = 1; lngIndex <= loopTo3; lngIndex++)
                {
                    strStatement = withBlock.GetMessageStatement(lngIndex);
                    intResult = withBlock.SetMessageStatement(lngIndex, strStatement);
                }

                // Test m/z conversion
                // Switch to isotopic masses

                withBlock.SetElementMode(ElementAndMassTools.emElementModeConstants.emIsotopicMass);
                withBlock.Compound.SetFormula("C19H36O5NH4");
                dblMass = withBlock.Compound.Mass;
                objResults.AppendText("Mass of " + withBlock.Compound.FormulaCapitalized + ": " + dblMass);
                for (short intCharge = 1; intCharge <= 4; intCharge++)
                    objResults.AppendText("  m/z of " + intCharge.ToString() + "+: " + withBlock.ConvoluteMass(dblMass, 0, intCharge));
                objResults.AppendText("");
                withBlock.Compound.SetFormula("C19H36O5NH3");
                dblMass = withBlock.Compound.Mass;
                objResults.AppendText("m/z values if we first lose a hydrogen before adding a proton");
                for (short intCharge = 1; intCharge <= 4; intCharge++)
                    objResults.AppendText("  m/z of " + intCharge.ToString() + "+: " + withBlock.ConvoluteMass(dblMass, 0, intCharge));


                // Test Capillary flow functions
                {
                    var withBlock1 = withBlock.CapFlow;
                    withBlock1.SetAutoComputeEnabled(false);
                    withBlock1.SetBackPressure(2000d, CapillaryFlow.uprUnitsPressureConstants.uprPsi);
                    withBlock1.SetColumnLength(40d, CapillaryFlow.ulnUnitsLengthConstants.ulnCM);
                    withBlock1.SetColumnID(50d, CapillaryFlow.ulnUnitsLengthConstants.ulnMicrons);
                    withBlock1.SetSolventViscosity(0.0089d, CapillaryFlow.uviUnitsViscosityConstants.uviPoise);
                    withBlock1.SetInterparticlePorosity(0.33d);
                    withBlock1.SetParticleDiameter(2d, CapillaryFlow.ulnUnitsLengthConstants.ulnMicrons);
                    withBlock1.SetAutoComputeEnabled(true);
                    objResults.AppendText("");
                    objResults.AppendText("Check capillary flow calcs");
                    objResults.AppendText("Linear Velocity: " + withBlock1.ComputeLinearVelocity(CapillaryFlow.ulvUnitsLinearVelocityConstants.ulvCmPerSec));
                    objResults.AppendText("Vol flow rate:   " + withBlock1.ComputeVolFlowRate(CapillaryFlow.ufrUnitsFlowRateConstants.ufrNLPerMin) + "  (newly computed)");
                    objResults.AppendText("Vol flow rate:   " + withBlock1.GetVolFlowRate());
                    objResults.AppendText("Back pressure:   " + withBlock1.ComputeBackPressure(CapillaryFlow.uprUnitsPressureConstants.uprPsi));
                    objResults.AppendText("Column Length:   " + withBlock1.ComputeColumnLength(CapillaryFlow.ulnUnitsLengthConstants.ulnCM));
                    objResults.AppendText("Column ID:       " + withBlock1.ComputeColumnID(CapillaryFlow.ulnUnitsLengthConstants.ulnMicrons));
                    objResults.AppendText("Column Volume:   " + withBlock1.ComputeColumnVolume(CapillaryFlow.uvoUnitsVolumeConstants.uvoNL));
                    objResults.AppendText("Dead time:       " + withBlock1.ComputeDeadTime(CapillaryFlow.utmUnitsTimeConstants.utmSeconds));
                    objResults.AppendText("");
                    objResults.AppendText("Repeat Computations, but in a different order (should give same results)");
                    objResults.AppendText("Vol flow rate:   " + withBlock1.ComputeVolFlowRate(CapillaryFlow.ufrUnitsFlowRateConstants.ufrNLPerMin));
                    objResults.AppendText("Column ID:       " + withBlock1.ComputeColumnID(CapillaryFlow.ulnUnitsLengthConstants.ulnMicrons));
                    objResults.AppendText("Back pressure:   " + withBlock1.ComputeBackPressure(CapillaryFlow.uprUnitsPressureConstants.uprPsi));
                    objResults.AppendText("Column Length:   " + withBlock1.ComputeColumnLength(CapillaryFlow.ulnUnitsLengthConstants.ulnCM));
                    objResults.AppendText("");
                    objResults.AppendText("Old Dead time: " + withBlock1.GetDeadTime(CapillaryFlow.utmUnitsTimeConstants.utmMinutes));
                    withBlock1.SetAutoComputeMode(CapillaryFlow.acmAutoComputeModeConstants.acmVolFlowRateUsingDeadTime);
                    withBlock1.SetDeadTime(25d, CapillaryFlow.utmUnitsTimeConstants.utmMinutes);
                    objResults.AppendText("Dead time is now 25.0 minutes");
                    objResults.AppendText("Vol flow rate: " + withBlock1.GetVolFlowRate(CapillaryFlow.ufrUnitsFlowRateConstants.ufrNLPerMin) + " (auto-computed since AutoComputeMode = acmVolFlowrateUsingDeadTime)");

                    // Confirm that auto-compute worked

                    objResults.AppendText("Vol flow rate: " + withBlock1.ComputeVolFlowRateUsingDeadTime(out dblNewPressure, CapillaryFlow.ufrUnitsFlowRateConstants.ufrNLPerMin, CapillaryFlow.uprUnitsPressureConstants.uprPsi) + "  (confirmation of computed volumetric flow rate)");
                    objResults.AppendText("New pressure: " + dblNewPressure);
                    objResults.AppendText("");

                    // Can set a new back pressure, but since auto-compute is on, and the
                    // auto-compute mode is acmVolFlowRateUsingDeadTime, the pressure will get changed back to
                    // the pressure needed to give a vol flow rate matching the dead time
                    withBlock1.SetBackPressure(2000d);
                    objResults.AppendText("Pressure set to 2000 psi, but auto-compute mode is acmVolFlowRateUsingDeadTime, so pressure");
                    objResults.AppendText("  was automatically changed back to pressure needed to give vol flow rate matching dead time");
                    objResults.AppendText("Pressure is now: " + withBlock1.GetBackPressure(CapillaryFlow.uprUnitsPressureConstants.uprPsi) + " psi (thus, not 2000 as one might expect)");
                    withBlock1.SetAutoComputeMode(CapillaryFlow.acmAutoComputeModeConstants.acmVolFlowRate);
                    objResults.AppendText("Changed auto-compute mode to acmVolFlowrate.  Can now set pressure to 2000 and it will stick; plus, vol flow rate gets computed.");
                    withBlock1.SetBackPressure(2000d, CapillaryFlow.uprUnitsPressureConstants.uprPsi);

                    // Calling GetVolFlowRate will get the new computed vol flow rate (since auto-compute is on)
                    objResults.AppendText("Vol flow rate: " + withBlock1.GetVolFlowRate());
                    withBlock1.SetMassRateSampleMass(1000d);
                    withBlock1.SetMassRateConcentration(1d, CapillaryFlow.ucoUnitsConcentrationConstants.ucoMicroMolar);
                    withBlock1.SetMassRateVolFlowRate(600d, CapillaryFlow.ufrUnitsFlowRateConstants.ufrNLPerMin);
                    withBlock1.SetMassRateInjectionTime(5d, CapillaryFlow.utmUnitsTimeConstants.utmMinutes);
                    objResults.AppendText("Mass flow rate: " + withBlock1.GetMassFlowRate(CapillaryFlow.umfMassFlowRateConstants.umfFmolPerSec) + " fmol/sec");
                    objResults.AppendText("Moles injected: " + withBlock1.GetMassRateMolesInjected(CapillaryFlow.umaMolarAmountConstants.umaFemtoMoles) + " fmoles");
                    withBlock1.SetMassRateSampleMass(1234d);
                    withBlock1.SetMassRateConcentration(1d, CapillaryFlow.ucoUnitsConcentrationConstants.ucoNgPerML);
                    objResults.AppendText("Computing mass flow rate for compound weighing 1234 g/mol and at 1 ng/mL concentration");
                    objResults.AppendText("Mass flow rate: " + withBlock1.GetMassFlowRate(CapillaryFlow.umfMassFlowRateConstants.umfAmolPerMin) + " amol/min");
                    objResults.AppendText("Moles injected: " + withBlock1.GetMassRateMolesInjected(CapillaryFlow.umaMolarAmountConstants.umaFemtoMoles) + " fmoles");
                    withBlock1.SetExtraColumnBroadeningLinearVelocity(4d, CapillaryFlow.ulvUnitsLinearVelocityConstants.ulvCmPerMin);
                    withBlock1.SetExtraColumnBroadeningDiffusionCoefficient(0.0003d, CapillaryFlow.udcDiffusionCoefficientConstants.udcCmSquaredPerMin);
                    withBlock1.SetExtraColumnBroadeningOpenTubeLength(5d, CapillaryFlow.ulnUnitsLengthConstants.ulnCM);
                    withBlock1.SetExtraColumnBroadeningOpenTubeID(250d, CapillaryFlow.ulnUnitsLengthConstants.ulnMicrons);
                    withBlock1.SetExtraColumnBroadeningInitialPeakWidthAtBase(30d, CapillaryFlow.utmUnitsTimeConstants.utmSeconds);
                    objResults.AppendText("Computing broadening for 30 second wide peak through a 250 um open tube that is 5 cm long (4 cm/min)");
                    objResults.AppendText(withBlock1.GetExtraColumnBroadeningResultantPeakWidth(CapillaryFlow.utmUnitsTimeConstants.utmSeconds).ToString());
                }
            }

            var udtFragSpectrumOptions = new Peptide.udtFragmentationSpectrumOptionsType();
            udtFragSpectrumOptions.Initialize();
            Peptide.udtFragmentationSpectrumDataType[] udtFragSpectrum = null;
            int lngIonCount;
            string strNewSeq;
            {
                var withBlock2 = mMwtWin.Peptide;
                withBlock2.SetSequence1LetterSymbol("K.AC!YEFGHRKACY*EFGHRK.G");
                // .SetSequence1LetterSymbol("K.ACYEFGHRKACYEFGHRK.G")

                // Can change the terminii to various standard groups
                withBlock2.SetNTerminusGroup(Peptide.ntgNTerminusGroupConstants.ntgCarbamyl);
                withBlock2.SetCTerminusGroup(Peptide.ctgCTerminusGroupConstants.ctgAmide);

                // Can change the terminii to any desired elements
                withBlock2.SetNTerminus("C2OH3"); // Acetyl group
                withBlock2.SetCTerminus("NH2"); // Amide group

                // Can mark third residue, Tyr, as phorphorylated
                withBlock2.SetResidue(3, "Tyr", true, true);

                // Can define that the * modification equals 15
                withBlock2.SetModificationSymbol("*", 15d, false, "");
                strNewSeq = "Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg*-Lys-Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg-Lys";
                objResults.AppendText(strNewSeq);
                withBlock2.SetSequence(strNewSeq);
                withBlock2.SetSequence("K.TQPLE*VK.-", Peptide.ntgNTerminusGroupConstants.ntgHydrogenPlusProton, Peptide.ctgCTerminusGroupConstants.ctgHydroxyl, blnIs3LetterCode: false);
                objResults.AppendText(withBlock2.GetSequence(true, false, true, false));
                objResults.AppendText(withBlock2.GetSequence(false, true, false, false));
                objResults.AppendText(withBlock2.GetSequence(true, false, true, true));
                withBlock2.SetCTerminusGroup(Peptide.ctgCTerminusGroupConstants.ctgNone);
                objResults.AppendText(withBlock2.GetSequence(true, false, true, true));
                udtFragSpectrumOptions = withBlock2.GetFragmentationSpectrumOptions();
                udtFragSpectrumOptions.DoubleChargeIonsShow = true;
                udtFragSpectrumOptions.DoubleChargeIonsThreshold = 300f;
                udtFragSpectrumOptions.IntensityOptions.BYIonShoulder = 0d;
                udtFragSpectrumOptions.TripleChargeIonsShow = true;
                udtFragSpectrumOptions.TripleChargeIonsThreshold = 400f;
                udtFragSpectrumOptions.IonTypeOptions[(int)Peptide.itIonTypeConstants.itAIon].ShowIon = true;
                withBlock2.SetFragmentationSpectrumOptions(udtFragSpectrumOptions);
                lngIonCount = withBlock2.GetFragmentationMasses(ref udtFragSpectrum);
            }

            MakeDataSet(lngIonCount, udtFragSpectrum);
            dgDataGrid.SetDataBinding(myDataSet, "DataTable1");
            objResults.AppendText(string.Empty);
            short intSuccess;
            string strResults = string.Empty;
            double[,] ConvolutedMSData2DOneBased;
            var ConvolutedMSDataCount = default(int);
            {
                var withBlock3 = mMwtWin;
                // Really big formula to test with: C489 H300 F27 Fe8 N72 Ni6 O27 S9
                short intChargeState = 1;
                bool blnAddProtonChargeCarrier = true;
                objResults.AppendText("Isotopic abundance test with Charge=" + intChargeState);
                ConvolutedMSData2DOneBased = new double[1, 2];
                string argstrFormulaIn = "C1255H43O2Cl";
                intSuccess = withBlock3.ComputeIsotopicAbundances(ref argstrFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount);
                objResults.AppendText(strResults);
                objResults.AppendText("Convert isotopic distribution to gaussian");
                var lstXYVals = new List<KeyValuePair<double, double>>();
                for (int intIndex = 1, loopTo4 = ConvolutedMSDataCount; intIndex <= loopTo4; intIndex++)
                    lstXYVals.Add(new KeyValuePair<double, double>(ConvolutedMSData2DOneBased[intIndex, 0], ConvolutedMSData2DOneBased[intIndex, 1]));
                int intResolution = 2000;
                double dblResolutionMass = 1000d;
                int intQualityFactor = 50;
                var lstGaussianData = withBlock3.ConvertStickDataToGaussian2DArray(lstXYVals, intResolution, dblResolutionMass, intQualityFactor);
                var sbResults = new StringBuilder();
                sbResults.AppendLine("m/z" + ControlChars.Tab + "Intensity");
                for (int intIndex = 0, loopTo5 = lstGaussianData.Count - 1; intIndex <= loopTo5; intIndex++)
                {
                    if (lstGaussianData[intIndex].Key >= 15175d && lstGaussianData[intIndex].Key < 15193d)
                    {
                        sbResults.AppendLine(lstGaussianData[intIndex].Key.ToString("0.000") + ControlChars.Tab + lstGaussianData[intIndex].Value.ToString("0.000"));
                    }
                }

                objResults.AppendText(sbResults.ToString());
                blnAddProtonChargeCarrier = false;
                objResults.AppendText("Isotopic abundance test with Charge=" + intChargeState + "; do not add a proton charge carrier");
                string argstrFormulaIn1 = "C1255H43O2Cl";
                intSuccess = withBlock3.ComputeIsotopicAbundances(ref argstrFormulaIn1, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount, blnAddProtonChargeCarrier);
                objResults.AppendText(strResults);
            }
        }

        public void TestFormulaFinder()
        {
            var oMwtWin = new MolecularWeightTool();
            oMwtWin.SetElementMode(ElementAndMassTools.emElementModeConstants.emIsotopicMass);
            oMwtWin.FormulaFinder.CandidateElements.Clear();
            oMwtWin.FormulaFinder.AddCandidateElement("C");
            oMwtWin.FormulaFinder.AddCandidateElement("H");
            oMwtWin.FormulaFinder.AddCandidateElement("N");
            oMwtWin.FormulaFinder.AddCandidateElement("O");

            // Abbreviations are supported, for example Serine
            oMwtWin.FormulaFinder.AddCandidateElement("Ser");
            var searchOptions = new FormulaFinderOptions()
            {
                LimitChargeRange = false,
                ChargeMin = 1,
                ChargeMax = 1,
                FindTargetMZ = false
            };
            cmdTestFormulaFinder.Enabled = false;
            Application.DoEvents();
            if (cboFormulaFinderTestMode.SelectedIndex == 0)
                FormulaFinderTest1(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 1)
                FormulaFinderTest2(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 2)
                FormulaFinderTest3(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 3)
                FormulaFinderTest4(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 4)
                FormulaFinderTest5(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 5)
                FormulaFinderTest6(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            cmdTestFormulaFinder.Enabled = true;
            if (cboFormulaFinderTestMode.SelectedIndex > 5)
            {
                MessageBox.Show("Formula finder test mode not recognized", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void FormulaFinderTest1(MolecularWeightTool oMwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {

            // Search for 200 Da, +/- 0.05 Da
            var lstResults = oMwtWin.FormulaFinder.FindMatchesByMass(200d, 0.05d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults);
        }

        private void FormulaFinderTest2(MolecularWeightTool oMwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {

            // Search for 200 Da, +/- 250 ppm
            var lstResults = oMwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, true);
        }

        private void FormulaFinderTest3(MolecularWeightTool oMwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.LimitChargeRange = true;
            searchOptions.ChargeMin = -4;
            searchOptions.ChargeMax = 6;

            // Search for 200 Da, +/- 250 ppm
            var lstResults = oMwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, true);
        }

        private void FormulaFinderTest4(MolecularWeightTool oMwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.LimitChargeRange = true;
            searchOptions.ChargeMin = -4;
            searchOptions.ChargeMax = 6;
            searchOptions.FindTargetMZ = true;

            // Search for 100 m/z, +/- 250 ppm
            var lstResults = oMwtWin.FormulaFinder.FindMatchesByMassPPM(100d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, true);
        }

        private void FormulaFinderTest5(MolecularWeightTool oMwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            oMwtWin.FormulaFinder.CandidateElements.Clear();
            oMwtWin.FormulaFinder.AddCandidateElement("C", 70d);
            oMwtWin.FormulaFinder.AddCandidateElement("H", 10d);
            oMwtWin.FormulaFinder.AddCandidateElement("N", 10d);
            oMwtWin.FormulaFinder.AddCandidateElement("O", 10d);

            // Search for percent composition results, maximum mass 400 Da
            var lstResults = oMwtWin.FormulaFinder.FindMatchesByPercentComposition(400d, 1d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, false, true);
        }

        private void FormulaFinderTest6(MolecularWeightTool oMwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.SearchMode = FormulaFinderOptions.eSearchMode.Bounded;

            // Search for 200 Da, +/- 250 ppm
            var lstResults = oMwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, true);
        }

        private void ShowFormulaFinderResults(string currentTask, FormulaFinderOptions searchOptions, List<FormulaFinderResult> lstResults, bool deltaMassIsPPM = false, bool percentCompositionSearch = false)
        {
            myDataSet = new DataSet("myDataSet");

            // Create a DataTable.
            var tDataTable = new DataTable("DataTable1");
            string massColumnName;
            if (deltaMassIsPPM)
            {
                massColumnName = "DeltaPPM";
            }
            else
            {
                massColumnName = "DeltaMass";
            }

            // Add columns to the table
            var cFormula = new DataColumn("Formula", typeof(string));
            var cMass = new DataColumn("Mass", typeof(double));
            var cDeltaMass = new DataColumn(massColumnName, typeof(double));
            var cCharge = new DataColumn("Charge", typeof(int));
            var cMZ = new DataColumn("M/Z", typeof(double));
            var cPercentComp = new DataColumn("PercentCompInfo", typeof(string));
            tDataTable.Columns.Add(cFormula);
            tDataTable.Columns.Add(cMass);
            tDataTable.Columns.Add(cDeltaMass);
            tDataTable.Columns.Add(cCharge);
            tDataTable.Columns.Add(cMZ);
            tDataTable.Columns.Add(cPercentComp);
            if (myDataSet.Tables.Count > 0)
            {
                myDataSet.Tables.Clear();
            }

            // Add the table to the DataSet.
            myDataSet.Tables.Add(tDataTable);

            // Populates the table.
            DataRow newRow;
            var sbPercentCompInfo = new StringBuilder();
            foreach (var result in lstResults)
            {
                newRow = tDataTable.NewRow();
                newRow["Formula"] = result.EmpiricalFormula;
                newRow["Mass"] = Math.Round(result.Mass, 4);
                if (deltaMassIsPPM)
                {
                    newRow[massColumnName] = result.DeltaMass.ToString("0.0");
                }
                else
                {
                    newRow[massColumnName] = result.DeltaMass.ToString("0.000");
                }

                newRow["Charge"] = result.ChargeState;
                if (searchOptions.FindCharge)
                {
                    newRow["M/Z"] = Math.Round(result.MZ, 3);
                }

                if (percentCompositionSearch)
                {
                    sbPercentCompInfo.Clear();
                    foreach (var percentCompValue in result.PercentComposition)
                        sbPercentCompInfo.Append(" " + percentCompValue.Key + "=" + percentCompValue.Value.ToString("0.00") + "%");
                    newRow["PercentCompInfo"] = sbPercentCompInfo.ToString().TrimStart();
                }
                else
                {
                    newRow["PercentCompInfo"] = string.Empty;
                }

                tDataTable.Rows.Add(newRow);
            }

            dgDataGrid.SetDataBinding(myDataSet, "DataTable1");
        }

        private void TestTrypticName()
        {
            const short DIM_CHUNK = 1000;
            const short ITERATIONS_TO_RUN = 5;
            const short MIN_PROTEIN_LENGTH = 50;
            const short MAX_PROTEIN_LENGTH = 200;
            const string POSSIBLE_RESIDUES = "ACDEFGHIKLMNPQRSTVWY";
            int lngMultipleIteration;
            string strProtein, strPeptideResidues;
            int lngResidueStart = default, lngResidueEnd = default;
            string[] strPeptideNameMwtWin;
            string strPeptideName;
            int lngMwtWinResultCount;
            int lngMwtWinDimCount;
            int lngIndex;
            float lngResidueRand, lngProteinLengthRand;
            string strNewResidue;
            int lngStartTime, lngStopTime;
            int lngMwtWinWorkTime;
            string strPeptideFragMwtWin;
            var lngMatchCount = default(int);
            var objResults = new frmTextbrowser();
            lblProgress.Text = string.Empty;
            lngMwtWinDimCount = DIM_CHUNK;
            strPeptideNameMwtWin = new string[lngMwtWinDimCount + 1];
            Cursor = Cursors.WaitCursor;
            objResults.Show();
            objResults.SetText = string.Empty;

            // '    Dim lngIcr2lsWorkTime As Long
            // '    Dim lngIcr2lsTime As Long
            // '    strPeptideFragIcr2ls As String
            // '    lngICR2lsDimCount = DIM_CHUNK
            // '    ReDim strPeptideNameIcr2ls(lngICR2lsDimCount)
            // '
            // '    Dim ICRTools As Object
            // '
            // '    Set ICRTools = CreateObject("ICR2LS.ICR2LScls")
            // '
            // '    objResults.AppendText("ICR2ls Version: " & ICRTools.ICR2LSversion)

            // strProtein = "MGNISFLTGGNPSSPQSIAESIYQLENTSVVFLSAWQRTTPDFQRAARASQEAMLHLDHIVNEIMRNRDQLQADGTYTGSQLEGLLNISRAVSVSPVTRAEQDDLANYGPGNGVLPSAGSSISMEKLLNKIKHRRTNSANFRIGASGEHIFIIGVDKPNRQPDSIVEFIVGDFCQHCSDIAALI"

            // Bigger protein
            strProtein = "MMKANVTKKTLNEGLGLLERVIPSRSSNPLLTALKVETSEGGLTLSGTNLEIDLSCFVPAEVQQPENFVVPAHLFAQIVRNLGGELVELELSGQELSVRSGGSDFKLQTGDIEAYPPLSFPAQADVSLDGGELSRAFSSVRYAASNEAFQAVFRGIKLEHHGESARVVASDGYRVAIRDFPASGDGKNLIIPARSVDELIRVLKDGEARFTYGDGMLTVTTDRVKMNLKLLDGDFPDYERVIPKDIKLQVTLPATALKEAVNRVAVLADKNANNRVEFLVSEGTLRLAAEGDYGRAQDTLSVTQGGTEQAMSLAFNARHVLDALGPIDGDAELLFSGSTSPAIFRARRWGRRVYGGHGHAARLRGLLRPLRGMSALAHHPESSPPLEPRPEFA";
            objResults.AppendText("Testing GetTrypticNameMultipleMatches() function");
            objResults.AppendText("MatchList for NL: " + mMwtWin.Peptide.GetTrypticNameMultipleMatches(strProtein, "NL", lngMatchCount));
            objResults.AppendText("MatchCount = " + lngMatchCount);
            objResults.AppendText(string.Empty);
            objResults.AppendText("Testing GetTrypticPeptideByFragmentNumber function");
            for (lngIndex = 1; lngIndex <= 43; lngIndex++)
            {
                strPeptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(strProtein, (short)lngIndex, ref lngResidueStart, ref lngResidueEnd);
                // '        strPeptideFragIcr2ls = ICRTools.TrypticPeptide(strProtein, CInt(lngIndex))
                // '
                // '        Debug.Assert strPeptideFragMwtWin = strPeptideFragIcr2ls

                if (Strings.Len(strPeptideFragMwtWin) > 1)
                {
                    // Make sure lngResidueStart and lngResidueEnd are correct
                    // Do this using .GetTrypticNameMultipleMatches()
                    strPeptideName = mMwtWin.Peptide.GetTrypticNameMultipleMatches(strProtein, Strings.Mid(strProtein, lngResidueStart, lngResidueEnd - lngResidueStart + 1));
                    Debug.Assert(Strings.InStr(strPeptideName, "t" + Strings.Trim(Conversion.Str(lngIndex))) > 0, "");
                }
            }

            objResults.AppendText("Check of GetTrypticPeptideByFragmentNumber Complete");
            objResults.AppendText(string.Empty);
            objResults.AppendText("Test tryptic digest of: " + strProtein);
            lngIndex = 1;
            do
            {
                strPeptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(strProtein, (short)lngIndex, ref lngResidueStart, ref lngResidueEnd);
                objResults.AppendText("Tryptic fragment " + Strings.Trim(lngIndex.ToString()) + ": " + strPeptideFragMwtWin);
                lngIndex += 1;
            }
            while (Strings.Len(strPeptideFragMwtWin) > 0);
            objResults.AppendText(string.Empty);
            VBMath.Randomize();
            for (lngMultipleIteration = 1; lngMultipleIteration <= ITERATIONS_TO_RUN; lngMultipleIteration++)
            {
                // Generate random protein
                lngProteinLengthRand = Conversion.Int((MAX_PROTEIN_LENGTH - MIN_PROTEIN_LENGTH + 1) * VBMath.Rnd() + MIN_PROTEIN_LENGTH);
                strProtein = "";
                var loopTo = lngProteinLengthRand;
                for (lngResidueRand = 1f; lngResidueRand <= loopTo; lngResidueRand++)
                {
                    strNewResidue = Strings.Mid(POSSIBLE_RESIDUES, (int)Math.Round(Conversion.Int(Strings.Len(POSSIBLE_RESIDUES)) * VBMath.Rnd() + 1f), 1);
                    strProtein += strNewResidue;
                }

                objResults.AppendText("Iteration: " + lngMultipleIteration + " = " + strProtein);
                lngMwtWinResultCount = 0;
                Debug.Write("Starting residue is ");
                lngStartTime = modMwtWinDllTest.GetTickCount();
                var loopTo1 = Strings.Len(strProtein);
                for (lngResidueStart = 1; lngResidueStart <= loopTo1; lngResidueStart++)
                {
                    if (lngResidueStart % 10 == 0)
                    {
                        Debug.Write(lngResidueStart + ", ");
                        Application.DoEvents();
                    }

                    var loopTo2 = Strings.Len(strProtein) - lngResidueStart;
                    for (lngResidueEnd = 1; lngResidueEnd <= loopTo2; lngResidueEnd++)
                    {
                        if (lngResidueEnd - lngResidueStart > 50)
                        {
                            break;
                        }

                        strPeptideResidues = Strings.Mid(strProtein, lngResidueStart, lngResidueEnd);
                        int arglngReturnResidueStart = 0;
                        int arglngReturnResidueEnd = 0;
                        strPeptideNameMwtWin[lngMwtWinResultCount] = mMwtWin.Peptide.GetTrypticName(strProtein, strPeptideResidues, ref arglngReturnResidueStart, ref arglngReturnResidueEnd, true);
                        lngMwtWinResultCount += 1;
                        if (lngMwtWinResultCount > lngMwtWinDimCount)
                        {
                            lngMwtWinDimCount += DIM_CHUNK;
                            Array.Resize(ref strPeptideNameMwtWin, lngMwtWinDimCount + 1);
                        }
                    }
                }

                lngStopTime = modMwtWinDllTest.GetTickCount();
                lngMwtWinWorkTime = lngStopTime - lngStartTime;
                Console.WriteLine("");
                Console.WriteLine("MwtWin time (" + lngMwtWinResultCount + " peptides) = " + lngMwtWinWorkTime + " msec");

                // '        lngIcr2lsResultCount = 0
                // '        Debug.Print "Starting residue is ";
                // '        lngStartTime = GetTickCount()
                // '        For lngResidueStart = 1 To Len(strProtein)
                // '            If lngResidueStart Mod 10 = 0 Then
                // '                Debug.Print lngResidueStart & ", ";
                // '                DoEvents
                // '            End If
                // '            ' Use DoEvents on every iteration since Icr2ls is quite slow
                // '            DoEvents
                // '
                // '            For lngResidueEnd = 1 To Len(strProtein) - lngResidueStart
                // '                If lngResidueEnd - lngResidueStart > 50 Then
                // '                    Exit For
                // '                End If
                // '
                // '                strPeptideResidues = Mid(strProtein, lngResidueStart, lngResidueEnd)
                // '                strPeptideNameIcr2ls(lngIcr2lsResultCount) = ICRTools.TrypticName(strProtein, strPeptideResidues)
                // '
                // '                lngIcr2lsResultCount = lngIcr2lsResultCount + 1
                // '                If lngIcr2lsResultCount > lngICR2lsDimCount Then
                // '                    lngICR2lsDimCount = lngICR2lsDimCount + DIM_CHUNK
                // '                    ReDim Preserve strPeptideNameIcr2ls(lngICR2lsDimCount)
                // '                End If
                // '            Next lngResidueEnd
                // '        Next lngResidueStart
                // '        lngStopTime = GetTickCount()
                // '        lngIcr2lsWorkTime = lngStopTime - lngStartTime
                // '        Debug.Print ""
                // '        Debug.Print "Icr2ls time (" & lngMwtWinResultCount & " peptides) = " & lngIcr2lsWorkTime & " msec"

                // '        ' Check that results match
                // '        For lngIndex = 0 To lngMwtWinResultCount - 1
                // '            If Left(strPeptideNameMwtWin(lngIndex), 1) = "t" Then
                // '                If Val(Right(strPeptideNameMwtWin(lngIndex), 1)) < 5 Then
                // '                    ' Icr2LS does not return the correct name when strPeptideResidues contains 5 or more tryptic peptides
                // '                    If strPeptideNameMwtWin(lngIndex) <> strPeptideNameIcr2ls(lngIndex) Then
                // '                        objResults.AppendText("Difference found, index = " & lngIndex & ", " & strPeptideNameMwtWin(lngIndex) & " vs. " & strPeptideNameIcr2ls(lngIndex))
                // '                        blnDifferenceFound = True
                // '                    End If
                // '                End If
                // '            Else
                // '                If strPeptideNameMwtWin(lngIndex) <> strPeptideNameIcr2ls(lngIndex) Then
                // '                    objResults.AppendText("Difference found, index = " & lngIndex & ", " & strPeptideNameMwtWin(lngIndex) & " vs. " & strPeptideNameIcr2ls(lngIndex))
                // '                    blnDifferenceFound = True
                // '                End If
                // '            End If
                // '        Next lngIndex

            }

            objResults.AppendText("Check of Tryptic Sequence functions Complete");
            Cursor = Cursors.Default;
        }

        private void UpdateResultsForCompound(ref Compound objCompound)
        {
            if (string.IsNullOrEmpty(objCompound.ErrorDescription))
            {
                txtFormula.Text = objCompound.FormulaCapitalized;
                FindMass();
            }
            else
            {
                lblStatus.Text = objCompound.ErrorDescription;
            }
        }

        private void cboStdDevMode_SelectedIndexChanged(object eventSender, EventArgs eventArgs)
        {
            switch (cboStdDevMode.SelectedIndex)
            {
                case 1:
                    {
                        mMwtWin.StdDevMode = ElementAndMassTools.smStdDevModeConstants.smScientific;
                        break;
                    }

                case 2:
                    {
                        mMwtWin.StdDevMode = ElementAndMassTools.smStdDevModeConstants.smDecimal;
                        break;
                    }

                default:
                    {
                        mMwtWin.StdDevMode = ElementAndMassTools.smStdDevModeConstants.smShort;
                        break;
                    }
            }
        }

        private void cboWeightMode_SelectedIndexChanged(object eventSender, EventArgs eventArgs)
        {
            switch (cboWeightMode.SelectedIndex)
            {
                case 1:
                    {
                        mMwtWin.SetElementMode(ElementAndMassTools.emElementModeConstants.emIsotopicMass);
                        break;
                    }

                case 2:
                    {
                        mMwtWin.SetElementMode(ElementAndMassTools.emElementModeConstants.emIntegerMass);
                        break;
                    }

                default:
                    {
                        mMwtWin.SetElementMode(ElementAndMassTools.emElementModeConstants.emAverageMass);
                        break;
                    }
            }
        }

        private void cmdClose_Click(object eventSender, EventArgs eventArgs)
        {
            mMwtWin = null;
            Close();
            Environment.Exit(0);
        }

        private void cmdConvertToEmpirical_Click(object eventSender, EventArgs eventArgs)
        {
            lblProgress.Text = string.Empty;
            {
                var withBlock = mMwtWin.Compound;
                withBlock.Formula = txtFormula.Text;
                withBlock.ConvertToEmpirical();
            }

            UpdateResultsForCompound(ref mMwtWin.Compound);
        }

        private void cmdExpandAbbreviations_Click(object eventSender, EventArgs eventArgs)
        {
            lblProgress.Text = string.Empty;
            {
                var withBlock = mMwtWin.Compound;
                withBlock.Formula = txtFormula.Text;
                withBlock.ExpandAbbreviations();
            }

            UpdateResultsForCompound(ref mMwtWin.Compound);
        }

        private void cmdFindMass_Click(object eventSender, EventArgs eventArgs)
        {
            FindMass();
            FindPercentComposition();
        }

        private void cmdTestFunctions_Click(object eventSender, EventArgs eventArgs)
        {
            TestAccessFunctions();
        }

        private void cmdTestGetTrypticName_Click(object eventSender, EventArgs eventArgs)
        {
            TestTrypticName();
        }

        private void rtfFormula_TextChanged(object sender, EventArgs e)
        {
            txtRTFSource.Text = rtfFormula.Rtf;
        }

        private void chkShowRTFSource_CheckedChanged(object sender, EventArgs e)
        {
            txtRTFSource.Visible = chkShowRTFSource.Checked;
        }

        private void mMwtWin_ProgressChanged(string taskDescription, float percentComplete)
        {
            ;
#error Cannot convert LocalDeclarationStatementSyntax - see comment for details
            /* Cannot convert LocalDeclarationStatementSyntax, System.NotSupportedException: StaticKeyword not supported!
               at ICSharpCode.CodeConverter.CSharp.SyntaxKindExtensions.ConvertToken(SyntaxKind t, TokenContext context)
               at ICSharpCode.CodeConverter.CSharp.CommonConversions.ConvertModifier(SyntaxToken m, TokenContext context)
               at ICSharpCode.CodeConverter.CSharp.CommonConversions.<ConvertModifiersCore>d__43.MoveNext()
               at System.Linq.Enumerable.<ConcatIterator>d__59`1.MoveNext()
               at System.Linq.Enumerable.WhereEnumerableIterator`1.MoveNext()
               at System.Linq.Buffer`1..ctor(IEnumerable`1 source)
               at System.Linq.OrderedEnumerable`1.<GetEnumerator>d__1.MoveNext()
               at Microsoft.CodeAnalysis.SyntaxTokenList.CreateNode(IEnumerable`1 tokens)
               at ICSharpCode.CodeConverter.CSharp.CommonConversions.ConvertModifiers(SyntaxNode node, IReadOnlyCollection`1 modifiers, TokenContext context, Boolean isVariableOrConst, SyntaxKind[] extraCsModifierKinds)
               at ICSharpCode.CodeConverter.CSharp.MethodBodyExecutableStatementVisitor.<VisitLocalDeclarationStatement>d__31.MoveNext()
            --- End of stack trace from previous location where exception was thrown ---
               at ICSharpCode.CodeConverter.CSharp.HoistedNodeStateVisitor.<AddLocalVariablesAsync>d__6.MoveNext()
            --- End of stack trace from previous location where exception was thrown ---
               at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.<DefaultVisitInnerAsync>d__3.MoveNext()

            Input:
                    Static dtLastUpdate As Global.System.DateTime

             */
            lblProgress.Text = mMwtWin.ProgressStepDescription + "; " + percentComplete.ToString("0.0") + "% complete";
            if (DateTime.UtcNow.Subtract(dtLastUpdate).TotalMilliseconds > 100d)
            {
                dtLastUpdate = DateTime.UtcNow;
                Application.DoEvents();
            }
        }

        private void mMwtWin_ProgressComplete()
        {
            lblProgress.Text = mMwtWin.ProgressStepDescription + "; 100% complete";
            Application.DoEvents();
        }

        private void mMwtWin_ProgressReset()
        {
            lblProgress.Text = mMwtWin.ProgressStepDescription;
            Application.DoEvents();
        }

        private void cmdTestFormulaFinder_Click(object sender, EventArgs e)
        {
            TestFormulaFinder();
        }
    }
}