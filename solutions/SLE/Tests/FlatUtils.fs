module Tests.FlatUtils

open System.IO

let flatMetaModelPath = Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "solutions", "SLE", "specs", "flat.indentia")

type Container = {
    Name: string
    Storage: ResizeArray<Container>
}

let rec countFeaturesInContainer (container:Container) : int =
    1 + Seq.sum [container.Storage |> Seq.sumBy countFeaturesInContainer]

let makeContainer(content: string) = { Name = content; Storage = ResizeArray<Container>() }

let matchGoalContainer _ (context: ResizeArray<Container>) =
    if context.Count = 0 then context.Add {Name = "root"; Storage = ResizeArray()}
    context[context.Count-1].Storage
