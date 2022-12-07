using Moq;
using Moq.Contrib.HttpClient;
using PageFetcher.Downloader;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace PageFetcher.Tests
{
    public class FileDownloaderTests
    {
        [Fact]
        public void TestFileDownloadConstructor()
        {
            var handler = new Mock<HttpMessageHandler>();

            var factory = new Mock<IHttpClientFactory>();
            Mock.Get(factory.Object)
                .Setup(x => x.CreateClient(FileDownloader.HttpClientName))
                .Returns(() => handler.CreateClient())
                .Verifiable();

            var textWriter = new Mock<TextWriter>(MockBehavior.Strict);

            var fileDownloader = new FileDownloader(textWriter.Object, factory.Object);
            factory.Verify(x => x.CreateClient(FileDownloader.HttpClientName), Times.Once(), "FileDownloader should use a specific HttpClient and only create it once.");
        }

        [Fact]
        public async Task TestFileDownloadWithSuccess()
        {
            var handler = new Mock<HttpMessageHandler>();

            var client = handler.CreateClient();
            var factory = handler.CreateClientFactory();

            Mock.Get(factory)
                .Setup(x => x.CreateClient(FileDownloader.HttpClientName))
                .Returns(() => handler.CreateClient());

            Uri uri = new Uri("https://www.google.com");
            byte[] testData = new byte[1024];
            RandomNumberGenerator.Fill(testData);

            handler.SetupRequest(HttpMethod.Get, uri)
                .ReturnsResponse(System.Net.HttpStatusCode.OK, new ByteArrayContent(testData))
                .Verifiable();

            string targetFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var textWriter = new Mock<TextWriter>(MockBehavior.Strict);

            try
            {
                var fileDownloader = new FileDownloader(textWriter.Object, factory);
                var downloadResult = await fileDownloader.DownloadFileAsync(uri, targetFile);
                Assert.True(downloadResult);
                Assert.True(testData.SequenceEqual(File.ReadAllBytes(targetFile)));
            }
            finally
            {
                File.Delete(targetFile);
            }
        }

        [Fact]
        public async Task TestFileWithNotFound()
        {
            var handler = new Mock<HttpMessageHandler>();

            var client = handler.CreateClient();
            var factory = handler.CreateClientFactory();

            Mock.Get(factory)
                .Setup(x => x.CreateClient(FileDownloader.HttpClientName))
                .Returns(() => handler.CreateClient());

            Uri uri = new Uri("https://www.google.com");

            handler.SetupRequest(HttpMethod.Get, uri)
                .ReturnsResponse(System.Net.HttpStatusCode.NotFound)
                .Verifiable();

            string targetFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var textWriter = new Mock<TextWriter>(MockBehavior.Strict);
            textWriter.Setup(tw => tw.WriteLine(
                "Unable to download {0}: {1} status code received.",
                uri,
                404
            ))
            .Verifiable();

            try
            {
                var fileDownloader = new FileDownloader(textWriter.Object, factory);
                var downloadResult = await fileDownloader.DownloadFileAsync(uri, targetFile);
                Assert.False(downloadResult);
                Assert.False(File.Exists(targetFile));
            }
            finally
            {
                File.Delete(targetFile);
            }
            textWriter.Verify();
        }

        [Fact]
        public async Task TestFileWithBadRequestWithMessage()
        {
            var handler = new Mock<HttpMessageHandler>();

            var client = handler.CreateClient();
            var factory = handler.CreateClientFactory();

            Mock.Get(factory)
                .Setup(x => x.CreateClient(FileDownloader.HttpClientName))
                .Returns(() => handler.CreateClient());

            Uri uri = new Uri("https://www.google.com");
            string responseMessage = "Your request is bad.";

            handler.SetupRequest(HttpMethod.Get, uri)
                .ReturnsResponse(System.Net.HttpStatusCode.BadRequest, new StringContent(responseMessage))
                .Verifiable();

            string targetFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var textWriter = new Mock<TextWriter>(MockBehavior.Strict);
            textWriter.Setup(tw => tw.WriteLine(
                "Unable to download {0}: {1} status code received with message {2}",
                uri,
                400,
                responseMessage
            ))
            .Verifiable();

            try
            {
                var fileDownloader = new FileDownloader(textWriter.Object, factory);
                var downloadResult = await fileDownloader.DownloadFileAsync(uri, targetFile);
                Assert.False(downloadResult);
                Assert.False(File.Exists(targetFile));
            }
            finally
            {
                File.Delete(targetFile);
            }
            textWriter.Verify();
        }

        [Fact]
        public async Task TestWithException()
        {
            var handler = new Mock<HttpMessageHandler>();

            var client = handler.CreateClient();
            var factory = handler.CreateClientFactory();

            Mock.Get(factory)
                .Setup(x => x.CreateClient(FileDownloader.HttpClientName))
                .Returns(() => handler.CreateClient());

            Uri uri = new Uri("https://www.google.com");
            string exceptionMessage = "Your request is bad.";

            handler.SetupRequest(HttpMethod.Get, uri)
                .Throws(new HttpRequestException(exceptionMessage))
                .Verifiable();

            string targetFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var textWriter = new Mock<TextWriter>(MockBehavior.Strict);
            textWriter.Setup(tw => tw.WriteLine(
                "Unable to download {0}: {1}",
                uri,
                It.Is<HttpRequestException>(e => e.Message == exceptionMessage)
            ))
            .Verifiable();

            try
            {
                var fileDownloader = new FileDownloader(textWriter.Object, factory);
                var downloadResult = await fileDownloader.DownloadFileAsync(uri, targetFile);
                Assert.False(downloadResult);
                Assert.False(File.Exists(targetFile));
            }
            finally
            {
                File.Delete(targetFile);
            }
            textWriter.Verify();
        }

    }
}
