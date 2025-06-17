module SLE.Core
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO

let getEnvOrDefault name defaultValue = match Environment.GetEnvironmentVariable(name) with | null -> defaultValue | value -> value
let tool = getEnvOrDefault "Tool" "SLE"
let modelName = getEnvOrDefault "Model" "automotive02"
let modelPath = getEnvOrDefault "ModelPath" (Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02", "automotive02_01.uvl"))
let modelDirectory = getEnvOrDefault "ModelDirectory" (Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02"))
let runIndex = Int32.Parse(getEnvOrDefault "RunIndex" "0")
let metaModelPath = Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "solutions", "SLE", "specs", "uvl.indentia")
let transformationPath = Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "solutions",   "SLE", "specs", "uvl2dot.scripta")

type Feature = {
    Name: string
    Mandatory: ResizeArray<Feature>
    Optional: ResizeArray<Feature>
    Alternative: ResizeArray<Feature>
    Or: ResizeArray<Feature>
    Constraints: ResizeArray<string>
}

type Step = | Atomic of string
            | Sequence of StepChain
            | Cycle of StepChain
and StepChain = Step list

type MetaModel = {MainClass: string; Start: string; Steps: Dictionary<string, string>}
type Transformation = {Templates: Dictionary<string,string>; Iterators: Dictionary<string,string>; }

let measureTime phaseName index action =
    let sw = Stopwatch.StartNew()
    let result = action()
    sw.Stop()
    printfn $"%s{tool};%s{modelName};%d{index};0;%s{phaseName};Time;%d{(sw.ElapsedTicks * 100L)}"
    printfn $"%s{tool};%s{modelName};%d{index};0;%s{phaseName};Memory;%d{Environment.WorkingSet}"
    result

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
            | _ -> ())
    chain
    |> List.iter (fun step ->
        match step with
        | Atomic _ -> ()
        | Sequence inner | Cycle inner -> fillInPairwise(dict, inner))

let unquote (s: string): string =
    let t = s.Trim()
    let u = if t.Length >= 2 && t[0] = t[t.Length-1] && (t[0] = '"' || t[0] = '\'') then t[1..t.Length-2] else t
    u.Replace("\\n", "\n")
    
let stripMeta (s: string) : string = if s.Contains("{") then let l = s.Split('{')[0] in let r = s.Split('}') in l + r[r.Length-1] else s

let parseTransformation (filename: string) : Transformation =
    let mutable templates = Dictionary<string,string>()
    let mutable iterators = Dictionary<string,string>()
    File.ReadAllLines(filename)
    |> Array.iter (fun line ->
        if not(String.IsNullOrWhiteSpace(line)) then
            let parts = line.Trim().Split(' ')
            match parts[0] with
            | "template" -> templates[parts[1]] <- unquote(line[14+parts[1].Length..])
            | "each" -> iterators[parts[1]] <- parts[5..] |> Array.fold (fun acc pair -> match pair.Split("=") with | [|k; v|] -> acc.Replace(k, v) | _ -> acc) templates[parts[3]]
            | _ -> ())
    {Templates = templates; Iterators = iterators}

let parseGrammar(filename: string) =
    let tokens = File.ReadAllText(filename).Split(' ')
    let steps, _ = parseStepChain None (tokens[2..] |> Array.toList) // skip [1] which is '::='
    let transitions = Dictionary<string, string>()
    fillInTransactions(transitions, steps)
    { MainClass = tokens[0]; Start = getToFirst steps[0]; Steps = transitions }

let Initialization() = (parseGrammar metaModelPath, parseTransformation transformationPath)

let makeFeature(content: string) = { Name = content; Mandatory = ResizeArray<Feature>(); Optional = ResizeArray<Feature>(); Alternative = ResizeArray<Feature>(); Or = ResizeArray<Feature>(); Constraints = ResizeArray<string>() }

let Load(grammar: MetaModel, path: string) : Feature =
    eprintfn $"Loading %s{path}"
    let lines =
        File.ReadAllLines(path)
        |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
        |> Array.map (fun line -> (line |> Seq.takeWhile ((=) '\t') |> Seq.length, line.Trim()))

    let mutable outOfFeatures : bool = false
    let contextStack = Stack<ResizeArray<Feature>>()
    let mutable previous = 0
    let mutable step = grammar.Start
    let mutable context = ResizeArray<Feature>(0) // fake context to start
    let mutable result = ResizeArray<Feature>(lines.Length)

    for indent, content in lines do
        let delta = indent - previous
        previous <- indent
        
        if indent = 0 && result.Count <> 0 then
            outOfFeatures <- true
        elif outOfFeatures then
            result[0].Constraints.Add(content)
        else
            if delta > 0 then
                match context.Count with
                | 0 -> ()
                | _ -> contextStack.Push(context)
                if grammar.Steps.ContainsKey(step) then step <- grammar.Steps[step]
            elif delta < 0 then // && indent <> 0 
                for _ in 1 .. abs delta do
                    if contextStack.Count > 0 then contextStack.Pop() |> ignore
                if delta % 2 <> 0 then
                    if grammar.Steps.ContainsKey(step) then step <- grammar.Steps[step]

            match step with
            | "root" -> ()
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
            | _ -> ()
    result[0]

let rec emitFeature (script: Transformation) (feature: Feature) (lines: ResizeArray<string>) =
    let handleKind (kind:string, children:ResizeArray<Feature>) =
        if script.Iterators.ContainsKey(kind) then
            for child in children do
                lines.Add(script.Iterators[kind].Replace("SOURCE", feature.Name).Replace("TARGET", child.Name))
                emitFeature script child lines
    ["mandatory", feature.Mandatory; "optional", feature.Optional; "alternative", feature.Alternative;  "or", feature.Or] |> List.iter handleKind

let Initial(model: string, features: Feature, script: Transformation) =
    let output = ResizeArray<string>()
    if script.Templates.ContainsKey("BEFORE") then output.Add(script.Templates["BEFORE"])
    emitFeature script features output
    if script.Templates.ContainsKey("AFTER") then output.Add(script.Templates["AFTER"])
    File.WriteAllLines(Path.Combine(modelDirectory, "results", $"{model}_{tool}.dot"), output)

let Update(grammar: MetaModel, script: Transformation, name: string, path: string) =
    let confix = path.Split("_01")
    if confix.Length = 2 then
        let prefix = name[..name.Length-3] 
        let rec processModels i =
            let nextModel = $"{confix[0]}_%02d{i}{confix[1]}"
            let nextName = $"{prefix}_%02d{i}"
            if File.Exists(nextModel) then
                measureTime "Update" i (fun() ->
                    let features = Load(grammar, nextModel)
                    Initial(nextName, features, script))
                processModels (i+1)
        processModels 2
