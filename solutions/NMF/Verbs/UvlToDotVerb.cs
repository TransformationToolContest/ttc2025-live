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
using System.Threading.Tasks;
using TTC2025.UvlToDot.UniversalVariability;

namespace NMFSolution.Verbs
{
    [Verb("uvl-to-dot", HelpText = "The actual benchmark")]
    internal class UvlToDotVerb : VerbBase, ISolution
    {
        private UniversalVariabilityGrammar _grammar;
        private FeatureModel _loadedFeatureModel;

        public Func<Model> ComputeChanges(string modelPath, string model, int iteration, string targetPath)
        {
            return () =>
            {
                Load(modelPath, model);
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
            var parser = _grammar.CreateParser();
            _loadedFeatureModel = parser.Initialize(File.ReadAllLines(modelPath)) as FeatureModel;
            if (parser.Context.Errors.Any()) throw new InvalidOperationException("Parser found errors");
        }

        protected override void ExecuteCore()
        {
            var runner = new BenchmarkRunner(this);
            runner.Execute();
        }
    }
}
