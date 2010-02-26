Option Strict On
Option Explicit On

Friend Class frmMwtWinDllTest
    ' Molecular Weight Calculator Dll test program

    ' -------------------------------------------------------------------------------
    ' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
    ' E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com
    ' Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/
    ' -------------------------------------------------------------------------------
    ' 
    ' Licensed under the Apache License, Version 2.0; you may not use this file except
    ' in compliance with the License.  You may obtain a copy of the License at 
    ' http://www.apache.org/licenses/LICENSE-2.0
    '
    ' Notice: This computer software was prepared by Battelle Memorial Institute, 
    ' hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
    ' Department of Energy (DOE).  All rights in the computer software are reserved 
    ' by DOE on behalf of the United States Government and the Contractor as 
    ' provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
    ' WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
    ' SOFTWARE.  This notice including this sentence must appear on any copies of 
    ' this computer software.

    Inherits System.Windows.Forms.Form
#Region "Windows Form Designer generated code "
    Public Sub New()
        'This call is required by the Windows Form Designer.
        InitializeComponent()

        InitializeControls()
    End Sub
    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal Disposing As Boolean)
        If Disposing Then
            If Not components Is Nothing Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(Disposing)
    End Sub
    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Public ToolTip1 As System.Windows.Forms.ToolTip
    Public WithEvents cmdTestGetTrypticName As System.Windows.Forms.Button
    'Public WithEvents grdFlexGrid As AxMSFlexGridLib.AxMSFlexGrid
    Public WithEvents cmdExpandAbbreviations As System.Windows.Forms.Button
    Public WithEvents cmdTestFunctions As System.Windows.Forms.Button
    Public WithEvents cboStdDevMode As System.Windows.Forms.ComboBox
    Public WithEvents cboWeightMode As System.Windows.Forms.ComboBox
    Public WithEvents cmdConvertToEmpirical As System.Windows.Forms.Button
    Public WithEvents cmdFindMass As System.Windows.Forms.Button
    Public WithEvents txtFormula As System.Windows.Forms.TextBox
    Public WithEvents cmdClose As System.Windows.Forms.Button
    Public WithEvents lblStdDevMode As System.Windows.Forms.Label
    Public WithEvents lblWeightMode As System.Windows.Forms.Label
    Public WithEvents lblStatusLabel As System.Windows.Forms.Label
    Public WithEvents lblMassAndStdDevLabel As System.Windows.Forms.Label
    Public WithEvents lblMassAndStdDev As System.Windows.Forms.Label
    Public WithEvents lblStatus As System.Windows.Forms.Label
    Public WithEvents lblMass As System.Windows.Forms.Label
    Public WithEvents lblMassLabel As System.Windows.Forms.Label
    Public WithEvents lblFormula As System.Windows.Forms.Label
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    Friend WithEvents dgDataGrid As System.Windows.Forms.DataGrid
    Friend WithEvents rtfFormula As System.Windows.Forms.RichTextBox
    Friend WithEvents chkShowRTFSource As System.Windows.Forms.CheckBox
    Public WithEvents txtRTFSource As System.Windows.Forms.TextBox
    Public WithEvents lblProgress As System.Windows.Forms.Label
    Public WithEvents lblDLLVersion As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.cmdTestGetTrypticName = New System.Windows.Forms.Button
        Me.cmdExpandAbbreviations = New System.Windows.Forms.Button
        Me.cmdTestFunctions = New System.Windows.Forms.Button
        Me.cboStdDevMode = New System.Windows.Forms.ComboBox
        Me.cboWeightMode = New System.Windows.Forms.ComboBox
        Me.cmdConvertToEmpirical = New System.Windows.Forms.Button
        Me.cmdFindMass = New System.Windows.Forms.Button
        Me.txtFormula = New System.Windows.Forms.TextBox
        Me.cmdClose = New System.Windows.Forms.Button
        Me.lblStdDevMode = New System.Windows.Forms.Label
        Me.lblWeightMode = New System.Windows.Forms.Label
        Me.lblStatusLabel = New System.Windows.Forms.Label
        Me.lblMassAndStdDevLabel = New System.Windows.Forms.Label
        Me.lblMassAndStdDev = New System.Windows.Forms.Label
        Me.lblStatus = New System.Windows.Forms.Label
        Me.lblMass = New System.Windows.Forms.Label
        Me.lblMassLabel = New System.Windows.Forms.Label
        Me.lblFormula = New System.Windows.Forms.Label
        Me.dgDataGrid = New System.Windows.Forms.DataGrid
        Me.rtfFormula = New System.Windows.Forms.RichTextBox
        Me.chkShowRTFSource = New System.Windows.Forms.CheckBox
        Me.txtRTFSource = New System.Windows.Forms.TextBox
        Me.lblDLLVersion = New System.Windows.Forms.Label
        Me.lblProgress = New System.Windows.Forms.Label
        CType(Me.dgDataGrid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'cmdTestGetTrypticName
        '
        Me.cmdTestGetTrypticName.BackColor = System.Drawing.SystemColors.Control
        Me.cmdTestGetTrypticName.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdTestGetTrypticName.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdTestGetTrypticName.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdTestGetTrypticName.Location = New System.Drawing.Point(256, 322)
        Me.cmdTestGetTrypticName.Name = "cmdTestGetTrypticName"
        Me.cmdTestGetTrypticName.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdTestGetTrypticName.Size = New System.Drawing.Size(89, 41)
        Me.cmdTestGetTrypticName.TabIndex = 19
        Me.cmdTestGetTrypticName.Text = "Test Get Tryptic Name"
        Me.cmdTestGetTrypticName.UseVisualStyleBackColor = False
        '
        'cmdExpandAbbreviations
        '
        Me.cmdExpandAbbreviations.BackColor = System.Drawing.SystemColors.Control
        Me.cmdExpandAbbreviations.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdExpandAbbreviations.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdExpandAbbreviations.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdExpandAbbreviations.Location = New System.Drawing.Point(288, 8)
        Me.cmdExpandAbbreviations.Name = "cmdExpandAbbreviations"
        Me.cmdExpandAbbreviations.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdExpandAbbreviations.Size = New System.Drawing.Size(89, 41)
        Me.cmdExpandAbbreviations.TabIndex = 4
        Me.cmdExpandAbbreviations.Text = "Expand Abbreviations"
        Me.cmdExpandAbbreviations.UseVisualStyleBackColor = False
        '
        'cmdTestFunctions
        '
        Me.cmdTestFunctions.BackColor = System.Drawing.SystemColors.Control
        Me.cmdTestFunctions.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdTestFunctions.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdTestFunctions.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdTestFunctions.Location = New System.Drawing.Point(512, 8)
        Me.cmdTestFunctions.Name = "cmdTestFunctions"
        Me.cmdTestFunctions.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdTestFunctions.Size = New System.Drawing.Size(89, 41)
        Me.cmdTestFunctions.TabIndex = 6
        Me.cmdTestFunctions.Text = "Test Functions"
        Me.cmdTestFunctions.UseVisualStyleBackColor = False
        '
        'cboStdDevMode
        '
        Me.cboStdDevMode.BackColor = System.Drawing.SystemColors.Window
        Me.cboStdDevMode.Cursor = System.Windows.Forms.Cursors.Default
        Me.cboStdDevMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboStdDevMode.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cboStdDevMode.ForeColor = System.Drawing.SystemColors.WindowText
        Me.cboStdDevMode.Location = New System.Drawing.Point(104, 258)
        Me.cboStdDevMode.Name = "cboStdDevMode"
        Me.cboStdDevMode.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cboStdDevMode.Size = New System.Drawing.Size(145, 22)
        Me.cboStdDevMode.TabIndex = 3
        '
        'cboWeightMode
        '
        Me.cboWeightMode.BackColor = System.Drawing.SystemColors.Window
        Me.cboWeightMode.Cursor = System.Windows.Forms.Cursors.Default
        Me.cboWeightMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboWeightMode.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cboWeightMode.ForeColor = System.Drawing.SystemColors.WindowText
        Me.cboWeightMode.Location = New System.Drawing.Point(104, 234)
        Me.cboWeightMode.Name = "cboWeightMode"
        Me.cboWeightMode.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cboWeightMode.Size = New System.Drawing.Size(145, 22)
        Me.cboWeightMode.TabIndex = 2
        '
        'cmdConvertToEmpirical
        '
        Me.cmdConvertToEmpirical.BackColor = System.Drawing.SystemColors.Control
        Me.cmdConvertToEmpirical.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdConvertToEmpirical.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdConvertToEmpirical.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdConvertToEmpirical.Location = New System.Drawing.Point(400, 8)
        Me.cmdConvertToEmpirical.Name = "cmdConvertToEmpirical"
        Me.cmdConvertToEmpirical.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdConvertToEmpirical.Size = New System.Drawing.Size(89, 41)
        Me.cmdConvertToEmpirical.TabIndex = 5
        Me.cmdConvertToEmpirical.Text = "Convert to &Empirical"
        Me.cmdConvertToEmpirical.UseVisualStyleBackColor = False
        '
        'cmdFindMass
        '
        Me.cmdFindMass.BackColor = System.Drawing.SystemColors.Control
        Me.cmdFindMass.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdFindMass.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdFindMass.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdFindMass.Location = New System.Drawing.Point(8, 330)
        Me.cmdFindMass.Name = "cmdFindMass"
        Me.cmdFindMass.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdFindMass.Size = New System.Drawing.Size(89, 33)
        Me.cmdFindMass.TabIndex = 8
        Me.cmdFindMass.Text = "&Calculate"
        Me.cmdFindMass.UseVisualStyleBackColor = False
        '
        'txtFormula
        '
        Me.txtFormula.AcceptsReturn = True
        Me.txtFormula.BackColor = System.Drawing.SystemColors.Window
        Me.txtFormula.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtFormula.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtFormula.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtFormula.Location = New System.Drawing.Point(104, 16)
        Me.txtFormula.MaxLength = 0
        Me.txtFormula.Name = "txtFormula"
        Me.txtFormula.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtFormula.Size = New System.Drawing.Size(129, 20)
        Me.txtFormula.TabIndex = 0
        Me.txtFormula.Text = "Cl2PhH4OH"
        '
        'cmdClose
        '
        Me.cmdClose.BackColor = System.Drawing.SystemColors.Control
        Me.cmdClose.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdClose.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdClose.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdClose.Location = New System.Drawing.Point(104, 330)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdClose.Size = New System.Drawing.Size(89, 33)
        Me.cmdClose.TabIndex = 9
        Me.cmdClose.Text = "Cl&ose"
        Me.cmdClose.UseVisualStyleBackColor = False
        '
        'lblStdDevMode
        '
        Me.lblStdDevMode.BackColor = System.Drawing.SystemColors.Control
        Me.lblStdDevMode.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblStdDevMode.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStdDevMode.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblStdDevMode.Location = New System.Drawing.Point(8, 258)
        Me.lblStdDevMode.Name = "lblStdDevMode"
        Me.lblStdDevMode.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblStdDevMode.Size = New System.Drawing.Size(73, 32)
        Me.lblStdDevMode.TabIndex = 18
        Me.lblStdDevMode.Text = "Std Dev Mode"
        '
        'lblWeightMode
        '
        Me.lblWeightMode.BackColor = System.Drawing.SystemColors.Control
        Me.lblWeightMode.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblWeightMode.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblWeightMode.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblWeightMode.Location = New System.Drawing.Point(8, 234)
        Me.lblWeightMode.Name = "lblWeightMode"
        Me.lblWeightMode.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblWeightMode.Size = New System.Drawing.Size(73, 17)
        Me.lblWeightMode.TabIndex = 17
        Me.lblWeightMode.Text = "Weight Mode"
        '
        'lblStatusLabel
        '
        Me.lblStatusLabel.BackColor = System.Drawing.SystemColors.Control
        Me.lblStatusLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblStatusLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatusLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblStatusLabel.Location = New System.Drawing.Point(8, 136)
        Me.lblStatusLabel.Name = "lblStatusLabel"
        Me.lblStatusLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblStatusLabel.Size = New System.Drawing.Size(73, 17)
        Me.lblStatusLabel.TabIndex = 16
        Me.lblStatusLabel.Text = "Status:"
        '
        'lblMassAndStdDevLabel
        '
        Me.lblMassAndStdDevLabel.BackColor = System.Drawing.SystemColors.Control
        Me.lblMassAndStdDevLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblMassAndStdDevLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMassAndStdDevLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblMassAndStdDevLabel.Location = New System.Drawing.Point(8, 64)
        Me.lblMassAndStdDevLabel.Name = "lblMassAndStdDevLabel"
        Me.lblMassAndStdDevLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblMassAndStdDevLabel.Size = New System.Drawing.Size(89, 40)
        Me.lblMassAndStdDevLabel.TabIndex = 15
        Me.lblMassAndStdDevLabel.Text = "Mass and StdDev:"
        '
        'lblMassAndStdDev
        '
        Me.lblMassAndStdDev.BackColor = System.Drawing.SystemColors.Control
        Me.lblMassAndStdDev.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblMassAndStdDev.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMassAndStdDev.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblMassAndStdDev.Location = New System.Drawing.Point(104, 64)
        Me.lblMassAndStdDev.Name = "lblMassAndStdDev"
        Me.lblMassAndStdDev.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblMassAndStdDev.Size = New System.Drawing.Size(209, 17)
        Me.lblMassAndStdDev.TabIndex = 14
        Me.lblMassAndStdDev.Text = "0"
        '
        'lblStatus
        '
        Me.lblStatus.BackColor = System.Drawing.SystemColors.Control
        Me.lblStatus.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblStatus.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatus.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblStatus.Location = New System.Drawing.Point(104, 139)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblStatus.Size = New System.Drawing.Size(225, 33)
        Me.lblStatus.TabIndex = 13
        '
        'lblMass
        '
        Me.lblMass.BackColor = System.Drawing.SystemColors.Control
        Me.lblMass.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblMass.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMass.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblMass.Location = New System.Drawing.Point(104, 40)
        Me.lblMass.Name = "lblMass"
        Me.lblMass.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblMass.Size = New System.Drawing.Size(129, 17)
        Me.lblMass.TabIndex = 12
        Me.lblMass.Text = "0"
        '
        'lblMassLabel
        '
        Me.lblMassLabel.BackColor = System.Drawing.SystemColors.Control
        Me.lblMassLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblMassLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMassLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblMassLabel.Location = New System.Drawing.Point(8, 40)
        Me.lblMassLabel.Name = "lblMassLabel"
        Me.lblMassLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblMassLabel.Size = New System.Drawing.Size(73, 17)
        Me.lblMassLabel.TabIndex = 11
        Me.lblMassLabel.Text = "Mass:"
        '
        'lblFormula
        '
        Me.lblFormula.BackColor = System.Drawing.SystemColors.Control
        Me.lblFormula.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblFormula.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFormula.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblFormula.Location = New System.Drawing.Point(8, 16)
        Me.lblFormula.Name = "lblFormula"
        Me.lblFormula.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblFormula.Size = New System.Drawing.Size(89, 17)
        Me.lblFormula.TabIndex = 10
        Me.lblFormula.Text = "Formula:"
        '
        'dgDataGrid
        '
        Me.dgDataGrid.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.dgDataGrid.DataMember = ""
        Me.dgDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText
        Me.dgDataGrid.Location = New System.Drawing.Point(360, 72)
        Me.dgDataGrid.Name = "dgDataGrid"
        Me.dgDataGrid.Size = New System.Drawing.Size(280, 365)
        Me.dgDataGrid.TabIndex = 20
        '
        'rtfFormula
        '
        Me.rtfFormula.Location = New System.Drawing.Point(104, 88)
        Me.rtfFormula.Name = "rtfFormula"
        Me.rtfFormula.ReadOnly = True
        Me.rtfFormula.Size = New System.Drawing.Size(232, 48)
        Me.rtfFormula.TabIndex = 21
        Me.rtfFormula.Text = ""
        '
        'chkShowRTFSource
        '
        Me.chkShowRTFSource.Location = New System.Drawing.Point(8, 378)
        Me.chkShowRTFSource.Name = "chkShowRTFSource"
        Me.chkShowRTFSource.Size = New System.Drawing.Size(88, 24)
        Me.chkShowRTFSource.TabIndex = 22
        Me.chkShowRTFSource.Text = "Show RTF Source"
        '
        'txtRTFSource
        '
        Me.txtRTFSource.AcceptsReturn = True
        Me.txtRTFSource.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtRTFSource.BackColor = System.Drawing.SystemColors.Window
        Me.txtRTFSource.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtRTFSource.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtRTFSource.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtRTFSource.Location = New System.Drawing.Point(112, 370)
        Me.txtRTFSource.MaxLength = 0
        Me.txtRTFSource.Multiline = True
        Me.txtRTFSource.Name = "txtRTFSource"
        Me.txtRTFSource.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtRTFSource.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtRTFSource.Size = New System.Drawing.Size(240, 67)
        Me.txtRTFSource.TabIndex = 23
        Me.txtRTFSource.Visible = False
        '
        'lblDLLVersion
        '
        Me.lblDLLVersion.BackColor = System.Drawing.SystemColors.Control
        Me.lblDLLVersion.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblDLLVersion.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDLLVersion.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblDLLVersion.Location = New System.Drawing.Point(8, 298)
        Me.lblDLLVersion.Name = "lblDLLVersion"
        Me.lblDLLVersion.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblDLLVersion.Size = New System.Drawing.Size(336, 16)
        Me.lblDLLVersion.TabIndex = 24
        Me.lblDLLVersion.Text = "DLL Version"
        '
        'lblProgress
        '
        Me.lblProgress.BackColor = System.Drawing.SystemColors.Control
        Me.lblProgress.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblProgress.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblProgress.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblProgress.Location = New System.Drawing.Point(104, 182)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblProgress.Size = New System.Drawing.Size(225, 39)
        Me.lblProgress.TabIndex = 25
        '
        'frmMwtWinDllTest
        '
        Me.AcceptButton = Me.cmdFindMass
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.CancelButton = Me.cmdClose
        Me.ClientSize = New System.Drawing.Size(652, 453)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.lblDLLVersion)
        Me.Controls.Add(Me.txtRTFSource)
        Me.Controls.Add(Me.txtFormula)
        Me.Controls.Add(Me.chkShowRTFSource)
        Me.Controls.Add(Me.rtfFormula)
        Me.Controls.Add(Me.dgDataGrid)
        Me.Controls.Add(Me.cmdTestGetTrypticName)
        Me.Controls.Add(Me.cmdExpandAbbreviations)
        Me.Controls.Add(Me.cmdTestFunctions)
        Me.Controls.Add(Me.cboStdDevMode)
        Me.Controls.Add(Me.cboWeightMode)
        Me.Controls.Add(Me.cmdConvertToEmpirical)
        Me.Controls.Add(Me.cmdFindMass)
        Me.Controls.Add(Me.cmdClose)
        Me.Controls.Add(Me.lblStdDevMode)
        Me.Controls.Add(Me.lblWeightMode)
        Me.Controls.Add(Me.lblStatusLabel)
        Me.Controls.Add(Me.lblMassAndStdDevLabel)
        Me.Controls.Add(Me.lblMassAndStdDev)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.lblMass)
        Me.Controls.Add(Me.lblMassLabel)
        Me.Controls.Add(Me.lblFormula)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Location = New System.Drawing.Point(4, 25)
        Me.Name = "frmMwtWinDllTest"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Text = "Mwt Win Dll Test"
        CType(Me.dgDataGrid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region

    Private WithEvents mMwtWin As MwtWinDll.MolecularWeightCalculator
    Private mMDIDListDataView As System.Data.DataView
    Private myDataSet As System.Data.DataSet

    Private Sub AppendColumnToTableStyle(ByRef tsTableStyle As System.Windows.Forms.DataGridTableStyle, ByVal strMappingName As String, ByVal strHeaderText As String, Optional ByVal intWidth As Integer = 75, Optional ByVal blnIsReadOnly As Boolean = False, Optional ByVal blnIsDateTime As Boolean = False, Optional ByVal intDecimalPlaces As Integer = -1)
        ' If intDecimalPlaces is >=0, then a format string is constructed to show the specified number of decimal places
        Dim TextCol As New DataGridTextBoxColumn
        Dim i As Integer

        With TextCol
            .MappingName = strMappingName
            .HeaderText = strHeaderText
            .Width = intWidth
            .ReadOnly = blnIsReadOnly
            If blnIsDateTime Then
                .Format = "g"
            ElseIf intDecimalPlaces >= 0 Then
                .Format = "0."
                For i = 0 To intDecimalPlaces - 1
                    .Format &= "0"
                Next i
            End If
        End With

        tsTableStyle.GridColumnStyles.Add(TextCol)

    End Sub

    Private Sub AppendBoolColumnToTableStyle(ByRef tsTableStyle As System.Windows.Forms.DataGridTableStyle, ByVal strMappingName As String, ByVal strHeaderText As String, Optional ByVal intWidth As Integer = 75, Optional ByVal blnIsReadOnly As Boolean = False, Optional ByVal blnSourceIsTrueFalse As Boolean = True)
        ' If intDecimalPlaces is >=0, then a format string is constructed to show the specified number of decimal places
        Dim BoolCol As New DataGridBoolColumn
        Dim i As Integer

        With BoolCol
            .MappingName = strMappingName
            .HeaderText = strHeaderText
            .Width = intWidth
            .ReadOnly = blnIsReadOnly
            If blnSourceIsTrueFalse Then
                .FalseValue = False
                .TrueValue = True
            Else
                .FalseValue = 0
                .TrueValue = 1
            End If
            .AllowNull = False
            .NullValue = Convert.DBNull
        End With

        tsTableStyle.GridColumnStyles.Add(BoolCol)

    End Sub

    Private Sub FindPercentComposition()
        Dim intIndex, intElementCount As Short
        Dim dblPctCompForCarbon As Double
        Dim strPctCompForCarbon As String


        Dim strPctCompositions() As String

        With mMwtWin.Compound
            .Formula = txtFormula.Text

            dblPctCompForCarbon = .GetPercentCompositionForElement(6)
            strPctCompForCarbon = .GetPercentCompositionForElementAsString(6)

            intElementCount = CShort(mMwtWin.GetElementCount)
            ReDim strPctCompositions(intElementCount)

            .GetPercentCompositionForAllElements(strPctCompositions)

        End With

        MakePercentCompositionDataSet(intElementCount, strPctCompositions)

        dgDataGrid.SetDataBinding(myDataSet, "DataTable1")

    End Sub


    Private Sub FindMass()

        lblProgress.Text = String.Empty

        ' Can simply compute the mass of a formula using ComputeMass
        lblMass.Text = CStr(mMwtWin.ComputeMass(txtFormula.Text))

        ' If we want to do more complex operations, need to fill mMwtWin.Compound with valid info
        ' Then, can read out values from it
        With mMwtWin.Compound
            .Formula = txtFormula.Text

            If .ErrorDescription = "" Then
                lblMass.Text = CStr(.Mass)
                lblStatus.Text = .CautionDescription
                txtFormula.Text = .FormulaCapitalized
                rtfFormula.Rtf = .FormulaRTF
                lblMassAndStdDev.Text = .MassAndStdDevString
            Else
                lblStatus.Text = .ErrorDescription
            End If
        End With

    End Sub

    Private Sub InitializeControls()
        mMwtWin = New MwtWinDll.MolecularWeightCalculator
        mMwtWin.ShowErrorDialogs = True

        lblDLLVersion.Text = "DLL Info: " & mMwtWin.AppDate & ", Version " & mMwtWin.AppVersion
        PopulateComboBoxes()
    End Sub

    Private Sub MakeDataSet(ByVal lngIonCount As Integer, ByVal udtFragSpectrum() As MwtWinDll.MWPeptideClass.udtFragmentationSpectrumDataType)
        ' Create a DataSet.
        myDataSet = New DataSet("myDataSet")

        ' Create a DataTable.
        Dim tDataTable As New DataTable("DataTable1")

        ' Create three columns, and add them to the table.
        Dim cCMass As New DataColumn("Mass", GetType(Double))
        Dim cIntensity As New DataColumn("Intensity", GetType(Double))
        Dim cSymbol As New DataColumn("Symbol", GetType(String))
        tDataTable.Columns.Add(cCMass)
        tDataTable.Columns.Add(cIntensity)
        tDataTable.Columns.Add(cSymbol)

        ' Add the table to the DataSet.
        myDataSet.Tables.Add(tDataTable)

        ' Populates the table. 
        Dim newRow1 As DataRow

        ' Create rows in the Customers Table.
        Dim lngIndex As Integer

        For lngIndex = 0 To lngIonCount - 1
            newRow1 = tDataTable.NewRow()
            newRow1("Mass") = udtFragSpectrum(lngIndex).Mass
            newRow1("Intensity") = udtFragSpectrum(lngIndex).Intensity
            newRow1("Symbol") = udtFragSpectrum(lngIndex).Symbol
            ' Add the row to the Customers table.
            tDataTable.Rows.Add(newRow1)
        Next lngIndex

    End Sub

    Private Sub MakePercentCompositionDataSet(ByVal intElementCount As Short, ByVal strPctCompositions() As String)

        Dim strSymbol As String

        ' Create a DataSet.
        myDataSet = New DataSet("myDataSet")

        ' Create a DataTable.
        Dim tDataTable As New DataTable("DataTable1")

        ' Create three columns, and add them to the table.
        Dim cElement As New DataColumn("Element", GetType(String))
        Dim cPctComp As New DataColumn("Pct Comp", GetType(String))

        tDataTable.Columns.Add(cElement)
        tDataTable.Columns.Add(cPctComp)


        ' Add the table to the DataSet.
        myDataSet.Tables.Add(tDataTable)

        ' Populates the table. 
        Dim newRow1 As DataRow

        ' Create rows in the Customers Table.
        Dim intIndex As Short

        For intIndex = 1 To intElementCount
            If strPctCompositions(intIndex) <> "" Then
                mMwtWin.GetElement(intIndex, strSymbol, 0, 0, 0, 0)
                newRow1 = tDataTable.NewRow()
                newRow1("Element") = strSymbol
                newRow1("Pct Comp") = strPctCompositions(intIndex)
                ' Add the row to the Customers table.
                tDataTable.Rows.Add(newRow1)
            End If

        Next intIndex

    End Sub

    Private Sub PopulateComboBoxes()
        With cboWeightMode
            .Items.Clear()
            .Items.Add("Average mass")
            .Items.Add("Isotopic mass")
            .Items.Add("Integer mass")
            .SelectedIndex = 0
        End With

        With cboStdDevMode
            .Items.Clear()
            .Items.Add("Short")
            .Items.Add("Scientific")
            .Items.Add("Decimal")
            .SelectedIndex = 0
        End With


    End Sub

    Public Sub TestAccessFunctions()
        Dim intIndex, intResult As Integer
        Dim lngIndex As Integer
        Dim lngItemCount As Integer
        Dim strSymbol, strFormula As String
        Dim sngCharge As Single
        Dim blnIsAminoAcid As Boolean
        Dim strOneLetterSymbol, strComment As String
        Dim strStatement As String
        Dim dblmass, dblUncertainty As Double
        Dim intIsotopeCount, intIsotopeCount2 As Short
        Dim dblIsotopeMasses() As Double
        Dim sngIsotopeAbundances() As Single
        Dim dblNewPressure As Double

        Dim objCompound As New MwtWinDll.MWCompoundClass

        Dim objResults As New frmTextbrowser

        lblProgress.Text = String.Empty

        objResults.Show()
        objResults.SetText = String.Empty

        With mMwtWin
            ' Test Abbreviations
            lngItemCount = .GetAbbreviationCount
            For intIndex = 1 To lngItemCount
                intResult = .GetAbbreviation(intIndex, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment)
                System.Diagnostics.Debug.Assert(intResult = 0, "")
                System.Diagnostics.Debug.Assert(.GetAbbreviationID(strSymbol) = intIndex, "")

                intResult = .SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment)
                System.Diagnostics.Debug.Assert(intResult = 0, "")
            Next intIndex

            ' Test Caution statements
            lngItemCount = .GetCautionStatementCount
            For intIndex = 1 To lngItemCount
                intResult = .GetCautionStatement(intIndex, strSymbol, strStatement)
                System.Diagnostics.Debug.Assert(intResult = 0, "")
                System.Diagnostics.Debug.Assert(.GetCautionStatementID(strSymbol) = intIndex, "")

                intResult = .SetCautionStatement(strSymbol, strStatement)
                System.Diagnostics.Debug.Assert(intResult = 0, "")
            Next intIndex

            ' Test Element access
            lngItemCount = .GetElementCount
            For intIndex = 1 To lngItemCount
                intResult = .GetElement(CShort(intIndex), strSymbol, dblmass, dblUncertainty, sngCharge, intIsotopeCount)
                System.Diagnostics.Debug.Assert(intResult = 0, "")
                System.Diagnostics.Debug.Assert(.GetElementID(strSymbol) = intIndex, "")

                intResult = .SetElement(strSymbol, dblmass, dblUncertainty, sngCharge, False)
                System.Diagnostics.Debug.Assert(intResult = 0, "")

                ReDim dblIsotopeMasses(intIsotopeCount + 1)
                ReDim sngIsotopeAbundances(intIsotopeCount + 1)

                intResult = .GetElementIsotopes(CShort(intIndex), intIsotopeCount2, dblIsotopeMasses, sngIsotopeAbundances)
                System.Diagnostics.Debug.Assert(intIsotopeCount = intIsotopeCount2, "")
                System.Diagnostics.Debug.Assert(intResult = 0, "")

                intResult = .SetElementIsotopes(strSymbol, intIsotopeCount, dblIsotopeMasses, sngIsotopeAbundances)
                System.Diagnostics.Debug.Assert(intResult = 0, "")
            Next intIndex

            ' Test Message Statements access
            lngItemCount = .GetMessageStatementCount
            For lngIndex = 1 To lngItemCount
                strStatement = .GetMessageStatement(lngIndex)

                intResult = .SetMessageStatement(lngIndex, strStatement)
            Next lngIndex

            ' Test Capillary flow functions

            With .CapFlow
                .SetAutoComputeEnabled(False)
                .SetBackPressure(2000, MwtWinDll.MWCapillaryFlowClass.uprUnitsPressureConstants.uprPsi)
                .SetColumnLength(40, MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnCM)
                .SetColumnID(50, MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnMicrons)
                .SetSolventViscosity(0.0089, MwtWinDll.MWCapillaryFlowClass.uviUnitsViscosityConstants.uviPoise)
                .SetInterparticlePorosity(0.33)
                .SetParticleDiameter(2, MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnMicrons)
                .SetAutoComputeEnabled(True)

                objResults.AppendText = ""
                objResults.AppendText = "Check capillary flow calcs"
                objResults.AppendText = "Linear Velocity: " & .ComputeLinearVelocity(MwtWinDll.MWCapillaryFlowClass.ulvUnitsLinearVelocityConstants.ulvCmPerSec)
                objResults.AppendText = "Vol flow rate:   " & .ComputeVolFlowRate(MwtWinDll.MWCapillaryFlowClass.ufrUnitsFlowRateConstants.ufrNLPerMin) & "  (newly computed)"

                objResults.AppendText = "Vol flow rate:   " & .GetVolFlowRate
                objResults.AppendText = "Back pressure:   " & .ComputeBackPressure(MwtWinDll.MWCapillaryFlowClass.uprUnitsPressureConstants.uprPsi)
                objResults.AppendText = "Column Length:   " & .ComputeColumnLength(MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnCM)
                objResults.AppendText = "Column ID:       " & .ComputeColumnID(MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnMicrons)
                objResults.AppendText = "Column Volume:   " & .ComputeColumnVolume(MwtWinDll.MWCapillaryFlowClass.uvoUnitsVolumeConstants.uvoNL)
                objResults.AppendText = "Dead time:       " & .ComputeDeadTime(MwtWinDll.MWCapillaryFlowClass.utmUnitsTimeConstants.utmSeconds)

                objResults.AppendText = ""

                objResults.AppendText = "Repeat Computations, but in a different order (should give same results)"
                objResults.AppendText = "Vol flow rate:   " & .ComputeVolFlowRate(MwtWinDll.MWCapillaryFlowClass.ufrUnitsFlowRateConstants.ufrNLPerMin)
                objResults.AppendText = "Column ID:       " & .ComputeColumnID(MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnMicrons)
                objResults.AppendText = "Back pressure:   " & .ComputeBackPressure(MwtWinDll.MWCapillaryFlowClass.uprUnitsPressureConstants.uprPsi)
                objResults.AppendText = "Column Length:   " & .ComputeColumnLength(MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnCM)

                objResults.AppendText = ""

                objResults.AppendText = "Old Dead time: " & .GetDeadTime(MwtWinDll.MWCapillaryFlowClass.utmUnitsTimeConstants.utmMinutes)

                .SetAutoComputeMode(MwtWinDll.MWCapillaryFlowClass.acmAutoComputeModeConstants.acmVolFlowrateUsingDeadTime)

                .SetDeadTime(25, MwtWinDll.MWCapillaryFlowClass.utmUnitsTimeConstants.utmMinutes)
                objResults.AppendText = "Dead time is now 25.0 minutes"

                objResults.AppendText = "Vol flow rate: " & .GetVolFlowRate(MwtWinDll.MWCapillaryFlowClass.ufrUnitsFlowRateConstants.ufrNLPerMin) & " (auto-computed since AutoComputeMode = acmVolFlowrateUsingDeadTime)"

                ' Confirm that auto-compute worked

                objResults.AppendText = "Vol flow rate: " & .ComputeVolFlowRateUsingDeadTime(MwtWinDll.MWCapillaryFlowClass.ufrUnitsFlowRateConstants.ufrNLPerMin, dblNewPressure, MwtWinDll.MWCapillaryFlowClass.uprUnitsPressureConstants.uprPsi) & "  (confirmation of computed volumetric flow rate)"
                objResults.AppendText = "New pressure: " & dblNewPressure

                objResults.AppendText = ""

                ' Can set a new back pressure, but since auto-compute is on, and the
                '  auto-compute mode is acmVolFlowRateUsingDeadTime, the pressure will get changed back to
                '  the pressure needed to give a vol flow rate matching the dead time
                .SetBackPressure(2000)
                objResults.AppendText = "Pressure set to 2000 psi, but auto-compute mode is acmVolFlowRateUsingDeadTime, so pressure"
                objResults.AppendText = "  was automatically changed back to pressure needed to give vol flow rate matching dead time"
                objResults.AppendText = "Pressure is now: " & .GetBackPressure(MwtWinDll.MWCapillaryFlowClass.uprUnitsPressureConstants.uprPsi) & " psi (thus, not 2000 as one might expect)"

                .SetAutoComputeMode(MwtWinDll.MWCapillaryFlowClass.acmAutoComputeModeConstants.acmVolFlowrate)
                objResults.AppendText = "Changed auto-compute mode to acmVolFlowrate.  Can now set pressure to 2000 and it will stick; plus, vol flow rate gets computed."

                .SetBackPressure(2000, MwtWinDll.MWCapillaryFlowClass.uprUnitsPressureConstants.uprPsi)

                ' Calling GetVolFlowRate will get the new computed vol flow rate (since auto-compute is on)
                objResults.AppendText = "Vol flow rate: " & .GetVolFlowRate

                .SetMassRateSampleMass(1000)
                .SetMassRateConcentration(1, MwtWinDll.MWCapillaryFlowClass.ucoUnitsConcentrationConstants.ucoMicroMolar)
                .SetMassRateVolFlowRate(600, MwtWinDll.MWCapillaryFlowClass.ufrUnitsFlowRateConstants.ufrNLPerMin)
                .SetMassRateInjectionTime(5, MwtWinDll.MWCapillaryFlowClass.utmUnitsTimeConstants.utmMinutes)

                objResults.AppendText = "Mass flow rate: " & .GetMassFlowRate(MwtWinDll.MWCapillaryFlowClass.umfMassFlowRateConstants.umfFmolPerSec) & " fmol/sec"
                objResults.AppendText = "Moles injected: " & .GetMassRateMolesInjected(MwtWinDll.MWCapillaryFlowClass.umaMolarAmountConstants.umaFemtoMoles) & " fmoles"

                .SetMassRateSampleMass(1234)
                .SetMassRateConcentration(1, MwtWinDll.MWCapillaryFlowClass.ucoUnitsConcentrationConstants.ucongperml)

                objResults.AppendText = "Computing mass flow rate for compound weighing 1234 g/mol and at 1 ng/mL concentration                "
                objResults.AppendText = "Mass flow rate: " & .GetMassFlowRate(MwtWinDll.MWCapillaryFlowClass.umfMassFlowRateConstants.umfAmolPerMin) & " amol/min"
                objResults.AppendText = "Moles injected: " & .GetMassRateMolesInjected(MwtWinDll.MWCapillaryFlowClass.umaMolarAmountConstants.umaFemtoMoles) & " fmoles"

                .SetExtraColumnBroadeningLinearVelocity(4, MwtWinDll.MWCapillaryFlowClass.ulvUnitsLinearVelocityConstants.ulvCmPerMin)
                .SetExtraColumnBroadeningDiffusionCoefficient(0.0003, MwtWinDll.MWCapillaryFlowClass.udcDiffusionCoefficientConstants.udcCmSquaredPerMin)
                .SetExtraColumnBroadeningOpenTubeLength(5, MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnCM)
                .SetExtraColumnBroadeningOpenTubeID(250, MwtWinDll.MWCapillaryFlowClass.ulnUnitsLengthConstants.ulnMicrons)
                .SetExtraColumnBroadeningInitialPeakWidthAtBase(30, MwtWinDll.MWCapillaryFlowClass.utmUnitsTimeConstants.utmSeconds)

                objResults.AppendText = "Computing broadening for 30 second wide peak through a 250 um open tube that is 5 cm long (4 cm/min)"
                objResults.AppendText = .GetExtraColumnBroadeningResultantPeakWidth(MwtWinDll.MWCapillaryFlowClass.utmUnitsTimeConstants.utmSeconds).ToString

            End With
        End With


        Dim udtFragSpectrumOptions As MwtWinDll.MWPeptideClass.udtFragmentationSpectrumOptionsType
        udtFragSpectrumOptions.Initialize()

        Dim udtFragSpectrum() As MwtWinDll.MWPeptideClass.udtFragmentationSpectrumDataType
        Dim lngIonCount As Integer
        Dim strNewSeq As String


        With mMwtWin.Peptide

            .SetSequence("K.ACYEFGHRKACYEFGHRK.G", MwtWinDll.MWPeptideClass.ntgNTerminusGroupConstants.ntgHydrogen, MwtWinDll.MWPeptideClass.ctgCTerminusGroupConstants.ctgHydroxyl, False, True)
            '.SetSequence("K.ACYEFGHRKACYEFGHRK.G")

            ' Can change the terminii to various standard groups
            .SetNTerminusGroup(MwtWinDll.MWPeptideClass.ntgNTerminusGroupConstants.ntgCarbamyl)
            .SetCTerminusGroup(MwtWinDll.MWPeptideClass.ctgCTerminusGroupConstants.ctgAmide)

            ' Can change the terminii to any desired elements
            .SetNTerminus("C2OH3") ' Acetyl group
            .SetCTerminus("NH2") ' Amide group

            ' Can mark third residue, Tyr, as phorphorylated
            .SetResidue(3, "Tyr", True, True)

            ' Can define that the * modification equals 15
            .SetModificationSymbol("*", 15, False, "")

            strNewSeq = "Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg*-Lys-Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg-Lys"
            objResults.AppendText = strNewSeq
            .SetSequence(strNewSeq)

            .SetSequence("K.TQPLE*VK.-", MwtWinDll.MWPeptideClass.ntgNTerminusGroupConstants.ntgHydrogenPlusProton, MwtWinDll.MWPeptideClass.ctgCTerminusGroupConstants.ctgHydroxyl, False, True)

            objResults.AppendText = .GetSequence(True, False, True, False)
            objResults.AppendText = .GetSequence(False, True, False, False)
            objResults.AppendText = .GetSequence(True, False, True, True)

            .SetCTerminusGroup(MwtWinDll.MWPeptideClass.ctgCTerminusGroupConstants.ctgNone)
            objResults.AppendText = .GetSequence(True, False, True, True)

            udtFragSpectrumOptions = .GetFragmentationSpectrumOptions()

            udtFragSpectrumOptions.DoubleChargeIonsShow = True
            udtFragSpectrumOptions.DoubleChargeIonsThreshold = 300
            udtFragSpectrumOptions.IntensityOptions.BYIonShoulder = 0

            udtFragSpectrumOptions.TripleChargeIonsShow = True
            udtFragSpectrumOptions.TripleChargeIonsThreshold = 400

            udtFragSpectrumOptions.IonTypeOptions(MwtWinDll.MWPeptideClass.itIonTypeConstants.itAIon).ShowIon = True

            .SetFragmentationSpectrumOptions(udtFragSpectrumOptions)

            lngIonCount = .GetFragmentationMasses(udtFragSpectrum)

        End With

        MakeDataSet(lngIonCount, udtFragSpectrum)
        dgDataGrid.SetDataBinding(myDataSet, "DataTable1")

        objResults.AppendText = String.Empty

        Dim intSuccess As Short
        Dim strResults As String
        Dim ConvolutedMSData2D(,) As Double
        Dim ConvolutedMSDataCount As Integer

        With mMwtWin
            intSuccess = .ComputeIsotopicAbundances("C1255H43O2Cl", 1, strResults, ConvolutedMSData2D, ConvolutedMSDataCount)
            objResults.AppendText = strResults
        End With

    End Sub

    Private Sub TestTrypticName()
        Const DIM_CHUNK As Short = 1000

        Const ITERATIONS_TO_RUN As Short = 5
        Const MIN_PROTEIN_LENGTH As Short = 50
        Const MAX_PROTEIN_LENGTH As Short = 200
        Const POSSIBLE_RESIDUES As String = "ACDEFGHIKLMNPQRSTVWY"

        Dim lngMultipleIteration As Integer

        Dim strProtein, strPeptideResidues As String
        Dim lngResidueStart, lngResidueEnd As Integer
        Dim strPeptideNameMwtWin() As String
        Dim strPeptideNameIcr2ls() As String
        Dim strPeptideName As String

        Dim lngMwtWinResultCount, lngIcr2lsResultCount As Integer
        Dim lngMwtWinDimCount, lngICR2lsDimCount As Integer
        Dim lngIndex As Integer
        Dim lngResidueRand, lngProteinLengthRand As Single
        Dim strNewResidue As String

        Dim lngStartTime, lngStopTime As Integer
        Dim lngMwtWinWorkTime As Integer
        Dim strPeptideFragMwtWin As String
        Dim lngMatchCount As Integer
        Dim blnDifferenceFound As Boolean

        Dim objResults As New frmTextbrowser

        lblProgress.Text = String.Empty

        lngMwtWinDimCount = DIM_CHUNK
        ReDim strPeptideNameMwtWin(lngMwtWinDimCount)

        Me.Cursor = System.Windows.Forms.Cursors.WaitCursor

        objResults.Show()
        objResults.SetText = String.Empty

        ''    Dim lngIcr2lsWorkTime As Long
        ''    Dim lngIcr2lsTime As Long
        ''    strPeptideFragIcr2ls As String
        ''    lngICR2lsDimCount = DIM_CHUNK
        ''    ReDim strPeptideNameIcr2ls(lngICR2lsDimCount)
        ''
        ''    Dim ICRTools As Object
        ''
        ''    Set ICRTools = CreateObject("ICR2LS.ICR2LScls")
        ''
        ''    objResults.AppendText = "ICR2ls Version: " & ICRTools.ICR2LSversion

        'strProtein = "MGNISFLTGGNPSSPQSIAESIYQLENTSVVFLSAWQRTTPDFQRAARASQEAMLHLDHIVNEIMRNRDQLQADGTYTGSQLEGLLNISRAVSVSPVTRAEQDDLANYGPGNGVLPSAGSSISMEKLLNKIKHRRTNSANFRIGASGEHIFIIGVDKPNRQPDSIVEFIVGDFCQHCSDIAALI"

        ' Bigger protein
        strProtein = "MMKANVTKKTLNEGLGLLERVIPSRSSNPLLTALKVETSEGGLTLSGTNLEIDLSCFVPAEVQQPENFVVPAHLFAQIVRNLGGELVELELSGQELSVRSGGSDFKLQTGDIEAYPPLSFPAQADVSLDGGELSRAFSSVRYAASNEAFQAVFRGIKLEHHGESARVVASDGYRVAIRDFPASGDGKNLIIPARSVDELIRVLKDGEARFTYGDGMLTVTTDRVKMNLKLLDGDFPDYERVIPKDIKLQVTLPATALKEAVNRVAVLADKNANNRVEFLVSEGTLRLAAEGDYGRAQDTLSVTQGGTEQAMSLAFNARHVLDALGPIDGDAELLFSGSTSPAIFRARRWGRRVYGGHGHAARLRGLLRPLRGMSALAHHPESSPPLEPRPEFA"

        objResults.AppendText = "Testing GetTrypticNameMultipleMatches() function"
        objResults.AppendText = "MatchList for NL: " & mMwtWin.Peptide.GetTrypticNameMultipleMatches(strProtein, "NL", lngMatchCount)
        objResults.AppendText = "MatchCount = " & lngMatchCount

        objResults.AppendText = String.Empty
        objResults.AppendText = "Testing GetTrypticPeptideByFragmentNumber function"
        For lngIndex = 1 To 43
            strPeptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(strProtein, CShort(lngIndex), lngResidueStart, lngResidueEnd)
            ''        strPeptideFragIcr2ls = ICRTools.TrypticPeptide(strProtein, CInt(lngIndex))
            ''
            ''        Debug.Assert strPeptideFragMwtWin = strPeptideFragIcr2ls

            If Len(strPeptideFragMwtWin) > 1 Then
                ' Make sure lngResidueStart and lngResidueEnd are correct
                ' Do this using .GetTrypticNameMultipleMatches()
                strPeptideName = mMwtWin.Peptide.GetTrypticNameMultipleMatches(strProtein, Mid(strProtein, lngResidueStart, lngResidueEnd - lngResidueStart + 1))
                System.Diagnostics.Debug.Assert(InStr(strPeptideName, "t" & Trim(Str(lngIndex))) > 0, "")
            End If
        Next lngIndex
        objResults.AppendText = "Check of GetTrypticPeptideByFragmentNumber Complete"
        objResults.AppendText = String.Empty


        objResults.AppendText = "Test tryptic digest of: " & strProtein
        lngIndex = 1
        Do
            strPeptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(strProtein, CShort(lngIndex), lngResidueStart, lngResidueEnd)
            objResults.AppendText = "Tryptic fragment " & Trim(CStr(lngIndex)) & ": " & strPeptideFragMwtWin
            lngIndex = lngIndex + 1
        Loop While Len(strPeptideFragMwtWin) > 0


        objResults.AppendText = String.Empty
        Randomize()
        For lngMultipleIteration = 1 To ITERATIONS_TO_RUN
            ' Generate random protein
            lngProteinLengthRand = Int((MAX_PROTEIN_LENGTH - MIN_PROTEIN_LENGTH + 1) * Rnd() + MIN_PROTEIN_LENGTH)

            strProtein = ""
            For lngResidueRand = 1 To lngProteinLengthRand
                strNewResidue = Mid(POSSIBLE_RESIDUES, CInt(Int(Len(POSSIBLE_RESIDUES)) * Rnd() + 1), 1)
                strProtein = strProtein & strNewResidue
            Next lngResidueRand

            objResults.AppendText = "Iteration: " & lngMultipleIteration & " = " & strProtein

            lngMwtWinResultCount = 0
            System.Diagnostics.Debug.Write("Starting residue is ")
            lngStartTime = GetTickCount()
            For lngResidueStart = 1 To Len(strProtein)
                If lngResidueStart Mod 10 = 0 Then
                    System.Diagnostics.Debug.Write(lngResidueStart & ", ")
                    System.Windows.Forms.Application.DoEvents()
                End If

                For lngResidueEnd = 1 To Len(strProtein) - lngResidueStart
                    If lngResidueEnd - lngResidueStart > 50 Then
                        Exit For
                    End If

                    strPeptideResidues = Mid(strProtein, lngResidueStart, lngResidueEnd)
                    strPeptideNameMwtWin(lngMwtWinResultCount) = mMwtWin.Peptide.GetTrypticName(strProtein, strPeptideResidues, 0, 0, True)

                    lngMwtWinResultCount = lngMwtWinResultCount + 1
                    If lngMwtWinResultCount > lngMwtWinDimCount Then
                        lngMwtWinDimCount = lngMwtWinDimCount + DIM_CHUNK
                        ReDim Preserve strPeptideNameMwtWin(lngMwtWinDimCount)
                    End If

                Next lngResidueEnd
            Next lngResidueStart
            lngStopTime = GetTickCount()
            lngMwtWinWorkTime = lngStopTime - lngStartTime
            System.Diagnostics.Debug.WriteLine("")
            System.Diagnostics.Debug.WriteLine("MwtWin time (" & lngMwtWinResultCount & " peptides) = " & lngMwtWinWorkTime & " msec")

            ''        lngIcr2lsResultCount = 0
            ''        Debug.Print "Starting residue is ";
            ''        lngStartTime = GetTickCount()
            ''        For lngResidueStart = 1 To Len(strProtein)
            ''            If lngResidueStart Mod 10 = 0 Then
            ''                Debug.Print lngResidueStart & ", ";
            ''                DoEvents
            ''            End If
            ''            ' Use DoEvents on every iteration since Icr2ls is quite slow
            ''            DoEvents
            ''
            ''            For lngResidueEnd = 1 To Len(strProtein) - lngResidueStart
            ''                If lngResidueEnd - lngResidueStart > 50 Then
            ''                    Exit For
            ''                End If
            ''
            ''                strPeptideResidues = Mid(strProtein, lngResidueStart, lngResidueEnd)
            ''                strPeptideNameIcr2ls(lngIcr2lsResultCount) = ICRTools.TrypticName(strProtein, strPeptideResidues)
            ''
            ''                lngIcr2lsResultCount = lngIcr2lsResultCount + 1
            ''                If lngIcr2lsResultCount > lngICR2lsDimCount Then
            ''                    lngICR2lsDimCount = lngICR2lsDimCount + DIM_CHUNK
            ''                    ReDim Preserve strPeptideNameIcr2ls(lngICR2lsDimCount)
            ''                End If
            ''            Next lngResidueEnd
            ''        Next lngResidueStart
            ''        lngStopTime = GetTickCount()
            ''        lngIcr2lsWorkTime = lngStopTime - lngStartTime
            ''        Debug.Print ""
            ''        Debug.Print "Icr2ls time (" & lngMwtWinResultCount & " peptides) = " & lngIcr2lsWorkTime & " msec"

            ''        ' Check that results match
            ''        For lngIndex = 0 To lngMwtWinResultCount - 1
            ''            If Left(strPeptideNameMwtWin(lngIndex), 1) = "t" Then
            ''                If Val(Right(strPeptideNameMwtWin(lngIndex), 1)) < 5 Then
            ''                    ' Icr2LS does not return the correct name when strPeptideResidues contains 5 or more tryptic peptides
            ''                    If strPeptideNameMwtWin(lngIndex) <> strPeptideNameIcr2ls(lngIndex) Then
            ''                        objResults.AppendText = "Difference found, index = " & lngIndex & ", " & strPeptideNameMwtWin(lngIndex) & " vs. " & strPeptideNameIcr2ls(lngIndex)
            ''                        blnDifferenceFound = True
            ''                    End If
            ''                End If
            ''            Else
            ''                If strPeptideNameMwtWin(lngIndex) <> strPeptideNameIcr2ls(lngIndex) Then
            ''                    objResults.AppendText = "Difference found, index = " & lngIndex & ", " & strPeptideNameMwtWin(lngIndex) & " vs. " & strPeptideNameIcr2ls(lngIndex)
            ''                    blnDifferenceFound = True
            ''                End If
            ''            End If
            ''        Next lngIndex

        Next lngMultipleIteration

        objResults.AppendText = "Check of Tryptic Sequence functions Complete"

        Me.Cursor = System.Windows.Forms.Cursors.Default
    End Sub

    Private Sub UpdateResultsForCompound(ByRef objCompound As MwtWinDll.MWCompoundClass)
        With objCompound
            If .ErrorDescription = "" Then
                txtFormula.Text = .FormulaCapitalized
                FindMass()
            Else
                lblStatus.Text = .ErrorDescription
            End If
        End With

    End Sub

    Private Sub cboStdDevMode_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboStdDevMode.SelectedIndexChanged
        Select Case cboStdDevMode.SelectedIndex
            Case 1
                mMwtWin.StdDevMode = MwtWinDll.MWElementAndMassRoutines.smStdDevModeConstants.smScientific
            Case 2
                mMwtWin.StdDevMode = MwtWinDll.MWElementAndMassRoutines.smStdDevModeConstants.smDecimal
            Case Else
                mMwtWin.StdDevMode = MwtWinDll.MWElementAndMassRoutines.smStdDevModeConstants.smShort
        End Select

    End Sub

    Private Sub cboWeightMode_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboWeightMode.SelectedIndexChanged
        Select Case cboWeightMode.SelectedIndex
            Case 1
                mMwtWin.SetElementMode(MwtWinDll.MWElementAndMassRoutines.emElementModeConstants.emIsotopicMass)
            Case 2
                mMwtWin.SetElementMode(MwtWinDll.MWElementAndMassRoutines.emElementModeConstants.emIntegerMass)
            Case Else
                mMwtWin.SetElementMode(MwtWinDll.MWElementAndMassRoutines.emElementModeConstants.emAverageMass)
        End Select

    End Sub

    Private Sub cmdClose_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdClose.Click
        mMwtWin = Nothing
        Me.Close()
        End
    End Sub

    Private Sub cmdConvertToEmpirical_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdConvertToEmpirical.Click

        lblProgress.Text = String.Empty

        With mMwtWin.Compound
            .Formula = txtFormula.Text
            .ConvertToEmpirical()
        End With

        UpdateResultsForCompound(mMwtWin.Compound)

    End Sub

    Private Sub cmdExpandAbbreviations_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdExpandAbbreviations.Click

        lblProgress.Text = String.Empty

        With mMwtWin.Compound
            .Formula = txtFormula.Text
            .ExpandAbbreviations()
        End With

        UpdateResultsForCompound(mMwtWin.Compound)
    End Sub

    Private Sub cmdFindMass_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdFindMass.Click
        FindMass()
        FindPercentComposition()
    End Sub

    Private Sub cmdTestFunctions_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdTestFunctions.Click
        TestAccessFunctions()
    End Sub

    Private Sub cmdTestGetTrypticName_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdTestGetTrypticName.Click
        TestTrypticName()
    End Sub

    Private Sub rtfFormula_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rtfFormula.TextChanged
        txtRTFSource.Text = rtfFormula.Rtf
    End Sub

    Private Sub chkShowRTFSource_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowRTFSource.CheckedChanged
        txtRTFSource.Visible = chkShowRTFSource.Checked
    End Sub

    Private Sub mMwtWin_ProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single) Handles mMwtWin.ProgressChanged
        Static dtLastUpdate As System.DateTime

        lblProgress.Text = mMwtWin.ProgressStepDescription & "; " & percentComplete.ToString("0.0") & "% complete"

        If System.DateTime.Now.Subtract(dtLastUpdate).TotalMilliseconds > 100 Then
            dtLastUpdate = System.DateTime.Now
            Application.DoEvents()
        End If
    End Sub

    Private Sub mMwtWin_ProgressComplete() Handles mMwtWin.ProgressComplete
        lblProgress.Text = mMwtWin.ProgressStepDescription & "; 100% complete"
        Application.DoEvents()
    End Sub

    Private Sub mMwtWin_ProgressReset() Handles mMwtWin.ProgressReset
        lblProgress.Text = mMwtWin.ProgressStepDescription
        Application.DoEvents()
    End Sub
End Class