using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageFetcher
{
    internal class PageDownloadResult
    {
        public bool IsSuccess { get; set; }
        public string FileName { get; set; }
        public DateTime? ExistingFileWriteTime { get; set; }
        public DateTime UpdatedFileWriteTime { get; set; }
    }
}
