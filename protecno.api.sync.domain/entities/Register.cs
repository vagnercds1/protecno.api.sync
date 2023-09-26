using Newtonsoft.Json;
using protecno.api.sync.domain.enumerators;
using System;
using System.ComponentModel.DataAnnotations;

namespace protecno.api.sync.domain.entities
{
    public class Register : BaseEntity
    { 
        public int? BaseInventarioId { get; set; }
                   
        public int? Id { get; set; }

        [JsonIgnore]
        public EInformationType? InformationType { get; set; }
         
        public bool? Ativo { get; set; }
         
        public string Codigo { get; set; }
         
        public string Descricao { get; set; } 
    }
}
