namespace Tests

open System.IO
open SLE.Core
open Microsoft.VisualStudio.TestTools.UnitTesting


[<TestClass>]
type UnitTests () =
    member this.countFeatures (feature:Feature) : int =
        1 +
        Seq.sum [
            feature.Mandatory |> Seq.sumBy this.countFeatures
            feature.Optional |> Seq.sumBy this.countFeatures
            feature.Alternative |> Seq.sumBy this.countFeatures
            feature.Or |> Seq.sumBy this.countFeatures
        ]
    
    [<TestMethod>]
    member this.Trivial () =
        Assert.IsTrue(true)
        
    [<TestMethod>]
    member this.FeatureName () =
        let feature = makeFeature "engine"
        Assert.AreEqual<string>("engine", feature.Name)

    [<TestMethod>]
    member this.FreshInnersEmpty () =
        let feature = makeFeature "foo"
        Assert.AreEqual<int>(0, feature.Mandatory.Count)
        Assert.AreEqual<int>(0, feature.Optional.Count)
        Assert.AreEqual<int>(0, feature.Alternative.Count)
        Assert.AreEqual<int>(0, feature.Or.Count)

    [<TestMethod>]
    member this.CountFeaturesInAutomotive01 () =
        let grammar = parseGrammar metaModelPath
        let features = Load(grammar, Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive01", "automotive01.uvl"))
        Assert.AreEqual<int>(708+1805, this.countFeatures(features))

    [<TestMethod>]
    member this.CountFeaturesInAutomotive02 () =
        let grammar = parseGrammar metaModelPath
        let features = Load(grammar, Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02", "automotive02_01.uvl"))
        Assert.AreEqual<int>(1396+4071+8442+101, this.countFeatures(features))
