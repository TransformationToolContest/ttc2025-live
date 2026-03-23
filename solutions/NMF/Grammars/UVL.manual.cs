using NMF.AnyText;
using NMF.AnyText.Grammars;
using NMF.AnyText.Rules;

namespace TTC2025.UvlToDot.UniversalVariability
{
    partial class UniversalVariabilityGrammar
    {
        protected override ParseContext CreateParseContext()
        {
            return new UniversalVariabilityParseContext(this);
        }

        public partial class FeatureRule
        {
            public override SymbolKind SymbolKind => SymbolKind.Function;
        }
    }

    public class UniversalVariabilityParseContext : ModelParseContext
    {
        public UniversalVariabilityParseContext(Grammar grammar, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) : base(grammar, stringComparison)
        {
        }

        protected override bool AcceptOneOrMoreAdd(OneOrMoreRule rule, RuleApplication toAdd, List<RuleApplication> added, ref ParsePositionDelta examined)
        {
            if (added.Count > 0 && toAdd.CurrentPosition.Col < added[added.Count - 1].CurrentPosition.Col)
            {
                examined = toAdd.CurrentPosition - added[0].CurrentPosition;
                return false;
            }
            return true;
        }

        protected override bool AcceptZeroOrMoreAdd(ZeroOrMoreRule star, RuleApplication toAdd, List<RuleApplication> added, ref ParsePositionDelta examined)
        {
            if (added.Count > 0 && toAdd.CurrentPosition.Col < added[added.Count - 1].CurrentPosition.Col)
            {
                examined = toAdd.CurrentPosition - added[0].CurrentPosition;
                return false;
            }
            return true;
        }

        protected override bool AcceptSequenceAdd(SequenceRule sequence, ref RuleApplication toAdd, List<RuleApplication> added, ref ParsePositionDelta examined)
        {
            if (added.Count > 0 && toAdd.Length != default && toAdd.CurrentPosition.Col < added[0].CurrentPosition.Col && toAdd.Rule.IsEpsilonAllowed())
            {
                examined = toAdd.CurrentPosition - added[0].CurrentPosition;
                var app = toAdd.Rule.CreateEpsilonRuleApplication(toAdd);
                toAdd = app;
            }
            return true;
        }
    }
}