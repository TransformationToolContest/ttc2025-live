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
            Write(parser.Context.Input, 1);
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
                    if (parser.Context.LastSuccessfulRootRuleApplication.IsPositive && !parser.Context.Errors.Any() && ReferencesIntact(fm))
                    {
                        Write(parser.Context.Input, targetIndex);
                        targetIndex++;
                        if (targetIndex == 100)
                        {
                            return;
                        }
                    }
                }
                index++;
            }
        }

        private bool ReferencesIntact(FeatureModel featureModel)
        {
            foreach (var featureConstraint in featureModel.Descendants().OfType<IFeatureConstraint>())
            {
                if (featureConstraint.Feature == null || !featureConstraint.Feature.Ancestors().Contains(featureModel))
                {
                    return false;
                }
            }
            return true;
        }

        private void Write(string[] contents, int index)
        {
            File.WriteAllLines($"../{Target}/{Target}_{index:00}.uvl", contents);
        }
    }
}
