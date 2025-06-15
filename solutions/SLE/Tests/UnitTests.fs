namespace Tests

open SLE.Core
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type UnitTests () =

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
