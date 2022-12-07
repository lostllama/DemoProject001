using CommandLine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageFetcher
{
    internal class CommandLineOptions
    {
        [Option("metadata", ResourceType = typeof(bool))]
        public bool ReadMetadata { get; set; }

        [Option("output-dir", ResourceType = typeof(string))]
        public string OutputDirectory { get; set; } = Directory.GetCurrentDirectory();


        [Value(0, ResourceType = typeof(string))]
        public IList<string> Urls { get; set; }
    }
}
