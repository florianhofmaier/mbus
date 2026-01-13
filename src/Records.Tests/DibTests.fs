module Mbus.Records.Tests.DibTests
//
// open System
// open Mbus
// open Mbus.BaseParsers
// open Mbus.Records
// open Xunit
// open FsUnit.Xunit
//
// let zeroDib = { Data = MbusValue.NoData; StNum = StNum.create 0UL; Fn = InstValue; Tariff = Tariff.create 0u; SubUnit = SubUnit.create 0us }
//
// let sunnyWeatherCases: obj[] list =
//         [
//             [| ReadOnlyMemory<byte>([| 0x00uy |]); zeroDib |]
//             [| ReadOnlyMemory<byte>([| 0x80uy; 0x00uy |]); zeroDib |]
//             [| ReadOnlyMemory<byte>([| 0x01uy |]); { zeroDib with Data = MbusValue.Int8 } |]
//             [| ReadOnlyMemory<byte>([| 0x02uy |]); { zeroDib with Data = MbusValue.Int16 } |]
//             [| ReadOnlyMemory<byte>([| 0x03uy |]); { zeroDib with Data = MbusValue.Int24 } |]
//             [| ReadOnlyMemory<byte>([| 0x04uy |]); { zeroDib with Data = MbusValue.Int32 } |]
//             [| ReadOnlyMemory<byte>([| 0x05uy |]); { zeroDib with Data = MbusValue.Real32 } |]
//             [| ReadOnlyMemory<byte>([| 0x06uy |]); { zeroDib with Data = MbusValue.Int48 } |]
//             [| ReadOnlyMemory<byte>([| 0x07uy |]); { zeroDib with Data = MbusValue.Int64 } |]
//             [| ReadOnlyMemory<byte>([| 0x09uy |]); { zeroDib with Data = MbusValue.Bcd2Digit } |]
//             [| ReadOnlyMemory<byte>([| 0x0Auy |]); { zeroDib with Data = MbusValue.Bcd4Digit } |]
//             [| ReadOnlyMemory<byte>([| 0x0Buy |]); { zeroDib with Data = MbusValue.Bcd6Digit } |]
//             [| ReadOnlyMemory<byte>([| 0x0Cuy |]); { zeroDib with Data = MbusValue.Bcd8Digit } |]
//             [| ReadOnlyMemory<byte>([| 0x0Duy |]); { zeroDib with Data = MbusValue.VarLen } |]
//             [| ReadOnlyMemory<byte>([| 0x0Euy |]); { zeroDib with Data = MbusValue.Bcd12Digit } |]
//             [| ReadOnlyMemory<byte>([| 0x40uy |]); { zeroDib with StNum = StNum.create 1UL } |]
//             [| ReadOnlyMemory<byte>([| 0x80uy; 0x01uy |]); { zeroDib with StNum = StNum.create 2UL } |]
//             [| ReadOnlyMemory<byte>([| 0xC0uy; 0x8Fuy; 0x8Fuy; 0x8Fuy; 0x8Fuy; 0x8Fuy; 0x8Fuy; 0x8Fuy; 0x8Fuy; 0x8Fuy; 0x0Fuy |]); { zeroDib with StNum = StNum.create 0x1FFFFFFFFFFUL } |]
//             [| ReadOnlyMemory<byte>([| 0x10uy |]); { zeroDib with Fn = MinValue } |]
//             [| ReadOnlyMemory<byte>([| 0x20uy |]); { zeroDib with Fn = MaxValue } |]
//             [| ReadOnlyMemory<byte>([| 0x30uy |]); { zeroDib with Fn = ValueInErrorState } |]
//             [| ReadOnlyMemory<byte>([| 0x80uy; 0x10uy |]); { zeroDib with Tariff = Tariff.create 1u } |]
//             [| ReadOnlyMemory<byte>([| 0x80uy; 0xB0uy; 0xB0uy; 0xB0uy; 0xB0uy; 0xB0uy; 0xB0uy; 0xB0uy; 0xB0uy; 0xB0uy; 0x30uy |]); { zeroDib with Tariff = Tariff.create 0xFFFFFu } |]
//             [| ReadOnlyMemory<byte>([| 0x80uy; 0x40uy |]); { zeroDib with SubUnit = SubUnit.create 1us } |]
//             [| ReadOnlyMemory<byte>([| 0x80uy; 0xC0uy; 0xC0uy; 0xC0uy; 0xC0uy; 0xC0uy; 0xC0uy; 0xC0uy; 0xC0uy; 0xC0uy; 0x40uy |]); { zeroDib with SubUnit = SubUnit.create 0x3FFus } |]
//         ]
//
// let runPU8 st =
//     match parseU8 st with
//     | Ok (v, s) -> v, s
//     | Error e -> failwithf $"Unexpected: %A{e}"
//
// let runPDataField st b =
//     match MbusValue.parse b st with
//     | Ok (v, s) -> v, s
//     | Error e -> failwithf $"Unexpected: %A{e}"
//
// let parseDib buf value pos=
//     let dif, st0 = runPU8 { Buf = buf; Off = 0 }
//     let df, st1 = runPDataField st0 dif
//     match Dib.parse dif df st1 with
//     | Ok (v, st2) ->
//         v |> should equal value
//         st2.Off |> should equal pos
//     | Error e -> failwithf $"Unexpected: %A{e}"
//
// let parseDibFail buf msg pos =
//     let dif, st0 = runPU8 { Buf = buf; Off = 0 }
//     let df, st1 = runPDataField st0 dif
//     match Dib.parse dif df st1 with
//     | Error e ->
//         e.Msg |> should equal msg
//         e.Pos |> should equal pos
//     | Ok _ -> failwith "Expected failure"
//
// [<Theory>]
// [<MemberData(nameof(sunnyWeatherCases))>]
// let ``parse dib when dib is valid should return correct Dib and ParserState`` (buf, dib)=
//     parseDib buf dib buf.Length
//
// [<Fact>]
// let ``parse dif when buffer contains remaining bytes with ext flag not set should return correct Dib and ParserState``() =
//     let buf = ReadOnlyMemory<byte>([| 0x10uy; 0x42uy; 0x42uy |])
//     let dib = { zeroDib with Fn = MinValue }
//     parseDib buf dib 1
//
// [<Fact>]
// let ``parse dif/dife when buffer contains remaining bytes with ext flag not set should return correct Dib and ParserState``() =
//     let buf = ReadOnlyMemory<byte>([| 0x90uy; 0x10uy; 0x42uy; 0x42uy |])
//     let dib = { zeroDib with Fn = MinValue; Tariff = Tariff.create 1u }
//     parseDib buf dib 2
//
// [<Fact>]
// let ``parse dib when too many dife present should fail with correct error msg``() =
//     let buf = ReadOnlyMemory<byte>(Array.create 12 0x80uy)
//     parseDibFail buf "too many DIB/VIB bytes (limit 11)" 11