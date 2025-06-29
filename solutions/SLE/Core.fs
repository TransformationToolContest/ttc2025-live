module SLE.Core
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO

let getEnvOrDefault name defaultValue = match Environment.GetEnvironmentVariable(name) with | null -> defaultValue | value -> value
let tool = getEnvOrDefault "Tool" "SLE"
let modelName = getEnvOrDefault "Model" "automotive02_04"
let modelPath = getEnvOrDefault "ModelPath" (Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02", "automotive02_04.uvl"))
let modelDirectory = getEnvOrDefault "ModelDirectory" (Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02"))
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
    printfn $"%s{tool};%s{modelName};%d{index};0;%s{phaseName};Time;%d{sw.ElapsedTicks * 100L}"
    printfn $"%s{tool};%s{modelName};%d{index};0;%s{phaseName};Memory;%d{Environment.WorkingSet}"
    result
    
let unquote (s: string): string = let t = s.Trim() in let l = t.Length in (if l >= 2 && t[0] = t[l-1] && (t[0] = '"' || t[0] = '\'') then t[1..l-2] else t).Replace("\\n", "\n")
let stripMeta (s: string) : string = if s.Contains("{") then let l = s.Split('{')[0] in let r = s.Split('}') in l + r[r.Length-1] else s
let makeSafeForSVG (text: string) : string = ["&", "&amp;"; "<=>", "⇔"; "=>", "⇒"; "<", "&lt;"; ">", "&gt;"; "\"", "&quot;"; "'", "&apos;" ] |> List.fold _.Replace text

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

let rec parseStepChain (dict: Dictionary<string, string>) (tokens: string[]) (startIdx: int) : string option * string option * int =
    let rec parseSequence idx (acc: string option) (past: string option) (prev: string option) : string option * string option * int =
        if idx >= tokens.Length then acc, past, idx
        else let token = tokens[idx]
             if token = "=>" then parseSequence (idx+1) acc past prev
             elif token = ")" || token = "]" then acc, past, idx+1
             elif token = "(" || token = "[" then
                 let first, last, remIdx = parseStepChain dict tokens (idx+1)
                 if prev.IsSome && first.IsSome then dict[prev.Value] <- first.Value
                 if token = "[" && last.IsSome && first.IsSome then dict[last.Value] <- first.Value
                 parseSequence remIdx (if acc.IsNone then first else acc) (if last.IsSome then last else past) None
             else
                  if prev.IsSome then dict[prev.Value] <- token 
                  parseSequence (idx+1) (if acc.IsNone then Some token else acc) (Some token) (Some token)
    parseSequence startIdx None None None

let parseGrammar(filename: string) =
    let tokens = File.ReadAllText(filename).Split(' ') 
    let transitions = Dictionary<string, string>()
    let firstStep, _, _ = parseStepChain transitions tokens 2 // skip tokens[1] which is '::='
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

let Load<'T> (grammar:MetaModel) (path:string) (make:string->'T) (goal:string->ResizeArray<'T>->ResizeArray<'T>) : 'T =
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
            | "goal" -> context <- goal content context
            | _ -> ()
    result[0]

let rec emitFeature (script: Transformation) (feature: Feature) (lines: ResizeArray<string>) =
    let handleKind (kind:string, children:ResizeArray<Feature>) =
        if script.Iterators.ContainsKey(kind) then
            for child in children do
                lines.Add(script.Iterators[kind].Replace("SOURCE", feature.Name).Replace("TARGET", child.Name))
                emitFeature script child lines
    ["mandatory", feature.Mandatory; "optional", feature.Optional; "alternative", feature.Alternative;  "or", feature.Or] |> List.iter handleKind

let rec emitConstraint (script: Transformation) (feature: Feature) (lines: ResizeArray<string>) =
    if script.Iterators.ContainsKey("constraint") then
        for child in feature.Constraints do
            lines.Add(script.Iterators["constraint"].Replace("TEXT", makeSafeForSVG(child)))

let Initial (model:string) (features:Feature) (script:Transformation) =
    let applyIfExists (script:Transformation) (name:string) (output:ResizeArray<string>) =
        if script.Templates.ContainsKey(name) then output.Add(script.Templates[name])
    let output = ResizeArray<string>()
    applyIfExists script "BEFORE" output
    emitFeature script features output
    if features.Constraints.Count > 0 then
        applyIfExists script "BEFORE_CONSTRAINTS" output
        emitConstraint script features output
        applyIfExists script "AFTER_CONSTRAINTS" output
    applyIfExists script "AFTER" output
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
