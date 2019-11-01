﻿namespace MCEControl {
    partial class About {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }



        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this._labelTitle = new System.Windows.Forms.Label();
            this._linkLabelMceController = new System.Windows.Forms.LinkLabel();
            this._buttonOk = new System.Windows.Forms.Button();
            this._linkLabelKindelSystems = new System.Windows.Forms.LinkLabel();
            this._labelSummary = new System.Windows.Forms.Label();
            this.iconMcec = new System.Windows.Forms.PictureBox();
            this._label1 = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.iconMcec)).BeginInit();
            this.SuspendLayout();
            // 
            // _labelTitle
            // 
            this._labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F);
            this._labelTitle.Location = new System.Drawing.Point(206, 16);
            this._labelTitle.Name = "_labelTitle";
            this._labelTitle.Size = new System.Drawing.Size(273, 41);
            this._labelTitle.TabIndex = 0;
            this._labelTitle.Text = "MCE Controller";
            this._labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _linkLabelMceController
            // 
            this._linkLabelMceController.Location = new System.Drawing.Point(220, 144);
            this._linkLabelMceController.Name = "_linkLabelMceController";
            this._linkLabelMceController.Size = new System.Drawing.Size(272, 16);
            this._linkLabelMceController.TabIndex = 3;
            this._linkLabelMceController.TabStop = true;
            this._linkLabelMceController.Tag = "https://github.com/tig/mcec/blob/master/license.md";
            this._linkLabelMceController.Text = "License Agreement";
            this._linkLabelMceController.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelMceControllerLinkClicked);
            // 
            // _buttonOk
            // 
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonOk.Location = new System.Drawing.Point(416, 176);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(80, 27);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "OK";
            this._buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // _linkLabelKindelSystems
            // 
            this._linkLabelKindelSystems.AutoSize = true;
            this._linkLabelKindelSystems.Location = new System.Drawing.Point(220, 88);
            this._linkLabelKindelSystems.Name = "_linkLabelKindelSystems";
            this._linkLabelKindelSystems.Size = new System.Drawing.Size(145, 13);
            this._linkLabelKindelSystems.TabIndex = 1;
            this._linkLabelKindelSystems.TabStop = true;
            this._linkLabelKindelSystems.Tag = "http://www.kindel.com";
            this._linkLabelKindelSystems.Text = "© 2019 Kindel Systems, LLC.";
            this._linkLabelKindelSystems.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelCharlieLinkClicked);
            // 
            // _labelSummary
            // 
            this._labelSummary.Location = new System.Drawing.Point(220, 112);
            this._labelSummary.Name = "_labelSummary";
            this._labelSummary.Size = new System.Drawing.Size(272, 32);
            this._labelSummary.TabIndex = 2;
            this._labelSummary.Text = "MCE Controller is distributed as freeware and published as open source under the " +
    "MIT License.";
            // 
            // iconMcec
            // 
            this.iconMcec.Image = ((System.Drawing.Image)(resources.GetObject("iconMcec.Image")));
            this.iconMcec.InitialImage = null;
            this.iconMcec.Location = new System.Drawing.Point(8, 16);
            this.iconMcec.Name = "iconMcec";
            this.iconMcec.Size = new System.Drawing.Size(192, 160);
            this.iconMcec.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconMcec.TabIndex = 5;
            this.iconMcec.TabStop = false;
            // 
            // _label1
            // 
            this._label1.AutoSize = true;
            this._label1.Location = new System.Drawing.Point(51, 183);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(113, 13);
            this._label1.TabIndex = 7;
            this._label1.Text = "Icon by Guillen Design";
            this._label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(220, 64);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(78, 13);
            this.labelVersion.TabIndex = 7;
            this.labelVersion.Text = "Version a.b.c.d";
            // 
            // About
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.CancelButton = this._buttonOk;
            this.ClientSize = new System.Drawing.Size(507, 214);
            this.ControlBox = false;
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this._label1);
            this.Controls.Add(this.iconMcec);
            this.Controls.Add(this._labelSummary);
            this.Controls.Add(this._linkLabelKindelSystems);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._linkLabelMceController);
            this.Controls.Add(this._labelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.iconMcec)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }

}
