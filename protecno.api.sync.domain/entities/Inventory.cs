  using CsvHelper.Configuration.Attributes;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using protecno.api.sync.domain.enumerators;
using System;

namespace protecno.api.sync.domain.entities
{
    [Delimiter(";")]
    [Table("inventario")]
    public class Inventory : BaseEntity
    {
        #region Ordinary Filds 

        public int BaseInventarioId { get; set; }

        public int Id { get; set; }

        public EInventoryType? TipoInventarioId { get; set; }

        public bool? Ativo { get; set; }

        public string Codigo { get; set; }

        [Computed]
        public string FilialCodigo { get; set; }

        [Computed]
        public string Filial { get; set; }

        private int? filialId;
        public int? FilialId
        {
            get
            {
                return this.filialId;
            }
            set
            {
                if (value > 0)
                    this.filialId = value;
            }
        }

        #endregion

        #region general fields

        public string CodigoAnterior { get; set; }

        public int? Incorporacao { get; set; }

        public int IncorporacaoAnterior { get; set; }

        public string Descricao { get; set; }

        public string DescricaoComplemento { get; set; }

        public string Marca { get; set; }

        public string Modelo { get; set; }

        public string Serie { get; set; }

        public string TAG { get; set; }

        [JsonIgnore]
        public string Anotacoes { get; set; }

        public string Observacao { get; set; }

        public string Auxiliar1 { get; set; }

        public string Auxiliar2 { get; set; }

        public string Auxiliar3 { get; set; }

        public string Auxiliar4 { get; set; }

        public string Auxiliar5 { get; set; }

        public string Auxiliar6 { get; set; }

        public string Auxiliar7 { get; set; }

        public string Auxiliar8 { get; set; }

        public int? StatusRegistroId { get; set; }

        public int? CondicaoUso { get; set; }

        public int? Quantidade { get; set; }

        public string Propriedades { get; set; }

        [Computed]
        public string Mensagem { get; set; }

        #endregion

        #region Registers 
         
        [Computed]
        public string FilialCodigoAnterior { get; set; }

        [Computed]
        public string FilialAnterior { get; set; }

        private int? filialIdAnterior;
        public int? FilialIdAnterior
        {
            get
            {
                return this.filialIdAnterior;
            }
            set
            {
                if (value > 0)
                    this.filialIdAnterior = value;
            }
        }

        [Computed]
        public string CentroCustoCodigo { get; set; }

        [Computed]
        public string CentroCusto { get; set; }

        private int? centroCustoId;
        public int? CentroCustoId
        {
            get
            {
                return this.centroCustoId;
            }
            set
            {
                if (value > 0)
                    this.centroCustoId = value;
            }
        }

        [Computed]
        public string ResponsavelCodigo { get; set; }

        [Computed]
        public string Responsavel { get; set; }

        private int? responsavelId;
        public int? ResponsavelId
        {
            get
            {
                return this.responsavelId;
            }
            set
            
            {
                if (value > 0)
                    this.responsavelId = value;
            }
        }


        [Computed]
        public string LocalCodigo { get; set; }

        [Computed]
        public string Local { get; set; }

        private int? localId;
        public int? LocalId
        {
            get
            {
                return this.localId;
            }
            set
            {
                if (value > 0)
                    this.localId = value;
            }
        }

        [Computed]
        public string ContaContabilCodigo { get; set; }

        [Computed]
        public string ContaContabil { get; set; }

        private int? contaContabilId;

        public int? ContaContabilId
        {
            get
            {
                return this.contaContabilId;
            }
            set
            {
                if (value > 0)
                    this.contaContabilId = value;
            }
        }
        #endregion 

        #region Accounting Fields

        public string Documento { get; set; }

        private decimal? vidaUtilMeses;

        public decimal? VidaUtilMeses
        {
            get => vidaUtilMeses;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.vidaUtilMeses = value;
            }
        }


        private decimal? vidaUtilAnos;
        public decimal? VidaUtilAnos
        {
            get => vidaUtilAnos;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.vidaUtilAnos = value;
            }
        }


        private decimal? percentualRemanescenteVidaUtil;
        public decimal? PercentualRemanescenteVidaUtil
        {
            get => percentualRemanescenteVidaUtil;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.percentualRemanescenteVidaUtil = value;
            }
        }


        private decimal? mesesRemanecentes;
        public decimal? MesesRemanecentes
        {
            get => mesesRemanecentes;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.mesesRemanecentes = value;
            }
        }


        private decimal? anosRemanescentes;
        public decimal? AnosRemanescentes
        {
            get => anosRemanescentes;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.anosRemanescentes = value;
            }
        }



        private decimal? valorResidual;
        public decimal? ValorResidual
        {
            get => valorResidual;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.valorResidual = value;
            }
        }




        private decimal? valorDepreciavel;
        public decimal? ValorDepreciavel
        {
            get => valorDepreciavel;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.valorDepreciavel = value;
            }
        }




        private decimal? valorEmUso;
        public decimal? ValorEmUso
        {
            get => valorEmUso;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.valorEmUso = value;
            }
        }


        private decimal? testeRecuperabilidade;
        public decimal? TesteRecuperabilidade
        {
            get => testeRecuperabilidade;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.testeRecuperabilidade = value;
            }
        }



        private int? anoFabricacao;
        public int? AnoFabricacao
        {
            get => anoFabricacao;
            set
            {
                if (Convert.ToInt32(value) > 0)
                    this.anoFabricacao = value;
            }
        }



        private decimal? valorOriginal;
        public decimal? ValorOriginal
        {
            get => valorOriginal;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.valorOriginal = value;
            }
        }


        private decimal? depreciacaoAcumulada;
        public decimal? DepreciacaoAcumulada
        {
            get => depreciacaoAcumulada;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.depreciacaoAcumulada = value;
            }
        }



        private decimal? custoReposicao;
        public decimal? CustoReposicao
        {
            get => custoReposicao;
            set
            {
                if (Convert.ToDecimal(value) > 0)
                    this.custoReposicao = value;
            }
        }


        private DateTime? dataAquisicao;
        public DateTime? DataAquisicao
        {
            get
            {
                return this.dataAquisicao;
            }
            set
            {
                this.dataAquisicao = (value == null) ? (DateTime?)null : (DateTime)value;
            }
        }

        

        private DateTime? dataEntradaOperacao;
        public DateTime? DataEntradaOperacao
        {
            get
            {
                return this.dataEntradaOperacao;
            }
            set
            {                
                this.dataEntradaOperacao = (value == null) ? (DateTime?)null : (DateTime)value;
            }
        }
        #endregion
    }
}
