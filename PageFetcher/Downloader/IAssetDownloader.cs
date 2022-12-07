using System;
using System.Threading.Tasks;

namespace PageFetcher.Downloader
{
    internal interface IAssetDownloader
    {
        Task<bool> DownloadPageAssetsAsync(Uri pageUri, string assetFolder, string htmlFile);
    }
}