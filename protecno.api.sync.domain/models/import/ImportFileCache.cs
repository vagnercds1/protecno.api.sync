using protecno.api.sync.domain.enumerators;
using System;
using System.IO;

namespace protecno.api.sync.domain.models.report
{
    public class ImportFileCache
    {
        public string ImportFileFullPath { get; set; }
         
        public ReportCache ImportReport { get; set; }
    }
}
