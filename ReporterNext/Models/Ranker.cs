using Newtonsoft.Json;

namespace ReporterNext.Models
{
    public class Ranker
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("rule")]
        public string Rule { get; set; }

        [JsonProperty("origin")]
        public long Origin { get; set; }
    }
}
