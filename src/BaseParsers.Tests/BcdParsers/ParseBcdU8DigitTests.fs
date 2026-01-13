module Mbus.BaseParsers.Tests.BcdParsers.ParseBcd8DigitTests

open System
open Mbus.BaseParsers.BcdParsers
open Mbus.BaseParsers.Core
open Mbus.BaseParsers.Tests
open Xunit

let runPBcd8Digit = TestHelpers.runParser parseBcd8Digit
let runPBcd8DigitErr = TestHelpers.runFailingParser parseBcd8Digit

[<Fact>]
let ``parseBcd8Digit at first position returns uint32 and advances by four`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x34uy; 0x56uy; 0x78uy |]; Off = 0 }
    let expected = 78563412u
    runPBcd8Digit testState expected 4

[<Fact>]
let ``parseBcd8Digit max value returns 99999999 and advances by four`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x99uy; 0x99uy; 0x99uy; 0x99uy |]; Off = 0 }
    let expected = 99999999u
    runPBcd8Digit testState expected 4

[<Fact>]
let ``parseBcd8Digit fails on empty buffer`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| |]; Off = 0 }
    runPBcd8DigitErr testState "unexpected end of buffer" 0

[<Fact>]
let ``parseBcd8Digit fails with buffer overflow`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x34uy; 0x56uy |]; Off = 0 }
    runPBcd8DigitErr testState "unexpected end of buffer" 3

[<Fact>]
let ``parseBcd8Digit fails with invalid BCD digit`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x3Auy; 0x56uy; 0x78uy |]; Off = 0 }
    runPBcd8DigitErr testState "invalid BCD byte: 0x3A" 1