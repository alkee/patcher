using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Patcher
{
    public class AppConfig
    {
        /// <summary>
        ///     source of <see cref="VersionInfo"/>
        /// </summary>
        public string VersionJsonUrl { get; set; }

        public string Title { get; set; }

#if DEBUG

        public static Task<AppConfig> LoadAsync(string filePath)
        {
            return Task.FromResult(new AppConfig
            {
                VersionJsonUrl = "https://www.skia.site/downloads/marsp/mars.version.json",
                Title = "Debugging",
            });
        }

#else
        public static async Task<AppConfig> LoadAsync(string filePath)
        {
            var file = File.Open(filePath, FileMode.Open);
            return await JsonSerializer.DeserializeAsync<AppConfig>(file);
        }
#endif
    }

    public class VersionInfo
    {
        public string Version { get; set; }
        public string PackageUrl { get; set; }
        public string ExecuteFilePath { get; set; }

        public static async Task<VersionInfo> LoadAsync(string filePath)
        {
            if (File.Exists(filePath) == false) return null;

            var file = File.Open(filePath, FileMode.Open);
            return await JsonSerializer.DeserializeAsync<VersionInfo>(file);
        }
    }
}