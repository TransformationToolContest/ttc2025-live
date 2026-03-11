using CommandLine;
using NMFSolution.Verbs;

Parser.Default.ParseArguments(args,
    typeof(ConvertToXmiVerb), typeof(UvlToDotVerb), typeof(UvlToDotIncrementalVerb), typeof(SortFeaturesVerb))
    .MapResult((VerbBase verb) => verb.Execute(), _ => 2);