module Mbus.BaseParsers.Tests.BcdParsers.ParseBcd12DigitTests

open System
open Mbus.BaseParsers.BcdParsers
open Mbus.BaseParsers.Core
open Mbus.BaseParsers.Tests
open Xunit

let runP = TestHelpers.runParser parseBcd12Digit
let runPErr = TestHelpers.runFailingParser parseBcd12Digit

[<Fact>]
let ``parseBcd12Digit at first position returns uint64 and advances by six`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x34uy; 0x56uy; 0x78uy; 0x90uy; 0x12uy |]; Off = 0 }
    let expected = 129078563412UL
    runP testState expected 6

[<Fact>]
let ``parseBcd12Digit max value returns 999999999999 and advances by six`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x99uy; 0x99uy; 0x99uy; 0x99uy; 0x99uy; 0x99uy |]; Off = 0 }
    let expected = 999999999999UL
    runP testState expected 6

[<Fact>]
let ``parseBcd12Digit fails on empty buffer`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| |]; Off = 0 }
    runPErr testState "unexpected end of buffer" 0

[<Fact>]
let ``parseBcd12Digit fails with buffer overflow`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x34uy; 0x56uy; 0x78uy; 0x90uy |]; Off = 0 }
    runPErr testState "unexpected end of buffer" 5

[<Fact>]
let ``parseBcd12Digit fails with invalid BCD byte`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x34uy; 0x5Auy; 0x78uy; 0x90uy; 0x12uy |]; Off = 0 }
    runPErr testState "invalid BCD byte: 0x5A" 2
