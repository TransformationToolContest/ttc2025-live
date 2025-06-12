// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Diagnostics
open System.IO

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
    let result = action()
    sw.Stop()
    printfn $"%s{tool};%s{model};%s{runIndex};0;%s{phaseName};Time;%d{sw.ElapsedMilliseconds}"
    result

// Data structure for parsed model

type Step =
    | AtomicStep of string
    | Cycle of Step list

type MetaModel = {
    MainClass: string
    Steps: Step list
}

// Parser method for MetaModel
let parseGrammar (path: string) =
    let content = File.ReadAllText(path).Replace(" ", "")

    let rec parseSteps (tokens: string list) : Step list * string list =
        match tokens with
        | [] -> [], []
        | head :: tail when head.StartsWith("[") ->
            let inner = head.TrimStart('[').TrimEnd(']').Split("=>") |> List.ofArray
            let innerSteps, rest = parseSteps inner
            let steps, remaining = parseSteps tail
            (Cycle innerSteps :: steps, remaining)
        | head :: tail ->
            let steps, remaining = parseSteps tail
            (AtomicStep head :: steps, remaining)

    let parts = content.Split("::=")
    let mainClass = parts.[0]
    let stepsStr = parts.[1].Split("=>") |> List.ofArray
    let steps, _ = parseSteps stepsStr

    { MainClass = mainClass; Steps = steps }

// Placeholder methods
let Initialization() =
    parseGrammar metaModelPath

let Load() = ()
let Initial() = ()
let Update() = ()

// Run each method in sequence and measure their time
[<EntryPoint>]
let main argv =
    printfn "Tool;Scenario;RunIndex;Iteration;PhaseName;MetricName;MetricValue"
    let grammar = measureTime "Initialization" Initialization
    measureTime "Load" Load
    measureTime "Initial" Initial
    measureTime "Update" Update
    0
