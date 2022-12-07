using System;
using System.Threading.Tasks;

namespace PageFetcher.Downloader
{
    internal interface IFileDownloader
    {
        Task<bool> DownloadFileAsync(Uri sourceUri, string localFile);
    }
}