module Mbus.BaseParsers.Tests.BcdParsers.ParseBcd2DigitTests

open System
open Mbus.BaseParsers.BcdParsers
open Mbus.BaseParsers.Core
open Mbus.BaseParsers.Tests
open Xunit

let runPBcd2Digit = TestHelpers.runParser parseBcd2Digit
let runPBcd2DigitErr = TestHelpers.runFailingParser parseBcd2Digit

[<Fact>]
let ``parseBcd2Digit at first position returns uint8 and advances by one`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x42uy |]; Off = 0 }
    let expected = 42uy
    runPBcd2Digit testState expected 1

[<Fact>]
let ``parseBcd2Digit max value returns 99 and advances by one`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x99uy |]; Off = 0 }
    let expected = 99uy
    runPBcd2Digit testState expected 1

[<Fact>]
let ``parseBcd2Digit fails on empty buffer`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| |]; Off = 0 }
    runPBcd2DigitErr testState "unexpected end of buffer" 0

[<Fact>]
let ``parseBcd2Digit fails with invalid BCD byte`` () =
    let testState = { Buf = ReadOnlyMemory<byte> [| 0x9Auy |]; Off = 0 }
    runPBcd2DigitErr testState "invalid BCD byte: 0x9A" 0
