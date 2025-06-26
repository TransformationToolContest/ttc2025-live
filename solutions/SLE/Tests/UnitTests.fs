namespace Tests

open System.IO
open SLE.Core
open Microsoft.VisualStudio.TestTools.UnitTesting


[<TestClass>]
[<DoNotParallelize>]
type UnitTests () =
    let _grammar = parseGrammar metaModelPath

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
        let features = Load(_grammar, Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive01", "automotive01.uvl"))
        Assert.AreEqual<int>(708+1805, this.countFeatures(features))

    [<TestMethod>]
    member this.CountFeaturesInAutomotive02_01 () =
        let features = Load(_grammar, Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02", "automotive02_01.uvl"))
        Assert.AreEqual<int>(1396+4071+8442+101, this.countFeatures(features))

    [<TestMethod>]
    member this.CountFeaturesInAutomotive02_02 () =
        let features = Load(_grammar, Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02", "automotive02_02.uvl"))
        Assert.AreEqual<int>(4336+1705+97+11604, this.countFeatures(features))

    // [<TestMethod>]
    // member this.CountFeaturesInAutomotive02_03 () =
    //     let features = Load(_grammar, Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02", "automotive02_03.uvl"))
    //     Assert.AreEqual<int>(4481+1776+90+12087, this.countFeatures(features))
    
    // [<TestMethod>]
    // member this.CountFeaturesInAutomotive02_04 () =
    //     let features = Load(_grammar, Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", "automotive02", "automotive02_04.uvl"))
    //     Assert.AreEqual<int>(4336+1705+97+11604, this.countFeatures(features))
