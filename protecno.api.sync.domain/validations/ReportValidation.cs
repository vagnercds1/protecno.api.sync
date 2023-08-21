using FluentValidation;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.models.report;
using System;

namespace protecno.api.sync.domain.validations
{
    public class ReportValidation : AbstractValidator<ReportRequest>
    {
        public ReportValidation()
        {
            RuleFor(x => x).Cascade(CascadeMode.Stop).Must(x => Enum.IsDefined(typeof(EReportType), x.ReportType)).WithMessage("ReportType precisa estar entre 1 e 22")
                                                     .Must(x => x.BaseInventarioId > 0).WithMessage("Informe o BaseInventoryId"); 

            RuleFor(x => x).Cascade(CascadeMode.Stop).Must(x =>  x.TipoInventarioId !=null).WithMessage("Informe TipoInventarioId")
                                                     .When(x => (x.ReportType == EReportType.InventarioFisicoCSV) || (x.ReportType == EReportType.InventarioFisicoPDF) || (x.ReportType == EReportType.InventarioFotosPDF));
        }
    }
}
