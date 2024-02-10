using System.Text;
using System.Text.RegularExpressions;

namespace DotMarkov;

public class MarkovGenerator
{
    //                <tokens, possible outcomes>
    private Dictionary<string, List<string>> corpus;

    public MarkovGenerator(string path)
    {
        corpus = [];
        Tokenize(path);

        string text = GenerateText(100);
        Console.WriteLine(text);
    }

    private string GenerateText(int length)
    {
        StringBuilder builder = new();

        var rng = new Random();

        string prevWord = corpus.Keys.ElementAt(rng.Next(corpus.Keys.Count));
        builder.Append(prevWord + " ");

        for (int i = 1; i < length; i++)
        {
            var possible = corpus[prevWord];
            if (possible.Count == 0)
            {
                prevWord = corpus.Keys.ElementAt(rng.Next(corpus.Keys.Count));
                builder.Append(prevWord + " ");
                continue;
            }
            int index = rng.Next(possible.Count);
            var word = possible[index];
            prevWord = word;

            builder.Append(word + " ");
        }

        return builder.ToString();
    }

    private void Tokenize(string path)
    {
        // Length in words of the token key
        foreach (var line in File.ReadLines(path))
        {
            if (line == "") continue;

            var words = line.Split(' ').ToList().FindAll(t => !Regex.IsMatch(t, @"(\d)"));

            for (int i = 0; i < words.Count; i++)
            {
                var token = words[i];

                if (!corpus.TryGetValue(token, out List<string>? value))
                {
                    value = [];
                    corpus.Add(token, value);
                }

                if (i + 1 >= words.Count)
                    break;

                var nextWord = words[i + 1];
                value.Add(nextWord);
            }
        }

        // Debugging stuff
        // int maxCount = 100;
        // int iter = 0;
        // foreach (var (token, outcomes) in corpus)
        // {
        //     StringBuilder builder = new();
        //     builder.Append($"|{token}".PadRight(12));

        //     for (int i = 0; i < outcomes.Count; i++)
        //     {
        //         if (i == 0)
        //             builder.Append("|");

        //         var outcome = outcomes[i];
        //         builder.Append($"{outcome} -> ");

        //         if (i == outcomes.Count - 1)
        //             builder.Append("|");
        //     }

        //     Console.WriteLine(builder);

        //     iter++;
        //     if (iter >= maxCount)
        //         break;
        // }
    }

    // private void GenerateTokens(string path)
    // {
    //     string lines = "";
    //     foreach (var line in File.ReadAllLines(path))
    //     {
    //         if (string.IsNullOrEmpty(line)) continue;
    //         lines += line + " ";
    //     }

    //     var punctuation = "\\[\\](){}!?.,:;'\"\\/*&^%$_+-–—=<>@|~";
    //     var ellipsis = "\\.{3}";

    //     var words = "[a-zA-Zа-яА-ЯёЁ]+";
    //     var compounds = "${words}-${words}";


    //     Console.WriteLine($"{punctuation}");

    //     var tokenizeRegex = new Regex($"({ellipsis}|{compounds}|{words}|[{punctuation}])");

    //     var newLineRegex = new Regex("/\ns*/g");
    //     var tokens = tokenizeRegex.Split(newLineRegex.Replace(lines, "§"))
    //         .ToList()
    //         .FindAll((t) =>
    //             !string.IsNullOrEmpty(t) &&
    //             !string.IsNullOrWhiteSpace(t) &&
    //             !Regex.IsMatch(t, "[0-9]|\\:"))
    //         .ToArray();

    //     foreach (var token in tokens[0..20])
    //     {
    //         Console.WriteLine(token);
    //     }
    // }

}