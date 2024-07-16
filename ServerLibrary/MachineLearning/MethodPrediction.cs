using Microsoft.ML.Data;

namespace ServerLibrary.MachineLearning
{
    public class MethodPrediction
    {
        [ColumnName("PredictedLabel")]
        public string? Method { get; set; }

        public float[] Score { get; set; }
    }

}
