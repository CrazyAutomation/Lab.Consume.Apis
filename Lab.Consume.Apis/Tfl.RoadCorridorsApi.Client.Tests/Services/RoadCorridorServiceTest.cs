using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Should;
using Tfl.RoadCorridorsApi.Client.Config;
using Tfl.RoadCorridorsApi.Client.Models;
using Tfl.RoadCorridorsApi.Client.Services;

namespace Tfl.RoadCorridorsApi.Client.Tests.Services
{
    [TestFixture]
    public class RoadCorridorServiceTest
    {
        private Mock<IOptions<TflApiSettings>> _mockTflApiSettings;
        private Mock<ILogger<RoadCorridorService>> _mockLogger;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;

        [SetUp]
        public void Setup()
        {
            _mockTflApiSettings = new Mock<IOptions<TflApiSettings>>();
            _mockLogger = new Mock<ILogger<RoadCorridorService>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        }

        [Test]
        public void ConstructorShouldThrowArgumentNullExceptionWhenNullHttpClientFactoryPassed()
        {
            //arrange
            Action act = () => new RoadCorridorService(null,_mockTflApiSettings.Object,_mockLogger.Object);

            //assert
            act.ShouldThrow<ArgumentNullException>();
            
        }

        [Test]
        public void ConstructorShouldThrowArgumentNullExceptionWhenNullTflApiSettingsPassed()
        {
            //arrange
            Action act = () => new RoadCorridorService(_mockHttpClientFactory.Object, null, _mockLogger.Object);

            //assert
            act.ShouldThrow<ArgumentNullException>();

        }

        [Test]
        public void ConstructorShouldThrowArgumentNullExceptionWhenNullLoggerPassed()
        {
            //arrange
            Action act = () => new RoadCorridorService(_mockHttpClientFactory.Object, null, _mockLogger.Object);

            //assert
            act.ShouldThrow<ArgumentNullException>();

        }

        [Test]
        public void ConstructorShouldThrowArgumentNullExceptionWhenNullApiIdPassed()
        {
            //arrange
            _mockTflApiSettings.Setup<TflApiSettings>(x => x.Value).Returns(new TflApiSettings{ApiId = null,ApiKey = "89101123ABCDEFGH"});

            //act
            Action act = () => new RoadCorridorService(_mockHttpClientFactory.Object, _mockTflApiSettings.Object, _mockLogger.Object);

            //assert
            act.ShouldThrow<ArgumentNullException>();

        }
        [Test]
        public void ConstructorShouldThrowArgumentNullExceptionWhenNullApiKeyPassed()
        {
            //arrange
            _mockTflApiSettings.Setup<TflApiSettings>(x => x.Value).Returns(new TflApiSettings { ApiId = "1234567", ApiKey = null });

            //act
            Action act = () => new RoadCorridorService(_mockHttpClientFactory.Object, _mockTflApiSettings.Object, _mockLogger.Object);

            //assert
            act.ShouldThrow<ArgumentNullException>();

        }

        [Test]        
        public void GetRoadStatusMethodShouldThrowArgumentNullExceptionWhenNullRoadIdPassed()
        {
            //arrange
            _mockTflApiSettings.Setup<TflApiSettings>(x => x.Value).Returns(new TflApiSettings { ApiId = "12345678", ApiKey = "89101123ABCDEFGH" });
            var roadCorridorService = new RoadCorridorService(_mockHttpClientFactory.Object, _mockTflApiSettings.Object, _mockLogger.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(30000);

            //assert            
            async Task<RoadCorridorResponse> ActualValueDelegate() => await roadCorridorService.GetRoadStatus(null, cancellationTokenSource.Token);
            Assert.That(ActualValueDelegate, Throws.Exception.TypeOf<ArgumentNullException>());

        }

        [Test]
        public async Task WhenGivenAValidRoadIdGetRoadStatusReturnHttpStatusCode200AndRoadStatus()
        {
            //arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"$type\":\"Tfl.Api.Presentation.Entities.RoadCorridor, " +
                                                "Tfl.Api.Presentation.Entities\",\"id\":\"a2\",\"displayName\":\"" +
                                                "A2\",\"statusSeverity\":\"Good\",\"statusSeverityDescription\":\"" +
                                                "No Exceptional Delays\",\"bounds\":\"[[-0.0857,51.44091],[0.17118,51.49438]]\"," +
                                                "\"envelope\":\"[[-0.0857,51.44091],[-0.0857,51.49438],[0.17118,51.49438],[0.17118,51.44091],[-0.0857,51.44091]]\"," +
                                                "\"url\":\"/Road/a2\"}]")
                });

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                                  .Returns(new HttpClient(mockHttpMessageHandler.Object));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://test.com/"),
                Timeout = new TimeSpan(0, 0, 45)
            };
            httpClient.DefaultRequestHeaders.Clear();

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            _mockTflApiSettings.Setup<TflApiSettings>(x => x.Value).Returns(new TflApiSettings { ApiId = "12345678", ApiKey = "89101123ABCDEFGH" });

            var roadCorridorService = new RoadCorridorService(_mockHttpClientFactory.Object, _mockTflApiSettings.Object, _mockLogger.Object);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(30000);

            //act
            var roadCorridorResponse = await roadCorridorService.GetRoadStatus("A2", cancellationTokenSource.Token);

            //assert
            roadCorridorResponse.ShouldNotBeNull();
            ((int)roadCorridorResponse.HttpStatus).ShouldEqual(200);
            roadCorridorResponse.RoadCorridor.ShouldNotBeNull();
            roadCorridorResponse.RoadCorridor.DisplayName.ShouldEqual("A2");
            roadCorridorResponse.RoadCorridor.RoadStatus.ShouldEqual("Good");
            roadCorridorResponse.RoadCorridor.RoadStatusDescription.ShouldEqual("No Exceptional Delays");
        }


        [Test]
        public async Task WhenGivenAnInvalidRoadIdGetRoadStatusReturnHttpStatusCode404()
        {
            //arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(mockHttpMessageHandler.Object));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://test.com/"),
                Timeout = new TimeSpan(0, 0, 45)
            };
            httpClient.DefaultRequestHeaders.Clear();

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            _mockTflApiSettings.Setup<TflApiSettings>(x => x.Value).Returns(new TflApiSettings { ApiId = "12345678", ApiKey = "89101123ABCDEFGH" });

            var roadCorridorService = new RoadCorridorService(_mockHttpClientFactory.Object, _mockTflApiSettings.Object, _mockLogger.Object);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(30000);

            //act
            var roadCorridorResponse = await roadCorridorService.GetRoadStatus("A338", cancellationTokenSource.Token);

            //assert
            roadCorridorResponse.ShouldNotBeNull();
            ((int)roadCorridorResponse.HttpStatus).ShouldEqual(404);
            roadCorridorResponse.RoadCorridor.ShouldEqual(null);
        }
    }
}
