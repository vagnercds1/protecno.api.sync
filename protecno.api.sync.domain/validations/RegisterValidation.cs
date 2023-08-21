using FluentValidation;
using protecno.api.sync.domain.models.register;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using System;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.inventory;
using Dapper;
using System.Threading.Tasks;
using protecno.api.sync.domain.common;
using protecno.api.sync.domain.extensions;
using System.Linq;
using protecno.api.sync.domain.helpers;
using System.Globalization;
using System.Drawing;

namespace protecno.api.sync.domain.validations
{
    public class RegisterValidationGet : AbstractValidator<RegisterPaginateRequest>
    {
        public RegisterValidationGet()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.TipoRegistroId != null).WithMessage("Informe TipoRegistroId");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.BaseInventarioId > 0).WithMessage("Informe o BaseInventoryId");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => Enum.IsDefined(typeof(ERegisterType), x.TipoRegistroId)).WithMessage("TipoRegistroId precisa estar entre 1 e 5")
                                                         .When(x => x.TipoRegistroId != null);
        }
    }

    public class RegisterValidationGetById : AbstractValidator<RegisterByIdRequest>
    {
        public RegisterValidationGetById()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.TipoRegistroId != null).WithMessage("Informe TipoRegistroId")
                                                         .Must(x => Enum.IsDefined(typeof(ERegisterType), x.TipoRegistroId)).WithMessage("TipoRegistroId precisa estar entre 1 e 5")
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
                                                         .Must(x => x.TipoRegistroId != null).WithMessage("Informe TipoRegistroId")
                                                         .Must(x => Enum.IsDefined(typeof(ERegisterType), x.TipoRegistroId)).WithMessage("TipoRegistroId precisa estar entre 1 e 5");
        }
    }

    public class RegisterDeleteValidation : AbstractValidator<RegisterDeleteRequest>
    { 
        public RegisterDeleteValidation()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.TipoRegistroId != null).WithMessage("Informe TipoRegistroId")
                                                         .Must(x => x.BaseInventarioId > 0).WithMessage("Informe o BaseInventarioId");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Codigo.Length <= 20).WithMessage("Codigo excedeu 20 caracteres")
                                                         .When(x => !string.IsNullOrEmpty(x.Codigo));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => string.IsNullOrEmpty(x.Codigo)).WithMessage("Informe apenas Codigo ou DeleteAll")
                                                         .When(x => x.DeleteAll);

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => !string.IsNullOrEmpty(x.Codigo)).WithMessage("Informe Codigo ou DeleteAll")
                                                         .When(x => !x.DeleteAll);
        } 
    }
}
