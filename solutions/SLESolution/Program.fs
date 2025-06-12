// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Diagnostics

// Load environment variables
// Load environment variables with default values
let tool = Environment.GetEnvironmentVariable("Tool") |> function null -> "SLE" | x -> x
let model = Environment.GetEnvironmentVariable("Model") |> function null -> "automotive01" | x -> x
let modelPath = Environment.GetEnvironmentVariable("ModelPath") |> function null -> "/Users/v.zaytsev/projects/ttc/live/models/automotive01/automotive01.uvl" | x -> x
let modelDirectory = Environment.GetEnvironmentVariable("ModelDirectory") |> function null -> "/Users/v.zaytsev/projects/ttc/live/models/automotive01" | x -> x
let runIndex = Environment.GetEnvironmentVariable("RunIndex") |> function null -> "0" | x -> x
let metaModelPath = "/Users/v.zaytsev/projects/ttc/live/solutions/SLESolution/specs/uvl.sle"

// Utility to measure execution time
let measureTime phaseName action =
    let sw = Stopwatch.StartNew()
    action()
    sw.Stop()
    printfn "%s;%s;%s;0;%s;Time;%d" tool model runIndex phaseName sw.ElapsedMilliseconds

// Placeholder methods
let Initialization() = ()
let Load() = ()
let Initial() = ()
let Update() = ()

// Run each method in sequence and measure their time
[<EntryPoint>]
let main argv =
    printfn "Tool;Scenario;RunIndex;Iteration;PhaseName;MetricName;MetricValue"
    measureTime "Initialization" Initialization
    measureTime "Load" Load
    measureTime "Initial" Initial
    measureTime "Update" Update
    0