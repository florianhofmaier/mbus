module Mbus.BaseParsers.Tests.BcdParsers.ParseBcd6DigitTests

open System
open Mbus.BaseParsers.BcdParsers
open Mbus.BaseParsers.Core
open Mbus.BaseParsers.Tests
open Xunit

let runP = TestHelpers.runParser parseBcd6Digit
let runPErr = TestHelpers.runFailingParser parseBcd6Digit

[<Fact>]
let ``parseBcd6Digit at first position returns uint32 and advances by three`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x34uy; 0x56uy |]; Off = 0 }
    let expected = 563412u
    runP testState expected 3

[<Fact>]
let ``parseBcd6Digit max value returns 999999 and advances by three`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x99uy; 0x99uy; 0x99uy |]; Off = 0 }
    let expected = 999999u
    runP testState expected 3

[<Fact>]
let ``parseBcd6Digit fails on empty buffer`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| |]; Off = 0 }
    runPErr testState "unexpected end of buffer" 0

[<Fact>]
let ``parseBcd6Digit fails with buffer overflow`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x34uy |]; Off = 0 }
    runPErr testState "unexpected end of buffer" 2

[<Fact>]
let ``parseBcd6Digit fails with invalid BCD byte`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x3Auy; 0x56uy |]; Off = 0 }
    runPErr testState "invalid BCD byte: 0x3A" 1

