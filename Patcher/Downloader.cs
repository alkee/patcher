using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Patcher
{
    internal class Downloader
    {
        public static async Task<HttpResponseMessage> HttpDownload(string remoteUrl, string localFilePath)
        {
            // 파일 다운로드는 System.Net.WebClient 를 쉽게 이용할 수 있겠지만,
            // redirection 등의 처리를 위한 Response / handler 를 제공하지 않으므로 HttpClient 사용
            using (var client = new HttpClient())
            {
                using var response = await client.GetAsync(remoteUrl);
                using var file = new FileStream(localFilePath, FileMode.Create);
                await response.Content.CopyToAsync(file);
                return response;
            }
        }
    }
}