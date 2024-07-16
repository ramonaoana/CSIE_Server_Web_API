
namespace ServerLibrary.Helpers
{
    public static class Constants
    {
        public static string Student { get; } = "Student";
        public static string Professor { get; } = "Professor";
        public static string ConnectionString { get; } = "Server=172.20.10.2,1433; Database=DB_CSIE; User Id= sa; password = admin; Trusted_Connection=False; Trust Server Certificate=True;";
        public static string PathModelNER { get; } = @"C:\Users\aramo\Desktop\CHATBOT_SECRETARIAT\CSIE\ServerLibrary\NER\ENTITIES_MODEL_NER\";
        public static string OPENAI_KEY { get; } = "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";
        public static string FILE_PATH { get; } = @"C:\\Users\\aramo\\Desktop\\CHATBOT_SECRETARIAT\\CSIE\\SERVER\\Files\\entities.txt";

        //CONSTANTS METHODS
        public static string Profesor { get; } = "PROFESOR";
        public static string Sala { get; } = "SALA";
        public static string Disciplina { get; } = "DISCIPLINA";
        public static string Curs { get; } = "C";
        public static string Seminar { get; } = "S";
        public static string REASON_REQUEST { get; } = "MOTIV_ELIBERARE_DOCUMENT";
        public static string SPECIALIZATION { get; } = "SPECIALIZARE";
    }
}
