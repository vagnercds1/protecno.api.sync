using FluentValidation;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.models.inventory;
using System;

namespace protecno.api.sync.domain.validations
{
    public class InventoryValidationGet : AbstractValidator<InventoryPaginateRequest>
    {
        public InventoryValidationGet()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.TipoInventarioId > 0).WithMessage("Informe TipoInventarioId");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => Enum.IsDefined(typeof(EInventoryType), x.TipoInventarioId)).WithMessage("TipoInventarioId inválido")
                                                         .When(x => x.TipoInventarioId != null);

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.BaseInventarioId > 0).WithMessage("Informe BaseInventarioId");
        }
    }

    public class InventoryValidationGetById : AbstractValidator<InventoryByIdRequest>
    {
        public InventoryValidationGetById()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.InventoryId > 0).WithMessage("Informe o id")
                                                         .Must(x => x.BaseInventarioId > 0).WithMessage("Informe BaseInventarioId");

        }
    }

    public class InventoryValidation : AbstractValidator<Inventory>
    {
        public InventoryValidation()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.BaseInventarioId > 0).WithMessage("Informe BaseInventarioId")
                                                     .Must(x => x.TipoInventarioId > 0).WithMessage("Informe TipoInventarioId")
                                                     .Must(x => Enum.IsDefined(typeof(EInventoryType), x.TipoInventarioId)).WithMessage("TipoInventarioId precisa estar entre 1 e 3")
                                                     .Must(x => !string.IsNullOrEmpty(x.Codigo)).WithMessage("Informe Codigo")
                                                     .Must(x => !string.IsNullOrEmpty(x.Descricao)).WithMessage("Informe Descricao")
                                                     .Must(x => x.FilialId > 0).WithMessage("Filial não localizada");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.CondicaoUso > 0 && x.CondicaoUso <= 5).WithMessage("CondicaoUso precisa estar entre 1 e 5")
                                                     .When(x => x.CondicaoUso != null);

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => Enum.IsDefined(typeof(EInventoryStatus), x.StatusRegistroId)).WithMessage("TipoInventarioId precisa estar entre 1 e 5")
                                                     .When(x => x.StatusRegistroId != null);

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.LocalId > 0).WithMessage("Local não localizado")
                                                     .When(x => !string.IsNullOrEmpty(x.LocalCodigo));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.ResponsavelId > 0).WithMessage("Responsavel não localizado")
                                                     .When(x => !string.IsNullOrEmpty(x.ResponsavelCodigo));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.CentroCustoId > 0).WithMessage("CentroCusto não localizado")
                                                     .When(x => !string.IsNullOrEmpty(x.CentroCustoCodigo));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.ContaContabilId > 0).WithMessage("ContaContabil não localizada")
                                                     .When(x => !string.IsNullOrEmpty(x.ContaContabilCodigo));

            #region Length Validations

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Codigo.Length <= 20).WithMessage("Codigo excedeu 20 caracteres");

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.CodigoAnterior.Length <= 20).WithMessage("CodigoAnterior excedeu 20 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.CodigoAnterior));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Descricao.Length <= 255).WithMessage("Descricao excedeu 255 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Descricao));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.DescricaoComplemento.Length <= 255).WithMessage("DescricaoComplemento excedeu 255 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.DescricaoComplemento));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Marca.Length <= 100).WithMessage("Marca excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Marca));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Modelo.Length <= 100).WithMessage("Modelo excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Modelo));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Serie.Length <= 100).WithMessage("Serie excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Serie));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.TAG.Length <= 100).WithMessage("TAG excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.TAG));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Observacao.Length <= 255).WithMessage("Observacao excedeu 255 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Observacao));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Auxiliar1.Length <= 100).WithMessage("Auxiliar1 excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Auxiliar1));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Auxiliar2.Length <= 100).WithMessage("Auxiliar2 excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Auxiliar2));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Auxiliar3.Length <= 100).WithMessage("Auxiliar3 excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Auxiliar3));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Auxiliar4.Length <= 100).WithMessage("Auxiliar4 excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Auxiliar4));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Auxiliar5.Length <= 100).WithMessage("Auxiliar5 excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Auxiliar5));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Auxiliar6.Length <= 100).WithMessage("Auxiliar6 excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Auxiliar6));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Auxiliar7.Length <= 100).WithMessage("Auxiliar7 excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Auxiliar7));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Auxiliar8.Length <= 100).WithMessage("Auxiliar8 excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Auxiliar8));

            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.Documento.Length <= 100).WithMessage("Documento excedeu 100 caracteres")
                                                     .When(x => !string.IsNullOrEmpty(x.Documento));
            #endregion
        }
    }

    public class InventoryDeleteValidation : AbstractValidator<InventoryDeleteRequest>
    {
        public InventoryDeleteValidation()
        {
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.BaseInventarioId > 0).WithMessage("Informe o BaseInventarioId")
                                                         .Must(x => x.TipoInventarioId > 0).WithMessage("Informe o TipoInventarioId");
            
            RuleFor(x => x).Cascade(CascadeMode.Continue).Must(x => x.InventoryIdList.Count == 0).WithMessage("Desmarque 'DeleteAll' caso queira enviar uma lista de Ids ")
                                                         .When(x => x.DeleteAll); 
        }
    }
}
