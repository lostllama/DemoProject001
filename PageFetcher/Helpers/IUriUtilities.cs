using System;

namespace PageFetcher.Helpers
{
    internal interface IUriUtilities
    {
        Uri GetAbsoluteAssetUri(Uri originalUri, string assetUrl);
        string UriToFileName(Uri uri);
    }
}