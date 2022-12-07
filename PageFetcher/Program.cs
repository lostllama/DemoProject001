using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using PageFetcher.Downloader;
using PageFetcher.Helpers;
using PageFetcher.PageMetadata;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace PageFetcher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Build the service collection
            var services = BuildServiceCollection();

            // Parse the arguments
            if (!TryParseArguments(args, out var options))
            {
                return;
            }

            // Run the downloader
            var downloader = services.GetRequiredService<MainApplication>();
            await downloader.ExecuteAsync(options);
        }

        /// <summary>
        /// Parses the commandline arguments
        /// </summary>
        private static bool TryParseArguments(string[] args, out CommandLineOptions options)
        {
            var parseResult = Parser.Default.ParseArguments<CommandLineOptions>(args);
            if (parseResult.Errors.Any())
            {
                foreach (var error in parseResult.Errors)
                {
                    Console.WriteLine(error.ToString());
                }
                options = default;
                return false;
            }

            options = parseResult.Value;
            return true;
        }

        /// <summary>
        /// Builds the service container
        /// </summary>
        private static IServiceProvider BuildServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddHttpClient()
                .AddHttpClient(FileDownloader.HttpClientName)
                .ConfigureHttpClient(client =>
                {
                    // HttpClient doesn't have a User-Agent header by default and some websites reject such requests
                    // so we will add one here
                    var version = Assembly.GetEntryAssembly().GetName().Version.ToString();
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PageFetcher", version));
                });

            services.AddSingleton<TextWriter>(Console.Out);
            services.AddSingleton<MainApplication>();
            services.AddTransient<IPageDownloader, PageDownloader>();
            services.AddTransient<IAssetDownloader, AssetDownloader>();
            services.AddTransient<IFileDownloader, FileDownloader>();
            services.AddTransient<IUriUtilities, UriUtilities>();
            services.AddTransient<IMetadataReader, MetadataReader>();
            return services.BuildServiceProvider();
        }
    }
}
