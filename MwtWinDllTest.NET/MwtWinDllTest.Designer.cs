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
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cmdTestGetTrypticName = new System.Windows.Forms.Button();
            this.cmdExpandAbbreviations = new System.Windows.Forms.Button();
            this.cmdTestFunctions = new System.Windows.Forms.Button();
            this.cboStdDevMode = new System.Windows.Forms.ComboBox();
            this.cboWeightMode = new System.Windows.Forms.ComboBox();
            this.cmdConvertToEmpirical = new System.Windows.Forms.Button();
            this.cmdFindMass = new System.Windows.Forms.Button();
            this.txtFormula = new System.Windows.Forms.TextBox();
            this.cmdClose = new System.Windows.Forms.Button();
            this.lblStdDevMode = new System.Windows.Forms.Label();
            this.lblWeightMode = new System.Windows.Forms.Label();
            this.lblStatusLabel = new System.Windows.Forms.Label();
            this.lblMassAndStdDevLabel = new System.Windows.Forms.Label();
            this.lblMassAndStdDev = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblMass = new System.Windows.Forms.Label();
            this.lblMassLabel = new System.Windows.Forms.Label();
            this.lblFormula = new System.Windows.Forms.Label();
            this.dgDataGrid = new System.Windows.Forms.DataGrid();
            this.rtfFormula = new System.Windows.Forms.RichTextBox();
            this.chkShowRTFSource = new System.Windows.Forms.CheckBox();
            this.txtRTFSource = new System.Windows.Forms.TextBox();
            this.lblDLLVersion = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.cmdTestFormulaFinder = new System.Windows.Forms.Button();
            this.cboFormulaFinderTestMode = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdTestGetTrypticName
            // 
            this.cmdTestGetTrypticName.BackColor = System.Drawing.SystemColors.Control;
            this.cmdTestGetTrypticName.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdTestGetTrypticName.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdTestGetTrypticName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdTestGetTrypticName.Location = new System.Drawing.Point(256, 322);
            this.cmdTestGetTrypticName.Name = "cmdTestGetTrypticName";
            this.cmdTestGetTrypticName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdTestGetTrypticName.Size = new System.Drawing.Size(89, 41);
            this.cmdTestGetTrypticName.TabIndex = 19;
            this.cmdTestGetTrypticName.Text = "Test Get Tryptic Name";
            this.cmdTestGetTrypticName.UseVisualStyleBackColor = false;
            this.cmdTestGetTrypticName.Click += new System.EventHandler(this.cmdTestGetTrypticName_Click);
            // 
            // cmdExpandAbbreviations
            // 
            this.cmdExpandAbbreviations.BackColor = System.Drawing.SystemColors.Control;
            this.cmdExpandAbbreviations.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdExpandAbbreviations.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdExpandAbbreviations.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdExpandAbbreviations.Location = new System.Drawing.Point(288, 8);
            this.cmdExpandAbbreviations.Name = "cmdExpandAbbreviations";
            this.cmdExpandAbbreviations.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdExpandAbbreviations.Size = new System.Drawing.Size(89, 41);
            this.cmdExpandAbbreviations.TabIndex = 4;
            this.cmdExpandAbbreviations.Text = "Expand Abbreviations";
            this.cmdExpandAbbreviations.UseVisualStyleBackColor = false;
            this.cmdExpandAbbreviations.Click += new System.EventHandler(this.cmdExpandAbbreviations_Click);
            // 
            // cmdTestFunctions
            // 
            this.cmdTestFunctions.BackColor = System.Drawing.SystemColors.Control;
            this.cmdTestFunctions.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdTestFunctions.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdTestFunctions.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdTestFunctions.Location = new System.Drawing.Point(512, 8);
            this.cmdTestFunctions.Name = "cmdTestFunctions";
            this.cmdTestFunctions.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdTestFunctions.Size = new System.Drawing.Size(89, 41);
            this.cmdTestFunctions.TabIndex = 6;
            this.cmdTestFunctions.Text = "Test Functions";
            this.cmdTestFunctions.UseVisualStyleBackColor = false;
            this.cmdTestFunctions.Click += new System.EventHandler(this.cmdTestFunctions_Click);
            // 
            // cboStdDevMode
            // 
            this.cboStdDevMode.BackColor = System.Drawing.SystemColors.Window;
            this.cboStdDevMode.Cursor = System.Windows.Forms.Cursors.Default;
            this.cboStdDevMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStdDevMode.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboStdDevMode.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cboStdDevMode.Location = new System.Drawing.Point(104, 258);
            this.cboStdDevMode.Name = "cboStdDevMode";
            this.cboStdDevMode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cboStdDevMode.Size = new System.Drawing.Size(145, 22);
            this.cboStdDevMode.TabIndex = 3;
            this.cboStdDevMode.SelectedIndexChanged += new System.EventHandler(this.cboStdDevMode_SelectedIndexChanged);
            // 
            // cboWeightMode
            // 
            this.cboWeightMode.BackColor = System.Drawing.SystemColors.Window;
            this.cboWeightMode.Cursor = System.Windows.Forms.Cursors.Default;
            this.cboWeightMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWeightMode.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboWeightMode.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cboWeightMode.Location = new System.Drawing.Point(104, 234);
            this.cboWeightMode.Name = "cboWeightMode";
            this.cboWeightMode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cboWeightMode.Size = new System.Drawing.Size(145, 22);
            this.cboWeightMode.TabIndex = 2;
            this.cboWeightMode.SelectedIndexChanged += new System.EventHandler(this.cboWeightMode_SelectedIndexChanged);
            // 
            // cmdConvertToEmpirical
            // 
            this.cmdConvertToEmpirical.BackColor = System.Drawing.SystemColors.Control;
            this.cmdConvertToEmpirical.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdConvertToEmpirical.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdConvertToEmpirical.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdConvertToEmpirical.Location = new System.Drawing.Point(400, 8);
            this.cmdConvertToEmpirical.Name = "cmdConvertToEmpirical";
            this.cmdConvertToEmpirical.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdConvertToEmpirical.Size = new System.Drawing.Size(89, 41);
            this.cmdConvertToEmpirical.TabIndex = 5;
            this.cmdConvertToEmpirical.Text = "Convert to &Empirical";
            this.cmdConvertToEmpirical.UseVisualStyleBackColor = false;
            this.cmdConvertToEmpirical.Click += new System.EventHandler(this.cmdConvertToEmpirical_Click);
            // 
            // cmdFindMass
            // 
            this.cmdFindMass.BackColor = System.Drawing.SystemColors.Control;
            this.cmdFindMass.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdFindMass.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdFindMass.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdFindMass.Location = new System.Drawing.Point(8, 330);
            this.cmdFindMass.Name = "cmdFindMass";
            this.cmdFindMass.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdFindMass.Size = new System.Drawing.Size(89, 33);
            this.cmdFindMass.TabIndex = 8;
            this.cmdFindMass.Text = "&Calculate";
            this.cmdFindMass.UseVisualStyleBackColor = false;
            this.cmdFindMass.Click += new System.EventHandler(this.cmdFindMass_Click);
            // 
            // txtFormula
            // 
            this.txtFormula.AcceptsReturn = true;
            this.txtFormula.BackColor = System.Drawing.SystemColors.Window;
            this.txtFormula.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtFormula.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFormula.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtFormula.Location = new System.Drawing.Point(104, 16);
            this.txtFormula.MaxLength = 0;
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtFormula.Size = new System.Drawing.Size(129, 20);
            this.txtFormula.TabIndex = 0;
            this.txtFormula.Text = "Cl2PhH4OH";
            // 
            // cmdClose
            // 
            this.cmdClose.BackColor = System.Drawing.SystemColors.Control;
            this.cmdClose.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdClose.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdClose.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdClose.Location = new System.Drawing.Point(104, 330);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdClose.Size = new System.Drawing.Size(89, 33);
            this.cmdClose.TabIndex = 9;
            this.cmdClose.Text = "Cl&ose";
            this.cmdClose.UseVisualStyleBackColor = false;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // lblStdDevMode
            // 
            this.lblStdDevMode.BackColor = System.Drawing.SystemColors.Control;
            this.lblStdDevMode.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblStdDevMode.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStdDevMode.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblStdDevMode.Location = new System.Drawing.Point(8, 258);
            this.lblStdDevMode.Name = "lblStdDevMode";
            this.lblStdDevMode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblStdDevMode.Size = new System.Drawing.Size(73, 32);
            this.lblStdDevMode.TabIndex = 18;
            this.lblStdDevMode.Text = "Std Dev Mode";
            // 
            // lblWeightMode
            // 
            this.lblWeightMode.BackColor = System.Drawing.SystemColors.Control;
            this.lblWeightMode.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblWeightMode.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWeightMode.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblWeightMode.Location = new System.Drawing.Point(8, 234);
            this.lblWeightMode.Name = "lblWeightMode";
            this.lblWeightMode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblWeightMode.Size = new System.Drawing.Size(73, 17);
            this.lblWeightMode.TabIndex = 17;
            this.lblWeightMode.Text = "Weight Mode";
            // 
            // lblStatusLabel
            // 
            this.lblStatusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.lblStatusLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblStatusLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatusLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblStatusLabel.Location = new System.Drawing.Point(8, 136);
            this.lblStatusLabel.Name = "lblStatusLabel";
            this.lblStatusLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblStatusLabel.Size = new System.Drawing.Size(73, 17);
            this.lblStatusLabel.TabIndex = 16;
            this.lblStatusLabel.Text = "Status:";
            // 
            // lblMassAndStdDevLabel
            // 
            this.lblMassAndStdDevLabel.BackColor = System.Drawing.SystemColors.Control;
            this.lblMassAndStdDevLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblMassAndStdDevLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMassAndStdDevLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMassAndStdDevLabel.Location = new System.Drawing.Point(8, 64);
            this.lblMassAndStdDevLabel.Name = "lblMassAndStdDevLabel";
            this.lblMassAndStdDevLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblMassAndStdDevLabel.Size = new System.Drawing.Size(89, 40);
            this.lblMassAndStdDevLabel.TabIndex = 15;
            this.lblMassAndStdDevLabel.Text = "Mass and StdDev:";
            // 
            // lblMassAndStdDev
            // 
            this.lblMassAndStdDev.BackColor = System.Drawing.SystemColors.Control;
            this.lblMassAndStdDev.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblMassAndStdDev.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMassAndStdDev.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMassAndStdDev.Location = new System.Drawing.Point(104, 64);
            this.lblMassAndStdDev.Name = "lblMassAndStdDev";
            this.lblMassAndStdDev.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblMassAndStdDev.Size = new System.Drawing.Size(209, 17);
            this.lblMassAndStdDev.TabIndex = 14;
            this.lblMassAndStdDev.Text = "0";
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.SystemColors.Control;
            this.lblStatus.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblStatus.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblStatus.Location = new System.Drawing.Point(104, 139);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblStatus.Size = new System.Drawing.Size(225, 33);
            this.lblStatus.TabIndex = 13;
            // 
            // lblMass
            // 
            this.lblMass.BackColor = System.Drawing.SystemColors.Control;
            this.lblMass.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblMass.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMass.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMass.Location = new System.Drawing.Point(104, 40);
            this.lblMass.Name = "lblMass";
            this.lblMass.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblMass.Size = new System.Drawing.Size(129, 17);
            this.lblMass.TabIndex = 12;
            this.lblMass.Text = "0";
            // 
            // lblMassLabel
            // 
            this.lblMassLabel.BackColor = System.Drawing.SystemColors.Control;
            this.lblMassLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblMassLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMassLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMassLabel.Location = new System.Drawing.Point(8, 40);
            this.lblMassLabel.Name = "lblMassLabel";
            this.lblMassLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblMassLabel.Size = new System.Drawing.Size(73, 17);
            this.lblMassLabel.TabIndex = 11;
            this.lblMassLabel.Text = "Mass:";
            // 
            // lblFormula
            // 
            this.lblFormula.BackColor = System.Drawing.SystemColors.Control;
            this.lblFormula.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblFormula.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormula.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblFormula.Location = new System.Drawing.Point(8, 16);
            this.lblFormula.Name = "lblFormula";
            this.lblFormula.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblFormula.Size = new System.Drawing.Size(89, 17);
            this.lblFormula.TabIndex = 10;
            this.lblFormula.Text = "Formula:";
            // 
            // dgDataGrid
            // 
            this.dgDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgDataGrid.DataMember = "";
            this.dgDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgDataGrid.Location = new System.Drawing.Point(360, 72);
            this.dgDataGrid.Name = "dgDataGrid";
            this.dgDataGrid.Size = new System.Drawing.Size(616, 417);
            this.dgDataGrid.TabIndex = 20;
            // 
            // rtfFormula
            // 
            this.rtfFormula.Location = new System.Drawing.Point(104, 88);
            this.rtfFormula.Name = "rtfFormula";
            this.rtfFormula.ReadOnly = true;
            this.rtfFormula.Size = new System.Drawing.Size(232, 48);
            this.rtfFormula.TabIndex = 21;
            this.rtfFormula.Text = "";
            this.rtfFormula.TextChanged += new System.EventHandler(this.rtfFormula_TextChanged);
            // 
            // chkShowRTFSource
            // 
            this.chkShowRTFSource.Location = new System.Drawing.Point(8, 378);
            this.chkShowRTFSource.Name = "chkShowRTFSource";
            this.chkShowRTFSource.Size = new System.Drawing.Size(88, 24);
            this.chkShowRTFSource.TabIndex = 22;
            this.chkShowRTFSource.Text = "Show RTF Source";
            this.chkShowRTFSource.CheckedChanged += new System.EventHandler(this.chkShowRTFSource_CheckedChanged);
            // 
            // txtRTFSource
            // 
            this.txtRTFSource.AcceptsReturn = true;
            this.txtRTFSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtRTFSource.BackColor = System.Drawing.SystemColors.Window;
            this.txtRTFSource.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtRTFSource.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRTFSource.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtRTFSource.Location = new System.Drawing.Point(112, 370);
            this.txtRTFSource.MaxLength = 0;
            this.txtRTFSource.Multiline = true;
            this.txtRTFSource.Name = "txtRTFSource";
            this.txtRTFSource.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtRTFSource.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRTFSource.Size = new System.Drawing.Size(240, 137);
            this.txtRTFSource.TabIndex = 23;
            this.txtRTFSource.Visible = false;
            // 
            // lblDLLVersion
            // 
            this.lblDLLVersion.BackColor = System.Drawing.SystemColors.Control;
            this.lblDLLVersion.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDLLVersion.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDLLVersion.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblDLLVersion.Location = new System.Drawing.Point(8, 298);
            this.lblDLLVersion.Name = "lblDLLVersion";
            this.lblDLLVersion.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDLLVersion.Size = new System.Drawing.Size(336, 16);
            this.lblDLLVersion.TabIndex = 24;
            this.lblDLLVersion.Text = "DLL Version";
            // 
            // lblProgress
            // 
            this.lblProgress.BackColor = System.Drawing.SystemColors.Control;
            this.lblProgress.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblProgress.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgress.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblProgress.Location = new System.Drawing.Point(104, 182);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblProgress.Size = new System.Drawing.Size(225, 39);
            this.lblProgress.TabIndex = 25;
            // 
            // cmdTestFormulaFinder
            // 
            this.cmdTestFormulaFinder.BackColor = System.Drawing.SystemColors.Control;
            this.cmdTestFormulaFinder.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdTestFormulaFinder.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdTestFormulaFinder.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdTestFormulaFinder.Location = new System.Drawing.Point(619, 8);
            this.cmdTestFormulaFinder.Name = "cmdTestFormulaFinder";
            this.cmdTestFormulaFinder.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdTestFormulaFinder.Size = new System.Drawing.Size(134, 27);
            this.cmdTestFormulaFinder.TabIndex = 26;
            this.cmdTestFormulaFinder.Text = "Test Formula Finder";
            this.cmdTestFormulaFinder.UseVisualStyleBackColor = false;
            this.cmdTestFormulaFinder.Click += new System.EventHandler(this.cmdTestFormulaFinder_Click);
            // 
            // cboFormulaFinderTestMode
            // 
            this.cboFormulaFinderTestMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFormulaFinderTestMode.BackColor = System.Drawing.SystemColors.Window;
            this.cboFormulaFinderTestMode.Cursor = System.Windows.Forms.Cursors.Default;
            this.cboFormulaFinderTestMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFormulaFinderTestMode.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboFormulaFinderTestMode.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cboFormulaFinderTestMode.Location = new System.Drawing.Point(619, 37);
            this.cboFormulaFinderTestMode.Name = "cboFormulaFinderTestMode";
            this.cboFormulaFinderTestMode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cboFormulaFinderTestMode.Size = new System.Drawing.Size(365, 22);
            this.cboFormulaFinderTestMode.TabIndex = 27;
            // 
            // frmMwtWinDllTest
            // 
            this.AcceptButton = this.cmdFindMass;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.cmdClose;
            this.ClientSize = new System.Drawing.Size(995, 505);
            this.Controls.Add(this.cboFormulaFinderTestMode);
            this.Controls.Add(this.cmdTestFormulaFinder);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.lblDLLVersion);
            this.Controls.Add(this.txtRTFSource);
            this.Controls.Add(this.txtFormula);
            this.Controls.Add(this.chkShowRTFSource);
            this.Controls.Add(this.rtfFormula);
            this.Controls.Add(this.dgDataGrid);
            this.Controls.Add(this.cmdTestGetTrypticName);
            this.Controls.Add(this.cmdExpandAbbreviations);
            this.Controls.Add(this.cmdTestFunctions);
            this.Controls.Add(this.cboStdDevMode);
            this.Controls.Add(this.cboWeightMode);
            this.Controls.Add(this.cmdConvertToEmpirical);
            this.Controls.Add(this.cmdFindMass);
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.lblStdDevMode);
            this.Controls.Add(this.lblWeightMode);
            this.Controls.Add(this.lblStatusLabel);
            this.Controls.Add(this.lblMassAndStdDevLabel);
            this.Controls.Add(this.lblMassAndStdDev);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblMass);
            this.Controls.Add(this.lblMassLabel);
            this.Controls.Add(this.lblFormula);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Location = new System.Drawing.Point(4, 25);
            this.Name = "frmMwtWinDllTest";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Mwt Win Dll Test";
            ((System.ComponentModel.ISupportInitialize)(this.dgDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip1;
        private System.Windows.Forms.Button cmdTestGetTrypticName;
        //private AxMSFlexGridLib.AxMSFlexGrid grdFlexGrid;
        private System.Windows.Forms.Button cmdExpandAbbreviations;
        private System.Windows.Forms.Button cmdTestFunctions;
        private System.Windows.Forms.ComboBox cboStdDevMode;
        private System.Windows.Forms.ComboBox cboWeightMode;
        private System.Windows.Forms.Button cmdConvertToEmpirical;
        private System.Windows.Forms.Button cmdFindMass;
        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Label lblStdDevMode;
        private System.Windows.Forms.Label lblWeightMode;
        private System.Windows.Forms.Label lblStatusLabel;
        private System.Windows.Forms.Label lblMassAndStdDevLabel;
        private System.Windows.Forms.Label lblMassAndStdDev;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblMass;
        private System.Windows.Forms.Label lblMassLabel;
        private System.Windows.Forms.Label lblFormula;
        private System.Windows.Forms.DataGrid dgDataGrid;
        private System.Windows.Forms.RichTextBox rtfFormula;
        private System.Windows.Forms.CheckBox chkShowRTFSource;
        private System.Windows.Forms.TextBox txtRTFSource;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Button cmdTestFormulaFinder;
        private System.Windows.Forms.ComboBox cboFormulaFinderTestMode;
        private System.Windows.Forms.Label lblDLLVersion;
    }
}