// using MarkovSharp;
// using MarkovSharp.TokenisationStrategies;

// namespace GuessWhoBot;

// public class StringMarkov(int level = 2) : GenericMarkov<string, string>(level), IMarkovStrategy<string, string>
// {

//     // Define how to split a phrase to collection of tokens
//     public override IEnumerable<string> SplitTokens(string input)
//     {
//         if (input == null)
//             return [GetPrepadUnigram()];

//         return input.Split(' ');
//     }

//     // Define how to join the generated tokens back to a phrase
//     public override string RebuildPhrase(IEnumerable<string> tokens)
//     {
//         return string.Join(" ", tokens);
//     }

//     // Define the value to signify the end of a phrase in the model
//     public override string GetTerminatorUnigram()
//     {
//         return "";
//     }

//     // Define a default padding value to use when no value is available
//     public override string GetPrepadUnigram()
//     {
//         return "";
//     }
// }
