module Mbus.BaseParsers.Tests.BinaryParsers.ParseU48Tests

open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Tests
open Xunit


let runPU48 = TestHelpers.runParser parseU48
let runPU48Err = TestHelpers.runFailingParser parseU48

[<Fact>]
let ``parseU48 at first position returns first u48 and advances by six`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy; 0x66uy; 0x77uy |] 0
    let expected = 0x665544332211UL
    runPU48 testState expected 6

[<Fact>]
let ``parseU48 at second position returns u48 at second position and advances by six`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy; 0x66uy; 0x77uy |] 1
    let expected = 0x776655443322UL
    runPU48 testState expected 7

[<Fact>]
let ``parseU48 at sixth last position returns last u48 and advances by six`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy; 0x66uy |] 0
    let expected = 0x665544332211UL
    runPU48 testState expected 6

[<Fact>]
let ``parseU48 max value returns 0xFFFFFFFFFFFF and advances by six`` () =
    let testState = TestStateHelpers.create [| 0xFFuy; 0xFFuy; 0xFFuy; 0xFFuy; 0xFFuy; 0xFFuy |] 0
    let expected = 0xFFFFFFFFFFFFUL
    runPU48 testState expected 6

[<Fact>]
let ``parseU48 fails on empty buffer`` () =
    runPU48Err TestStateHelpers.empty "unexpected end of buffer" 0

[<Fact>]
let ``parseU48 fails at last position`` () =
    let testState = TestStateHelpers.create [| 0x11uy |] 0
    runPU48Err testState "unexpected end of buffer" 1

[<Fact>]
let ``parseU48 fails at fifth last position`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy |] 0
    runPU48Err testState "unexpected end of buffer" 5
