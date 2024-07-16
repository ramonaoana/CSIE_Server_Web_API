using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.KernelMemory;
using Microsoft.ML;
using Newtonsoft.Json.Linq;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Levenshtein;
using ServerLibrary.MachineLearning;
using ServerLibrary.Repositories.Contracts;
using System.Reflection;
using System.Text.Json;
using ChatMessage = BaseLibrary.DTOs.ChatMessage;
using Constants = ServerLibrary.Helpers.Constants;

delegate Task<ChatResponse> MethodController(BaseLibrary.DTOs.ChatMessage chatMessage);

namespace ServerLibrary.Repositories.Implementations
{
    public class ChatMessageRepository : IChat
    {
        static string trainingFilePath = "C:\\Users\\aramo\\Desktop\\CHATBOT_SECRETARIAT\\CSIE\\ServerLibrary\\MachineLearning\\Data.txt";
        static string _modelFilePath = "C:\\Users\\aramo\\Desktop\\CHATBOT_SECRETARIAT\\CSIE\\ServerLibrary\\MachineLearning\\data.zip";
        static MLContext mLContext;
        static IDataView trainingDataView;
        static ITransformer model;
        static PredictionEngine<MachineLearning.Data, MethodPrediction> predictionEngine;

        static DbContextOptions<AppDbContext> options;
        static AppDbContext dbContext;

        static void Initialize()
        {
            options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Constants.ConnectionString)
                .Options;
            dbContext = new AppDbContext(options);
        }

        static MethodPrediction PredictMethodForSubjectLine(string subjectLine)
        {
            var modelFilePath = "C:\\Users\\aramo\\Desktop\\CHATBOT_SECRETARIAT\\CSIE\\ServerLibrary\\MachineLearning\\data.zip";
            var predictor = new MethodPredictor(modelFilePath);
            var predictedMethod = predictor.PredictMethodForSubjectLine(subjectLine);
            return predictedMethod;
        }

        static IEstimator<ITransformer> ProcessData()
        {
            var pipeline = mLContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Method", outputColumnName: "Label")
                  .Append(mLContext.Transforms.Text.FeaturizeText(inputColumnName: "Question", outputColumnName: "DataSubjectFeaturized"))
                  .Append(mLContext.Transforms.Concatenate("Features", "DataSubjectFeaturized"))
                  .AppendCacheCheckpoint(mLContext);
            return pipeline;
        }

        static IEstimator<ITransformer> BuildAndTrainModel(IDataView trainingDataView, IEstimator<ITransformer> pipeline)
        {
            var trainingPipeline = pipeline.Append(mLContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(mLContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
            model = trainingPipeline.Fit(trainingDataView);
            return trainingPipeline;
        }

        public async Task<GeneralResponse> TrainModelML()
        {
            mLContext = new MLContext(seed: 0);
            trainingDataView = mLContext.Data.LoadFromTextFile<MachineLearning.Data>(trainingFilePath, hasHeader: true);
            var pipeline = ProcessData();
            var trainingPipeline = BuildAndTrainModel(trainingDataView, pipeline);
            mLContext.Model.Save(model, trainingDataView.Schema, _modelFilePath);
            return new GeneralResponse(true, "Modelul a fost salvat");
        }

        public async Task<ResponseRequest<ChatResponse>> ClassifyMessage(ChatMessage chatMessage)
        {
            var result = PredictMethodForSubjectLine("New invoice");
            var prediction = PredictMethodForSubjectLine(chatMessage.Message);
            Initialize();
            float confidenceThreshold = 0.5f;
            float maxScore = prediction.Score.Max();
            string predictedMethod = "";
            if (maxScore < confidenceThreshold)
            {
                var findAnnounce = await FindAnnounce(chatMessage.Message);
                if (findAnnounce.Flag)
                {
                    ChatResponse resp = new ChatResponse()
                    {
                        Response = "S-a afisat urmatorul anunt la avizier: " + findAnnounce.obj,
                        ResponseType = (int)TypeMessage.SimpleMessage,
                        Document = null
                    };
                    return new ResponseRequest<ChatResponse>(true, resp);
                }
                else
                {
                    predictedMethod = "Unknown";
                    ChatResponse chatResponse = new ChatResponse()
                    {
                        Response = "Nu detin informatii despre acest subiect 🙁 Aveti posibilitatea de a accesa formularul nostru de contact si de a trimite un mesaj catre un reprezentant pentru a primi raspunsul la intrebare.",
                        ResponseType = (int)TypeMessage.UnknownMessage,
                        Document = null
                    };
                    return new ResponseRequest<ChatResponse>(true, chatResponse);
                }
            }
            else
            {
                predictedMethod = prediction.Method;
                ChatResponse response = await CallMethod(predictedMethod, chatMessage);
                return new ResponseRequest<ChatResponse>(true, response);
            }
        }

        static async Task<ChatResponse> CallMethod(string prediction, ChatMessage question)
        {

            MethodInfo method = typeof(ChatMessageRepository).GetMethod(prediction, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                MethodController methodDelegate = (MethodController)Delegate.CreateDelegate(typeof(MethodController), null, method);
                return await methodDelegate(question);
            }
            else
            {
                return null;
            }
        }



        public async Task<ResponseRequest<string>> FindAnnounce(string message)
        {
            Initialize();
            AnnounceRepository repository = new AnnounceRepository(dbContext);

            OpenAIRepository openAIRepository = new OpenAIRepository();
            OpenAIRequest request = new OpenAIRequest()
            {
                Prompt = "Te comporti ca o metoda si vei returna DOAR cuvantul cheie din urmatoarea intrebare fara alte explicatii: " + message,
                Content = ""
            };
            string result = await openAIRepository.CompleteSentence(request);
            JObject jsonObject = JObject.Parse(result);

            string content = (string)jsonObject["choices"][0]["message"]["content"];

            var query = await repository.GetAnnounce(content);

            string jsonString = "";
            if (query.Flag)
            {
                return new ResponseRequest<string>(true, query.obj.Content);
            }
            else return new ResponseRequest<string>(false, "Nu s-au gasit informatii");
            return new ResponseRequest<string>(true, content);
        }

        public async Task<ChatResponse> getStudentCertificate(ChatMessage message)
        {
            Initialize();
            bool isReasonValid = false;
            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var checkRole = await s.IsStudent(message.UserId);
            GeneralResponse generalResponse;
            byte[] document = null;
            if (checkRole.Flag)
            {
                if (checkRole.obj.Equals(true))
                {
                    var resultStatus = await s.CheckStatusUser(message.UserId);
                    if (!resultStatus.Flag)
                    {
                        generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
                    }
                    else
                    {
                        string reason = "";
                        var entities = await GetKeywordsNER(message);
                        if (entities != null)
                        {
                            foreach (var entity in entities)
                            {
                                if (entity.Value.Equals(Constants.REASON_REQUEST))
                                {
                                    reason = entity.Key;
                                    isReasonValid = true;
                                    break;
                                }
                            }

                            if (isReasonValid)
                            {
                                var response = await s.GenerateDocument(message.UserId, reason);
                                document = response.Bytes;
                                generalResponse = new GeneralResponse(true, "Adeverinta de student a fost generata!");
                            }
                            else generalResponse = new GeneralResponse(false, "Din pacate nu ati mentionat un motiv intemeiat pentru eliberarea adeverintei. Aceasta se elibereaza pentru locul de munca, servicii medicale, reduceri, etc. ");
                        }
                        else generalResponse = new GeneralResponse(false, "Nu s-a identificat motivul de eliberare al documentului in mesaj pentru ca n-a fost specificat sau nu este un motiv intemeiat");
                    }
                }
                else generalResponse = new GeneralResponse(false, "Nu se poate elibera o adeverinta de student pentru profesori");
            }
            else generalResponse = new GeneralResponse(false, "Nu a fost gasit rolul utilizatorului de student/profesor in baza de date");


            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.MessageWithDocument,
                    Response = generalResponse.Message,
                    Document = document
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizatorul care a trimis intrebarea " + message.Message + " Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit.",
                    Content = ""
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getEmailProfessor(ChatMessage message)
        {
            Initialize();
            var entities = await GetKeywordsNER(message);
            var messageResult = "";
            if (entities != null)
            {
                string name = "";
                bool containsName = false;
                foreach (var entity in entities)
                {
                    if (entity.Value.Equals("PROFESOR"))
                    {
                        name += entity.Key;
                        containsName = true;
                        break;
                    }
                }
                if (containsName)
                {
                    ProfessorRepository repository = new ProfessorRepository(dbContext);
                    var emailResult = await repository.GetEmailAddress(name);
                    if (emailResult.Flag)
                    {
                        messageResult = "Adresa de email a profesorului " + name + " este: " + emailResult.obj;
                    }
                    else messageResult = "Nu s-a gasit adresa de email a profesorului " + name;
                }
                else messageResult = "Nu a fost identificat numele profesorului";
            }
            else messageResult = "Nu a fost identificat numele niciunui profesor in intrebare.";

            ChatResponse response = new ChatResponse()
            {
                ResponseType = 1,
                Response = messageResult
            };
            return response;
        }
        public async Task<Dictionary<string, string>> GetKeywordsNER(ChatMessage chatMessage)
        {
            var result = PythonInitializer.Instance.GetKeywords(chatMessage.Message.ToUpper());
            Dictionary<string, string> finalResult = new Dictionary<string, string>();
            if (result != null)
            {
                foreach (var key in result.Keys)
                {
                    string keyword = key;
                    if (result[key] == "DISCIPLINA")
                    {
                        var getFullName = await ReplaceAbbreviation(key);
                        keyword = getFullName.obj;
                    }

                    if (result[key] == "DISCIPLINA" ||
                        result[key] == "MOTIV_ELIBERARE_DOCUMENT" ||
                        result[key] == "PROFESOR" ||
                        result[key] == "SPECIALIZARE" ||
                        result[key] == "ZI_SAPTAMANA" ||
                        result[key] == "CICLU")
                    {
                        try
                        {
                            string correctedEntity = AlgorithmLevenshtein.GetInstance().CorrectWord(keyword);
                            if (correctedEntity != null)
                            {
                                if (correctedEntity.Contains('\t'))
                                {
                                    correctedEntity = correctedEntity.Replace('\t', ' ');
                                }
                                finalResult.Add(correctedEntity, result[key]);
                            }
                        }
                        catch (Exception ex) { }
                    }
                    else finalResult.Add(key, result[key]);
                }
            }
            else return null;
            return finalResult;
        }

        public async Task<ResponseRequest<string>> TestClassifyMessage(ChatMessage chatMessage)
        {
            var result = PredictMethodForSubjectLine("New invoice");
            var prediction = PredictMethodForSubjectLine(chatMessage.Message);
            Initialize();
            float confidenceThreshold = 0.75f;
            float maxScore = prediction.Score.Max();
            string predictedMethod = "";
            if (maxScore < confidenceThreshold)
            {
                var findAnnounce = await FindAnnounce(chatMessage.Message);
                if (findAnnounce.Flag)
                {
                    predictedMethod = "FindAnnounce";
                }
                else
                {
                    predictedMethod = "Unknown";
                }
            }
            else
            {
                predictedMethod = prediction.Method;
            }
            return new ResponseRequest<string>(true, predictedMethod + " -> scor : " + maxScore);
        }

        public async Task<ChatResponse> getInfoScholarship(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.Informatii_Burse);
            return response;
        }

        public async Task<ChatResponse> getInfoExam(ChatMessage message)
        {
            Initialize();
            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var resultStatus = await s.CheckStatusUser(message.UserId);
            GeneralResponse generalResponse;
            string jsonString = "";

            if (!resultStatus.Flag)
            {
                generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
            }
            else
            {
                Dictionary<string, string> keywords = await GetKeywordsNER(message);
                string course = "";
                bool isMentionedCourse = false;
                var entities = await GetKeywordsNER(message);
                if (entities != null)
                {
                    foreach (var entity in entities)
                    {
                        if (entity.Value.Equals("DISCIPLINA"))
                        {
                            course = entity.Key;
                            isMentionedCourse = true;
                            break;
                        }
                    }

                    if (isMentionedCourse)
                    {
                        ExamRepository examRepository = new ExamRepository(dbContext);
                        var resultExam = await examRepository.GetInfoExam(message.UserId, course);
                        if (resultExam.Flag)
                        {
                            jsonString = JsonSerializer.Serialize(resultExam);
                            generalResponse = new GeneralResponse(true, jsonString);
                        }
                        else generalResponse = new GeneralResponse(false, "Nu s-au identificat informatiile despre examen");
                    }
                    else generalResponse = new GeneralResponse(false, "Nu am identificat disciplina pentru care se va sustine examenul");
                }
                else generalResponse = new GeneralResponse(false, "Nu am identificat disciplina");
            }
            if (generalResponse.Flag)
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = content,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Studentul a pus o intrebare pe chatbot si metoda aferenta a returnat urmatorul mesaj:" + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit raspunsul corespunzator la intrebare.",
                    Content = ""
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getExamGrade(ChatMessage message)
        {
            Initialize();
            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var resultStatus = await s.CheckStatusUser(message.UserId);
            GeneralResponse generalResponse;
            string jsonString = "";

            if (!resultStatus.Flag)
            {
                generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
            }
            else
            {
                Dictionary<string, string> keywords = await GetKeywordsNER(message);
                string course = "";
                bool isMentionedCourse = false;
                var entities = await GetKeywordsNER(message);
                if (entities != null)
                {
                    foreach (var entity in entities)
                    {
                        if (entity.Value.Equals("DISCIPLINA"))
                        {
                            course = entity.Key;
                            isMentionedCourse = true;
                            break;
                        }
                    }

                    if (isMentionedCourse)
                    {
                        ExamRepository examRepository = new ExamRepository(dbContext);
                        var resultExam = await examRepository.GetInfoExam(message.UserId, course);
                        if (resultExam.Flag)
                        {
                            var examGrade = await examRepository.GetExamGrade(resultExam.obj.ExamId);
                            if (examGrade.Flag)
                            {
                                jsonString = JsonSerializer.Serialize(examGrade.obj);
                                generalResponse = new GeneralResponse(true, jsonString);
                            }
                            else generalResponse = new GeneralResponse(false, "Momentan nu s-au afisat notele obtinute la examen"); ;

                        }
                        else generalResponse = new GeneralResponse(false, "Nu s-au identificat informatiile despre examen");

                    }
                    else generalResponse = new GeneralResponse(false, "Nu am identificat disciplina pentru care se va sustine examenul");
                }
                else generalResponse = new GeneralResponse(false, "Nu am identificat disciplina pentru care se va sustine examenul");
            }
            if (generalResponse.Flag)
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = content,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> CallMethodQueryPDF(ChatMessage message, int documentType)
        {
            var response = await QueryPdf(message, documentType);
            string messageResult = response.obj;
            if (messageResult == "INFO NOT FOUND")
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = "Nu s-au gasit informatiile cautate in baza de date.",
                    Document = null
                };
            }
            else return new ChatResponse()
            {
                ResponseType = (int)TypeMessage.SimpleMessage,
                Response = messageResult,
                Document = null
            };
        }

        public async Task<ChatResponse> CallMethodQuerySyllabus(ChatMessage message, int documentId)
        {
            var response = await QuerySyllabus(message, documentId);
            string messageResult = response.obj;
            if (messageResult == "INFO NOT FOUND")
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = "Nu s-au gasit informatiile cautate in baza de date.",
                    Document = null
                };
            }
            else return new ChatResponse()
            {
                ResponseType = (int)TypeMessage.SimpleMessage,
                Response = messageResult,
                Document = null
            };
        }

        public async Task<ChatResponse> getAmountScholarship(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.Cuantum_Burse);
            return response;
        }

        private async Task<ResponseRequest<string>> ReplaceAbbreviation(string disciplina)
        {
            Dictionary<string, string> abbreviations = new Dictionary<string, string>
             {
            { "POO", "PROGRAMARE ORIENTATA OBIECT" },
            { "BPC", "BAZELE PROGRAMARII CALCULATOARELOR" },
            { "BCO", "BAZELE CERCETARILOR OPERATIONALE" },
            { "ENGLEZA", "LIMBA ENGLEZA SI COMUNICARE DE SPECIALITATE" },
            { "BTI", "BAZELE TEHNOLOGIEI INFORMATIEI" },
            { "SPORT", "EDUCATIE FIZICA SI SPORT" },
            { "FRANCEZA", "LIMBA FRANCEZA SI COMUNICARE DE SPECIALITATE" },
            { "ATP", "ALGORITMI SI TEHNICI DE PROGRAMARE" },
            { "BCE", "BAZELE CIBERNETICII ECONOMICE" },
            { "BD", "BAZE DE DATE" },
            { "PROBABILITATI", "PROBABILITATI SI STATISTICA MATEMATICA" },
            { "MACRO", "MACROECONOMIE CANTITATIVA" },
            { "PEAG", "PROGRAMARE EVOLUTIVA SI ALGORITMI GENETICI" },
            { "SDD", "STRUCTURI DE DATE" },
            { "SGBD", "SISTEME DE GESTIUNE A BAZELOR DE DATE" },
            { "MRAI", "MANAGEMENTUL RISCULUI IN AFACERI INTERNATIONALE" },
            { "GFI", "GESTIUNEA FINANCIARA A INTREPRINDERII" },
            { "ANDROID", "DISPOZITIVE SI APLICATII MOBILE" },
            { "CTS", "CALITATE SI TESTARE SOFTWARE" },
            { "DAD", "DEZVOLTAREA APLICATIILOR DISTRIBUITE SI CLOUD" },
            { "TPI", "TEHNICI DE PROCESARE A IMAGINILIOR" },
            { "SBD", "SISTEME DE BAZE DE DATE" },
            { "GIS", "SISTEME INFORMATICE GEOGRAFICE" },
            { "SIG", "SISTEME INFORMATICE GEOGRAFICE" },
            { "PP", "PROCESARE PARALELA" }};

            if (abbreviations.TryGetValue(disciplina, out string value))
            {
                return new ResponseRequest<string>(true, value);
            }
            else
            {
                return new ResponseRequest<string>(false, null);
            }
        }

        public async Task<ChatResponse> getInfoCollegeAdmission(ChatMessage message)
        {
            Initialize();
            Dictionary<string, string> keywords = await GetKeywordsNER(message);
            string specialization = "";
            bool isPresent = false;
            var entities = await GetKeywordsNER(message);
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (entity.Value.Equals(Constants.SPECIALIZATION))
                    {
                        specialization = entity.Key;
                        isPresent = true;
                        break;
                    }
                }
                string messageResult = "";
                int docType;
                if (isPresent)
                {
                    if (specialization.ToUpper().Contains("INFO") || specialization.ToUpper().Contains("IE"))
                    {
                        docType = (int)DocumentTypeBD.Admitere_LICENTA_IE;
                    }
                    else if (specialization.ToUpper().Contains("CIB"))
                    {
                        docType = (int)DocumentTypeBD.ADMITERE_LICENTA_CIB;
                    }
                    else if (specialization.ToUpper().Contains("EN"))
                    {
                        docType = (int)DocumentTypeBD.Admitere_LICENTA_IE_ENG;
                    }
                    else docType = (int)DocumentTypeBD.Admitere_LICENTA_STAT;

                    var response = await CallMethodQueryPDF(message, docType);
                    return response;
                }
            }
            return new ChatResponse()
            {
                ResponseType = (int)TypeMessage.SimpleMessage,
                Response = "Nu am identificat specializarea pentru care doriti informatiile despre admitere",
                Document = null
            };
        }
        public async Task<ChatResponse> getScheduleProfessor(ChatMessage message)
        {
            Initialize();
            Dictionary<string, string> keywords = await GetKeywordsNER(message);
            string name = "";
            bool containsName = false;
            var entities = await GetKeywordsNER(message);
            GeneralResponse generalResponse;
            var jsonString = "";
            ResponseList<InfoSchedule> response = null;
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (entity.Value.Equals("PROFESOR"))
                    {
                        name = entity.Key;
                        containsName = true;
                        break;
                    }
                }
                if (containsName)
                {
                    ScheduleProfessorRepository scheduleProfessorRepository = new ScheduleProfessorRepository(dbContext);
                    var queryProfessor = await scheduleProfessorRepository.CheckProfessor(name);
                    if (queryProfessor.Flag)
                    {
                        var querySchedule = await scheduleProfessorRepository.GetSchedule(name);
                        response = querySchedule;
                        if (querySchedule.Flag)
                        {
                            List<InfoSchedule> infoSchedules = response.Items;
                            DataScheduleJson dataScheduleJson = new DataScheduleJson()
                            {
                                Message = "S-au obtinut urmatoarele informatii despre orarul profesorului: ",
                                DataSchedule = infoSchedules
                            };
                            jsonString = JsonSerializer.Serialize(dataScheduleJson);
                            generalResponse = new GeneralResponse(true, "Datele au fost preluate cu succes");
                        }
                        else generalResponse = new GeneralResponse(false, "Nu au fost identificate informatiile despre orarul profesorului");
                    }
                    else generalResponse = new GeneralResponse(false, "Nu a fost gasit profesorul in baza de date");
                }
                else generalResponse = new GeneralResponse(false, "Nu a fost identificat numele profesorului in intrebare");
            }
            else generalResponse = new GeneralResponse(false, "Nu a fost identificat numele profesorului in intrebare");
            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.JsonMessage,
                    Response = jsonString,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }


        public async Task<ChatResponse> getInfoMaster(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.Admitere_MASTERAT_PROGRAME);
            return response;
        }

        public async Task<ChatResponse> getInfoStudentCalendar(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.Calendar_Student);
            return response;
        }

        public async Task<ChatResponse> getInfoBachelorDegree(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.Lucrare_Licenta);
            return response;
        }

        public async Task<ChatResponse> getInfoMasterDegree(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.Lucrare_Disertatie);
            return response;
        }

        public async Task<ChatResponse> getInfoCourse(ChatMessage message)
        {
            Initialize();
            Dictionary<string, string> keywords = await GetKeywordsNER(message);
            string course = "";
            bool containsCourse = false;
            var entities = await GetKeywordsNER(message);
            GeneralResponse generalResponse = null;

            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (entity.Value.Equals("DISCIPLINA"))
                    {
                        course = entity.Key;
                        containsCourse = true;
                        break;
                    }
                }
                if (!containsCourse)
                {
                    generalResponse = new GeneralResponse(false, "Nu am identificat disciplina");
                }
                else
                {
                    CourseInfoRepository courseInfoRepository = new CourseInfoRepository(dbContext);
                    var queryCourse = await courseInfoRepository.GetCourse(course);
                    if (queryCourse.Flag)
                    {
                        DocumentRepository documentRepository = new DocumentRepository(dbContext);
                        var queryDocument = await documentRepository.GetDocument(queryCourse.obj.DocumentId);
                        if (queryDocument.Flag)
                        {
                            ChatResponse resp = await CallMethodQuerySyllabus(message, queryDocument.obj.DocumentId);
                            generalResponse = new GeneralResponse(true, resp.Response);
                        }
                        else generalResponse = new GeneralResponse(false, "Nu am gasit fisa disciplinei");
                    }
                    else generalResponse = new GeneralResponse(false, "Nu am identificat cursul dupa denumirea furnizata"); ;
                }
            }
            else generalResponse = new GeneralResponse(false, "Nu am obtinut informatiile despre disciplina pentru a interoga baza de date");
            TypeMessage typeMessage;
            if (generalResponse.Flag)
            {
                typeMessage = TypeMessage.SimpleMessage;
            }
            else typeMessage = TypeMessage.ErrorMessage;
            return new ChatResponse()
            {
                ResponseType = (int)typeMessage,
                Response = generalResponse.Message,
                Document = null
            };
        }

        public async Task<ChatResponse> getInfoBachelorDegreeInternship(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.Stagiu_Practica_Licenta);
            return response;
        }

        public async Task<ChatResponse> getInfoMasterInternship(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.Stagiu_Practica_Masterat);
            return response;
        }

        public async Task<ChatResponse> getInfoPlagiarism(ChatMessage message)
        {
            Initialize();
            var response = await CallMethodQueryPDF(message, (int)DocumentTypeBD.ANTIPLAGIAT);
            return response;
        }

        public async Task<ChatResponse> getSchedule(ChatMessage message)
        {
            Initialize();
            bool isReasonValid = false;

            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var resultStatus = await s.CheckStatusUser(message.UserId);
            GeneralResponse generalResponse;
            string jsonString = "";

            if (!resultStatus.Flag)
            {
                generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
            }
            else
            {
                ScheduleRepository scheduleRepository = new ScheduleRepository(dbContext);
                var schedule = await scheduleRepository.GetSchedule(message.UserId);
                if (!schedule.Flag)
                {
                    generalResponse = new GeneralResponse(false, "Nu s-au putut identifica informatiile solicitate in baza de date.");
                }
                else
                {
                    List<InfoSchedule> infoSchedules = schedule.Items;

                    DataScheduleJson dataScheduleJson = new DataScheduleJson()
                    {
                        Message = "S-au obtinut urmatoarele informatii despre orar: ",
                        DataSchedule = infoSchedules
                    };

                    jsonString = JsonSerializer.Serialize(dataScheduleJson);
                    generalResponse = new GeneralResponse(true, "Datele au fost preluate cu succes");
                }
            }
            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.JsonMessage,
                    Response = jsonString,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getScheduleDay(ChatMessage message)
        {
            Initialize();
            bool isReasonValid = false;

            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var resultStatus = await s.CheckStatusUser(message.UserId);
            GeneralResponse generalResponse;
            string jsonString = "";

            if (!resultStatus.Flag)
            {
                generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
            }
            else
            {
                Dictionary<string, string> keywords = await GetKeywordsNER(message);
                string day = "";
                bool containsDay = false;
                var entities = await GetKeywordsNER(message);
                if (entities != null)
                {
                    foreach (var entity in entities)
                    {
                        if (entity.Value.Equals("ZI_SAPTAMANA"))
                        {
                            day = entity.Key;
                            containsDay = true;
                            break;
                        }
                    }
                    if (!containsDay)
                    {
                        generalResponse = new GeneralResponse(false, "Nu am identificat ziua saptamanii");
                    }
                    else
                    {
                        if (day.Equals("MAINE", StringComparison.OrdinalIgnoreCase))
                        {
                            DayOfWeekType nextDay = GetNextDay();
                            day = GetDayOfWeekString(nextDay);
                        }
                        else if (day.Equals("POIMAINE", StringComparison.OrdinalIgnoreCase))
                        {
                            DayOfWeekType nextDay = GetDayAfterTomorrow();
                            day = GetDayOfWeekString(nextDay);
                        }

                        ScheduleRepository scheduleRepository = new ScheduleRepository(dbContext);
                        var schedule = await scheduleRepository.GetScheduleForOneDay(message.UserId, day);
                        if (!schedule.Flag)
                        {
                            generalResponse = new GeneralResponse(false, "Nu s-au gasit informatiile in baza de date");
                        }
                        else if (schedule.Items.Count() == 0)
                        {
                            generalResponse = new GeneralResponse(false, "Studentul nu are cursuri in ziua de " + day);
                        }
                        else
                        {
                            List<InfoSchedule> infoSchedules = schedule.Items;
                            DataScheduleJson dataScheduleJson = new DataScheduleJson()
                            {
                                Message = "S-au obtinut urmatoarele informatii despre orar: ",
                                DataSchedule = infoSchedules
                            };
                            jsonString = JsonSerializer.Serialize(dataScheduleJson);
                            generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                        }
                    }
                }
                else generalResponse = new GeneralResponse(false, "Nu s-au identificat ziua in intrebare. ");
            }
            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.JsonMessage,
                    Response = jsonString,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        private DayOfWeekType GetNextDay()
        {
            DateTime nextDay = DateTime.Today.AddDays(1);
            return (DayOfWeekType)nextDay.DayOfWeek;
        }

        private DayOfWeekType GetDayAfterTomorrow()
        {
            DateTime dayAfterTomorrow = DateTime.Today.AddDays(2);
            return (DayOfWeekType)dayAfterTomorrow.DayOfWeek;
        }

        private static string GetDayOfWeekString(DayOfWeekType dayOfWeekType)
        {
            switch (dayOfWeekType)
            {
                case DayOfWeekType.LUNI:
                    return "LUNI";
                case DayOfWeekType.MARTI:
                    return "MARTI";
                case DayOfWeekType.MIERCURI:
                    return "MIERCURI";
                case DayOfWeekType.JOI:
                    return "JOI";
                case DayOfWeekType.VINERI:
                    return "VINERI";
                case DayOfWeekType.SAMBATA:
                    return "SAMBATA";
                default:
                    throw new ArgumentException("Invalid DayOfWeekType value.");
            }
        }

        public async Task<ChatResponse> getScheduleDayAndHour(ChatMessage message)
        {
            Initialize();
            bool isReasonValid = false;

            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var resultStatus = await s.CheckStatusUser(message.UserId);
            GeneralResponse generalResponse;
            string jsonString = "";

            if (!resultStatus.Flag)
            {
                generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
            }
            else
            {
                Dictionary<string, string> keywords = await GetKeywordsNER(message);
                string day = "";
                bool containsDay = false;
                bool containsHour = false;
                var entities = await GetKeywordsNER(message);

                foreach (var entity in entities)
                {
                    if (entity.Value.Equals("ZI_SAPTAMANA"))
                    {
                        day = entity.Key;
                        containsDay = true;
                    }
                    if (entity.Value.Equals("TIMP"))
                    {
                        containsHour = true;
                    }
                }
                if (!containsDay && !containsHour)
                {
                    generalResponse = new GeneralResponse(false, "Nu am identificat ziua saptamanii si nici informatiile despre timp sau spatiu.");
                }
                else
                {
                    ScheduleRepository scheduleRepository = new ScheduleRepository(dbContext);
                    var schedule = await scheduleRepository.GetScheduleDayAndHour(message.UserId, ParamType.TIMP, day);
                    if (!schedule.Flag)
                    {
                        generalResponse = new GeneralResponse(false, "Nu s-au gasit informatiile in baza de date");
                    }
                    else
                    {
                        List<InfoSchedule> infoSchedules = schedule.Items;
                        OpenAIRepository openAIRepository = new OpenAIRepository();
                        string json = JsonSerializer.Serialize(infoSchedules);
                        OpenAIRequest request = new OpenAIRequest()
                        {
                            Prompt = "S-a interogat baza de date si s-au obtinut urmatoarele informatii: " + json + ".Formuleaza tu raspunsul pe baza informatiilor furnizate pentru intrebarea:",
                            Content = message.Message
                        };
                        string result = await openAIRepository.CompleteSentence(request);
                        JObject jsonObject = JObject.Parse(result);

                        string content = (string)jsonObject["choices"][0]["message"]["content"];
                        DataScheduleJson dataScheduleJson = new DataScheduleJson()
                        {
                            Message = content,
                            DataSchedule = infoSchedules
                        };
                        jsonString = JsonSerializer.Serialize(dataScheduleJson);
                        generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                    }
                }
            }
            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.JsonMessage,
                    Response = jsonString,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }
        public async Task<ChatResponse> getCoursesYear(ChatMessage message)
        {
            Initialize();
            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var resultStatus = await s.CheckStatusUser(message.UserId);
            GeneralResponse generalResponse;
            string jsonString = "";

            if (!resultStatus.Flag)
            {
                generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
            }
            else
            {
                Dictionary<string, string> keywords = await GetKeywordsNER(message);
                string year = "";
                bool isMentionedYear = false;
                var entities = await GetKeywordsNER(message);

                foreach (var entity in entities)
                {
                    if (entity.Value.Equals("ANUL"))
                    {
                        year = entity.Key;
                        isMentionedYear = true;
                    }
                }
                if (isMentionedYear)
                {
                    string numStr = year.Substring(5);
                    int yearNr;
                    if (int.TryParse(numStr, out yearNr))
                    {
                        CourseSemesterYearRepository courseSemesterYearRepository = new CourseSemesterYearRepository(dbContext);
                        var queryYear = await courseSemesterYearRepository.getAcademicYearByName(message.UserId, yearNr);
                        if (queryYear.Flag)
                        {
                            var queryCourses = await courseSemesterYearRepository.getYearCourses(message.UserId, queryYear.obj.Year);
                            if (queryCourses.Flag)
                            {
                                List<CoursesYearInfo> courses = queryCourses.Items;
                                ResultCourses result = new ResultCourses()
                                {
                                    Message = "In anul " + yearNr + " se vor sustine urmatoarele cursuri",
                                    Courses = courses
                                };
                                jsonString = JsonSerializer.Serialize(result);
                                generalResponse = new GeneralResponse(true, jsonString);
                            }
                            else generalResponse = new GeneralResponse(false, "Nu au fost identificate cursurile");
                        }
                        else generalResponse = new GeneralResponse(false, "Nu s-a gasit anul universitar in baza de date");
                    }
                    else generalResponse = new GeneralResponse(false, "Nu s-a gasit anul universitar");
                }
                else generalResponse = new GeneralResponse(false, "Nu ati furnizat anul universitar");
            }
            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.JsonMessage,
                    Response = jsonString,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getInfoScheduleCourse(ChatMessage message)
        {
            Initialize();
            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var resultStatus = await s.CheckStatusUser(message.UserId);
            GeneralResponse generalResponse;
            string jsonString = "";

            if (!resultStatus.Flag)
            {
                generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
            }
            else
            {
                Dictionary<string, string> keywords = await GetKeywordsNER(message);
                CourseType courseType = CourseType.Curs;
                string course = "";
                bool isMentionedCourseType = false;
                bool isMentionedCourse = false;
                var entities = await GetKeywordsNER(message);

                foreach (var entity in entities)
                {
                    if (entity.Value.Equals("CURS"))
                    {
                        courseType = CourseType.Curs;
                        isMentionedCourseType = true;
                    }
                    else if (entity.Value.Equals("SEMINAR"))
                    {
                        courseType = CourseType.Seminar;
                        isMentionedCourseType = true;
                    }
                    else if (entity.Value.Equals("DISCIPLINA"))
                    {
                        course = entity.Key;
                        isMentionedCourse = true;
                    }
                }

                if (!isMentionedCourseType && !isMentionedCourse)
                {
                    generalResponse = new GeneralResponse(false, "Nu a fost mentionata disciplina si tipul, respectiv curs/seminar.");
                }
                else
                {
                    ScheduleRepository scheduleRepository = new ScheduleRepository(dbContext);
                    var schedule = await scheduleRepository.GetScheduleCourse(message.UserId, courseType, course); ;

                    if (!schedule.Flag)
                    {
                        generalResponse = new GeneralResponse(false, "Nu s-a gasit informatia in baza de date");
                    }
                    else
                    {
                        List<InfoSchedule> infoSchedules = schedule.Items;
                        OpenAIRepository openAIRepository = new OpenAIRepository();
                        string json = JsonSerializer.Serialize(infoSchedules);
                        OpenAIRequest request = new OpenAIRequest()
                        {
                            Prompt = "S-a interogat baza de date si s-au obtinut urmatoarele informatii: " + json + ".Formuleaza tu raspunsul pe baza informatiilor furnizate pentru intrebarea:",
                            Content = message.Message
                        };
                        string result = await openAIRepository.CompleteSentence(request);
                        JObject jsonObject = JObject.Parse(result);

                        string content = (string)jsonObject["choices"][0]["message"]["content"];
                        DataScheduleJson dataScheduleJson = new DataScheduleJson()
                        {
                            Message = content,
                            DataSchedule = infoSchedules
                        };
                        jsonString = JsonSerializer.Serialize(dataScheduleJson);
                        generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                    }
                }
            }
            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.JsonMessage,
                    Response = jsonString,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getGroupsAndSeries(ChatMessage message)
        {
            Initialize();
            StudentCertificateRepository s = new StudentCertificateRepository(dbContext);
            var resultStatus = await s.CheckStatusUser(message.UserId);
            GeneralResponse generalResponse;
            string jsonString = "";
            string messageResponse = "";

            if (!resultStatus.Flag)
            {
                generalResponse = new GeneralResponse(false, "Studentul nu este inscris in anul universitar curent");
            }
            else
            {
                GroupSeriesRepository repository = new GroupSeriesRepository(dbContext);
                var query = await repository.GetGroupsAndSeries(message.UserId);
                if (!query.Flag)
                {
                    generalResponse = new GeneralResponse(false, "Nu s-au gasit informatii cu privire la grupe si serii in baza de date");
                }
                else
                {
                    List<InfoSeriesNameGroups> infoSeries = query.Items;
                    OpenAIRepository openAIRepository = new OpenAIRepository();
                    string json = JsonSerializer.Serialize(infoSeries);
                    OpenAIRequest request = new OpenAIRequest()
                    {
                        Prompt = "S-a interogat baza de date si s-au obtinut urmatoarele informatii: " + json + ".Formuleaza tu raspunsul pe baza informatiilor furnizate pentru intrebarea:",
                        Content = message.Message
                    };
                    string result = await openAIRepository.CompleteSentence(request);
                    JObject jsonObject = JObject.Parse(result);

                    string content = (string)jsonObject["choices"][0]["message"]["content"];
                    messageResponse = content;
                    generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                }
            }
            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = messageResponse,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getGroupsForSeries(ChatMessage message)
        {
            Initialize();
            GeneralResponse generalResponse;
            string jsonString = "";
            Dictionary<string, string> keywords = await GetKeywordsNER(message);
            string series = "";
            bool isMentionedSeries = false;
            var entities = await GetKeywordsNER(message);
            string messageResponse = "";

            foreach (var entity in entities)
            {
                if (entity.Value.Equals("SERIE"))
                {
                    series = entity.Key;
                    isMentionedSeries = true;
                    break;
                }
            }
            if (isMentionedSeries)
            {
                string[] words = series.Split(' ');
                string seriesName = words[1];
                GroupSeriesRepository repository = new GroupSeriesRepository(dbContext);
                var query = await repository.GetGroupsForSeries(message.UserId, seriesName);
                if (!query.Flag)
                {
                    generalResponse = new GeneralResponse(false, "Nu s-au gasit informatii cu privire la grupe si serii in baza de date");
                }
                else
                {
                    OpenAIRepository openAIRepository = new OpenAIRepository();
                    string json = JsonSerializer.Serialize(query.obj);
                    OpenAIRequest request = new OpenAIRequest()
                    {
                        Prompt = "S-a interogat baza de date si s-au obtinut urmatoarele informatii: " + json + ".Formuleaza tu raspunsul pe baza informatiilor furnizate pentru intrebarea:",
                        Content = message.Message
                    };
                    string result = await openAIRepository.CompleteSentence(request);
                    JObject jsonObject = JObject.Parse(result);

                    string content = (string)jsonObject["choices"][0]["message"]["content"];
                    messageResponse = content;
                    generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                }
            }
            else generalResponse = new GeneralResponse(false, "Nu a fost mentionata seria in intrebare");

            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = messageResponse,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getGroupsBySpecializationAndYear(ChatMessage message)
        {
            Initialize();
            GeneralResponse generalResponse = null;
            string jsonString = "";
            Dictionary<string, string> keywords = await GetKeywordsNER(message);
            string spec = "";
            bool isMentionedSpecialization = false;
            string year = "";
            bool isMentionedYear = false;

            var entities = await GetKeywordsNER(message);
            string messageResponse = "";

            foreach (var entity in entities)
            {
                if (entity.Value.Equals("SPECIALIZARE"))
                {
                    spec = entity.Key;
                    isMentionedSpecialization = true;
                }

                if (entity.Value.Equals("ANUL"))
                {
                    year = entity.Key;
                    isMentionedYear = true;
                }
            }
            if (isMentionedSpecialization && isMentionedYear)
            {
                string[] words = year.Split(' ');
                string yearString = words[1];
                bool nrIsValid = false;
                int yearNr = 0;
                try
                {
                    yearNr = int.Parse(yearString);
                    nrIsValid = true;
                }
                catch
                {
                }

                if (nrIsValid)
                {

                    GroupSeriesRepository repository = new GroupSeriesRepository(dbContext);
                    var query = await repository.GetGroupsBySpecializationAndYear(message.UserId, spec, yearNr);
                    if (!query.Flag)
                    {
                        generalResponse = new GeneralResponse(false, "Nu s-au gasit informatii cu privire la grupe si serii in baza de date");
                    }
                    else
                    {
                        OpenAIRepository openAIRepository = new OpenAIRepository();
                        string json = JsonSerializer.Serialize(query.Items);
                        OpenAIRequest request = new OpenAIRequest()
                        {
                            Prompt = "S-a interogat baza de date si s-au obtinut urmatoarele informatii: " + json + ".Formuleaza tu raspunsul pe baza informatiilor furnizate pentru intrebarea:",
                            Content = message.Message
                        };
                        string result = await openAIRepository.CompleteSentence(request);
                        JObject jsonObject = JObject.Parse(result);

                        string content = (string)jsonObject["choices"][0]["message"]["content"];
                        messageResponse = content;
                        generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                    }
                }
                else generalResponse = new GeneralResponse(false, "Nu s-a putut identifica anul");
            }
            else if (!isMentionedYear)
            {
                generalResponse = new GeneralResponse(false, "Nu a fost mentionat anul in intrebare");
            }
            else if (!isMentionedSpecialization)
            {
                generalResponse = new GeneralResponse(false, "Nu a fost mentionata specializarea in intrebare");
            }

            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = messageResponse,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getSeriesForSpecialization(ChatMessage message)
        {
            Initialize();
            GeneralResponse generalResponse = null;
            string jsonString = "";
            Dictionary<string, string> keywords = await GetKeywordsNER(message);
            string spec = "";
            bool isMentionedSpecialization = false;

            var entities = await GetKeywordsNER(message);
            string messageResponse = "";

            foreach (var entity in entities)
            {
                if (entity.Value.Equals("SPECIALIZARE"))
                {
                    spec = entity.Key;
                    isMentionedSpecialization = true;
                    break;
                }
            }
            if (isMentionedSpecialization)
            {
                GroupSeriesRepository repository = new GroupSeriesRepository(dbContext);
                var query = await repository.GetSeriesForSpecialization(message.UserId, spec);
                if (!query.Flag)
                {
                    generalResponse = new GeneralResponse(false, "Nu s-au gasit informatii cu privire la grupe si serii in baza de date");
                }
                else
                {
                    OpenAIRepository openAIRepository = new OpenAIRepository();
                    string json = JsonSerializer.Serialize(query.Items);
                    OpenAIRequest request = new OpenAIRequest()
                    {
                        Prompt = "S-a interogat baza de date si s-au obtinut urmatoarele informatii: " + json + ".Formuleaza tu raspunsul pe baza informatiilor furnizate pentru intrebarea:",
                        Content = message.Message
                    };
                    string result = await openAIRepository.CompleteSentence(request);
                    JObject jsonObject = JObject.Parse(result);

                    string content = (string)jsonObject["choices"][0]["message"]["content"];
                    messageResponse = content;
                    generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                }
            }
            else generalResponse = new GeneralResponse(false, "Nu s-a putut identifica specializarea");


            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = messageResponse,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getGroupsBySpecCycleAndYear(ChatMessage message)
        {
            Initialize();
            GeneralResponse generalResponse = null;
            string jsonString = "";
            Dictionary<string, string> keywords = await GetKeywordsNER(message);
            string spec = "";
            bool isMentionedSpecialization = false;
            string year = "";
            bool isMentionedYear = false;
            string cycle = "";
            bool isMentionedCycle = false;

            var entities = await GetKeywordsNER(message);
            string messageResponse = "";

            foreach (var entity in entities)
            {
                if (entity.Value.Equals("SPECIALIZARE"))
                {
                    spec = entity.Key;
                    isMentionedSpecialization = true;
                }

                if (entity.Value.Equals("ANUL"))
                {
                    year = entity.Key;
                    isMentionedYear = true;
                }

                if (entity.Value.Equals("CICLU"))
                {
                    cycle = entity.Key;
                    isMentionedCycle = true;
                }
            }
            if (isMentionedSpecialization && isMentionedYear && isMentionedCycle)
            {
                string[] words = year.Split(' ');
                string yearString = words[1];
                bool nrIsValid = false;
                int yearNr = 0;
                try
                {
                    yearNr = int.Parse(yearString);
                    nrIsValid = true;
                }
                catch
                {
                }

                if (nrIsValid)
                {

                    GroupSeriesRepository repository = new GroupSeriesRepository(dbContext);
                    var query = await repository.GetGroupsBySpecCycleAndYear(message.UserId, spec, cycle, yearNr);
                    if (!query.Flag)
                    {
                        generalResponse = new GeneralResponse(false, "Nu s-au gasit informatii cu privire la grupe si serii in baza de date");
                    }
                    else
                    {
                        OpenAIRepository openAIRepository = new OpenAIRepository();
                        string json = JsonSerializer.Serialize(query.Items);
                        OpenAIRequest request = new OpenAIRequest()
                        {
                            Prompt = "S-a interogat baza de date si s-au obtinut urmatoarele informatii: " + json + ".Formuleaza tu raspunsul pe baza informatiilor furnizate pentru intrebarea:",
                            Content = message.Message
                        };
                        string result = await openAIRepository.CompleteSentence(request);
                        JObject jsonObject = JObject.Parse(result);

                        string content = (string)jsonObject["choices"][0]["message"]["content"];
                        messageResponse = content;
                        generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                    }
                }
                else generalResponse = new GeneralResponse(false, "Nu s-a putut identifica anul");
            }
            else if (!isMentionedYear)
            {
                generalResponse = new GeneralResponse(false, "Nu a fost mentionat anul in intrebare");
            }
            else if (!isMentionedSpecialization)
            {
                generalResponse = new GeneralResponse(false, "Nu a fost mentionata specializarea in intrebare");
            }
            else if (!isMentionedCycle)
            {
                generalResponse = new GeneralResponse(false, "Nu a fost mentionat ciclul universitar (licenta/master) in intrebare");
            }

            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = messageResponse,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }

        public async Task<ChatResponse> getSeriesForGroup(ChatMessage message)
        {
            Initialize();
            GeneralResponse generalResponse;
            string jsonString = "";
            Dictionary<string, string> keywords = await GetKeywordsNER(message);
            string group = "";
            bool isMentionedGroup = false;
            var entities = await GetKeywordsNER(message);
            string messageResponse = "";

            foreach (var entity in entities)
            {
                if (entity.Value.Equals("GRUPA"))
                {
                    group = entity.Key;
                    isMentionedGroup = true;
                    break;
                }
            }
            if (isMentionedGroup)
            {
                string[] words = group.Split(' ');
                string groupString = words[1];
                int groupNr = 0;
                bool nrIsValid = false;
                try
                {
                    groupNr = int.Parse(groupString);
                    nrIsValid = true;
                }
                catch
                {
                    generalResponse = new GeneralResponse(false, "Nu s-a gasit numarul grupei in intrebare");
                }

                if (nrIsValid)
                {
                    GroupSeriesRepository repository = new GroupSeriesRepository(dbContext);
                    var query = await repository.GetSeriesForGroup(message.UserId, groupNr);
                    if (!query.Flag)
                    {
                        generalResponse = new GeneralResponse(false, "Nu s-au gasit informatii cu privire la grupe si serii in baza de date");
                    }
                    else
                    {
                        OpenAIRepository openAIRepository = new OpenAIRepository();
                        string json = JsonSerializer.Serialize(query.obj);
                        OpenAIRequest request = new OpenAIRequest()
                        {
                            Prompt = "S-a interogat baza de date si s-au obtinut urmatoarele informatii: " + json + ".Formuleaza tu raspunsul pe baza informatiilor furnizate pentru intrebarea:",
                            Content = message.Message
                        };
                        string result = await openAIRepository.CompleteSentence(request);
                        JObject jsonObject = JObject.Parse(result);

                        string content = (string)jsonObject["choices"][0]["message"]["content"];
                        messageResponse = content;
                        generalResponse = new GeneralResponse(true, "S-au preluat informatiile din baza de date");
                    }
                }
                else generalResponse = new GeneralResponse(false, "Nu a fost mentionat numarul grupei in intrebare");
            }
            else generalResponse = new GeneralResponse(false, "Numarul grupei este incorect");

            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.SimpleMessage,
                    Response = messageResponse,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }
        public async Task<ResponseRequest<string>> QueryPdf(ChatMessage chatMessage, int typeDocument)
        {
            Initialize();
            var id = 1;
            var memory = new KernelMemoryBuilder()
                    .WithOpenAIDefaults(Constants.OPENAI_KEY)
                    .Build<MemoryServerless>();

            DocumentRepository documentRepository = new DocumentRepository(dbContext);
            var query = await documentRepository.GetDocument(typeDocument);

            var doc = query.obj;

            byte[] documentBytes = doc.Content;
            string tempFilePath = Path.Combine(Path.GetTempPath(), doc.DocumentName + ".pdf");
            File.WriteAllBytes(tempFilePath, documentBytes);
            await memory.ImportDocumentAsync(tempFilePath, "doc" + id);
            id++;
            File.Delete(tempFilePath);
            bool isReadyAsync = true;
            for (int i = 1; i < id; i++)
            {
                if (!await memory.IsDocumentReadyAsync("doc" + i))
                {
                    isReadyAsync = false;
                    break;
                }
            }
            string message = "";
            if (isReadyAsync)
            {
                Func<string, Task<string>> Chat = async (string input) =>
                {
                    var answer = await memory.AskAsync(input);
                    return answer.Result;
                };
                try
                {
                    message = await Chat(chatMessage.Message);
                }
                catch (Exception ex)
                {
                    isReadyAsync = false;
                    message = "Nu s-au gasit informatii in documente.";

                }
            }
            return new ResponseRequest<string>(isReadyAsync, message);
        }

        public async Task<ResponseRequest<string>> QuerySyllabus(ChatMessage chatMessage, int documentId)
        {

            Initialize();
            var id = 1;
            var memory = new KernelMemoryBuilder()
                    .WithOpenAIDefaults(Constants.OPENAI_KEY)
                    .Build<MemoryServerless>();

            DocumentRepository documentRepository = new DocumentRepository(dbContext);
            var query = await documentRepository.GetSyllabus(documentId);
            if (query.Flag)
            {
                var doc = query.obj;

                byte[] documentBytes = doc.Content;
                string tempFilePath = Path.Combine(Path.GetTempPath(), doc.DocumentName + ".pdf");
                File.WriteAllBytes(tempFilePath, documentBytes);
                await memory.ImportDocumentAsync(tempFilePath, "doc" + id);
                id++;
                File.Delete(tempFilePath);
                bool isReadyAsync = true;
                for (int i = 1; i < id; i++)
                {
                    if (!await memory.IsDocumentReadyAsync("doc" + i))
                    {
                        isReadyAsync = false;
                        break;
                    }
                }
                string message = "";
                if (isReadyAsync)
                {
                    Func<string, Task<string>> Chat = async (string input) =>
                    {
                        var answer = await memory.AskAsync(input);
                        return answer.Result;
                    };
                    try
                    {
                        message = await Chat(chatMessage.Message);
                    }
                    catch (Exception ex)
                    {
                        isReadyAsync = false;
                        message = "Nu s-au gasit informatii in documente.";

                    }
                }
                return new ResponseRequest<string>(isReadyAsync, message);
            }
            else return new ResponseRequest<string>(query.Flag, "Nu s-a gasit fisa disciplinei");
        }

        public async Task<ChatResponse> getJobs(ChatMessage message)
        {
            Initialize();
            JobOpeningsRepository repository = new JobOpeningsRepository(dbContext);
            var query = await repository.GetJobOpenings();
            List<AnnouncementInfo> resultItems = new List<AnnouncementInfo>();
            string jsonString = "";
            GeneralResponse generalResponse = null;
            if (query.Flag)
            {
                string mess = "S-au identificat următoarele anunturi cu privire la oportunitățile de carieră:";
                resultItems = query.Items;
                DataJobs data = new DataJobs()
                {
                    Message = mess,
                    Items = resultItems
                };
                jsonString = JsonSerializer.Serialize(data);
                generalResponse = new GeneralResponse(true, mess);
            }
            else
            {
                generalResponse = new GeneralResponse(false, "Nu s-au putut identifica oportunitatile de angajare in baza de date");
            }

            if (generalResponse.Flag)
            {
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ItemsWithLink,
                    Response = jsonString,
                    Document = null
                };
            }
            else
            {
                OpenAIRepository openAIRepository = new OpenAIRepository();
                OpenAIRequest request = new OpenAIRequest()
                {
                    Prompt = "Te comporti ca un chatbot si imi vei trimite DOAR raspunsul formulat pentru utilizator. Pentru obtinerea informatiilor, metoda a returnat urmatorul rezultat: " + generalResponse.Message + ". Formuleaza raspunsul pentru utilizator pentru a intelege de ce nu a primit rezultatul dorit la intrebarea: ",
                    Content = message.Message
                };
                string result = await openAIRepository.CompleteSentence(request);
                JObject jsonObject = JObject.Parse(result);

                string content = (string)jsonObject["choices"][0]["message"]["content"];
                return new ChatResponse()
                {
                    ResponseType = (int)TypeMessage.ErrorMessage,
                    Response = content,
                    Document = null
                };
            }
        }
    }
}


