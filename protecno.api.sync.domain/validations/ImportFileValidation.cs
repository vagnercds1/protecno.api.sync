using FluentValidation;
using Microsoft.AspNetCore.Http;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.models.import;
using System;

namespace protecno.api.sync.domain.validations
{
    public class ImportFileValidation : AbstractValidator<ImportFileRequest>
    {
        public ImportFileValidation(IFormFile file)
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => Enum.IsDefined(typeof(EImportFileType), x.InformationType) && x.InformationType != EImportFileType.Undefined).WithMessage("Informe FileType válido")
                                                         .Must(x => x.BaseInventoryId > 0).WithMessage("Informe BaseInventoryId")
                                                         .Must(x => (file != null && file.Length > 0)).WithMessage("Nenhum arquivo foi enviado");
        }
    }
}
