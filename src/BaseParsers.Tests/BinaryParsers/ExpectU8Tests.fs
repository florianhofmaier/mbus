module Mbus.BaseParsers.Tests.BinaryParsers.ExpectU8Tests

open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Tests
open Xunit

let runEU8 expect = TestHelpers.runParser (expectU8 expect "")
let runEU8Err expect = TestHelpers.runFailingParser (expectU8 expect "")

[<Fact>]
let ``expectU8 at first position with matching byte advances by one`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy |] 0
    runEU8 0x11uy testState () 1

[<Fact>]
let ``expectU8 at first position with mismatching byte returns error`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy |] 0
    runEU8Err 0x22uy testState ": expect 0x22, got 0x11" 0

[<Fact>]
let ``expectU8 fails at end of buffer`` () =
    let testState = TestStateHelpers.create [| 0x42uy |] 1
    runEU8Err 0x42uy testState "unexpected end of buffer" 1
