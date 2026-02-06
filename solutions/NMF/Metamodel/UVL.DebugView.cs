using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTC2025.UvlToDot.UniversalVariability
{
    [DebuggerDisplay("!{Inner}")]
    public partial class NotConstraint
    {

    }

    [DebuggerDisplay("{FeatureString}")]
    public partial class FeatureConstraint
    {
        public string FeatureString => Feature != null ? Feature.Name : "(unresolved)";
    }

    [DebuggerDisplay("{Left} | {Right}")]
    public partial class OrConstraint
    {

    }

    [DebuggerDisplay("{Left} & {Right}")]
    public partial class AndConstraint
    {
    }

    [DebuggerDisplay("{Given} => {Consequence}")]
    public partial class ImpliesConstraint
    {

    }

    [DebuggerDisplay("{Left} <=> {Right}")]
    public partial class  EquivalenceConstraint
    {
        
    }
}
