using Newtonsoft.Json;
using System.Collections.Generic;

namespace protecno.api.sync.domain.models
{
    public class ContainerResult<T> where T : class
    {
        [JsonProperty(PropertyName = "total")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "itens")]
        public List<T> ListItens { get; set; }
    }
}
