using Python.Runtime;

namespace ServerLibrary.Helpers
{
    public sealed class PythonInitializer
    {
        private static readonly Lazy<PythonInitializer> instance = new Lazy<PythonInitializer>(() => new PythonInitializer());

        private PythonInitializer()
        {
            Runtime.PythonDLL = @"C:\Users\aramo\AppData\Local\Programs\Python\Python39\python39.dll";
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
        public static PythonInitializer Instance
        {
            get
            {
                return instance.Value;
            }
        }
        public Dictionary<string, string> GetKeywords(string text)
        {
            Dictionary<string, string> entities = new Dictionary<string, string>();
            text = text.ToUpper();
            using (Py.GIL())
            {
                dynamic spacy = Py.Import("spacy");
                dynamic nlp = spacy.load(Constants.PathModelNER);

                dynamic doc = nlp(text);
                foreach (dynamic ent in doc.ents)
                {
                    entities[(string)ent.text] = (string)ent.label_;
                }
            }
            return entities;
        }
    }
}

