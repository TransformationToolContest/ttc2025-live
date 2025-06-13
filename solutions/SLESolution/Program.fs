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
let transformationPath = "/Users/v.zaytsev/projects/ttc/live/solutions/SLESolution/specs/uvl2dot.sle"

let measureTime phaseName action =
    let sw = Stopwatch.StartNew()
    let result = action()
    sw.Stop()
    printfn $"%s{tool};%s{model};%s{runIndex};0;%s{phaseName};Time;%d{sw.ElapsedTicks}"
    result

type Feature = {
    Name: string
    Mandatory: ResizeArray<Feature>
    Optional: ResizeArray<Feature>
    Alternative: ResizeArray<Feature>
    Or: ResizeArray<Feature>
    Constraints: ResizeArray<string>
}

type Step =
    | Atomic of string
    | Sequence of StepChain
    | Cycle of StepChain
and StepChain = Step list

type MetaModel = {
    MainClass: string
    Start: string
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
    | Sequence c | Cycle c -> getToLast (List.last c)
    
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

type Transformation = {Templates: Dictionary<string,string>; Iterators: Dictionary<string,string*((string*string) list)>; }

let unquote (s: string): string =
    let t = s.Trim()
    let u = if t.Length >= 2 && t[0] = t[t.Length-1] && (t[0] = '\"' || t[0] = '\'') then t[1..t.Length-2] else t
    u.Replace("\\n", "\n")
    
let stripMeta (s: string) : string = if s.Contains("{") then s.Split("{")[0] + s.Split("}")[s.Split("}").Length-1] else s

let parseTransformation (filename: string) : Transformation =
    let mutable templates = Dictionary<string,string>()
    let mutable iterators = Dictionary<string,string*((string*string) list)>()
    File.ReadAllLines(filename)
    |> Array.toList
    |> List.iter (fun line ->
        if not(String.IsNullOrWhiteSpace(line)) then
            let parts = line.Trim().Split(' ')
            match parts[0] with
            | "template" -> templates[parts[1]] <- unquote(line[14+parts[1].Length..])
            | "each" -> iterators[parts[1]] <- (parts[3], parts[5..] |> Array.choose (fun pair -> match pair.Split("=") with | [|k; v|] -> Some (k, v) | _ -> None) |> Array.toList)
            | _ -> ()
    )
    {Templates = templates; Iterators = iterators}

let parseGrammar(filename: string) =
    let tokens = File.ReadAllText(filename).Split(' ') |> Array.toList
    let steps, _ = parseStepChain None tokens[2..] // skip [1] which is '::='
    let transitions = Dictionary<string, string>()
    fillInTransactions(transitions, steps)
    { MainClass = tokens[0]; Start = getToFirst steps[0]; Steps = transitions }

let Initialization() = (parseGrammar metaModelPath, parseTransformation transformationPath)

let makeFeature(content: string) = { Name = content; Mandatory = ResizeArray<Feature>(); Optional = ResizeArray<Feature>(); Alternative = ResizeArray<Feature>(); Or = ResizeArray<Feature>(); Constraints = ResizeArray<string>() }

let Load(grammar: MetaModel) : Feature =
    let lines =
        File.ReadAllLines(modelPath)
        |> Array.toList
        |> List.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
        |> List.map (fun line -> (line |> Seq.takeWhile ((=) '\t') |> Seq.length, line.Trim()))

    let mutable outOfFeatures : bool = false
    let contextStack = ResizeArray<ResizeArray<Feature>>()
    let mutable lastIndent = 0
    let mutable step = grammar.Start
    let mutable context : ResizeArray<Feature> = ResizeArray<Feature>() // fake context to start
    // let mutable result : Feature list = []
    let mutable result : ResizeArray<Feature> = ResizeArray<Feature>()

    for indent, content in lines do
        let delta = indent - lastIndent
        lastIndent <- indent
        
        if indent = 0 && result.Count <> 0 then
            outOfFeatures <- true
        elif outOfFeatures then
            result[0].Constraints.Add(content)
        else
            if delta > 0 then
                match context.Count with
                | 0 -> ()
                | _ -> contextStack.Add(context)
                if grammar.Steps.ContainsKey(step) then step <- grammar.Steps[step]
            elif delta < 0 && indent <> 0 then
                for _ in 1 .. abs delta do
                    if contextStack.Count > 0 then contextStack.RemoveAt(contextStack.Count-1)
                if delta % 2 <> 0 then
                    if grammar.Steps.ContainsKey(step) then step <- grammar.Steps[step]

            match step with
            | "root" ->
                ()
            | "make" ->
                let feat = makeFeature(unquote(stripMeta(content)))
                context.Add(feat)
                if result.Count = 0 then result.Add(feat)
            | "goal" ->
                match content with
                | "mandatory" -> context <- context[context.Count-1].Mandatory
                | "optional" -> context <- context[context.Count-1].Optional
                | "alternative" -> context <- context[context.Count-1].Alternative
                | "or" -> context <- context[context.Count-1].Or
                | _ -> printfn $"Unrecognised goal: '{content}'"
                ()
            | _ -> ()
    result[0]

let writeDot (filename: string) (lines: seq<string>) =
    File.WriteAllLines(filename, lines)

let applySubs (template: string) (subs: (string * string) list) =
    subs
    |> List.fold (fun (acc:string) -> acc.Replace) template

let rec emitFeature (xform: Transformation) (parent: Feature option) (feature: Feature) (lines: ResizeArray<string>) =
    let handleKind kind children =
        if xform.Iterators.ContainsKey(kind) then
            let (templName, pairs) = xform.Iterators.[kind]
            let template = xform.Templates.[templName]
            for child in children do
                let subs =
                    [("SOURCE", feature.Name); ("TARGET", child.Name)] @ pairs
                lines.Add(applySubs template subs)
                emitFeature xform (Some feature) child lines
    handleKind "mandatory" feature.Mandatory
    handleKind "optional" feature.Optional
    handleKind "alternative" feature.Alternative
    handleKind "or" feature.Or

let Initial(features: Feature, xform: Transformation) =
    let output = ResizeArray<string>()
    if xform.Templates.ContainsKey("BEFORE") then
        output.Add(xform.Templates["BEFORE"])
    emitFeature xform None features output
    let outPath = Path.Combine(modelDirectory, model + ".dot")
    if xform.Templates.ContainsKey("AFTER") then
        output.Add(xform.Templates["AFTER"])
    writeDot outPath output

let Update() = ()

// Run each method in sequence and measure their time
[<EntryPoint>]
let main argv =
    // printfn "Tool;Scenario;RunIndex;Iteration;PhaseName;MetricName;MetricValue"
    let grammar, xform = measureTime "Initialization" Initialization
    printfn $"Loading %s{model}"
    let features = measureTime "Load" (fun () -> Load(grammar))
    measureTime "Initial" (fun () -> Initial(features, xform))
    measureTime "Update" Update
    0
