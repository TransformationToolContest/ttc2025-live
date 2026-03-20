using CommandLine;
using NMF.AnyText;
using System;
using System.Collections.Generic;
using System.Text;
using TTC2025.UvlToDot.UniversalVariability;

namespace NMFSolution.Verbs
{
    [Verb("statistics", HelpText = "Displays statistics of the current folder")]
    internal class StatisticsVerb : VerbBase
    {
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
            Console.WriteLine($"Lines of code initial: {parser.Context.Input.Length}");
            var index = 2;
            var diffsTotal = 0;
            var addedLinesTotal = 0;
            var removedLinesTotal = 0;
            while (File.Exists($"{modelName}_{index:00}.diff"))
            {
                var diffs = DiffParser.ToTextEdits(File.ReadLines($"{modelName}_{index:00}.diff"));
                diffsTotal += diffs.Count;
                addedLinesTotal += diffs.Sum(d => d.NewText.Length - 1);
                removedLinesTotal += diffs.Sum(d => d.End.Line - d.Start.Line);
                index++;
            }

            Console.WriteLine($"# Diffs: {index - 1}");
            Console.WriteLine($"avg. #edits per diff: {diffsTotal / (index - 2.0)}");
            Console.WriteLine($"avg. #lines added per edit: {((double)addedLinesTotal) / diffsTotal}");
            Console.WriteLine($"avg. #lines removed per edit: {((double)removedLinesTotal) / diffsTotal}");
        }
    }
}
