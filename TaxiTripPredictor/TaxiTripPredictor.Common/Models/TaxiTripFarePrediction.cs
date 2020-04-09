using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaxiTripPredictor.Common.Models
{
    public class TaxiTripFarePrediction
    {
        [ColumnName("Score")]
        public float FareAmount;
    }
}
