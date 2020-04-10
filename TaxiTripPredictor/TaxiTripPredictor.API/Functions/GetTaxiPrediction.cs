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
using TaxiTripPredictor.API.Helpers;
using TaxiTripPredictor.API.Models;

namespace TaxiTripPredictor.API.Functions
{
    public class GetTaxiPrediction
    {
        private readonly ILogger<GetTaxiPrediction> _logger;
        private readonly IConfiguration _config;
        private CosmosClient _cosmosClient;

        private Container _taxiContainer;

        public GetTaxiPrediction(
            ILogger<GetTaxiPrediction> logger,
            IConfiguration config,
            CosmosClient cosmosClient)
        {
            _logger = logger;
            _config = config;
            _cosmosClient = cosmosClient;

            _taxiContainer = _cosmosClient.GetContainer(_config[Settings.COSMOS_DATABASE_NAME], _config[Settings.COSMOS_COLLECTION_NAME]);
        }

        [FunctionName(nameof(GetTaxiPrediction))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TaxiPrediction/{id}")] HttpRequest req,
            string id)
        {
            IActionResult result = null;

            try
            {
                QueryDefinition query = new QueryDefinition(
                    $"SELECT * FROM {_taxiContainer.Id} c WHERE c.id = @id")
                    .WithParameter("@id", id);

                FeedIterator<TaxiTripDTO> iterator = _taxiContainer.GetItemQueryIterator<TaxiTripDTO>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(id),
                        MaxItemCount = 1
                    });

                if (iterator == null)
                {
                    _logger.LogInformation($"TaxiTrip {id} not found!");
                    result = new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                while (iterator.HasMoreResults)
                {
                    FeedResponse<TaxiTripDTO> taxiTripDTO = await iterator.ReadNextAsync();
                    result = new OkObjectResult(taxiTripDTO);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
