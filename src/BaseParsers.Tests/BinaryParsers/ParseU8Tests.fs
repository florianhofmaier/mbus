module Mbus.BaseParsers.Tests.BinaryParsers.ParseU8Tests

open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Tests
open Xunit

let runPU8 = TestHelpers.runParser parseU8
let runPU8Err = TestHelpers.runFailingParser parseU8

[<Fact>]
let ``parseU8 at first position returns first byte and advances by one`` () =
    let testState = TestStateHelpers.create [| 0xFFuy; |] 0
    runPU8 testState 0xFFuy 1

[<Fact>]
let ``parseU8 at second position returns second byte and advances by one`` () =
    let testState = TestStateHelpers.create [| 0xFFuy; 0x00uy; |] 1
    runPU8 testState 0x00uy 2

[<Fact>]
let ``parseU8 at last position returns last byte and advances by one`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; |] 2
    runPU8 testState 0x33uy 3

[<Fact>]
let ``parseU8 fails on empty buffer`` () =
    runPU8Err TestStateHelpers.empty "unexpected end of buffer" 0

[<Fact>]
let ``parseU8 fails at end of buffer`` () =
    let testState = TestStateHelpers.create [| 0xFFuy; |] 1
    runPU8Err testState "unexpected end of buffer" 1
