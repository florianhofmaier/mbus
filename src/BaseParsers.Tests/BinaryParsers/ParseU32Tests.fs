module Mbus.BaseParsers.Tests.BinaryParsers.ParseU32Tests

open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Tests
open Xunit


let runPU32 = TestHelpers.runParser parseU32
let runPU32Err = TestHelpers.runFailingParser parseU32

[<Fact>]
let ``parseU32 at first position returns first u32 and advances by four`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy; 0x66uy |] 0
    let expected = 0x44332211u
    runPU32 testState expected 4

[<Fact>]
let ``parseU32 at second position returns u32 at second position and advances by four`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy; 0x66uy |] 1
    let expected = 0x55443322u
    runPU32 testState expected 5

[<Fact>]
let ``parseU32 at fourth last position returns last u32 and advances by four`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy; 0x44uy; 0x55uy; 0x66uy |] 2
    let expected = 0x66554433u
    runPU32 testState expected 6

[<Fact>]
let ``parseU32 max value returns 0xFFFFFFFF and advances by four`` () =
    let testState = TestStateHelpers.create [| 0xFFuy; 0xFFuy; 0xFFuy; 0xFFuy |] 0
    let expected = 0xFFFFFFFFu
    runPU32 testState expected 4

[<Fact>]
let ``parseU32 fails on empty buffer`` () =
    runPU32Err TestStateHelpers.empty "unexpected end of buffer" 0

[<Fact>]
let ``parseU32 fails at last position`` () =
    let testState = TestStateHelpers.create [| 0x11uy |] 0
    runPU32Err testState "unexpected end of buffer" 1

[<Fact>]
let ``parseU32 fails at second last position`` () =
    let testState = TestStateHelpers.create [| 0x11uy; 0x22uy; 0x33uy |] 0
    runPU32Err testState "unexpected end of buffer" 3
