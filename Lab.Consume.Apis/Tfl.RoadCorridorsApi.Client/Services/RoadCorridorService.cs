using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tfl.RoadCorridorsApi.Client.Common;
using Tfl.RoadCorridorsApi.Client.Config;
using Tfl.RoadCorridorsApi.Client.Models;

namespace Tfl.RoadCorridorsApi.Client.Services
{
    public class RoadCorridorService : IRoadCorridorService
    {
        private readonly TflApiSettings _tflApiSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RoadCorridorService> _logger;

        public RoadCorridorService(IHttpClientFactory httpClientFactory, IOptions<TflApiSettings> tflApiSettings, ILogger<RoadCorridorService> logger)
        {
            Guard.Against<ArgumentNullException>(httpClientFactory == null, "httpClientFactory is null");
            Guard.Against<ArgumentNullException>(logger == null, "logger is null");
            Guard.Against<ArgumentNullException>(tflApiSettings == null, "tflApiSettings is null");

            _httpClientFactory = httpClientFactory;
            _tflApiSettings = tflApiSettings?.Value;
            _logger = logger;


            Guard.Against<ArgumentNullException>(string.IsNullOrEmpty(_tflApiSettings?.ApiId), "ApiId is null");
            Guard.Against<ArgumentNullException>(string.IsNullOrEmpty(_tflApiSettings?.ApiKey), "ApiKey is null");


        }

        public async Task<RoadCorridorResponse> GetRoadStatus(string roadId, CancellationToken cancellationToken)
        {
            Guard.Against<ArgumentNullException>(string.IsNullOrEmpty(roadId), "roadId is null");
   
            IList<RoadCorridor> roadCorridor;

            try
            {
                var httpClient = _httpClientFactory.CreateClient("TflRoadClient");

                var roadRequestUri = $"Road/{roadId}?app_id={_tflApiSettings.ApiId}&app_key={_tflApiSettings.ApiKey}";
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, roadRequestUri);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new RoadCorridorResponse
                    {
                        HttpStatus = HttpStatusCode.NotFound,
                        Message = $"The following road ids are not recognized: {roadId}"
                    };
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                roadCorridor = JsonConvert.DeserializeObject<List<RoadCorridor>>(content);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{exception.Source}:TFL Road Corridors: Failed to get road status.");
                return new RoadCorridorResponse
                {
                    HttpStatus = HttpStatusCode.InternalServerError
                };
            }

            return new RoadCorridorResponse
            {
                RoadCorridor = roadCorridor?[0],
                HttpStatus = HttpStatusCode.OK
            };
        }
    }
}
