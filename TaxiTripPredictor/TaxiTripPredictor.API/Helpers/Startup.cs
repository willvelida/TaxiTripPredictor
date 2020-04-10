using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaxiTripPredictor.API.Helpers;
using TaxiTripPredictor.Common.Models;

[assembly: FunctionsStartup(typeof(Startup))]
namespace TaxiTripPredictor.API.Helpers
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });

            var config = (IConfiguration)builder.Services.First(s => s.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            builder.Services.AddPredictionEnginePool<TaxiTrip, TaxiTripFarePrediction>()
                .FromUri(
                modelName: "TaxiTripModel",
                uri: config[Settings.MODEL_URL],
                period: TimeSpan.FromMinutes(1));

            builder.Services.AddSingleton((sp) =>
            {
                CosmosClientBuilder cosmosClientBuilder = new CosmosClientBuilder(config[Settings.COSMOS_CONNECTION_STRING]);

                return cosmosClientBuilder.WithConnectionModeDirect()
                    .Build();
            });
        }
    }
}
