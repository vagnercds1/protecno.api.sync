using Newtonsoft.Json;
using protecno.api.sync.domain.models.generics;

namespace protecno.api.sync.domain.models
{
    public class ObjectResult
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string[] Message { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        [JsonProperty(PropertyName = "pagination")]
        public Pagination Pagination { get; set; }
    }
}
