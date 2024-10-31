using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace WrldBxInstaller
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            txtInstallPath = new TextBox();
            btnInstall = new Button();
            btnBrowse = new Button();
            label1 = new Label();
            labelInfo = new Label();
            SuspendLayout();
            // 
            // txtInstallPath
            // 
            txtInstallPath.Location = new Point(12, 156);
            txtInstallPath.Name = "txtInstallPath";
            txtInstallPath.Size = new Size(300, 23);
            txtInstallPath.TabIndex = 0;
            txtInstallPath.Text = "C:\\Program Files\\WrldBx";
            // 
            // btnInstall
            // 
            btnInstall.Location = new Point(12, 185);
            btnInstall.Name = "btnInstall";
            btnInstall.Size = new Size(75, 23);
            btnInstall.TabIndex = 1;
            btnInstall.Text = "Install";
            btnInstall.UseVisualStyleBackColor = true;
            btnInstall.Click += btnInstall_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(318, 156);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(75, 23);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(122, 15);
            label1.TabIndex = 3;
            label1.Text = "WrldBx Script Installer";
            // 
            // labelInfo
            // 
            labelInfo.AutoSize = true;
            labelInfo.Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelInfo.ImageAlign = ContentAlignment.TopCenter;
            labelInfo.Location = new Point(12, 50);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(573, 80);
            labelInfo.TabIndex = 4;
            labelInfo.Text = resources.GetString("labelInfo.Text");
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            ClientSize = new Size(596, 229);
            Controls.Add(labelInfo);
            Controls.Add(label1);
            Controls.Add(btnBrowse);
            Controls.Add(btnInstall);
            Controls.Add(txtInstallPath);
            Name = "Form1";
            Text = "Installer";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.TextBox txtInstallPath;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.ProgressBar progressBar;

        #endregion
    }
}
