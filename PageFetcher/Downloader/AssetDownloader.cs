using HtmlAgilityPack;
using PageFetcher.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageFetcher.Downloader
{
    /// <summary>
    /// This only downloads assets explicitly linked from the page. It does not download assets mentioned in CSS files or referenced through JavaScript.
    /// If CSS files were also required, we could substitute AngleSharp for HtmlAgilityPack
    /// </summary>
    internal class AssetDownloader : IAssetDownloader
    {
        private readonly IFileDownloader _fileDownloader;
        private readonly IUriUtilities _uriUtilities;

        public AssetDownloader(
            IFileDownloader fileDownloader,
            IUriUtilities uriUtilities)
        {
            _fileDownloader = fileDownloader;
            _uriUtilities = uriUtilities;
        }

        public async Task<bool> DownloadPageAssetsAsync(Uri pageUri, string assetFolder, string htmlFile)
        {
            PrepareAssetsFolder(assetFolder);

            HtmlDocument document = new();
            document.Load(htmlFile);

            // Get a unique list of files we need to download, along with the nodes we have to update
            var descriptors = GetAssetDescriptors(pageUri, assetFolder, document);

            // Update the attributes
            foreach (var descriptor in descriptors)
            {
                if (!await _fileDownloader.DownloadFileAsync(descriptor.Uri, descriptor.LocalFilePath))
                {
                    return false;
                }

                string relativePath = Path.GetRelativePath(Path.GetDirectoryName(htmlFile), descriptor.LocalFilePath);
                foreach (var usage in descriptor.Usages)
                {
                    usage.Node.Attributes[usage.AttributeName].Value = relativePath;
                }
            }

            // Save the updated file
            document.Save(htmlFile);

            return true;
        }

        private IList<AssetDescriptor> GetAssetDescriptors(Uri pageUri, string assetFolder, HtmlDocument document)
        {

            var result = new List<AssetDescriptor>();
            var srcNodes = document.DocumentNode.SelectNodes("//*[@src]")?.OfType<HtmlNode>() ?? Enumerable.Empty<HtmlNode>();
            var linkNodes = document.DocumentNode.SelectNodes("//link[@href]")?.OfType<HtmlNode>() ?? Enumerable.Empty<HtmlNode>();

            // Create asset descriptors for all of the src urls
            foreach (var srcGrp in srcNodes.GroupBy(n => _uriUtilities.GetAbsoluteAssetUri(pageUri, n.Attributes["src"].Value)))
            {
                string localFileName = _uriUtilities.UriToFileName(srcGrp.Key);
                string localFilePath = Path.Combine(assetFolder, localFileName);

                var usages = srcGrp.Select(n => new AssetUsage(n, "src")).ToList();
                result.Add(new AssetDescriptor(srcGrp.Key, localFilePath, usages));
            }

            // Create asset descriptors for all of the link href urls
            foreach (var hrefGrp in linkNodes.GroupBy(n => _uriUtilities.GetAbsoluteAssetUri(pageUri, n.Attributes["href"].Value)))
            {
                string localFileName = _uriUtilities.UriToFileName(hrefGrp.Key);
                string localFilePath = Path.Combine(assetFolder, localFileName);

                var usages = hrefGrp.Select(n => new AssetUsage(n, "href")).ToList();
                result.Add(new AssetDescriptor(hrefGrp.Key, localFilePath, usages));
            }

            return result;
        }

        private static void PrepareAssetsFolder(string assetFolder)
        {
            if (Directory.Exists(assetFolder))
            {
                Directory.Delete(assetFolder, true);
            }
            Directory.CreateDirectory(assetFolder);
        }

        private record AssetDescriptor(Uri Uri, string LocalFilePath, IList<AssetUsage> Usages);
        private record AssetUsage(HtmlNode Node, string AttributeName);
    }
}
