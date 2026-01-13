module Mbus.BaseParsers.Tests.BinaryParsers.PeekU8Tests

open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Tests
open Xunit

let runPU8 = TestHelpers.runParser peekU8
let runPU8Err = TestHelpers.runFailingParser peekU8

[<Fact>]
let ``peekU8 at first position returns first byte and doesn't advance`` () =
    let testState = TestStateHelpers.create [| 0x01uy; 0x02uy; 0x03uy |] 0
    runPU8 testState 0x01uy 0

[<Fact>]
let ``peekU8 at second position returns second byte and doesn't advance`` () =
    let testState = TestStateHelpers.create [| 0x01uy; 0x02uy; 0x03uy |] 1
    runPU8 testState 0x02uy 1

[<Fact>]
let ``peekU8 at last position returns last byte and doesn't advance`` () =
    let testState = TestStateHelpers.create [| 0x01uy; 0x02uy; 0x03uy |] 2
    runPU8 testState 0x03uy 2

[<Fact>]
let ``peekU8 fails on empty buffer`` () =
    runPU8Err TestStateHelpers.empty "unexpected end of buffer" 0

[<Fact>]
let ``peekU8 fails at end of buffer`` () =
    let testState = TestStateHelpers.create [| 0x01uy |] 1
    runPU8Err testState "unexpected end of buffer" 1
