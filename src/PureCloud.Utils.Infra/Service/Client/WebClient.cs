using System.Net.Http;
using System.Threading.Tasks;

namespace PureCloud.Utils.Infra.Service.Client
{
    public class WebClient
    {
        public static async Task<byte[]> DownloadFileFromUrl(string url)
        {
            byte[] file = null;
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(url))
                {
                    do
                    {
                        file = await result.Content.ReadAsByteArrayAsync();
                    } while (!result.IsSuccessStatusCode);
                }
            }
            return file;
        }
    }
}
