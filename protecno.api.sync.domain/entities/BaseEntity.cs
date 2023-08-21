using Dapper.Contrib.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace protecno.api.sync.domain.entities
{
    public class BaseEntity
    {          
        public int UsuarioId { get; set; }

        [Computed]
        public string Usuario { get; set; } 
       
        public int? OrigemId { get; set; }

        [Computed]
        public string Origem { get; set; } 

        public DateTime DataCadastro { get; set; }
         
        public DateTime DataAtualizacao { get; set; }

        [JsonIgnore]
        public DateTime DataSync { get; set; }
    }
}
