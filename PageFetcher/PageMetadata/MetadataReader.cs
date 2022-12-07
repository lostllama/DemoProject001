using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageFetcher.PageMetadata
{
    internal class MetadataReader : IMetadataReader
    {
        public MetadataReaderResult ReadMetadataFromHtml(string htmlFile)
        {
            HtmlDocument doc = new();
            doc.Load(htmlFile);

            return new MetadataReaderResult()
            {
                NumImages = doc.DocumentNode.SelectNodes("//img")?.Count ?? 0,
                NumLinks = doc.DocumentNode.SelectNodes("//a[@href]")?.Count ?? 0
            };
        }
    }
}
