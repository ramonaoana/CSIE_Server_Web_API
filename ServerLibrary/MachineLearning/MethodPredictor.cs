using Microsoft.ML;

namespace ServerLibrary.MachineLearning
{
    public class MethodPredictor
    {
        private readonly string _modelFilePath;
        private readonly MLContext _mLContext;
        private PredictionEngine<MachineLearning.Data, MethodPrediction> _predictionEngine;

        public MethodPredictor(string modelFilePath)
        {
            _modelFilePath = modelFilePath;
            _mLContext = new MLContext();
            LoadModel();
        }

        private void LoadModel()
        {
            ITransformer model = _mLContext.Model.Load(_modelFilePath, out var modelInputSchema);
            _predictionEngine = _mLContext.Model.CreatePredictionEngine<MachineLearning.Data, MethodPrediction>(model);
        }

        public MethodPrediction PredictMethodForSubjectLine(string subjectLine)
        {
            var dataSubject = new MachineLearning.Data() { Question = subjectLine };
            var result = _predictionEngine.Predict(dataSubject);
            return result;
        }
    }
}
