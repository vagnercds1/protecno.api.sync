using Newtonsoft.Json;
using protecno.api.sync.domain.entities;
using System.Collections.Generic;

namespace protecno.api.sync.domain.models.inventory
{
    public class InventoryResult : Inventory
    {
        [JsonProperty(PropertyName = "message")]
        public string ResultMessage { get; set; }

        [JsonProperty(PropertyName = "history_list")]
        public List<History> HistoryList { get; set; }

        [JsonProperty(PropertyName = "annotations")]
        public List<Annotation> Annotations { get; set; }
    }
}
