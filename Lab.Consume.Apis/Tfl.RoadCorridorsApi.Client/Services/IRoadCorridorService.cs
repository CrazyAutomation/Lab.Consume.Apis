using System.Threading;
using System.Threading.Tasks;
using Tfl.RoadCorridorsApi.Client.Models;

namespace Tfl.RoadCorridorsApi.Client.Services
{
    public interface IRoadCorridorService
    {
        Task<RoadCorridorResponse> GetRoadStatus(string roadId, CancellationToken cancellationToken);
    }
}
