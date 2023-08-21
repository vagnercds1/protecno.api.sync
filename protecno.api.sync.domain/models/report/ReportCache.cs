using System;
using System.IO;

namespace protecno.api.sync.domain.models.report
{
    public class ReportCache
    {
        public string ResportType { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }

        public string ReturnFileName { get; set; }

        public string FullPath { get; set; }
    }
}
