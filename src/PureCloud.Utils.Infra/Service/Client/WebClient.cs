using System.Net.Http;
using System.Threading.Tasks;

namespace PureCloud.Utils.Infra.Service.Client
{
    public class WebClient
    {
        public static async Task<byte[]> DownloadFileFromUrl(string url)
        {
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                        return await result.Content.ReadAsByteArrayAsync();
                }
            }
            return null;
        }
    }
}
