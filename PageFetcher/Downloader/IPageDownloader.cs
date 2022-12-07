using System;
using System.IO;
using System.Threading.Tasks;

namespace PageFetcher.Downloader
{
    internal interface IPageDownloader
    {
        Task<PageDownloadResult> DownloadPageAsync(Uri pageUri, string outputDirectory);
    }
}