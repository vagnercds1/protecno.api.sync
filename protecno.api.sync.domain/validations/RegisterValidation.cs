using FluentValidation;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.models.register;
using System;

namespace protecno.api.sync.domain.validations
{
    public class RegisterValidationGet : AbstractValidator<RegisterPaginateRequest>
    {
        public RegisterValidationGet()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.InformationType != null).WithMessage("Informe InformationType");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.BaseInventarioId > 0).WithMessage("Informe o BaseInventoryId");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => (int)x.InformationType >=1 && (int)x.InformationType <= 5).WithMessage("TipoRegistroId precisa estar entre 1 e 5")
                                                         .When(x => x.InformationType != null);
        }
    }

    public class RegisterValidationGetById : AbstractValidator<RegisterByIdRequest>
    {
        public RegisterValidationGetById()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.InformationType != null).WithMessage("Informe TipoRegistroId")
                                                         .Must(x => Enum.IsDefined(typeof(EInformationType), x.InformationType)).WithMessage("TipoRegistroId precisa estar entre 1 e 5")
                                                         .Must(x => x.BaseInventarioId > 0).WithMessage("Informe BaseInventarioId")
                                                         .Must(x => x.RegisterId > 0).WithMessage("Informe Id");
        }
    }

    public class RegisterValidation : AbstractValidator<Register>
    {
        public RegisterValidation()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => Enum.IsDefined(typeof(EOrigin), x.OrigemId)).WithMessage("Informe OrigemId")
                                                         .Must(x => !string.IsNullOrEmpty(x.Codigo)).WithMessage("Informe Codigo")
                                                         .Must(x => !string.IsNullOrEmpty(x.Descricao)).WithMessage("Informe Descricao")
                                                         .Must(x => x.BaseInventarioId > 0).WithMessage("Informe BaseInventarioId")
                                                         .Must(x => x.Codigo.Length <= 20).WithMessage("Codigo excedeu 20 caracteres")
                                                         .Must(x => x.Descricao.Length <= 80).WithMessage("Codigo excedeu 80 caracteres")
                                                         .Must(x => x.InformationType != null).WithMessage("Informe TipoRegistroId")
                                                         .Must(x => (int)x.InformationType >= 1 && (int)x.InformationType <= 5).WithMessage("TipoRegistroId precisa estar entre 1 e 5");
        }
    }

    public class RegisterDeleteValidation : AbstractValidator<RegisterDeleteRequest>
    {
        public RegisterDeleteValidation()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.BaseInventoryId > 0).WithMessage("Informe o BaseInventoryId");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.RegisterCodeList.Count > 0).WithMessage("Informe a lista RegisterCodeList que deseja excluir")
                                                         .When(x => !x.DeleteAll);

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.InformationType != null).WithMessage("Informe o RegisterTypeId")
                                                         .When(x => x.DeleteAll);

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.RegisterCodeList == null || x.RegisterCodeList.Count == 0).WithMessage("Desmarque 'DeleteAll' caso queira enviar uma lista de Codigos ")
                                                         .When(x => x.DeleteAll);
        }
    }
}
