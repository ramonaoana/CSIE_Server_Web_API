using ServerLibrary.Helpers;

namespace ServerLibrary.Levenshtein
{
    public class AlgorithmLevenshtein
    {
        private static readonly Lazy<AlgorithmLevenshtein> instance = new Lazy<AlgorithmLevenshtein>(() => new AlgorithmLevenshtein());
        private readonly HashSet<string> dictionary;

        private AlgorithmLevenshtein()
        {
            string dictionaryFilePath = Constants.FILE_PATH;
            dictionary = new HashSet<string>(File.ReadAllLines(dictionaryFilePath));
        }
        public static AlgorithmLevenshtein GetInstance()
        {
            return instance.Value;
        }

        public string CorrectWord(string inputWord)
        {
            string closestWord = inputWord;
            int shortestDistance = int.MaxValue;

            foreach (var word in dictionary)
            {
                if (inputWord == null || word == null || inputWord.Length == 0 || word.Length == 0)
                {
                    closestWord = inputWord;
                }
                else
                {
                    int distance = LevenshteinDistance(inputWord, word);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestWord = word;
                    }
                }
            }
            return closestWord;
        }


        private int LevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[s.Length, t.Length];
        }
    }
}
