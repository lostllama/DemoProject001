using PageFetcher.Downloader;
using PageFetcher.PageMetadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PageFetcher
{
    internal class MainApplication
    {
        private readonly TextWriter _log;
        private readonly IMetadataReader _metadataReader;
        private readonly IPageDownloader _pageDownloader;


        public MainApplication(
            TextWriter log,
            IMetadataReader metadataReader,
            IPageDownloader pageDownloader)
        {
            _log = log;
            _metadataReader = metadataReader;
            _pageDownloader = pageDownloader;
        }

        public async Task ExecuteAsync(CommandLineOptions options)
        {
            foreach (var url in options.Urls)
            {
                // validate the URL's format
                if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUri))
                {
                    _log.WriteLine("{0} is not a valid URL.", url);
                    continue;
                }

                // validate the scheme
                bool isHttpOrHttps = string.Equals(parsedUri.Scheme, "http") || string.Equals(parsedUri.Scheme, "https");
                if (!isHttpOrHttps)
                {
                    _log.WriteLine("{0} is not a valid HTTP/HTTPS URL.", url);
                    continue;
                }

                // Downlaod the page
                var downloadResult = await _pageDownloader.DownloadPageAsync(parsedUri, options.OutputDirectory);

                if (!downloadResult.IsSuccess)
                {
                    continue;
                }

                // Display metadata
                if (options.ReadMetadata)
                {
                    var lastFeteched = downloadResult.ExistingFileWriteTime ?? downloadResult.UpdatedFileWriteTime;
                    var metadata = _metadataReader.ReadMetadataFromHtml(downloadResult.FileName);
                    _log.WriteLine("site: {0}", parsedUri.Host);
                    _log.WriteLine("num_links: {0}", metadata.NumLinks);
                    _log.WriteLine("images: {0}", metadata.NumImages);
                    _log.WriteLine("last_fetch: {0:dddd MMM dd yyyy HH:mm 'UTC'}", lastFeteched);
                }
            }
        }
    }
}
