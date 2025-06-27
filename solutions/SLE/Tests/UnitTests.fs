namespace Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.IO
open SLE.Core
open FeatureUtils

[<TestClass>]
[<DoNotParallelize>]
type UnitTests () =
    let _grammar = parseGrammar metaModelPath

    member this.solutionPath (folder:string) (uvl:string) =
        Path.Combine(Directory.GetCurrentDirectory().Split("solutions")[0], "models", folder, uvl + ".uvl")
    
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
    member this.CountFeaturesInBerkeleyDB () =
        let features = Load _grammar (this.solutionPath "berkeleydb" "berkeleydb") makeFeature matchGoalFeature
        Assert.AreEqual<int>(76, countFeatures(features))

    [<TestMethod>]
    member this.CountFeaturesInAutomotive01 () =
        let features = Load _grammar (this.solutionPath "automotive01" "automotive01") makeFeature matchGoalFeature
        Assert.AreEqual<int>(708+1805, countFeatures(features))

    [<TestMethod>]
    member this.CountFeaturesInAutomotive02_01 () =
        let features = Load _grammar (this.solutionPath "automotive02" "automotive02_01") makeFeature matchGoalFeature
        Assert.AreEqual<int>(1396+4071+8442+101, countFeatures(features))

    [<TestMethod>]
    member this.CountFeaturesInAutomotive02_02 () =
        let features = Load _grammar (this.solutionPath "automotive02" "automotive02_02") makeFeature matchGoalFeature
        Assert.AreEqual<int>(4336+1705+97+11604, countFeatures(features))
    
    // [<TestMethod>]
    // member this.CountFeaturesInAutomotive02_03 () =
    //     let features = Load _grammar (this.solutionPath "automotive02" "automotive02_03") makeFeature matchGoalFeature
    //     Assert.AreEqual<int>(4336+1705+97+11604, this.countFeatures(features))
    //
    // [<TestMethod>]
    // member this.CountFeaturesInAutomotive02_04 () =
    //     let features = Load _grammar (this.solutionPath "automotive02" "automotive02_04") makeFeature matchGoalFeature
    //     Assert.AreEqual<int>(4336+1705+97+11604, this.countFeatures(features))
