module Mbus.BaseParsers.Tests.BinaryParsers.ParseU24Tests

open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Tests
open Xunit


let runPU24 = TestHelpers.runParser parseU24
let runPU24Err = TestHelpers.runFailingParser parseU24

[<Fact>]
let ``parseU24 at first position returns first u24 and advances by three`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy |] 0
    let expected = 0x332211u
    runPU24 testState expected 3

[<Fact>]
let ``parseU24 at second position returns u24 at second position and advances by three`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy |] 1
    let expected = 0x443322u
    runPU24 testState expected 4

[<Fact>]
let ``parseU24 at third last position returns last u24 and advances by three`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy |] 2
    let expected = 0x554433u
    runPU24 testState expected 5

[<Fact>]
let ``parseU24 max value returns 0xFFFFFF and advances by three`` () =
    let testState = TestStateHelpers.create [| 0xFFuy; 0xFFuy; 0xFFuy |] 0
    let expected = 0xFFFFFFu
    runPU24 testState expected 3

[<Fact>]
let ``parseU24 fails on empty buffer`` () =
    runPU24Err TestStateHelpers.empty "unexpected end of buffer" 0

[<Fact>]
let ``parseU24 fails at last position`` () =
    let testState = TestStateHelpers.create [| 0x11uy |] 0
    runPU24Err testState "unexpected end of buffer" 1

[<Fact>]
let ``parseU24 fails at second last position`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy |] 0
    runPU24Err testState "unexpected end of buffer" 2
