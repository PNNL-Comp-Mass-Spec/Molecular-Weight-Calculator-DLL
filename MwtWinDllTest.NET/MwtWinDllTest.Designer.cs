using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace MwtWinDllTest
{
    partial class frmMwtWinDllTest
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
            ToolTip1 = new ToolTip(components);
            cmdTestGetTrypticName = new Button();
            cmdTestGetTrypticName.Click += new EventHandler(cmdTestGetTrypticName_Click);
            cmdExpandAbbreviations = new Button();
            cmdExpandAbbreviations.Click += new EventHandler(cmdExpandAbbreviations_Click);
            cmdTestFunctions = new Button();
            cmdTestFunctions.Click += new EventHandler(cmdTestFunctions_Click);
            cboStdDevMode = new ComboBox();
            cboStdDevMode.SelectedIndexChanged += new EventHandler(cboStdDevMode_SelectedIndexChanged);
            cboWeightMode = new ComboBox();
            cboWeightMode.SelectedIndexChanged += new EventHandler(cboWeightMode_SelectedIndexChanged);
            cmdConvertToEmpirical = new Button();
            cmdConvertToEmpirical.Click += new EventHandler(cmdConvertToEmpirical_Click);
            cmdFindMass = new Button();
            cmdFindMass.Click += new EventHandler(cmdFindMass_Click);
            txtFormula = new TextBox();
            cmdClose = new Button();
            cmdClose.Click += new EventHandler(cmdClose_Click);
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
            rtfFormula = new RichTextBox();
            rtfFormula.TextChanged += new EventHandler(rtfFormula_TextChanged);
            chkShowRTFSource = new CheckBox();
            chkShowRTFSource.CheckedChanged += new EventHandler(chkShowRTFSource_CheckedChanged);
            txtRTFSource = new TextBox();
            lblDLLVersion = new Label();
            lblProgress = new Label();
            cmdTestFormulaFinder = new Button();
            cmdTestFormulaFinder.Click += new EventHandler(cmdTestFormulaFinder_Click);
            cboFormulaFinderTestMode = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dgDataGrid).BeginInit();
            SuspendLayout();
            //
            // cmdTestGetTrypticName
            //
            cmdTestGetTrypticName.BackColor = SystemColors.Control;
            cmdTestGetTrypticName.Cursor = Cursors.Default;
            cmdTestGetTrypticName.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cmdTestGetTrypticName.ForeColor = SystemColors.ControlText;
            cmdTestGetTrypticName.Location = new Point(307, 396);
            cmdTestGetTrypticName.Name = "cmdTestGetTrypticName";
            cmdTestGetTrypticName.RightToLeft = RightToLeft.No;
            cmdTestGetTrypticName.Size = new Size(107, 51);
            cmdTestGetTrypticName.TabIndex = 19;
            cmdTestGetTrypticName.Text = "Test Get Tryptic Name";
            cmdTestGetTrypticName.UseVisualStyleBackColor = false;
            //
            // cmdExpandAbbreviations
            //
            cmdExpandAbbreviations.BackColor = SystemColors.Control;
            cmdExpandAbbreviations.Cursor = Cursors.Default;
            cmdExpandAbbreviations.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cmdExpandAbbreviations.ForeColor = SystemColors.ControlText;
            cmdExpandAbbreviations.Location = new Point(346, 10);
            cmdExpandAbbreviations.Name = "cmdExpandAbbreviations";
            cmdExpandAbbreviations.RightToLeft = RightToLeft.No;
            cmdExpandAbbreviations.Size = new Size(106, 50);
            cmdExpandAbbreviations.TabIndex = 4;
            cmdExpandAbbreviations.Text = "Expand Abbreviations";
            cmdExpandAbbreviations.UseVisualStyleBackColor = false;
            //
            // cmdTestFunctions
            //
            cmdTestFunctions.BackColor = SystemColors.Control;
            cmdTestFunctions.Cursor = Cursors.Default;
            cmdTestFunctions.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cmdTestFunctions.ForeColor = SystemColors.ControlText;
            cmdTestFunctions.Location = new Point(614, 10);
            cmdTestFunctions.Name = "cmdTestFunctions";
            cmdTestFunctions.RightToLeft = RightToLeft.No;
            cmdTestFunctions.Size = new Size(107, 50);
            cmdTestFunctions.TabIndex = 6;
            cmdTestFunctions.Text = "Test Functions";
            cmdTestFunctions.UseVisualStyleBackColor = false;
            //
            // cboStdDevMode
            //
            cboStdDevMode.BackColor = SystemColors.Window;
            cboStdDevMode.Cursor = Cursors.Default;
            cboStdDevMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStdDevMode.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cboStdDevMode.ForeColor = SystemColors.WindowText;
            cboStdDevMode.Location = new Point(125, 318);
            cboStdDevMode.Name = "cboStdDevMode";
            cboStdDevMode.RightToLeft = RightToLeft.No;
            cboStdDevMode.Size = new Size(174, 24);
            cboStdDevMode.TabIndex = 3;
            //
            // cboWeightMode
            //
            cboWeightMode.BackColor = SystemColors.Window;
            cboWeightMode.Cursor = Cursors.Default;
            cboWeightMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWeightMode.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cboWeightMode.ForeColor = SystemColors.WindowText;
            cboWeightMode.Location = new Point(125, 288);
            cboWeightMode.Name = "cboWeightMode";
            cboWeightMode.RightToLeft = RightToLeft.No;
            cboWeightMode.Size = new Size(174, 24);
            cboWeightMode.TabIndex = 2;
            //
            // cmdConvertToEmpirical
            //
            cmdConvertToEmpirical.BackColor = SystemColors.Control;
            cmdConvertToEmpirical.Cursor = Cursors.Default;
            cmdConvertToEmpirical.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cmdConvertToEmpirical.ForeColor = SystemColors.ControlText;
            cmdConvertToEmpirical.Location = new Point(480, 10);
            cmdConvertToEmpirical.Name = "cmdConvertToEmpirical";
            cmdConvertToEmpirical.RightToLeft = RightToLeft.No;
            cmdConvertToEmpirical.Size = new Size(107, 50);
            cmdConvertToEmpirical.TabIndex = 5;
            cmdConvertToEmpirical.Text = "Convert to &Empirical";
            cmdConvertToEmpirical.UseVisualStyleBackColor = false;
            //
            // cmdFindMass
            //
            cmdFindMass.BackColor = SystemColors.Control;
            cmdFindMass.Cursor = Cursors.Default;
            cmdFindMass.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cmdFindMass.ForeColor = SystemColors.ControlText;
            cmdFindMass.Location = new Point(10, 406);
            cmdFindMass.Name = "cmdFindMass";
            cmdFindMass.RightToLeft = RightToLeft.No;
            cmdFindMass.Size = new Size(106, 41);
            cmdFindMass.TabIndex = 8;
            cmdFindMass.Text = "&Calculate";
            cmdFindMass.UseVisualStyleBackColor = false;
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
            cmdClose.BackColor = SystemColors.Control;
            cmdClose.Cursor = Cursors.Default;
            cmdClose.DialogResult = DialogResult.Cancel;
            cmdClose.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cmdClose.ForeColor = SystemColors.ControlText;
            cmdClose.Location = new Point(125, 406);
            cmdClose.Name = "cmdClose";
            cmdClose.RightToLeft = RightToLeft.No;
            cmdClose.Size = new Size(107, 41);
            cmdClose.TabIndex = 9;
            cmdClose.Text = "Cl&ose";
            cmdClose.UseVisualStyleBackColor = false;
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
            rtfFormula.Location = new Point(125, 108);
            rtfFormula.Name = "rtfFormula";
            rtfFormula.ReadOnly = true;
            rtfFormula.Size = new Size(278, 59);
            rtfFormula.TabIndex = 21;
            rtfFormula.Text = "";
            //
            // chkShowRTFSource
            //
            chkShowRTFSource.Location = new Point(10, 465);
            chkShowRTFSource.Name = "chkShowRTFSource";
            chkShowRTFSource.Size = new Size(105, 30);
            chkShowRTFSource.TabIndex = 22;
            chkShowRTFSource.Text = "Show RTF Source";
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
            cmdTestFormulaFinder.BackColor = SystemColors.Control;
            cmdTestFormulaFinder.Cursor = Cursors.Default;
            cmdTestFormulaFinder.Font = new Font("Arial", 8.0f, FontStyle.Regular, GraphicsUnit.Point, Conversions.ToByte(0));
            cmdTestFormulaFinder.ForeColor = SystemColors.ControlText;
            cmdTestFormulaFinder.Location = new Point(743, 10);
            cmdTestFormulaFinder.Name = "cmdTestFormulaFinder";
            cmdTestFormulaFinder.RightToLeft = RightToLeft.No;
            cmdTestFormulaFinder.Size = new Size(161, 33);
            cmdTestFormulaFinder.TabIndex = 26;
            cmdTestFormulaFinder.Text = "Test Formula Finder";
            cmdTestFormulaFinder.UseVisualStyleBackColor = false;
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
            AcceptButton = cmdFindMass;
            AutoScaleBaseSize = new Size(6, 16);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            CancelButton = cmdClose;
            ClientSize = new Size(995, 505);
            Controls.Add(cboFormulaFinderTestMode);
            Controls.Add(cmdTestFormulaFinder);
            Controls.Add(lblProgress);
            Controls.Add(lblDLLVersion);
            Controls.Add(txtRTFSource);
            Controls.Add(txtFormula);
            Controls.Add(chkShowRTFSource);
            Controls.Add(rtfFormula);
            Controls.Add(dgDataGrid);
            Controls.Add(cmdTestGetTrypticName);
            Controls.Add(cmdExpandAbbreviations);
            Controls.Add(cmdTestFunctions);
            Controls.Add(cboStdDevMode);
            Controls.Add(cboWeightMode);
            Controls.Add(cmdConvertToEmpirical);
            Controls.Add(cmdFindMass);
            Controls.Add(cmdClose);
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

        #endregion

        private ToolTip ToolTip1;
        private Button cmdTestGetTrypticName;
        //private AxMSFlexGridLib.AxMSFlexGrid grdFlexGrid;
        private Button cmdExpandAbbreviations;
        private Button cmdTestFunctions;
        private ComboBox cboStdDevMode;
        private ComboBox cboWeightMode;
        private Button cmdConvertToEmpirical;
        private Button cmdFindMass;
        private TextBox txtFormula;
        private Button cmdClose;
        private Label lblStdDevMode;
        private Label lblWeightMode;
        private Label lblStatusLabel;
        private Label lblMassAndStdDevLabel;
        private Label lblMassAndStdDev;
        private Label lblStatus;
        private Label lblMass;
        private Label lblMassLabel;
        private Label lblFormula;
        private DataGrid dgDataGrid;
        private RichTextBox rtfFormula;
        private CheckBox chkShowRTFSource;
        private TextBox txtRTFSource;
        private Label lblProgress;
        private Button cmdTestFormulaFinder;
        private ComboBox cboFormulaFinderTestMode;
        private Label lblDLLVersion;
    }
}