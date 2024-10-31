using System;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WrldBxInstaller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnInstall_Click(object sender, EventArgs e)
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

                progressBar.Visible = true;
                progressBar.Value = 0;

                string sourceSolutionPath = Path.Combine(Application.StartupPath, "..\\..\\WrldBxScript");
               
                await Task.Delay(200);

                progressBar.Value = 30;

                await Task.Run(() => CopyDirectory(sourceSolutionPath, installPath));

                string exePath = Path.Combine(installPath, "WrldBxScript\\bin\\Release\\WrldBxScript.exe"); // Path to the executable in the copied directory
                progressBar.Value = 90;

                
                string envVariableName = "wrldbx";
                Environment.SetEnvironmentVariable(envVariableName, installPath, EnvironmentVariableTarget.User);
                progressBar.Value = 100;
                MessageBox.Show($"WrldBx Script has been installed to {installPath}.\nEnvironment variable '{envVariableName}' has been set.", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during installation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false; // Hide the progress bar after installation completes
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

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Create all directories in the destination
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDir, destinationDir));
            }

            // Copy all files to the destination directory
            foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(sourceDir, destinationDir), true);
            }
        }


    }
}
