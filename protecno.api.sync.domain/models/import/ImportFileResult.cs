﻿using protecno.api.sync.domain.entities;
using System.Net;

namespace protecno.api.sync.domain.models.report
{
    public class ImportFileResult
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Message { get; set; }

        public ImportFileCache CacheImport { get; set; }
    }
}
