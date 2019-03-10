using System.Net;

namespace Tfl.RoadCorridorsApi.Client.Models
{
    public class RoadCorridorResponse
    {
        public HttpStatusCode HttpStatus { get; set; }
        public string Message { get; set; }
        public RoadCorridor RoadCorridor { get; set; }
    }
}
