module Program

open SLE.Core

[<EntryPoint>]
let main _ = // printfn "Tool;Scenario;RunIndex;Iteration;PhaseName;MetricName;MetricValue"
    let grammar, script = measureTime "Initialization" 0 Initialization
    let features = measureTime "Load" 0 (fun () -> Load grammar modelPath makeFeature matchGoalFeature)
    measureTime "Initial" 0 (fun () -> Initial modelName features script)
    Update grammar script modelName modelPath
    0
