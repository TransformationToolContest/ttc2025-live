// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO

let getEnvOrDefault name defaultValue = match Environment.GetEnvironmentVariable(name) with | null -> defaultValue | value -> value
let tool = getEnvOrDefault "Tool" "SLE"
let model = getEnvOrDefault "Model" "automotive01"
let modelPath = getEnvOrDefault "ModelPath" "/Users/v.zaytsev/projects/ttc/live/models/automotive01/automotive01.uvl"
let modelDirectory = getEnvOrDefault "ModelDirectory" "/Users/v.zaytsev/projects/ttc/live/models/automotive01"
let runIndex = getEnvOrDefault "RunIndex" "0"
let metaModelPath = "/Users/v.zaytsev/projects/ttc/live/solutions/SLESolution/specs/uvl.sle"

let measureTime phaseName action =
    let sw = Stopwatch.StartNew()
    let result = action()
    sw.Stop()
    printfn $"%s{tool};%s{model};%s{runIndex};0;%s{phaseName};Time;%d{sw.ElapsedMilliseconds}"
    result

type Feature = {
    Name: string
    Mandatory: Feature list
    Optional: Feature list
    Alternative: Feature list
    Or: Feature list
}

type Step =
    | Atomic of string
    | Sequence of StepChain
    | Cycle of StepChain
and StepChain = Step list

type MetaModel = {
    MainClass: string
    Steps: Dictionary<string, string>
}

let rec parseStepChain (parent: Step option) (tokens: string list) : StepChain * string list =
    let rec parseSeq (parent: Step option) (tokens: string list) (acc: StepChain) : StepChain * string list =
        match tokens with
        | [] -> List.rev acc, []
        | token :: rest ->
            if token = "=>" then parseSeq parent rest acc
            elif token = ")" || token = "]" then List.rev acc, rest
            elif token = "(" || token = "[" then
                let steps, rem = parseStepChain None rest
                let seqStep = (if token = "(" then Sequence steps else Cycle steps)
                parseSeq parent rem (seqStep :: acc)
            else parseSeq parent rest ((Atomic token) :: acc)
    parseSeq parent tokens []

let rec getToFirst step =
    match step with
    | Atomic a -> a
    | Sequence c | Cycle c -> getToFirst (List.head c)
    
let rec getToLast step =
    match step with
    | Atomic a -> a
    | Sequence c | Cycle c -> getToFirst (List.last c)
    
let rec fillInTransactions (dict: Dictionary<string,string>, chain: StepChain) =
    let rec fillInPairwise(dict: Dictionary<string,string>, chain: StepChain) =
        chain
        |> List.pairwise
        |> List.iter (fun (a, b) ->
            match a, b with
            | Atomic nameA, Atomic nameB -> dict[nameA] <- nameB
            | Atomic nameA, Sequence seqB -> dict[nameA] <- getToFirst b; fillInTransactions(dict, seqB) 
            | Atomic nameA, Cycle cycB -> dict[nameA] <- getToFirst b; fillInPairwise(dict, cycB); fillInTransactions(dict, cycB); dict[getToLast b] <- getToFirst b
            | _ -> ()
        )

    chain
    |> List.iter (fun step ->
        match step with
        | Atomic _ -> ()
        | Sequence inner | Cycle inner -> fillInPairwise(dict, inner)
        )

let Initialization() =
    let tokens = File.ReadAllText(metaModelPath).Split(' ') |> Array.toList
    let steps, _ = parseStepChain None tokens[2..] // skip [1] which is '::='
    let transitions = Dictionary<string, string>()
    fillInTransactions(transitions, steps)
    { MainClass = tokens[0]; Steps = transitions }

let Load(grammar: MetaModel) =
    let lines = File.ReadAllLines(modelPath) |> Array.toList |> List.map (fun line -> ((line |> Seq.takeWhile ((=) '\t') |> Seq.length), line.Trim()))

    // Simple stepper to go through grammar steps (not recursive)
    let rec processSteps (steps: StepChain) (lines: (int * string) list) (context: Feature option) (lastInstance: Feature option) : Feature option =
        match steps, lines with
        | [], _ | _, [] -> context
        | step :: restSteps, (indent, content) :: restLines ->
            match step with
            | Atomic "root" ->
                let feat = { Name = content; Mandatory = []; Optional = []; Alternative = []; Or = [] }
                processSteps restSteps restLines (Some feat) None
            | Atomic "make" ->
                let feat = { Name = content; Mandatory = []; Optional = []; Alternative = []; Or = [] }
                // context not changed for now, but you may want to attach this feature somewhere
                processSteps restSteps restLines context (Some feat)
            | Atomic "goal" ->
                processSteps restSteps restLines lastInstance lastInstance
            | _ ->
                processSteps restSteps restLines context lastInstance

    // Start processing with top-level grammar steps, all parsed lines, and empty context/instance
    processSteps grammar.Steps lines None None


let Initial() = ()
let Update() = ()

// Run each method in sequence and measure their time
[<EntryPoint>]
let main argv =
    printfn "Tool;Scenario;RunIndex;Iteration;PhaseName;MetricName;MetricValue"
    let grammar = measureTime "Initialization" Initialization
    let features = measureTime "Load" (fun () -> Load(grammar))
    measureTime "Initial" Initial
    measureTime "Update" Update
    0
