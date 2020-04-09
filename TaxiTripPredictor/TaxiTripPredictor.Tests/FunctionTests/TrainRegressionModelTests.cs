using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using TaxiTripPredictor.ModelTrainer.Functions;
using TaxiTripPredictor.ModelTrainer.Helpers;
using Xunit;

namespace TaxiTripPredictor.Tests.FunctionTests
{
    public class TrainRegressionModelTests
    {
        private Mock<ILogger<TrainRegressionModel>> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private Mock<MLContext> _mlContextMock;

        private TrainRegressionModel _func;

        public TrainRegressionModelTests()
        {
            _loggerMock = new Mock<ILogger<TrainRegressionModel>>();
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c[Settings.STORAGE_ACCOUNT_NAME]).Returns("storageaccount");
            _configMock.Setup(c => c[Settings.STORAGE_ACCOUNT_KEY]).Returns("storagekey");
            _configMock.Setup(c => c[Settings.MODEL_PATH]).Returns("modelpath");
            _configMock.Setup(c => c[Settings.MODEL_CONTAINER_NAME]).Returns("modelcontainer");
            _mlContextMock = new Mock<MLContext>();

            _func = new TrainRegressionModel(
                _loggerMock.Object,
                _configMock.Object,
                _mlContextMock.Object);
        }

        [Fact]
        public void train_regression_model_happy_path()
        {

        }

        [Fact]
        public void train_regression_model_should_throw_exception()
        {

        }
    }
}
