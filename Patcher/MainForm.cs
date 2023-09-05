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
        private readonly AppConfig cfg;

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
                var oldVersion = await VersionInfo.LoadAsync(versionFileName);
                var response = await Downloader.HttpDownload(cfg.VersionJsonUrl, tmpFileName);
                var newVersion = await VersionInfo.LoadAsync(tmpFileName);
                await WaitForExitAsync(oldVersion?.ExecuteFilePath ?? "");
                if (newVersion.Version == oldVersion?.Version)
                { // 
                    statusLabel.Text = $"({newVersion.Version}) Version is up to date";
                    await ExecuteAndClose(newVersion);
                    return;
                }

                statusLabel.Text = $"Downloading package...";
                var packageFileName = Path.GetFileName(newVersion.PackageUrl);
                response = await Downloader.HttpDownload(newVersion.PackageUrl, packageFileName);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"{response.StatusCode}");
                statusLabel.Text = $"Installing package...";
                await Task.Run(() => { System.IO.Compression.ZipFile.ExtractToDirectory(packageFileName, ".", true); });
                File.Move(tmpFileName, versionFileName, true);
                statusLabel.Text = $"Running...";
                await ExecuteAndClose(newVersion);
            }
            catch (HttpRequestException requestError)
            {
                statusLabel.Text = $"failed to reqeust url({requestError.Message})";
            }
            catch (InvalidOperationException processError)
            {
                statusLabel.Text = $"failed to execute({processError.Message})";
            }
            catch (InvalidDataException unzipError)
            {
                statusLabel.Text = $"failed to upzip({unzipError.Message})";
            }
            statusLabel.ForeColor = System.Drawing.Color.Red;
            progressBar.Visible = false;
            ControlBox = true; // show close button
        }

        private async Task ExecuteAndClose(VersionInfo version)
        {
            Process.Start(version.ExecuteFilePath, version.Arguments ?? "");
            await Task.Delay(2000);
            Close();
        }


        private async Task WaitForExitAsync(string processPath)
        {
            var currentProcess = Process.GetCurrentProcess();
            var parentProcess = currentProcess.GetParent();
            var parentProcessName = parentProcess?.ProcessName ?? "";
            if (string.Compare(Path.GetFileNameWithoutExtension(processPath), parentProcessName, true) != 0)
            {
                Debug.WriteLine($"parent : {parentProcessName}, required : {processPath}  mismatched");
                return;
            }
            statusLabel.Text = $"waiting for preparing...";
            await parentProcess.WaitForExitAsync();
        }

    }
}