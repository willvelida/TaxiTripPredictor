using Microsoft.Azure.Storage.Blob;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaxiTripPredictor.Common.Models;

namespace TaxiTripPredictor.ModelTrainer.Helpers
{
    public static class ModelTrainerHelpers
    {
        public static async Task TrainAndSaveModel(
            MLContext mlContext,
            string trainFilePath,
            string testFilePath,
            string modelPath,
            CloudBlobContainer blobContainer)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(
                trainFilePath,
                hasHeader: true,
                separatorChar: ',');

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "FareAmount")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "VendorIdEncoded", inputColumnName: "VendorId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "RateCodeEncoded", inputColumnName: "RateCode"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "PaymentTypeEncoded", inputColumnName: "PaymentType"))
                .Append(mlContext.Transforms.Concatenate("Features", "VendorIdEncoded", "RateCodeEncoded", "PassengerCount", "TripDistance", "PaymentTypeEncoded"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);

            // TODO: Extend the EvaluateModel method to return other types of metrics
            var modelMetrics = EvaluateModel(mlContext, model, testFilePath);

            // TODO: Fine tune acceptance criteria for model
            if (modelMetrics > 0.7)
            {
                mlContext.Model.Save(model, dataView.Schema, modelPath);
                await AzureStorageHelpers.UploadBlobToStorage(blobContainer, modelPath);
            }
            else
            {
                // POOR FIT
            }
        }

        private static double EvaluateModel(MLContext mlContext, ITransformer model, string testFilePath)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(testFilePath, hasHeader: true, separatorChar: ',');

            var predictions = model.Transform(dataView);

            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

            double rSquaredValue = metrics.RSquared;

            return rSquaredValue;
        }
    }
}
