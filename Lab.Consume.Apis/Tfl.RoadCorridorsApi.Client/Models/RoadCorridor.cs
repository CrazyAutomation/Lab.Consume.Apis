using Newtonsoft.Json;

namespace Tfl.RoadCorridorsApi.Client.Models
{
    public class RoadCorridor
    {
        public string DisplayName { get; set; }
        [JsonProperty("StatusSeverity")]
        public string RoadStatus { get; set; }
        [JsonProperty("StatusSeverityDescription")]
        public string RoadStatusDescription { get; set; }
    }
}
