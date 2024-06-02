using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace proNETprojekt
{
    public partial class Form1 : Form
    {
        private string configFilePath = "config.xml";

        public Form1()
        {
            InitializeComponent();
            LoadEncryptionConfig();

            this.FormClosing += Form1_FormClosing;
        }

        private void LoadEncryptionConfig()
        {
            if (File.Exists(configFilePath))
            {
                EncryptionConfig config = EncryptionConfig.LoadConfigFromFile(configFilePath);
                EncryptionKeyTextBox.Text = config.EncryptionKey;
                InitializationVectorTextBox.Text = config.InitializationVector;
            }
        }

        private void SaveEncryptionConfig()
        {
            EncryptionConfig config = new EncryptionConfig
            {
                EncryptionKey = EncryptionKeyTextBox.Text,
                InitializationVector = InitializationVectorTextBox.Text
            };
            EncryptionConfig.SaveConfigToFile(configFilePath, config);
        }

        private async void EncryptButton_Click(object sender, EventArgs e)
        {
            var files = FileListBox.SelectedItems.Cast<string>().ToList();
            var encryptionConfig = GetEncryptionConfig();

            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    EncryptFile(file, encryptionConfig);
                }
            });

            MessageBox.Show("Encryption completed successfully.");
        }

        private async void DecryptButton_Click(object sender, EventArgs e)
        {
            var files = FileListBox.SelectedItems.Cast<string>().ToList();
            var encryptionConfig = GetEncryptionConfig();

            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    DecryptFile(file, encryptionConfig);
                }
            });

            MessageBox.Show("Decryption completed successfully.");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveEncryptionConfig();
        }

        private EncryptionConfig GetEncryptionConfig()
        {
            return new EncryptionConfig
            {
                EncryptionKey = EncryptionKeyTextBox.Text,
                InitializationVector = InitializationVectorTextBox.Text
            };
        }

        private void EncryptFile(string filePath, EncryptionConfig encryptionConfig)
        {
            byte[] encryptedBytes;
            using (var inputStream = File.OpenRead(filePath))
            using (var outputStream = File.OpenWrite(filePath + ".encrypted"))
            {
                encryptedBytes = Encrypt(inputStream, encryptionConfig);
                outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            }
        }

        private void DecryptFile(string encryptedFilePath, EncryptionConfig encryptionConfig)
        {
            byte[] decryptedBytes;
            using (var inputStream = File.OpenRead(encryptedFilePath))
            using (var outputStream = File.OpenWrite(Path.Combine(Path.GetDirectoryName(encryptedFilePath), Path.GetFileNameWithoutExtension(encryptedFilePath)) + ".decrypted"))
            {
                decryptedBytes = Decrypt(inputStream, encryptionConfig);
                outputStream.Write(decryptedBytes, 0, decryptedBytes.Length);
            }
        }

        private byte[] Encrypt(Stream inputStream, EncryptionConfig encryptionConfig)
        {
            using (var aes = new System.Security.Cryptography.AesCryptoServiceProvider())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionConfig.EncryptionKey);
                aes.IV = Encoding.UTF8.GetBytes(encryptionConfig.InitializationVector);
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new System.Security.Cryptography.CryptoStream(ms, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        inputStream.CopyTo(cs);
                    }
                    return ms.ToArray();
                }
            }
        }

        private byte[] Decrypt(Stream inputStream, EncryptionConfig encryptionConfig)
        {
            using (var aes = new System.Security.Cryptography.AesCryptoServiceProvider())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionConfig.EncryptionKey);
                aes.IV = Encoding.UTF8.GetBytes(encryptionConfig.InitializationVector);
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        inputStream.CopyTo(cs);
                    }
                    return ms.ToArray();
                }
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileListBox.Items.Clear();
                FileListBox.Items.AddRange(openFileDialog.FileNames);
            }
        }

        private async void SendToServerButton_Click(object sender, EventArgs e)
        {
            var files = FileListBox.SelectedItems.Cast<string>().ToList();

            foreach (var file in files)
            {
                await SendFileToServer(file);
            }
        }

        private async Task SendFileToServer(string filePath)
        {
            try
            {
                using (var httpClient = new HttpClient())
                using (var fileStream = File.OpenRead(filePath))
                {
                    var content = new StreamContent(fileStream);
                    var response = await httpClient.PostAsync("https://your-server-url.com/upload", content);
                    response.EnsureSuccessStatusCode();
                }

                MessageBox.Show("File sent to server successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending file to server: " + ex.Message);
            }
        }
    }
}
