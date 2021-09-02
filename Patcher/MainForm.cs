using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Patcher
{
    public partial class MainForm : Form
    {
        private AppConfig cfg;

        public MainForm(AppConfig config)
        {
            cfg = config;

            InitializeComponent();
            if (string.IsNullOrEmpty(config.Title) == false)
                Text = config.Title;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            progressBar.MarqueeAnimationSpeed = 10;
            progressBar.Style = ProgressBarStyle.Marquee;
            var versionFileName = Path.GetFileName(cfg.VersionJsonUrl);
            var tmpFileName = versionFileName + ".tmp";
            statusLabel.Text = $"checking version... {versionFileName}";

            try
            {
                var response = await Downloader.HttpDownload(cfg.VersionJsonUrl, tmpFileName);
                var newVersion = await VersionInfo.LoadAsync(tmpFileName);
                var oldVersion = await VersionInfo.LoadAsync(versionFileName);
                if (newVersion.Version == oldVersion?.Version)
                { // 
                    statusLabel.Text = $"({newVersion.Version}) Version is up to date";
                    await Task.Delay(2000);
                    Close();
                    Process.Start(newVersion.ExecuteFilePath);
                    return;
                }

                statusLabel.Text = $"Downloading package...";
                var packageFileName = Path.GetFileName(newVersion.PackageUrl);
                response = await Downloader.HttpDownload(newVersion.PackageUrl, packageFileName);
                statusLabel.Text = $"Installing package...";
                await Task.Run(() => { System.IO.Compression.ZipFile.ExtractToDirectory(packageFileName, ".", true); });
                File.Move(tmpFileName, versionFileName, true);
                statusLabel.Text = $"Running...";
                await Task.Delay(2000);
                Close();
                Process.Start(newVersion.ExecuteFilePath);
            }
            catch (HttpRequestException requestError)
            {
                statusLabel.Text = $"failed to reqeust url({requestError.Message})";
            }
            catch (InvalidOperationException processError)
            {
                statusLabel.Text = $"failed to execute({processError.Message})";
            }
            await Task.Delay(5000);
            Close();
        }
    }
}