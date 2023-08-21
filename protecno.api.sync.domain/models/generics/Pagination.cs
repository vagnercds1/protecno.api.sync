using Newtonsoft.Json;

namespace protecno.api.sync.domain.models.generics
{
    public class Pagination
    {
        [JsonProperty(PropertyName = "page")]
        public int Page { get; set; }

        [JsonProperty(PropertyName = "page_size")]
        public int PageSize { get; set; }

        [JsonProperty(PropertyName = "order_fild")]
        public string OrderFild { get; set; }

        [JsonProperty(PropertyName = "order")]
        public string Order { get; set; }

        [JsonProperty(PropertyName = "total")]
        public int Total { get; set; }
    }
}
