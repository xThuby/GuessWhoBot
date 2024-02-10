using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace DotMarkov;

public class MarkovGenerator(int level = 2)
{
    private readonly int level = level;

    private readonly List<string> sourcePhrases = [];

    public MarkovChain Chain = new();

    public void Learn(List<string> phrases)
    {
        var newTerms = phrases.Where(s => !sourcePhrases.Contains(s));
        foreach (var phrase in newTerms)
        {
            Learn(phrase);
        }
    }

    public void Learn(string phrase)
    {
        if (string.IsNullOrEmpty(phrase))
            return;

        if (SplitTokens(phrase).Count < level)
            return;

        sourcePhrases.Add(phrase);

        var tokens = SplitTokens(phrase);

        LearnTokens(tokens);
    }

    private void LearnTokens(List<string> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            var current = tokens[i];
            var key = new List<string>();

            for (var j = level; j > 0; j--)
            {
                string previous;
                try
                {
                    if (i - j < 0)
                    {
                        key.Add("");
                    }
                    else
                    {
                        previous = tokens[i - j];
                        key.Add(previous);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    previous = "";
                    key.Add(previous);
                }
            }

            // add the current token to the markov model at the composite key
            Chain.AddOrCreate(key, current);
        }
    }

    public List<string> Walk(int lines = 1, string seed = "")
    {
        if (lines < 1)
        {
            throw new ArgumentException("Invalid argument - line count for walk must be a positive integer", nameof(lines));
        }

        var sentences = new List<string>();

        int genCount = 0;
        int created = 0;
        while (created < lines)
        {
            if (genCount == lines * 10)
            {
                Console.WriteLine($"Breaking out of walk early - {genCount} generations did not produce {lines} distinct lines ({sentences.Count} were created)");
                break;
            }
            var result = WalkLine(seed);
            if ((!EnsureUniqueWalk || !SourcePhrases.Contains(result)) && (!EnsureUniqueWalk || !sentences.Contains(result)))
            {
                sentences.Add(result);
                created++;
                yield return result;
            }
            genCount++;
        }
    }

    private string WalkLine(string seed)
    {
        var paddedSeed = PadArrayLow(SplitTokens(seed)?.ToArray());
        var built = new List<string>();

        // Allocate a queue to act as the memory, which is n 
        // levels deep of previous words that were used
        var q = new Queue(paddedSeed);

        // If the start of the generated text has been seeded,
        // append that before generating the rest
        if (!seed.Equals(GetPrepadUnigram()))
        {
            built.AddRange(SplitTokens(seed));
        }

        while (built.Count < 1500)
        {
            // Choose a new token to add from the model
            var key = new List<string>(q.Cast<string>().ToArray());
            if (Chain.Contains(key))
            {
                string chosen;

                if (built.Count == 0)
                {
                    chosen = new UnweightedRandomUnigramSelector<TUnigram>().SelectUnigram(Chain.GetValuesForKey(key));
                }
                else
                {
                    chosen = UnigramSelector.SelectUnigram(Chain.GetValuesForKey(key));
                }

                q.Dequeue();
                q.Enqueue(chosen);
                built.Add(chosen);
            }
            else
            {
                break;
            }
        }

        return RebuildPhrase(built);
    }

    private static string GetPrepadUnigram()
    {
        return " ";
    }

    private string[] PadArrayLow(string[] input)
    {
        if (input == null)
        {
            input = new List<string>().ToArray();
        }

        var splitCount = input.Length;
        if (splitCount > level)
        {
            input = input.Skip(splitCount - level).Take(level).ToArray();
        }

        var p = new string[level];
        var j = 0;
        for (var i = level - input.Length; i < level; i++)
        {
            p[i] = input[j];
            j++;
        }
        for (var i = level - input.Length; i > 0; i--)
        {
            p[i - 1] = "";
        }

        return p;
    }

    private List<string> SplitTokens(string phrase)
    {
        return [.. phrase.Split(' ')];
    }

    private string RebuildPhrase(List<string> tokens)
    {
        return string.Join(' ', tokens);
    }
}

public class MarkovChain
{
    public MarkovChain()
    {
        ChainDictionary = new ConcurrentDictionary<List<string>, List<string>>();
    }

    internal ConcurrentDictionary<List<string>, List<string>> ChainDictionary { get; }
    private readonly object lockObj = new object();

    /// <summary>
    /// The number of states in the chain
    /// </summary>
    public int Count => ChainDictionary.Count;

    internal bool Contains(List<string> key)
    {
        return ChainDictionary.ContainsKey(key);
    }

    /// <summary>
    /// Add a TGram to the markov models store with a composite key of the previous [Level] number of TGrams
    /// </summary>
    /// <param name="key">The composite key under which to add the TGram value</param>
    /// <param name="value">The value to add to the store</param>
    internal void AddOrCreate(List<string> key, string value)
    {
        lock (lockObj)
        {
            if (!ChainDictionary.ContainsKey(key))
            {
                ChainDictionary.TryAdd(key, [value]);
            }
            else
            {
                ChainDictionary[key].Add(value);
            }
        }
    }

    internal List<string> GetValuesForKey(List<string> key)
    {
        return ChainDictionary[key];
    }

    // internal IEnumerable<StateStatistic<string>> GetStatistics()
    // {
    //     var stats = ChainDictionary.Keys.Select(a => new StateStatistic<string>(a, ChainDictionary[a]))
    //         .OrderByDescending(a => a.Next.Sum(x => x.Count));

    //     return stats;
    // }
}