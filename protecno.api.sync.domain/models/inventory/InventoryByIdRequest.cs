using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace protecno.api.sync.domain.models.inventory
{
    public class InventoryByIdRequest
    {
        [JsonProperty(PropertyName = "register_id")]
        public int InventoryId { get; set; }

        [Required]
        public int BaseInventarioId { get; set; } 
    }
}
