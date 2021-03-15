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
            this.txtData = new System.Windows.Forms.TextBox();
            this.MainMenuControl = new System.Windows.Forms.MainMenu(this.components);
            this.mnuFile = new System.Windows.Forms.MenuItem();
            this.mnuFileExit = new System.Windows.Forms.MenuItem();
            this.mnuEdit = new System.Windows.Forms.MenuItem();
            this.mnuEditCut = new System.Windows.Forms.MenuItem();
            this.mnuEditCopy = new System.Windows.Forms.MenuItem();
            this.mnuEditPaste = new System.Windows.Forms.MenuItem();
            this.mnuEditSep1 = new System.Windows.Forms.MenuItem();
            this.mnuEditFontSizeDecrease = new System.Windows.Forms.MenuItem();
            this.mnuEditFontSizeIncrease = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // txtData
            // 
            this.txtData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtData.Location = new System.Drawing.Point(0, 0);
            this.txtData.Multiline = true;
            this.txtData.Name = "txtData";
            this.txtData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtData.Size = new System.Drawing.Size(488, 316);
            this.txtData.TabIndex = 0;
            this.txtData.WordWrap = false;
            // 
            // MainMenuControl
            // 
            this.MainMenuControl.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuFile,
            this.mnuEdit});
            // 
            // mnuFile
            // 
            this.mnuFile.Index = 0;
            this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuFileExit});
            this.mnuFile.Text = "&File";
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Index = 0;
            this.mnuFileExit.Text = "E&xit";
            this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
            // 
            // mnuEdit
            // 
            this.mnuEdit.Index = 1;
            this.mnuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuEditCut,
            this.mnuEditCopy,
            this.mnuEditPaste,
            this.mnuEditSep1,
            this.mnuEditFontSizeDecrease,
            this.mnuEditFontSizeIncrease});
            this.mnuEdit.Text = "&Edit";
            // 
            // mnuEditCut
            // 
            this.mnuEditCut.Index = 0;
            this.mnuEditCut.Text = "Cu&t";
            this.mnuEditCut.Click += new System.EventHandler(this.mnuEditCut_Click);
            // 
            // mnuEditCopy
            // 
            this.mnuEditCopy.Index = 1;
            this.mnuEditCopy.Text = "&Copy";
            this.mnuEditCopy.Click += new System.EventHandler(this.mnuEditCopy_Click);
            // 
            // mnuEditPaste
            // 
            this.mnuEditPaste.Index = 2;
            this.mnuEditPaste.Text = "&Paste";
            this.mnuEditPaste.Click += new System.EventHandler(this.mnuEditPaste_Click);
            // 
            // mnuEditSep1
            // 
            this.mnuEditSep1.Index = 3;
            this.mnuEditSep1.Text = "-";
            // 
            // mnuEditFontSizeDecrease
            // 
            this.mnuEditFontSizeDecrease.Index = 4;
            this.mnuEditFontSizeDecrease.Shortcut = System.Windows.Forms.Shortcut.F3;
            this.mnuEditFontSizeDecrease.Text = "Decrease Font Size";
            this.mnuEditFontSizeDecrease.Click += new System.EventHandler(this.mnuEditFontSizeDecrease_Click);
            // 
            // mnuEditFontSizeIncrease
            // 
            this.mnuEditFontSizeIncrease.Index = 5;
            this.mnuEditFontSizeIncrease.Shortcut = System.Windows.Forms.Shortcut.F4;
            this.mnuEditFontSizeIncrease.Text = "Increase Font Size";
            this.mnuEditFontSizeIncrease.Click += new System.EventHandler(this.mnuEditFontSizeIncrease_Click);
            // 
            // frmTextbrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 314);
            this.Controls.Add(this.txtData);
            this.Menu = this.MainMenuControl;
            this.Name = "frmTextbrowser";
            this.Text = "frmTextbrowser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.MainMenu MainMenuControl;
        private System.Windows.Forms.MenuItem mnuFile;
        private System.Windows.Forms.MenuItem mnuFileExit;
        private System.Windows.Forms.MenuItem mnuEdit;
        private System.Windows.Forms.MenuItem mnuEditCut;
        private System.Windows.Forms.MenuItem mnuEditCopy;
        private System.Windows.Forms.MenuItem mnuEditPaste;
        private System.Windows.Forms.MenuItem mnuEditSep1;
        private System.Windows.Forms.MenuItem mnuEditFontSizeDecrease;
        private System.Windows.Forms.MenuItem mnuEditFontSizeIncrease;
    }
}