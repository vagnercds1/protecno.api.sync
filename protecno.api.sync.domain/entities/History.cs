using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;

namespace protecno.api.sync.domain.entities
{
    [Table("historico")]
    public class History
    {
        [JsonProperty(PropertyName = "register_type")]
        public string TipoRegistro { get; set; }

        [JsonProperty(PropertyName = "register_id")]
        public int RegistroId { get; set; } 

        [JsonProperty(PropertyName = "fild")]
        public string CampoAlterado { get; set; }

        [JsonProperty(PropertyName = "from")]
        public string De { get; set; }

        [JsonProperty(PropertyName = "to")]
        public string Para { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Mensagem { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public int UsuarioId { get; set; }

        [Computed]
        [JsonProperty(PropertyName = "user")]
        public string Usuario { get; set; }

        [JsonProperty(PropertyName = "origin_id")]
        public int OrigemId { get; set; }

        [Computed]
        [JsonProperty(PropertyName = "origin")]
        public string Origem { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime DataAtualizacao { get; set; }

        public string TransactionId { get; set; }
    }
}
