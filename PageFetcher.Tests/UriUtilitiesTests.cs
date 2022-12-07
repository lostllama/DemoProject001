using PageFetcher.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PageFetcher.Tests
{
    public class UriUtilitiesTests
    {
        [Theory]
        [InlineData(new object[] { "https://www.google.com", "www.google.com" })]
        [InlineData(new object[] { "https://www.example.com:8080", "www.example.com-8080" })]
        [InlineData(new object[] { "https://www.example.com/my/test/file.html", "www.example.com_my_test_file.html" })]
        [InlineData(new object[] { "https://www.example.com:8080/my/other/test", "www.example.com-8080_my_other_test" })]
        public void TestUriToFileName(string url, string expected)
        {
            Uri uri = new Uri(url);
            UriUtilities utilities = new UriUtilities();
            string actual = utilities.UriToFileName(uri);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new object[] { "https://www.google.com/my/test/page", "/logo.png", "https://www.google.com/logo.png" })]
        [InlineData(new object[] { "https://www.google.com/my/test/page", "./test/logo.png", "https://www.google.com/my/test/test/logo.png" })]
        [InlineData(new object[] { "https://www.google.com/my/test/page", "../logo.png", "https://www.google.com/my/logo.png" })]
        [InlineData(new object[] { "https://www.google.com/my/test/page", "logo.png", "https://www.google.com/my/test/logo.png" })]
        [InlineData(new object[] { "https://www.google.com/my/test/page", "https://www.example.com/test.jpg", "https://www.example.com/test.jpg" })]
        [InlineData(new object[] { "https://www.google.com/my/test/page", "//www.example.com/test.jpg", "https://www.example.com/test.jpg" })]
        public void TestAbsoluteAssetUri(string originalUrl, string assetUrl, string expected)
        {
            Uri originalUri = new Uri(originalUrl);
            UriUtilities utilities = new UriUtilities();
            string actual = utilities.GetAbsoluteAssetUri(originalUri, assetUrl).ToString();
            Assert.Equal(expected, actual);
        }
    }
}
