namespace PageFetcher.PageMetadata
{
    internal interface IMetadataReader
    {
        MetadataReaderResult ReadMetadataFromHtml(string htmlFile);
    }
}