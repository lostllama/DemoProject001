using Microsoft.Extensions.Logging;
using PageFetcher.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PageFetcher.Downloader
{
    internal class PageDownloader : IPageDownloader
    {
        private readonly IFileDownloader _fileDownloader;
        private readonly IAssetDownloader _assetDownloader;
        private readonly IUriUtilities _uriUtilities;

        public PageDownloader(
            IFileDownloader fileDownloader,
            IAssetDownloader assetDownloader,
            IUriUtilities uriUtilities)
        {
            _fileDownloader = fileDownloader;
            _assetDownloader = assetDownloader;
            _uriUtilities = uriUtilities;
        }

        public async Task<PageDownloadResult> DownloadPageAsync(Uri pageUri, string outputDirectory)
        {
            string uriAsFileName = _uriUtilities.UriToFileName(pageUri);
            string localFileName = $"{uriAsFileName}.html";
            string localFilePath = Path.Combine(outputDirectory, localFileName);

            // If the file already exists, save the file info for later
            DateTime? existingFileWriteTime = File.Exists(localFilePath) ? new FileInfo(localFilePath).LastWriteTimeUtc : null;

            // Download the file
            if (!await _fileDownloader.DownloadFileAsync(pageUri, localFilePath))
            {
                return new PageDownloadResult() { IsSuccess = false, FileName = localFilePath };
            }

            // Download its assets
            if (!await _assetDownloader.DownloadPageAssetsAsync(pageUri, Path.Combine(outputDirectory, uriAsFileName), localFilePath))
            {
                return new PageDownloadResult() { IsSuccess = false, FileName = localFilePath };
            }

            // Get the latest information
            DateTime updatedFileWriteTime = new FileInfo(localFilePath).LastWriteTimeUtc;

            return new PageDownloadResult() { IsSuccess = true, FileName = localFilePath, ExistingFileWriteTime = existingFileWriteTime, UpdatedFileWriteTime = updatedFileWriteTime };
        }


    }
}
