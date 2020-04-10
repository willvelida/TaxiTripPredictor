using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.ML;
using TaxiTripPredictor.Common.Models;
using TaxiTripPredictor.API.Helpers;
using TaxiTripPredictor.API.Models;

namespace TaxiTripPredictor.API.Functions
{
    public class CreateTaxiPrediction
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosClient _cosmosClient;
        private readonly PredictionEnginePool<TaxiTrip, TaxiTripFarePrediction> _predictionEnginePool;

        private Container _taxiContainer;

        public CreateTaxiPrediction(
            ILogger<CreateTaxiPrediction> logger,
            IConfiguration config,
            CosmosClient cosmosClient,
            PredictionEnginePool<TaxiTrip, TaxiTripFarePrediction> predictionEnginePool)
        {
            _logger = logger;
            _config = config;
            _cosmosClient = cosmosClient;
            _predictionEnginePool = predictionEnginePool;

            _taxiContainer = _cosmosClient.GetContainer(_config[Settings.COSMOS_DATABASE_NAME], _config[Settings.COSMOS_COLLECTION_NAME]);
        }

        [FunctionName(nameof(CreateTaxiPrediction))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "/TaxiPrediction")] HttpRequest req)
        {
            IActionResult result = null;

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var input = JsonConvert.DeserializeObject<TaxiTrip>(requestBody);

                TaxiTripFarePrediction prediction = _predictionEnginePool.Predict(
                    modelName: "TaxiTripModel",
                    example: input);

                var insertedPrediction = new TaxiTripDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    VendorId = input.VendorId,
                    RateCode = input.RateCode,
                    PassengerCount = input.PassengerCount,
                    TripTime = input.TripTime,
                    TripDistance = input.TripDistance,
                    PaymentType = input.PaymentType,
                    FareAmount = input.FareAmount,
                    PredictedFareAmount = prediction.FareAmount
                };

                ItemResponse<TaxiTripDTO> response = await _taxiContainer.CreateItemAsync(
                    insertedPrediction,
                    new PartitionKey(insertedPrediction.VendorId));

                _logger.LogInformation($"Inserting prediction cost: {response.RequestCharge} RU/s");
                result = new StatusCodeResult(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong. Exception thrown: {ex.Message}");
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
