module SLE.Core
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO

let getEnvOrDefault name defaultValue = match Environment.GetEnvironmentVariable(name) with | null -> defaultValue | value -> value
let tool = getEnvOrDefault "Tool" "SLE"
let modelName = getEnvOrDefault "Model" "linux"
let modelPath = getEnvOrDefault "ModelPath" (Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02", "automotive02_04.uvl"))
let modelDirectory = getEnvOrDefault "ModelDirectory" (Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "linux"))
let metaModelPath = Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "solutions", "SLE", "specs", "uvl.indentia")
let transformationPath = Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "solutions",   "SLE", "specs", "uvl2dot.scripta")

type Feature = { Name: string
                 Mandatory: ResizeArray<Feature>
                 Optional: ResizeArray<Feature>
                 Alternative: ResizeArray<Feature>
                 Or: ResizeArray<Feature>
                 Constraints: ResizeArray<string> }
type MetaModel = {MainClass: string; Start: string; Steps: Dictionary<string, string>}
type Transformation = {Templates: Dictionary<string,string>; Iterators: Dictionary<string,string>; }

let addExtra(e: obj, element: string) = match e with | :? Feature as f -> f.Constraints.Add(element) | _ -> ()

let measureTime phaseName index action =
    let sw = Stopwatch.StartNew()
    let result = action()
    sw.Stop()
    printfn $"%s{tool};%s{modelName};%d{index};0;%s{phaseName};Time;%d{(sw.ElapsedTicks * 100L)}"
    printfn $"%s{tool};%s{modelName};%d{index};0;%s{phaseName};Memory;%d{Environment.WorkingSet}"
    result
    
let unquote (s: string): string =
    let t = s.Trim()
    let l = t.Length
    let u = if l >= 2 && t[0] = t[l-1] && (t[0] = '"' || t[0] = '\'') then t[1..l-2] else t
    u.Replace("\\n", "\n")
    
let stripMeta (s: string) : string = if s.Contains("{") then let l = s.Split('{')[0] in let r = s.Split('}') in l + r[r.Length-1] else s

let parseTransformation (filename: string) : Transformation =
    let mutable templates = Dictionary<string,string>()
    let mutable iterators = Dictionary<string,string>()
    File.ReadAllLines(filename)
    |> Array.iter (fun line ->
            let parts = line.TrimEnd().Split(' ')
            match parts[0] with
            | "template" -> templates[parts[1]] <- unquote(line[14+parts[1].Length..])
            | "each" -> iterators[parts[1]] <- parts[5..] |> Array.fold (fun acc pair -> match pair.Split("=") with | [|k; v|] -> acc.Replace(k, v) | _ -> acc) templates[parts[3]]
            | _ -> ())
    {Templates = templates; Iterators = iterators}

let rec parseStepChain (dict: Dictionary<string, string>) (tokens: string list) : string option * string option * string list =
    let rec parseSequence tokens (acc: string option) (last: string option) (prev: string option) : string option * string option * string list =
        match tokens with
        | [] -> acc, last, []
        | token :: rest ->
            if token = "=>" then parseSequence rest acc last prev
            elif token = ")" || token = "]" then acc, last, rest
            elif token = "(" || token = "[" then
                let firstInner, lastInner, rem = parseStepChain dict rest
                match prev, firstInner with
                | Some nameA, Some nameB -> dict[nameA] <- nameB
                | _ -> ()
                if token = "[" then
                    match lastInner, firstInner with
                    | Some l, Some f -> dict[l] <- f
                    | _ -> ()
                parseSequence rem (if acc.IsNone then firstInner else acc) (if lastInner.IsSome then lastInner else last) None
            else match prev with
                 | Some nameA -> dict[nameA] <- token
                 | _ -> ()
                 parseSequence rest (if acc.IsNone then Some token else acc) (Some token) (Some token)
    parseSequence tokens None None None

let parseGrammar(filename: string) =
    let tokens = File.ReadAllText(filename).Split(' ') |> Array.toList |> List.skip 2 // skip [1] which is '::='
    let transitions = Dictionary<string, string>()
    let firstStep, _, _ = parseStepChain transitions tokens
    { MainClass = tokens[0]; Start = firstStep.Value; Steps = transitions }

let Initialization() = (parseGrammar metaModelPath, parseTransformation transformationPath)

let makeFeature(content: string) = { Name = content; Mandatory = ResizeArray<Feature>(); Optional = ResizeArray<Feature>(); Alternative = ResizeArray<Feature>(); Or = ResizeArray<Feature>(); Constraints = ResizeArray<string>() }

let matchGoalFeature (target: string) (context: ResizeArray<Feature>) =
    match target with
    | "features" -> context
    | "mandatory" -> context[context.Count-1].Mandatory
    | "optional" -> context[context.Count-1].Optional
    | "alternative" -> context[context.Count-1].Alternative
    | "or" -> context[context.Count-1].Or
    | _ -> printfn $"Unrecognised goal: '{target}'"; ResizeArray<Feature>()

let ReadIndexedLines path =
    File.ReadLines(path)
    |> Seq.choose (fun line -> let trimmed = line.TrimStart() in if line = "" then None else Some (line.Length - trimmed.Length, trimmed))

let Load<'T> (grammar:MetaModel) (path:string) (make:string -> 'T) (matchGoal:string -> ResizeArray<'T> -> ResizeArray<'T>) : 'T =
    eprintfn $"Loading %s{path}"
    let mutable outOfFeatures : bool = false
    let contextStack = Stack<ResizeArray<'T>>()
    let mutable previous = 0
    let mutable step = grammar.Start
    let mutable context = ResizeArray<'T>(0) // fake context to start
    let mutable result = ResizeArray<'T>()

    for indent, content in ReadIndexedLines path do
        let delta = indent - previous
        previous <- indent
        
        if indent = 0 && result.Count <> 0 then
            outOfFeatures <- true
        elif outOfFeatures then
            addExtra(result[0], content)
        else
            if delta > 0 then
                if context.Count > 0 then contextStack.Push(context)
                if grammar.Steps.ContainsKey(step) then step <- grammar.Steps[step]
            elif delta < 0 then // && indent <> 0 
                for _ in 1 .. (min -delta contextStack.Count) do contextStack.Pop() |> ignore
                if delta % 2 <> 0 then
                    if grammar.Steps.ContainsKey(step) then step <- grammar.Steps[step]

            match step with
            | "make" -> let feat = make(unquote(stripMeta(content)))
                        context.Add(feat)
                        if result.Count = 0 then result.Add(feat)
            | "goal" -> context <- matchGoal content context
            | _ -> ()
    result[0]

let rec emitFeature (script: Transformation) (feature: Feature) (lines: ResizeArray<string>) =
    let handleKind (kind:string, children:ResizeArray<Feature>) =
        if script.Iterators.ContainsKey(kind) then
            for child in children do
                lines.Add(script.Iterators[kind].Replace("SOURCE", feature.Name).Replace("TARGET", child.Name))
                emitFeature script child lines
    ["mandatory", feature.Mandatory; "optional", feature.Optional; "alternative", feature.Alternative;  "or", feature.Or] |> List.iter handleKind

let Initial (model:string) (features:Feature) (script:Transformation) =
    let output = ResizeArray<string>()
    if script.Templates.ContainsKey("BEFORE") then output.Add(script.Templates["BEFORE"])
    emitFeature script features output
    if script.Templates.ContainsKey("AFTER") then output.Add(script.Templates["AFTER"])
    File.WriteAllLines(Path.Combine(modelDirectory, "results", $"{model}_{tool}.dot"), output)

let Update (grammar:MetaModel) (script:Transformation) (name:string) (path:string) =
    let confix = path.Split("_01")
    if confix.Length = 2 then
        let rec processModels i =
            let nextModel = $"{confix[0]}_%02d{i}{confix[1]}"
            let nextName = $"{name}_%02d{i}"
            if File.Exists(nextModel) then
                measureTime "Update" i (fun() -> Initial nextName (Load grammar nextModel makeFeature matchGoalFeature) script)
                processModels (i+1)
        processModels 2
