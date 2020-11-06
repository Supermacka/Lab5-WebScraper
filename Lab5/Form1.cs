using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonExtract_Click(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            if (textBoxURL.Text.Contains(@"https://"))
            {
                // Download WebURL
                Task<string> downloadURL = client.GetStringAsync(textBoxURL.Text);
                downloadURL.Wait();

                // Regex expression
                string pattern = "<img.*src=\"(.*?)\"";
                Regex rgx = new Regex(pattern);
                   
                // Variables
                string imgLink = "";
                int found = 0;
                
                // Clear list if user would download more images
                listBox1.Items.Clear();

                foreach (Match imgMatch in rgx.Matches(downloadURL.Result.ToString()))
                {
                    //Making sure only links with full url are read
                    if (imgMatch.Groups[1].Length > @"https://gp.se".Length)
                    {
                        if (!imgMatch.ToString().Contains(@"https://"))
                        {
                            imgLink = imgMatch.Groups[1].ToString().Insert(0, @"https://gp.se");
                        }
                        else
                        {
                            imgLink = imgMatch.Groups[1].ToString();
                        }
                        listBox1.Items.Add(imgLink);
                        found++;
                    }
                }
                // Change label 
                labelCountImages.Text = $"Found: {found} images";

                // Change control properties
                buttonSave.ForeColor = Color.White;
                buttonSave.BackColor = Color.Indigo;
                buttonSave.Enabled = true;
            }
            //If URL is not valid
            else
            {
                string message = "Enter the full URL-name";
                string title = "Could not find URL";
                MessageBox.Show(message, title);
            }
        }

        private async void buttonSave_Click(object sender, EventArgs e)
        {
            // File types
            string[] fileEndings = new string[] { "bmp", "png", "gif", "jpg", "jpeg" };

            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK && !Directory.EnumerateFileSystemEntries(dialog.SelectedPath).Any())
                {
                    int imageName = 1;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = listBox1.Items.Count - 1;
                    buttonSave.Text = "Downloading...";

                    foreach (string item in listBox1.Items)
                    {
                        if (item.ToString().Length != 0)
                        {
                            // Determine filetype and add to file-name 
                            for (int j = 0; j < fileEndings.Length; j++)
                            {
                                if (item.Contains(fileEndings[j]))
                                {
                                    string fileType = $".{fileEndings[j]}";

                                    // Download img from url and add to list. Then save to directory.
                                    var list = new List<Task>();
                                    using (WebClient client = new WebClient())
                                    {
                                        var task = client.DownloadFileTaskAsync(new Uri(item), $@"{dialog.SelectedPath}\Image{imageName}{fileType}");
                                        list.Add(task);
                                        progressBar1.Value++;
                                    }
                                    await Task.WhenAny(list);
                                    break;
                                }
                            }
                            imageName++;

                            // Change label 
                            labelCountImages.Text = $"Downloading: {imageName} images";
                        }
                    }
                    buttonSave.Text = "Completed";
                    buttonSave.BackColor = Color.Green;
                    buttonSave.Enabled = false;
                    
                    labelCountImages.Text = $"{imageName} images downloaded";
                    labelCountImages.ForeColor = Color.Green;
                }
                // If directory-folder contains exisiting files
                else
                {
                    string message = "Choose a folder that is empty";
                    string title = "Folder not empty!";
                    MessageBox.Show(message, title);
                }
            }
        }
    }
}
