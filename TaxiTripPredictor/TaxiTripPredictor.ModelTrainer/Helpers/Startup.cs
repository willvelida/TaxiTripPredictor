using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaxiTripPredictor.ModelTrainer.Helpers;

[assembly: FunctionsStartup(typeof(Startup))]
namespace TaxiTripPredictor.ModelTrainer.Helpers
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

            builder.Services.AddSingleton(sp => new MLContext(seed: 0));
        }
    }
}
