namespace BizHawk.Client.EmuHawk
{
    partial class AmstradCpcCoreEmulationSettings
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
			this.OkBtn = new System.Windows.Forms.Button();
			this.CancelBtn = new System.Windows.Forms.Button();
			this.label4 = new BizHawk.WinForms.Controls.LocLabelEx();
			this.MachineSelectionComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new BizHawk.WinForms.Controls.LocLabelEx();
			this.determEmucheckBox1 = new System.Windows.Forms.CheckBox();
			this.lblAutoLoadText = new BizHawk.WinForms.Controls.LocLabelEx();
			this.autoLoadcheckBox1 = new System.Windows.Forms.CheckBox();
			this.lblBorderInfo = new BizHawk.WinForms.Controls.LocLabelEx();
			this.label2 = new BizHawk.WinForms.Controls.LocLabelEx();
			this.borderTypecomboBox1 = new System.Windows.Forms.ComboBox();
			this.textBoxMachineNotes = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// OkBtn
			// 
			this.OkBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkBtn.Location = new System.Drawing.Point(310, 432);
			this.OkBtn.Name = "OkBtn";
			this.OkBtn.Size = new System.Drawing.Size(60, 23);
			this.OkBtn.TabIndex = 3;
			this.OkBtn.Text = "&OK";
			this.OkBtn.UseVisualStyleBackColor = true;
			this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = new System.Drawing.Point(376, 432);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = new System.Drawing.Size(60, 23);
			this.CancelBtn.TabIndex = 4;
			this.CancelBtn.Text = "&Cancel";
			this.CancelBtn.UseVisualStyleBackColor = true;
			this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(12, 46);
			this.label4.Name = "label4";
			this.label4.Text = "Emulated Machine:";
			// 
			// MachineSelectionComboBox
			// 
			this.MachineSelectionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MachineSelectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.MachineSelectionComboBox.FormattingEnabled = true;
			this.MachineSelectionComboBox.Location = new System.Drawing.Point(12, 62);
			this.MachineSelectionComboBox.Name = "MachineSelectionComboBox";
			this.MachineSelectionComboBox.Size = new System.Drawing.Size(424, 21);
			this.MachineSelectionComboBox.TabIndex = 13;
			this.MachineSelectionComboBox.SelectionChangeCommitted += new System.EventHandler(this.MachineSelectionComboBox_SelectionChangeCommitted);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 14);
			this.label1.Name = "label1";
			this.label1.Text = "Amstrad CPC Emulation Settings";
			// 
			// determEmucheckBox1
			// 
			this.determEmucheckBox1.AutoSize = true;
			this.determEmucheckBox1.Location = new System.Drawing.Point(12, 373);
			this.determEmucheckBox1.Name = "determEmucheckBox1";
			this.determEmucheckBox1.Size = new System.Drawing.Size(135, 17);
			this.determEmucheckBox1.TabIndex = 21;
			this.determEmucheckBox1.Text = "Deterministic Emulation";
			this.determEmucheckBox1.UseVisualStyleBackColor = true;
			// 
			// lblAutoLoadText
			// 
			this.lblAutoLoadText.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblAutoLoadText.Location = new System.Drawing.Point(155, 373);
			this.lblAutoLoadText.Name = "lblAutoLoadText";
			this.lblAutoLoadText.Text = "When enabled, the tape will be started and stopped automatically whenever the " +
    "tape motor state changes";
			this.lblAutoLoadText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLoadcheckBox1
			// 
			this.autoLoadcheckBox1.AutoSize = true;
			this.autoLoadcheckBox1.Location = new System.Drawing.Point(12, 396);
			this.autoLoadcheckBox1.Name = "autoLoadcheckBox1";
			this.autoLoadcheckBox1.Size = new System.Drawing.Size(128, 17);
			this.autoLoadcheckBox1.TabIndex = 26;
			this.autoLoadcheckBox1.Text = "Auto Tape Start/Stop";
			this.autoLoadcheckBox1.UseVisualStyleBackColor = true;
			// 
			// lblBorderInfo
			// 
			this.lblBorderInfo.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblBorderInfo.Location = new System.Drawing.Point(177, 315);
			this.lblBorderInfo.Name = "lblBorderInfo";
			this.lblBorderInfo.Text = "null";
			this.lblBorderInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 315);
			this.label2.Name = "label2";
			this.label2.Text = "Rendered Border Type:";
			// 
			// borderTypecomboBox1
			// 
			this.borderTypecomboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.borderTypecomboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.borderTypecomboBox1.FormattingEnabled = true;
			this.borderTypecomboBox1.Location = new System.Drawing.Point(12, 331);
			this.borderTypecomboBox1.Name = "borderTypecomboBox1";
			this.borderTypecomboBox1.Size = new System.Drawing.Size(159, 21);
			this.borderTypecomboBox1.TabIndex = 28;
			this.borderTypecomboBox1.SelectedIndexChanged += new System.EventHandler(this.BorderTypeComboBox_SelectedIndexChanged);
			// 
			// textBoxMachineNotes
			// 
			this.textBoxMachineNotes.AcceptsReturn = true;
			this.textBoxMachineNotes.AcceptsTab = true;
			this.textBoxMachineNotes.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxMachineNotes.Location = new System.Drawing.Point(12, 90);
			this.textBoxMachineNotes.Multiline = true;
			this.textBoxMachineNotes.Name = "textBoxMachineNotes";
			this.textBoxMachineNotes.ReadOnly = true;
			this.textBoxMachineNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxMachineNotes.Size = new System.Drawing.Size(424, 222);
			this.textBoxMachineNotes.TabIndex = 31;
			// 
			// AmstradCpcCoreEmulationSettings
			// 
			this.AcceptButton = this.OkBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelBtn;
			this.ClientSize = new System.Drawing.Size(448, 467);
			this.Controls.Add(this.textBoxMachineNotes);
			this.Controls.Add(this.lblBorderInfo);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.borderTypecomboBox1);
			this.Controls.Add(this.lblAutoLoadText);
			this.Controls.Add(this.autoLoadcheckBox1);
			this.Controls.Add(this.determEmucheckBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.MachineSelectionComboBox);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.OkBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "AmstradCpcCoreEmulationSettings";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Core Emulation Settings";
			this.Load += new System.EventHandler(this.IntvControllerSettings_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button CancelBtn;
        private BizHawk.WinForms.Controls.LocLabelEx label4;
        private System.Windows.Forms.ComboBox MachineSelectionComboBox;
        private BizHawk.WinForms.Controls.LocLabelEx label1;
        private System.Windows.Forms.CheckBox determEmucheckBox1;
        private BizHawk.WinForms.Controls.LocLabelEx lblAutoLoadText;
        private System.Windows.Forms.CheckBox autoLoadcheckBox1;
        private BizHawk.WinForms.Controls.LocLabelEx lblBorderInfo;
        private BizHawk.WinForms.Controls.LocLabelEx label2;
        private System.Windows.Forms.ComboBox borderTypecomboBox1;
		private System.Windows.Forms.TextBox textBoxMachineNotes;
	}
}