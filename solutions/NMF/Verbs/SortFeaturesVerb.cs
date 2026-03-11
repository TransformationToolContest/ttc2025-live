using CommandLine;
using NMF.Expressions;
using NMF.Models;
using NMF.Models.Repository;
using NMFSolution.Transformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TTC2025.UvlToDot.UniversalVariability;

namespace NMFSolution.Verbs
{
    [Verb("sort-features")]
    internal class SortFeaturesVerb : VerbBase
    {
        [Value(0, Required = true, HelpText = "Path to the UVL model instance")]
        public string UvlPath { get; set; }

        [Value(1, Required = true, HelpText = "Path to the sorted feature models")]
        public string SortedPath { get; set; }

        protected override void ExecuteCore()
        {
            ModelElement.RaiseDeletionEvents = false;
            var uvl = new UniversalVariabilityGrammar();
            var parser = uvl.CreateParser();
            var repo = new ModelRepository();

            var directoryName = Path.GetFileName(Environment.CurrentDirectory);
            var targetDirectoryName = Path.GetFileName(SortedPath);

            Directory.CreateDirectory(SortedPath);

            foreach (var path in Directory.GetFiles(".", UvlPath))
            {
                Console.WriteLine($"Processing {path}");
                var lines = File.ReadAllLines(path);
                var parsed = parser.Initialize(lines) as FeatureModel;
                if (parsed == null)
                {
                    throw new InvalidOperationException($"Document {path} containts errors: " + string.Join(", ", parser.Context.Errors));
                }

                var targetPath = Path.Combine(SortedPath, Path.GetFileName(path).Replace(directoryName, targetDirectoryName));
                ReorderFeatures(parsed.Features);
                ReorderConstraints(parsed);
                parser.Initialize(parsed);

                File.WriteAllLines(targetPath, parser.Context.Input.Where(l => !string.IsNullOrEmpty(l)));
            }

            Environment.CurrentDirectory = SortedPath;
            var index = 2;
            while (File.Exists($"{targetDirectoryName}_{index:00}.uvl"))
            {
                using (var diffWriter = File.CreateText($"{targetDirectoryName}_{index:00}.diff"))
                {
                    Console.WriteLine($"Create diff {targetDirectoryName}_{index:00}.diff");
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"diff --no-index {targetDirectoryName}_{index - 1:00}.uvl {targetDirectoryName}_{index:00}.uvl",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    });
                    process.OutputDataReceived += (o, e) =>
                    {
                        diffWriter.WriteLine(e.Data);
                    };
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                    index++;
                }
            }
        }

        private void ReorderConstraints(IFeatureModel featureModel)
        {
            var constraintsSorted = featureModel.Constraints.OrderBy(Notation).ToList();
            featureModel.Constraints.Clear();
            foreach (var constraint in constraintsSorted)
            {
                featureModel.Constraints.Add(constraint);
            }
        }

        private string Notation(IConstraint constraint)
        {
            var sw = new StringWriter();
            DotWriter.WriteConstraint(constraint, sw);
            return sw.ToString();
        }

        private void ReorderFeatures(IListExpression<IFeature> features)
        {
            var newOrder = features.OrderBy(f => f.Name).ToList();
            features.Clear();
            foreach (var feature in newOrder)
            {
                features.Add(feature);
                foreach (var group in feature.Groups)
                {
                    switch (group)
                    {
                        case OrFeatureGroup or:
                            ReorderFeatures(or.Features);
                            break;
                        case AlternativeFeatureGroup alt:
                            ReorderFeatures(alt.Features);
                            break;
                        case OptionalFeatureGroup optional:
                            ReorderFeatures(optional.Features);
                            break;
                        case MandatoryFeatureGroup mandatory:
                            ReorderFeatures(mandatory.Features);
                            break;
                    }
                }
            }
        }
    }
}
