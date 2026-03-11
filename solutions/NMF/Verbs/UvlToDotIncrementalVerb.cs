using CommandLine;
using NMF.AnyText;
using NMF.Models;
using NMF.Models.Repository;
using NMFSolution.Benchmark;
using NMFSolution.Transformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TTC2025.UvlToDot.UniversalVariability;
using Parser = NMF.AnyText.Parser;

namespace NMFSolution.Verbs
{
    [Verb("uvl-to-dot-inc", HelpText = "The actual benchmark executed incrementally")]
    internal class UvlToDotIncrementalVerb : VerbBase, ISolution
    {
        private UniversalVariabilityGrammar _grammar;
        private FeatureModel _loadedFeatureModel;
        private Parser _parser;

        public Func<Model> ComputeChanges(string modelPath, string model, int iteration, string targetPath)
        {
            ModelElement.RaiseDeletionEvents = false;

            var diffPath = Path.ChangeExtension(modelPath, "diff");
            var diffLines = File.ReadLines(diffPath);
            var edits = DiffParser.ToTextEdits(diffLines);
            Console.Error.WriteLine("Updating to iteration " + iteration.ToString());
            return () =>
            {
                _loadedFeatureModel = (FeatureModel) _parser.Update(edits);
                if (_parser.Context.Errors.Any()) throw new InvalidOperationException("Parser found errors");
                return Initial(modelPath, model, targetPath);
            };
        }
        
        public Model Initial(string modelPath, string model, string targetPath)
        {
            DotWriter.WriteToDot(_loadedFeatureModel, File.CreateText(targetPath));
            return null;
        }

        public void Initialize()
        {
            _grammar = new UniversalVariabilityGrammar();
            _grammar.Initialize();
        }

        public void Load(string modelPath, string model)
        {
            Console.Error.WriteLine("Loading " + modelPath);
            _parser = _grammar.CreateParser();
            _loadedFeatureModel = _parser.Initialize(File.ReadAllLines(modelPath)) as FeatureModel;
            if (_parser.Context.Errors.Any()) throw new InvalidOperationException("Parser found errors");
        }

        protected override void ExecuteCore()
        {
            var runner = new BenchmarkRunner(this);
            runner.Execute();
        }
    }
}
