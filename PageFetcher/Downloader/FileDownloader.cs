using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PageFetcher.Downloader
{
    internal class FileDownloader : IFileDownloader
    {
        public const string HttpClientName = "UserAgentClient";
        private readonly TextWriter _log;
        private readonly HttpClient _httpClient;

        public FileDownloader(
            TextWriter log,
            IHttpClientFactory httpClientFactory)
        {
            _log = log;
            _httpClient = httpClientFactory.CreateClient(HttpClientName);
        }

        public async Task<bool> DownloadFileAsync(Uri sourceUri, string localFile)
        {
            try
            {
                // HttpClient will try to pull the body 
                using var response = await _httpClient.GetAsync(sourceUri, HttpCompletionOption.ResponseHeadersRead);

                // Log failure information
                if (!response.IsSuccessStatusCode)
                {
                    var failureResponse = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(failureResponse))
                    {
                        _log.WriteLine("Unable to download {0}: {1} status code received with message {2}",
                            sourceUri,
                            (int)response.StatusCode,
                            failureResponse.Length > 200 ? failureResponse[..200] : failureResponse // we probably don't want a huge page of HTML logged, so let's just log the first 200 characters
                        );
                    }
                    else
                    {
                        _log.WriteLine("Unable to download {0}: {1} status code received.",
                            sourceUri,
                            (int)response.StatusCode
                        );
                    }
                    return false;
                }

                using var sourceStream = await response.Content.ReadAsStreamAsync();
                using var outFile = File.Create(localFile);
                await sourceStream.CopyToAsync(outFile);
            }
            catch (HttpRequestException ex)
            {
                _log.WriteLine("Unable to download {0}: {1}", sourceUri, ex);
                return false;
            }
            return true;
        }
    }
}
