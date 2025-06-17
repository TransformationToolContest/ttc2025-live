using NMF.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTC2025.UvlToDot.UniversalVariability;

namespace NMFSolution.Transformation
{
    internal class DotWriter
    {
        public static void WriteToDot(IFeatureModel featureModel, TextWriter writer)
        {
            writer.WriteLine("digraph FeatureModel {");
            writer.WriteLine("rankdir=\"TB\"");
            writer.WriteLine("newrank=true");
            writer.WriteLine("bgcolor=\"#1e1e1e\"");
            writer.WriteLine("edge [color=white]");
            writer.WriteLine("node [style=filled fontcolor=\"white\" fontname=\"Arial Unicode MS, Arial\"];");
            writer.WriteLine();

            foreach (var feature in featureModel.Features)
            {
                WriteFeature(feature, writer);
            }
            if (featureModel.Constraints.Any())
            {
                WriteConstraints(featureModel, writer);
            }
            writer.WriteLine("}");
            writer.Flush();
        }

        private static void WriteConstraints(IFeatureModel featureModel, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteLine("subgraph cluster_constraints{");
            writer.WriteLine("    label=\"Constraints\" color=\"white\" fontcolor=\"white\"");
            writer.WriteLine("    constraints [shape=\"box\" color=\"#1e1e1e\" label=<<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" cellborder=\"0\">");
            writer.Write("    ");
            foreach (var constraint in featureModel.Constraints)
            {
                writer.Write("    <tr><td align=\"left\">");
                WriteConstraint(constraint, writer);
                writer.WriteLine("</td></tr>");
            }
            writer.WriteLine("</table>>]");
            writer.WriteLine("}");
        }

        private static void WriteConstraint(IConstraint constraint, TextWriter writer)
        {
            switch (constraint)
            {
                case FeatureConstraint featureConstraint:
                    WriteConstraint(featureConstraint, writer);
                    break;
                case ImpliesConstraint impliesConstraint:
                    WriteConstraint(impliesConstraint, writer);
                    break;
                case OrConstraint orConstraint:
                    WriteConstraint(orConstraint, writer);
                    break;
                case AndConstraint andConstraint:
                    WriteConstraint(andConstraint, writer);
                    break;
                case EquivalenceConstraint equivalenceConstraint:
                    WriteConstraint(equivalenceConstraint, writer);
                    break;
                case NotConstraint notConstraint:
                    WriteNotConstraint(notConstraint, writer);
                    break;
                default:
                    Console.Error.WriteLine($"Constraint type {constraint.GetType().Name} not supported.");
                    break;
            }
        }

        private static void WriteNotConstraint(INotConstraint notConstraint, TextWriter writer)
        {
            writer.Write("!");
            WriteConstraint(notConstraint.Inner, writer);
        }

        private static void WriteConstraint(IFeatureConstraint constraint, TextWriter writer)
        {
            writer.Write(constraint.Feature.Name);
        }

        private static void WriteConstraint(IImpliesConstraint constraint, TextWriter writer)
        {
            WriteConstraint(constraint.Given, writer);
            writer.Write(" =&gt; ");
            WriteConstraint(constraint.Consequence, writer);
        }

        private static void WriteConstraint(IOrConstraint constraint, TextWriter writer)
        {
            WriteConstraint(constraint.Left, writer);
            writer.Write(" | ");
            WriteConstraint(constraint.Right, writer);
        }

        private static void WriteConstraint(IAndConstraint constraint, TextWriter writer)
        {
            WriteConstraint(constraint.Left, writer);
            writer.Write(" &amp; ");
            WriteConstraint(constraint.Right, writer);
        }

        private static void WriteConstraint(IEquivalenceConstraint constraint, TextWriter writer)
        {
            WriteConstraint(constraint.Left, writer);
            writer.Write(" &lt;=&gt; ");
            WriteConstraint(constraint.Right, writer);
        }

        private static void WriteFeature(IFeature feature, TextWriter writer)
        {
            foreach (var group in feature.Groups)
            {
                WriteFeatureGroup((dynamic)group, feature, writer);
            }
            writer.WriteLine($"{feature.Name} [fillcolor=\"#ABACEA\" tooltip=\"Cardinality: None\" shape=\"{(feature.IsAbstract.GetValueOrDefault() ? "invhouse" : "box")}\"]");
        }

        private static void WriteFeatureGroup(IAlternativeFeatureGroup featureGroup, IFeature parent, TextWriter writer)
        {
            foreach (var subFeature in featureGroup.Features)
            {
                WriteFeature(subFeature, writer);
                writer.WriteLine($"{parent.Name} -> {subFeature.Name} [arrowhead=\"none\", arrowtail=\"odot\", dir=\"both\"]");
            }
        }

        private static void WriteFeatureGroup(IOrFeatureGroup featureGroup, IFeature parent, TextWriter writer)
        {
            foreach (var subFeature in featureGroup.Features)
            {
                WriteFeature(subFeature, writer);
                writer.WriteLine($"{parent.Name} -> {subFeature.Name} [arrowhead=\"none\", arrowtail=\"dot\", dir=\"both\"]");
            }
        }

        private static void WriteFeatureGroup(IMandatoryFeatureGroup featureGroup, IFeature parent, TextWriter writer)
        {
            foreach (var subFeature in featureGroup.Features)
            {
                WriteFeature(subFeature, writer);
                writer.WriteLine($"{parent.Name} -> {subFeature.Name} [arrowhead=\"dot\", arrowtail=\"none\", dir=\"both\"]");
            }
        }

        private static void WriteFeatureGroup(IOptionalFeatureGroup featureGroup, IFeature parent, TextWriter writer)
        {
            foreach (var subFeature in featureGroup.Features)
            {
                WriteFeature(subFeature, writer);
                writer.WriteLine($"{parent.Name} -> {subFeature.Name} [arrowhead=\"odot\", arrowtail=\"none\", dir=\"both\"]");
            }
        }
    }
}
