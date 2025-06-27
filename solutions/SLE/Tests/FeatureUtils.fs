module Tests.FeatureUtils

open SLE.Core

let rec countFeatures (feature:Feature) : int =
    1 +
    Seq.sum [
        feature.Mandatory |> Seq.sumBy countFeatures
        feature.Optional |> Seq.sumBy countFeatures
        feature.Alternative |> Seq.sumBy countFeatures
        feature.Or |> Seq.sumBy countFeatures
    ]