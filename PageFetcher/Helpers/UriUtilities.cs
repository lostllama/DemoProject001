using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageFetcher.Helpers
{
    internal class UriUtilities : IUriUtilities
    {
        /// <summary>
        /// Reformats the Uri as a valid Windows filename
        /// </summary>
        public string UriToFileName(Uri uri)
        {
            var fileNameBuilder = new StringBuilder();
            fileNameBuilder.Append(uri.Host);

            // We should record the port number
            if (!uri.IsDefaultPort)
            {
                fileNameBuilder.Append('-'); // : isn't allowed as part of a filename on Windows, Linux/Mac should be OK
                fileNameBuilder.Append(uri.Port);
            }

            // We should retain the path
            if (uri.AbsolutePath != "/")
            {
                fileNameBuilder.Append(uri.AbsolutePath.Replace('/', '_')); // / isn't allowed as part of a filename on Windows, Linux/Mac should be OK
            }

            return fileNameBuilder.ToString();
        }

        /// <summary>
        /// Gets the absolute URL from the mixture of relative and absolute URLs present in HTM pages
        /// </summary>
        public Uri GetAbsoluteAssetUri(Uri originalUri, string assetUrl)
        {
            // It's common for some libraries to use /// to indicate content is available
            // by both HTTP and HTTPS, so we'll go with HTTPS.
            if (assetUrl.StartsWith("//", StringComparison.Ordinal))
            {
                assetUrl = $"https:{assetUrl}";
            }

            bool isRelativeUrl = assetUrl.StartsWith("/", StringComparison.Ordinal) // admittedly this is an absolute path, but on Linux Uri.TryCreate makes it a file:/// URL
                || assetUrl.StartsWith("./", StringComparison.Ordinal)
                || assetUrl.StartsWith("../", StringComparison.Ordinal);

            if (!isRelativeUrl && Uri.TryCreate(assetUrl, UriKind.Absolute, out var absoluteAssetUri))
            {
                return absoluteAssetUri;
            }

            return new Uri(originalUri, assetUrl);
        }
    }
}
