using CommandLine;
using NMF.AnyText;
using NMF.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TTC2025.UvlToDot.UniversalVariability;

namespace NMFSolution.Verbs
{
    [Verb("isolate", HelpText = "Isolates the changes in the models of the current directory")]
    internal class IsolateVerb : VerbBase
    {
        [Value(0, Required = true, HelpText = "The name of the result model")]
        public string Target { get; set; }

        protected override void ExecuteCore()
        {
            ModelElement.RaiseDeletionEvents = false;

            var modelName = Path.GetFileName(Environment.CurrentDirectory);
            if (!File.Exists($"{modelName}_01.uvl"))
            {
                Console.Error.WriteLine($"Could not find {modelName}_01.uvl");
                return;
            }
            var grammar = new UniversalVariabilityGrammar();
            var parser = grammar.CreateParser();
            parser.Initialize(File.ReadAllLines($"{modelName}_01.uvl"));
            if (!Directory.Exists($"../{Target}")) Directory.CreateDirectory($"../{Target}");
            Write(parser.Context.Input, 1, Enumerable.Empty<int>());
            CreateModels(modelName, parser);
            Environment.CurrentDirectory = Path.GetFullPath("../" + Target);
            SortFeaturesVerb.GenerateDiffs(Target);
        }

        private void CreateModels(string modelName, NMF.AnyText.Parser parser)
        {
            var index = 2;
            var targetIndex = 2;
            while (File.Exists($"{modelName}_{index:00}.diff"))
            {
                Console.WriteLine($"Process {modelName}_{index:00}.diff (targetIndex = {targetIndex})");
                var diffs = DiffParser.ToTextEdits(File.ReadLines($"{modelName}_{index:00}.diff"));
                foreach (var diff in diffs)
                {
                    var fm = parser.Update(diff) as FeatureModel;

                    Write(parser.Context.Input, targetIndex, parser.Context.Errors.Select(e => e.Position.Line).ToHashSet());
                    targetIndex++;
                    if (targetIndex == 100)
                    {
                        return;
                    }
                }
                index++;
            }
        }

        private void Write(string[] contents, int index, IEnumerable<int> exceptLines)
        {
            File.WriteAllLines($"../{Target}/{Target}_{index:00}.uvl", contents.Where((_,i) => !exceptLines.Contains(i)));
        }
    }
}
