﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace DownloadManager
{
    public partial class DownloadForm : Form
    {
        WebClient client;
        public string Url { get; set; }
        public string FileName { get; set; }
        public double FileSize { get; set; }
        public double Percentage { get; set; }
        private MainForm mainForm;

        public DownloadForm(MainForm form)
        {
            InitializeComponent();
            mainForm = form;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(txtAddress.Text) && !string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                Url = txtAddress.Text;
                Uri uri = new Uri(this.Url);
                FileName = Path.GetFileName(uri.AbsolutePath);
                client.DownloadFileAsync(uri, txtPath.Text + "/" + FileName);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            client.CancelAsync();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog() { Description = "Выберите путь." })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = dialog.SelectedPath;
                }
            }
        }
        
        private void DownloadForm_Load(object sender, EventArgs e)
        {
            client = new WebClient();
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
            txtPath.Text = Environment.SpecialFolder.Desktop.ToString();
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Database.FilesRow row = App.DB.Files.NewFilesRow();
            row.Url = Url;
            row.FileName = FileName;
            row.FileSize = (string.Format("{0:0.##}", FileSize / 1024));
            row.DateTime = DateTime.Now;
            App.DB.Files.AddFilesRow(row);
            App.DB.AcceptChanges();
            App.DB.WriteXml(string.Format("{0}/data.dat", Application.StartupPath));
            ListViewItem item = new ListViewItem(row.Id.ToString());
            item.SubItems.Add(row.Url);
            item.SubItems.Add(row.FileName);
            item.SubItems.Add(row.FileSize);
            item.SubItems.Add(row.DateTime.ToLongDateString());
            mainForm.listView1.Items.Add(item);
            this.Close();
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Minimum = 0;
            double receive = double.Parse(e.BytesReceived.ToString());
            FileSize = double.Parse(e.TotalBytesToReceive.ToString());
            Percentage = receive / FileSize * 100;
            lblStatus.Text = $"Downloaded {string.Format("{0:0.##}", Percentage)}";
            progressBar.Value = int.Parse(Math.Truncate(Percentage).ToString());
            progressBar.Update();
        }
    }
}
