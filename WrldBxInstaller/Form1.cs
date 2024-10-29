using System;
using System.IO;
using System.Windows.Forms;

namespace WrldBxInstaller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            string installPath = txtInstallPath.Text;

            if (string.IsNullOrEmpty(installPath))
            {
                MessageBox.Show("Pls Enter a Valid Path, ;)");
                return;
            }

            try
            {
                if (!Directory.Exists(installPath))
                {
                    Directory.CreateDirectory(installPath);
                }

                string sourceExePath = Path.Combine(Application.StartupPath, "..\\..\\WrldBxScript\\bin\\Release\\WrldBxScript.exe");
                string destExePath = Path.Combine(installPath, "WrldBxScript.exe");

                File.Copy(sourceExePath, destExePath, true);

                string envVariableName = "wrldbx";
                Environment.SetEnvironmentVariable(envVariableName, installPath, EnvironmentVariableTarget.User);
                MessageBox.Show($"WrldBx Script has been installed to {installPath}.\nEnvironment variable '{envVariableName}' has been set.", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during installation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInstallPath.Text = folderDialog.SelectedPath;
                }
            }
        }

       
    }
}
