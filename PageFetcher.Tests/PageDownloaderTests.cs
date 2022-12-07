using Moq;
using PageFetcher.Downloader;
using PageFetcher.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PageFetcher.Tests
{
    public class PageDownloaderTests
    {
        [Fact]
        public async void TestDownloadFile()
        {
            Uri testUri = new Uri("https://www.example.com");
            var uriUtilities = new Mock<IUriUtilities>(MockBehavior.Strict);
            uriUtilities.Setup(u => u.UriToFileName(testUri)).Returns("www.example.com").Verifiable();

            string outputFolder = Path.Combine(Path.GetTempPath(), "unittest");
            try
            {
                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true);
                }
                Directory.CreateDirectory(outputFolder);

                string expectedFileName = Path.Combine(outputFolder, "www.example.com.html");
                DateTime expectedLastWriteTime = default;
                var downloaderMock = new Mock<IFileDownloader>(MockBehavior.Strict);
                downloaderMock
                    .Setup(u => u.DownloadFileAsync(testUri, expectedFileName))
                    .Callback(() =>
                    {
                        File.WriteAllBytes(expectedFileName, Array.Empty<byte>());
                        expectedLastWriteTime = new FileInfo(expectedFileName).LastWriteTimeUtc;
                    })
                    .Returns(Task.FromResult(true))
                    .Verifiable();

                string expectedAssetsFolder = Path.Combine(outputFolder, "www.example.com");
                var assetDownloaderMock = new Mock<IAssetDownloader>(MockBehavior.Strict);
                assetDownloaderMock
                    .Setup(u => u.DownloadPageAssetsAsync(testUri, expectedAssetsFolder, expectedFileName))
                    .Returns(Task.FromResult(true))
                    .Verifiable();

                var instance = new PageDownloader(downloaderMock.Object, assetDownloaderMock.Object, uriUtilities.Object);
                var result = await instance.DownloadPageAsync(testUri, outputFolder);
                uriUtilities.Verify();
                downloaderMock.Verify();
                assetDownloaderMock.Verify();

                Assert.NotNull(result);
                Assert.True(result.IsSuccess);
                Assert.Equal(expectedFileName, result.FileName);
                Assert.False(result.ExistingFileWriteTime.HasValue);
                Assert.Equal(expectedLastWriteTime, result.UpdatedFileWriteTime);
            }
            finally
            {
                Directory.Delete(outputFolder, true);
            }
        }

        [Fact]
        public async void TestDownloadFileAgain()
        {
            Uri testUri = new Uri("https://www.example.com");
            var uriUtilities = new Mock<IUriUtilities>(MockBehavior.Strict);
            uriUtilities.Setup(u => u.UriToFileName(testUri)).Returns("www.example.com").Verifiable();

            string outputFolder = Path.Combine(Path.GetTempPath(), "unittest");
            try
            {
                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true);
                }
                Directory.CreateDirectory(outputFolder);

                string expectedFileName = Path.Combine(outputFolder, "www.example.com.html");
                File.WriteAllBytes(expectedFileName, Array.Empty<byte>());
                DateTime initialWriteTime = new FileInfo(expectedFileName).LastWriteTimeUtc;

                await Task.Delay(TimeSpan.FromSeconds(5));

                DateTime expectedLastWriteTime = default;
                var downloaderMock = new Mock<IFileDownloader>(MockBehavior.Strict);
                downloaderMock
                    .Setup(u => u.DownloadFileAsync(testUri, expectedFileName))
                    .Callback(() =>
                    {
                        File.WriteAllBytes(expectedFileName, Array.Empty<byte>());
                        expectedLastWriteTime = new FileInfo(expectedFileName).LastWriteTimeUtc;
                    })
                    .Returns(Task.FromResult(true))
                    .Verifiable();

                string expectedAssetsFolder = Path.Combine(outputFolder, "www.example.com");
                var assetDownloaderMock = new Mock<IAssetDownloader>(MockBehavior.Strict);
                assetDownloaderMock
                    .Setup(u => u.DownloadPageAssetsAsync(testUri, expectedAssetsFolder, expectedFileName))
                    .Returns(Task.FromResult(true))
                    .Verifiable();

                var instance = new PageDownloader(downloaderMock.Object, assetDownloaderMock.Object, uriUtilities.Object);
                var result = await instance.DownloadPageAsync(testUri, outputFolder);
                uriUtilities.Verify();
                downloaderMock.Verify();
                assetDownloaderMock.Verify();

                Assert.NotNull(result);
                Assert.True(result.IsSuccess);
                Assert.Equal(expectedFileName, result.FileName);
                Assert.True(result.ExistingFileWriteTime.HasValue);
                Assert.Equal(initialWriteTime, result.ExistingFileWriteTime.Value);
                Assert.Equal(expectedLastWriteTime, result.UpdatedFileWriteTime);
            }
            finally
            {
                Directory.Delete(outputFolder, true);
            }
        }

        [Fact]
        public async void TestDownloadFailed()
        {
            Uri testUri = new Uri("https://www.example.com");
            var uriUtilities = new Mock<IUriUtilities>(MockBehavior.Strict);
            uriUtilities.Setup(u => u.UriToFileName(testUri)).Returns("www.example.com").Verifiable();

            string outputFolder = Path.Combine(Path.GetTempPath(), "unittest");
            try
            {
                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true);
                }
                Directory.CreateDirectory(outputFolder);

                string expectedFileName = Path.Combine(outputFolder, "www.example.com.html");

                var downloaderMock = new Mock<IFileDownloader>(MockBehavior.Strict);
                downloaderMock
                    .Setup(u => u.DownloadFileAsync(testUri, expectedFileName))
                    .Returns(Task.FromResult(false))
                    .Verifiable();

                var assetDownloaderMock = new Mock<IAssetDownloader>(MockBehavior.Strict);

                var instance = new PageDownloader(downloaderMock.Object, assetDownloaderMock.Object, uriUtilities.Object);
                var result = await instance.DownloadPageAsync(testUri, outputFolder);
                uriUtilities.Verify();
                downloaderMock.Verify();
                assetDownloaderMock.VerifyNoOtherCalls();

                Assert.NotNull(result);
                Assert.False(result.IsSuccess);
                Assert.Equal(expectedFileName, result.FileName);
                Assert.False(result.ExistingFileWriteTime.HasValue);
            }
            finally
            {
                Directory.Delete(outputFolder, true);
            }
        }

        [Fact]
        public async void TestDownloadFileAssetsFailure()
        {
            Uri testUri = new Uri("https://www.example.com");
            var uriUtilities = new Mock<IUriUtilities>(MockBehavior.Strict);
            uriUtilities.Setup(u => u.UriToFileName(testUri)).Returns("www.example.com").Verifiable();

            string outputFolder = Path.Combine(Path.GetTempPath(), "unittest");
            try
            {
                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true);
                }
                Directory.CreateDirectory(outputFolder);

                string expectedFileName = Path.Combine(outputFolder, "www.example.com.html");
                var downloaderMock = new Mock<IFileDownloader>(MockBehavior.Strict);
                downloaderMock
                    .Setup(u => u.DownloadFileAsync(testUri, expectedFileName))
                    .Callback(() =>
                    {
                        File.WriteAllBytes(expectedFileName, Array.Empty<byte>());
                    })
                    .Returns(Task.FromResult(true))
                    .Verifiable();

                string expectedAssetsFolder = Path.Combine(outputFolder, "www.example.com");
                var assetDownloaderMock = new Mock<IAssetDownloader>(MockBehavior.Strict);
                assetDownloaderMock
                    .Setup(u => u.DownloadPageAssetsAsync(testUri, expectedAssetsFolder, expectedFileName))
                    .Returns(Task.FromResult(false))
                    .Verifiable();

                var instance = new PageDownloader(downloaderMock.Object, assetDownloaderMock.Object, uriUtilities.Object);
                var result = await instance.DownloadPageAsync(testUri, outputFolder);
                uriUtilities.Verify();
                downloaderMock.Verify();
                assetDownloaderMock.Verify();

                Assert.NotNull(result);
                Assert.False(result.IsSuccess);
                Assert.Equal(expectedFileName, result.FileName);
                Assert.False(result.ExistingFileWriteTime.HasValue);
            }
            finally
            {
                Directory.Delete(outputFolder, true);
            }
        }
    }
}
