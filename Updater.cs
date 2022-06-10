using Deathlon.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deathlon
{
    public partial class Updater : Form
    {
        private string version = "1.3";
        string PATH = System.AppDomain.CurrentDomain.BaseDirectory + "\\";
        string EXE_PATH = System.AppDomain.CurrentDomain.BaseDirectory + "\\Deathlon.exe";
        Assembly currentAssembly = Assembly.GetEntryAssembly();

        public Updater()
        {
            InitializeComponent();
            removeOldVersion();
        }

        private void Updater_Load(object sender, EventArgs e)
        {
            bar.Width = 0;
            WebClient webClient = new WebClient();
            string file = "";
            Opacity = 0;
            try
            {
                file = webClient.DownloadString("https://pastebin.com/raw/PzxysUev");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load. Make sure you're connected to the internet!");
                Application.Exit();
                return;
            }

            Dictionary<string, string> appconfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(file);


            // (Convert.ToDouble(appconfig["version"]) > Convert.ToDouble(version)
            if(appconfig["version"] != version)
                {
                Opacity = 100;
                System.IO.File.Move(PATH + Path.GetFileName(currentAssembly.Location), "Deathlon_outdated.exe");
                startDownload(appconfig["link"], EXE_PATH);
            }
            else
            {

                this.Hide();
                var form1 = new Form1();
                this.Hide();
                form1.ShowDialog();
                this.Close();
            }
        }

        private void startDownload(string file, string location)
        {
            Thread thread = new Thread(() => {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(file), location);
            });
            thread.Start();
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                //label2.Text = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                //progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());   
                bar.Width = (int)Math.Truncate((percentage / 100) * 358);
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {

                Application.Exit();
                if (File.Exists(EXE_PATH))
                    System.Diagnostics.Process.Start(EXE_PATH);
            });
        }

        private void removeOldVersion()
        {
            try
            {
                if (File.Exists(PATH + "Deathlon_outdated.exe"))
                    File.Delete(PATH + "Deathlon_outdated.exe");
            }
            catch { }
        }
    }
}
