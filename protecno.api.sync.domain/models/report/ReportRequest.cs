using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;

namespace protecno.api.sync.domain.models.report
{
    public class ReportRequest : Inventory
    {
        public EReportType ReportType { get; set; }

        public int? TipoRegistroId { get; set; }
    }
}
