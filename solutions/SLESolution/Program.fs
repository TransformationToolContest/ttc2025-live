// For more information see https://aka.ms/fsharp-console-apps
open System
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
        | Sequence chain | Cycle chain ->
            let idx = chain |> List.tryFindIndex (fun s -> obj.ReferenceEquals(s, step))
            match idx with
            | Some i when i + 1 < List.length chain ->
                match chain.[i + 1].Content with
                | Atomic "self" -> Some chain.[0] // Return the entire sequence by returning its first step
                | _ -> Some chain.[i + 1]
            | _ -> None
        | _ -> None
    | None ->
        match step.Content with
        | Sequence (h :: _) | Cycle (h :: _) -> Some h
        | _ -> None


type MetaModel = {
    MainClass: string
    Steps: StepChain
}

let rec parseStepChain (parent: Step option) (tokens: string list) : StepChain * string list =
    let rec parseSeq (parent: Step option) (tokens: string list) (acc: StepChain) : StepChain * string list =
        match tokens with
        | [] -> List.rev acc, []
        | token :: rest ->
            if token = ")" || token = "]" then List.rev acc, rest
            elif token = "(" || token = "[" then
                let steps, rem = parseStepChain None rest
                let seqStep = { Content = (if token = "(" then Sequence steps else Cycle steps); Parent = parent }
                for s in steps do s.Parent <- Some seqStep
                parseSeq parent rem (seqStep :: acc)
            else parseSeq parent rest ({ Content = Atomic token; Parent = parent } :: acc)
    parseSeq parent tokens []

let Initialization() =
    let tokens = File.ReadAllText(metaModelPath).Split(" ") |> Array.toList
    let steps, _ = parseStepChain None tokens[2..] // note how we skip over [1] which is '::='
    { MainClass = tokens[0]; Steps = steps }

let Load(grammar: MetaModel) =
    let lines = File.ReadAllLines(modelPath) |> Array.toList

    // Helper: pattern match on Step with new StepKind definition
    let (|AtomicStep|SequenceStep|CycleStep|) (step: Step) =
        match step.Content with
        | Atomic s -> AtomicStep s
        | Sequence sc -> SequenceStep sc
        | Cycle sc -> CycleStep sc
        | Nothing -> failwith "Nothing step"

    let rec parseLines steps (lines: (string * int) list) context =
        match lines, steps with
        | [], _ | _, [] -> context
        | (line, indent) :: rest, step :: remainingSteps ->
            match step with
            | AtomicStep "root" ->
                let rootFeature = { Name = line.Trim(); Mandatory = []; Optional = []; Alternative = []; Or = [] }
                parseLines remainingSteps rest rootFeature
            | AtomicStep "make" ->
                let newFeature = { Name = line.Trim(); Mandatory = []; Optional = []; Alternative = []; Or = [] }
                context // Extend to handle hierarchy properly
            | AtomicStep "goal" ->
                context // Extend context handling properly
            | SequenceStep seqSteps | CycleStep seqSteps ->
                parseLines (seqSteps @ remainingSteps) lines context
            | AtomicStep "self" ->
                parseLines (step :: remainingSteps) lines context
            | _ -> context

    let indentedLines = lines |> List.map (fun line -> line.TrimStart('\t'), line.Length - line.TrimStart('\t').Length)
    parseLines grammar.Steps indentedLines { Name = ""; Mandatory = []; Optional = []; Alternative = []; Or = [] }

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
