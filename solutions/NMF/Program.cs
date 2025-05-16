﻿using CommandLine;
using NMFSolution.Verbs;

Parser.Default.ParseArguments(args,
    typeof(ConvertToXmiVerb), typeof(UvlToDotVerb))
    .MapResult((VerbBase verb) => verb.Execute(), _ => 2);