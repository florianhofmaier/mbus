module Mbus.Records.Tests.StorageNumberTests
//
//     open System
//     open Mbus.Records
//     open Xunit
//     open FsUnit.Xunit
//
//     [<Fact>]
//     let ``create StNum with 0 should return ok`` () =
//         let stNum = StNum.create 0
//         mat
//         StNum.value stNum |> should equal 0
//
//     [<Fact>]
//     let ``create StNum with a valid value should not throw`` () =
//         let stNum = StNum.create 42UL
//         StNum.value stNum |> should equal 42UL
//
//     [<Fact>]
//     let ``create StNum with max value should not throw`` () =
//         let maxVal = (1UL <<< 41) - 1UL
//         let stNum = StNum.create maxVal
//         StNum.value stNum |> should equal maxVal
//
//     [<Fact>]
//     let ``create StNum with value > max should throw`` () =
//         let tooBig = (1UL <<< 41)
//         fun () -> StNum.create tooBig
//         |> should throw typeof<ArgumentException>
//
//     [<Fact>]
//     let ``create StNum with UINT64MAX should throw`` () =
//         fun () -> StNum.create UInt64.MaxValue
//         |> should throw typeof<ArgumentException>