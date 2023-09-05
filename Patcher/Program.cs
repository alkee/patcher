using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Patcher
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main()
        {
            // https://docs.microsoft.com/ko-kr/dotnet/csharp/language-reference/proposals/csharp-7.1/async-main
            return MainAsync().GetAwaiter().GetResult();
        }

        private static async Task<int> MainAsync()
        {
            // read config
            var configFileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(Application.ExecutablePath), ".config");
            Debug.WriteLine($"reading config - {configFileName}");

            try
            {
                var config = await AppConfig.LoadAsync(configFileName);
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(config));
            }
            catch (FileNotFoundException fileNotFound)
            {
                MessageBox.Show($"{Path.GetFileName(fileNotFound.FileName)} NOT FOUND", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
            catch (System.Text.Json.JsonException jsonException)
            {
                MessageBox.Show($"{jsonException.Message} at line {jsonException.LineNumber ?? 0}", "Invalid config file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 2;
            }

            return 0;
        }

    }
}