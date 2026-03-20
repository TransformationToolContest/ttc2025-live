using CommandLine;
using NMFSolution.Verbs;

Parser.Default.ParseArguments(args,
    typeof(StatisticsVerb),
    typeof(ConvertToXmiVerb), 
    typeof(UvlToDotVerb), 
    typeof(UvlToDotIncrementalVerb), 
    typeof(SortFeaturesVerb),
    typeof(IsolateVerb))
    .MapResult((VerbBase verb) => verb.Execute(), _ => 2);