using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using F23.StringSimilarity;

namespace NLPtesting
{
    class Program
    {
        static void Main(string[] args)
        {

            List<List<string>> Inputs = new List<List<string>>();

            using (StreamReader sr = new StreamReader("D:\\Projetos\\Clientes\\Santander\\Similaridade\\blip-intentions5.txt"))
            {
                String line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split('\t');
                    Inputs.Add(new List<string> { parts[0], parts[1] });
                }
            }


            List<List<string>> examplesAnalysed = new List<List<string>>();
            List<List<string>> examples = new List<List<string>>();

            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i][0].Equals("Segunda via"))
                {
                    examplesAnalysed.Add(Inputs[i]);
                }
                else
                {
                    examples.Add(Inputs[i]);
                }
            }

            examplesAnalysed.ToArray();
            examples.ToArray();

            var dist1 = new JaroWinkler();
            var dist2 = new Cosine();
            List<string> text = new List<string>();
            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();
            foreach (var answer in examplesAnalysed)
            {
                //Console.WriteLine(answer[0] + " - " + answer[1]);
                text.Add(answer[0] + '\t' + answer[1] + '\n');
                var processedAnswer = RemoveAcentuation(answer[1].ToLower());
                var filteredOptions = examples
                    .Select(option => RemoveAcentuation(option[1].ToLower()))
                    .Select(option => new KeyValuePair<string, double>(option, (dist1.Distance(option, processedAnswer) + dist2.Distance(option, processedAnswer))/2))
                    .Where(keyvalue => keyvalue.Value < 0.4)
                    .OrderBy(option => option.Value);


                for (int i = 0; i < filteredOptions.ToArray().Length; i++)
                {

                    text.Add('\t' + filteredOptions.ToArray()[i].Key + '\t' + filteredOptions.ToArray()[i].Value + '\n');
                    // Console.WriteLine($@"    {filteredOptions.ToArray()[i].Key} - {filteredOptions.ToArray()[i].Value}");
                }

                // Console.WriteLine("-----------------------------");
            }
            watch.Stop();

            using (StreamWriter writer = new StreamWriter("D:\\Projetos\\Clientes\\Santander\\Similaridade\\Resultados.txt"))
            {
                for (int i = 0; i < text.Count; i++)
                {
                    writer.Write(text[i]);
                }
            }


            Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Number of examples: {text.Count}");
            Console.ReadKey();
        }

        static string RemoveAcentuation(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}