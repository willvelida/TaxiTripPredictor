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
using System.Linq;

namespace TaxiTripPredictor.API.Functions
{
    public class DeleteTaxiPrediction
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosClient _cosmosClient;

        private Container _taxiContainer;

        public DeleteTaxiPrediction(
            ILogger<DeleteTaxiPrediction> logger,
            IConfiguration config,
            CosmosClient cosmosClient)
        {
            _logger = logger;
            _config = config;
            _cosmosClient = cosmosClient;

            _taxiContainer = _cosmosClient.GetContainer(_config[Settings.COSMOS_DATABASE_NAME], _config[Settings.COSMOS_COLLECTION_NAME]);
        }

        [FunctionName(nameof(DeleteTaxiPrediction))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "TaxiPrediction/{id}")] HttpRequest req,
            string id)
        {
            IActionResult result = null;

            try
            {
                QueryDefinition getContactQueryDefinition = new QueryDefinition(
                    $"SELECT * FROM {_taxiContainer.Id} c WHERE c.id = @id")
                    .WithParameter("@id", id);

                FeedIterator<TaxiTripDTO> getResultSet = _taxiContainer.GetItemQueryIterator<TaxiTripDTO>
                    (
                        getContactQueryDefinition,
                        requestOptions: new QueryRequestOptions()
                        {
                            MaxItemCount = 1
                        }
                    );

                if (getResultSet == null)
                {
                    _logger.LogInformation($"TaxiTrip {id} not found!");
                    result = new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                while (getResultSet.HasMoreResults)
                {
                    FeedResponse<TaxiTripDTO> response = await getResultSet.ReadNextAsync();
                    TaxiTripDTO contact = response.First();
                    ItemResponse<TaxiTripDTO> itemResponse = await _taxiContainer.DeleteItemAsync<TaxiTripDTO>
                        (id: id, partitionKey: new PartitionKey(contact.VendorId));
                    result = new OkResult();
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
