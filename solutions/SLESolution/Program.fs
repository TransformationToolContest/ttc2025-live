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

type StepKind =
    | Atomic of string
    | Sequence of StepChain
    | Cycle of StepChain
    | Nothing
and Step = {
    Content: StepKind
    mutable Parent: Step option
}
and StepChain = Step list

let next (step: Step) : Step option =
    match step.Parent with
    | Some parentStep ->
        match parentStep.Content with
        | Sequence chain -> match (chain |> List.tryFindIndex (fun s -> obj.ReferenceEquals(s, step))) with | Some i when i+1 < List.length chain -> Some chain[i+1] | _ -> None
        | Cycle cycle -> match (cycle |> List.tryFindIndex (fun s -> obj.ReferenceEquals(s, step))) with | Some i -> (if i+1 < List.length cycle then Some cycle[i+1] else Some cycle[0]) | _ -> None
        | _ -> None
    | None -> None

type MetaModel = {
    MainClass: string
    Steps: StepChain
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
                for s in steps do s.Parent <- Some seqStep
                parseSeq parent rem (seqStep :: acc)
            else parseSeq parent rest ({ Content = Atomic token; Parent = parent } :: acc)
    parseSeq parent tokens []

let fillInTransactions (dict: Dictionary<string,string>, chain: StepChain) =
    chain
    |> List.pairwise
    |> List.iter (fun (a, b) ->
        match a.Content, b.Content with
        | Atomic nameA, Atomic nameB -> dict[nameA] <- nameB
        | _ -> () // Only record atomic to atomic transitions
    )

let Initialization() =
    let tokens = File.ReadAllText(metaModelPath).Split(' ') |> Array.toList
    let steps, _ = parseStepChain None tokens[2..] // skip [1] which is '::='

    // Build the transitions dictionary
    let transitions = Dictionary<string, string>()
    let rec fillTransitions (chain: StepChain) =
        chain
        |> List.pairwise
        |> List.iter (fun (a, b) ->
            match a.Content, b.Content with
            | Atomic nameA, Atomic nameB -> transitions[nameA] <- nameB
            | Atomic nameA, Sequence s when s <> [] ->
                match s.Head.Content with
                | Atomic nameB -> transitions.[nameA] <- nameB
                | _ -> ()
            | Atomic nameA, Cycle c when c <> [] ->
                match c.Head.Content with
                | Atomic nameB -> transitions.[nameA] <- nameB
                | _ -> ()
            | _ -> ()
        )
        // Recursively fill for sequences/cycles
        chain
        |> List.iter (fun step ->
            match step.Content with
            | Sequence s | Cycle s -> fillTransitions s
            | _ -> ()
        )
    fillTransitions steps

    // You can print or return transitions here for debugging
    // transitions |> Seq.iter (fun kv -> printfn "%s -> %s" kv.Key kv.Value)

    { MainClass = tokens[0]; Steps = steps }

let Load(grammar: MetaModel) =
    let lines = File.ReadAllLines(modelPath) |> Array.toList |> List.map (fun line -> ((line |> Seq.takeWhile ((=) '\t') |> Seq.length), line.Trim()))

    // Simple stepper to go through grammar steps (not recursive)
    let rec processSteps (steps: StepChain) (lines: (int * string) list) (context: Feature option) (lastInstance: Feature option) : Feature option =
        match steps, lines with
        | [], _ | _, [] -> context
        | step :: restSteps, (indent, content) :: restLines ->
            match step.Content with
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
