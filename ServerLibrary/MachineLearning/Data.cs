using Microsoft.ML.Data;

namespace ServerLibrary.MachineLearning
{
    public class Data
    {
        [LoadColumn(0)]
        public string Question { get; set; }
        [LoadColumn(1)]
        public string Method { get; set; }
    }
}
