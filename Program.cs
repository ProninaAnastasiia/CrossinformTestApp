using System.Collections.Concurrent;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        //string filePath = @"C:\Users\soul_\Desktop\aaa.txt";
        
        var sw = new Stopwatch();
        sw.Start();
        
        if (args.Length == 0)
        {
            Console.WriteLine("Необходимо указать путь к текстовому файлу.");
            return;
        }
        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Указанный файл не существует.");
            return;
        }
        string text = File.ReadAllText(filePath);
        
        string[] words = text.Split(new char[] { ' ', ',', ';', '.', ':', '!' }, StringSplitOptions.RemoveEmptyEntries);

        int wordsCount = words.Length;
        int threadsCount = Environment.ProcessorCount; // доступное количество потоков

        var tasks = new List<Task<IDictionary<string, int>>>();

        if (threadsCount <= wordsCount)
        {
            for (int i = 0; i < threadsCount; i++)
            {
                int startIndex = i * wordsCount / threadsCount;
                int endIndex = (i + 1) * wordsCount / threadsCount;

                tasks.Add(Task.Run(() => TripletsMaker(words[startIndex..endIndex])));
            }
        }
        else
        {
            for (int i = 0; i < wordsCount; i++)
            {
                tasks.Add(Task.Run(() => TripletsMaker(words[i..i++])));
            }
        }

        Task.WhenAll(tasks).Wait();

        // Слияние результатов всех задач в один словарь с количеством для каждого триплета
        var mergedResult = new Dictionary<string, int>();
        foreach (var task in tasks)
        {
            var partialResult = task.Result;
            foreach (var kvp in partialResult)
            {
                if (mergedResult.ContainsKey(kvp.Key))
                {
                    mergedResult[kvp.Key] += kvp.Value;
                }
                else
                {
                    mergedResult[kvp.Key] = kvp.Value;
                }
            }
        }

        // 10 самых часто встречающихся в тексте триплетов
        var topTriplets = mergedResult.OrderByDescending(kvp => kvp.Value).Take(10).Select(kvp => kvp.Key);
        
        Console.WriteLine("10 самых часто встречающихся в тексте триплетов: "+string.Join(", ", topTriplets));
        sw.Stop();
        Console.WriteLine("Время работы программы в миллисекундах: " + sw.ElapsedMilliseconds);
    }

    static IDictionary<string, int> TripletsMaker(string[] words)
    {
        var triplets = new ConcurrentDictionary<string, int>();
        var length = 3;

        foreach (var word in words)
        {
            for (int i = length; i <= word.Length; i++) 
                triplets.AddOrUpdate(word.Substring(i - length, length), 1, (_, count) => count + 1);
        }

        return triplets;
    }
    
    
}