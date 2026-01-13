module Mbus.BaseParsers.Tests.BcdParsers.ParseBcd4DigitTests

open System
open Mbus.BaseParsers.BcdParsers
open Mbus.BaseParsers.Core
open Mbus.BaseParsers.Tests
open Xunit

let runP = TestHelpers.runParser parseBcd4Digit
let runPErr = TestHelpers.runFailingParser parseBcd4Digit

[<Fact>]
let ``parseBcd4Digit at first position returns uint16 and advances by two`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x34uy |]; Off = 0 }
    let expected = 3412us
    runP testState expected 2

[<Fact>]
let ``parseBcd4Digit max value returns 9999 and advances by two`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x99uy; 0x99uy |]; Off = 0 }
    let expected = 9999us
    runP testState expected 2

[<Fact>]
let ``parseBcd4Digit fails on empty buffer`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| |]; Off = 0 }
    runPErr testState "unexpected end of buffer" 0

[<Fact>]
let ``parseBcd4Digit fails with buffer overflow`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy |]; Off = 0 }
    runPErr testState "unexpected end of buffer" 1

[<Fact>]
let ``parseBcd4Digit fails with invalid BCD byte`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x12uy; 0x3Auy |]; Off = 0 }
    runPErr testState "invalid BCD byte: 0x3A" 1

