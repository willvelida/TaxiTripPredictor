using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using TaxiTripPredictor.ModelTrainer.Helpers;

namespace TaxiTripPredictor.ModelTrainer.Functions
{
    public class TrainRegressionModel
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly MLContext _mlContext;

        public TrainRegressionModel(
            ILogger<TrainRegressionModel> logger,
            IConfiguration config,
            MLContext mlContext)
        {
            _logger = logger;
            _config = config;
            _mlContext = mlContext;
        }

        [FunctionName(nameof(TrainRegressionModel))]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer)
        {
            try
            {
                CloudBlobClient cloudBlobClient = AzureStorageHelpers.ConnectToBlobClient(_config[Settings.STORAGE_ACCOUNT_NAME], _config[Settings.STORAGE_ACCOUNT_KEY]);
                CloudBlobContainer cloudBlobContainer = AzureStorageHelpers.GetBlobContainer(cloudBlobClient, _config[Settings.MODEL_CONTAINER_NAME]);

                // TODO: Can we download these from Blob Storage instead?
                string testDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "taxi-fare-test.csv");
                string trainDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "taxi-fare-train.csv");
                string modelPath = _config[Settings.MODEL_PATH];

                await ModelTrainerHelpers.TrainAndSaveModel(
                    _mlContext,
                    trainDataPath,
                    testDataPath,
                    modelPath,
                    cloudBlobContainer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong. Exception thrown: {ex.Message}");
                throw;
            }
        }
    }
}
