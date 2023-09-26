using protecno.api.sync.domain.enumerators;

namespace protecno.api.sync.domain.models.import
{
    public class ImportFileRequest
    {
        public EImportFileType InformationType { get; set; }

        public int? BaseInventoryId { get; set; }
    }
}
