module Frames.Tests.MbusFramesTests

open System
open Mbus
open Mbus.BaseParsers.Core
open Mbus.Frames
open Mbus.Records
open Mbus.Records.DataInfoBlocks
open Xunit
open FsUnit.Xunit

let parseFrame buf =
    FrameParser.parseAny { Off = 0; Buf = ReadOnlyMemory<uint8>(buf) }

let parseValidFrame buf =
    match parseFrame buf with
    | Ok (frame, _) -> frame
    | Error e -> failwithf $"unexpected: %A{e}"

let parseFrameWithError buf =
    match parseFrame buf with
    | Ok (frame, _) -> failwithf $"expected error but got {frame}"
    | Error e -> e

[<Fact>]
let ``parse Cnf frame when byte is 0xE5 should return Cnf`` () =
    let buf = [| 0xE5uy; |]
    parseValidFrame buf |> should equal Confirmation

[<Fact>]
let ``parse Cnf frame when byte is not 0xE5 should return error`` () =
    let buf = [| 0x42uy; |]
    let expected = { Pos = 0; Msg = "invalid start byte: 0x42"; Ctx = [ ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Short frame when frame is valid SND-NKE should return short frame`` () =
    let buf = [| 0x10uy; 0x40uy; 0x01uy; 0x41uy; 0x16uy |]
    let expected = Frame.ShortFrame { PrmAdr = 0x01uy; CField = 0x40uy }
    parseValidFrame buf |> should equal expected

[<Fact>]
let ``parse Short frame when frame is valid REQ-UD1 should return short frame`` () =
    let buf = [| 0x10uy; 0x7Auy; 0x99uy; 0x13uy; 0x16uy |]
    let expected = Frame.ShortFrame { PrmAdr = 0x99uy; CField = 0x7Auy }
    parseValidFrame buf |> should equal expected

[<Fact>]
let ``parse Short frame when frame is valid REQ-UD2 should return short frame`` () =
    let buf = [| 0x10uy; 0x5Buy; 0x42uy; 0x9Duy; 0x16uy |]
    let expected = Frame.ShortFrame { PrmAdr = 0x42uy; CField = 0x5Buy }
    parseValidFrame buf |> should equal expected

[<Fact>]
let ``parse Short frame when start byte is invalid`` () =
    let buf = [| 0x11uy; 0x40uy; 0x01uy; 0x41uy; 0x16uy |]
    let expected = { Pos = 0; Msg = "invalid start byte: 0x11"; Ctx = [ ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Short frame when CRC is invalid should return error`` () =
    let buf = [| 0x10uy; 0x40uy; 0x01uy; 0x42uy; 0x16uy |]
    let expected = { Pos = 3; Msg = "invalid checksum: expect 0x41, got 0x42"; Ctx = [ "short frame" ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Short frame when stop byte is invalid should return error`` () =
    let buf = [| 0x10uy; 0x40uy; 0x01uy; 0x41uy; 0x15uy |]
    let expected = { Pos = 4; Msg = "invalid stop byte: expect 0x16, got 0x15"; Ctx = [ "short frame" ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Long frame when frame is valid SND-UD should return long frame`` () =
    let buf = [| 0x68uy; 0x06uy; 0x06uy; 0x68uy; 0x53uy; 0x00uy; 0x51uy; 0x01uy; 0x7Auy; 0x01uy; 0x20uy; 0x16uy |]
    let result = parseValidFrame buf
    match result with
    | Frame.LongFrame frame ->
        frame.CField |> should equal 0x53uy
        frame.PrmAdr |> should equal 0x00uy
        frame.Tpl |> should equal (Tpl.CiOnly Command)
        match frame.Apl with
        | UserData records ->
            records.Length |> should equal 1
            let record = records[0]
            match record with
            | Data dr ->
                dr.Value |> should equal (MbusValue.Int8 1y)
                dr.StNum |> should equal StorageNumber.zero
                dr.Fn|> should equal InstValue
                dr.Tariff |> should equal Tariff.zero
                dr.SubUnit |> should equal SubUnit.zero
                dr.Vib |> should equal (Normal { Def = { Val = Address; Unit = NoUnit; Scaler = 1m }; Ext = [] })
            | _ -> failwith "Expected DataRecord"
        | _ -> failwith "Expected Data APL"
    | _ -> failwith "Expected Long frame"

[<Fact>]
let ``parse Long frame when start byte 1 is invalid should return error`` () =
    let buf = [| 0x00uy; 0x06uy; 0x06uy; 0x68uy; 0x53uy; 0x00uy; 0x51uy; 0x01uy; 0x7Auy; 0x01uy; 0x20uy; 0x16uy |]
    let expected = { Pos = 0; Msg = "invalid start byte: 0x00"; Ctx = [ ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Long frame when start byte 2 is invalid should return error`` () =
    let buf = [| 0x68uy; 0x06uy; 0x06uy; 0x67uy; 0x53uy; 0x00uy; 0x51uy; 0x01uy; 0x7Auy; 0x01uy; 0x20uy; 0x16uy |]
    let expected = { Pos = 3; Msg = "invalid start byte: expect 0x68, got 0x67"; Ctx = [ "long frame" ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Long frame when length bytes mismatch should return error`` () =
    let buf = [| 0x68uy; 0x06uy; 0x07uy; 0x68uy; 0x53uy; 0x00uy; 0x51uy; 0x01uy; 0x7Auy; 0x01uy; 0x20uy; 0x16uy |]
    let expected = { Pos = 2; Msg = "length byte mismatch: expect 0x06, got 0x07"; Ctx = [ "long frame" ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Long frame when ci is invalid should return error`` () =
    let buf = [| 0x68uy; 0x06uy; 0x06uy; 0x68uy; 0x53uy; 0x00uy; 0x71uy; 0x01uy; 0x7Auy; 0x01uy; 0x20uy; 0x16uy |]
    let expected = { Pos = 6; Msg = "unexpected CI for TPL: 0x71"; Ctx = [ "long frame" ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Long frame when CRC is invalid should return error`` () =
    let buf = [| 0x68uy; 0x06uy; 0x06uy; 0x68uy; 0x53uy; 0x00uy; 0x51uy; 0x01uy; 0x7Auy; 0x01uy; 0x21uy; 0x16uy |]
    let expected = { Pos = 10; Msg = "invalid checksum: expect 0x20, got 0x21"; Ctx = [ "long frame" ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Long frame when stop byte is invalid should return error`` () =
    let buf = [| 0x68uy; 0x06uy; 0x06uy; 0x68uy; 0x53uy; 0x00uy; 0x51uy; 0x01uy; 0x7Auy; 0x01uy; 0x20uy; 0x01uy |]
    let expected = { Pos = 11; Msg = "invalid stop byte: expect 0x16, got 0x01"; Ctx = [ "long frame" ] }
    parseFrameWithError buf |> should equal expected

[<Fact>]
let ``parse Long frame when frame is valid RSP-UD should return long frame`` () =
    let buf = [| 0x68uy ;0x1Buy ;0x1Buy ;0x68uy ;0x08uy ;0x03uy ;0x72uy ;0x93uy ;0x22uy; 0x54uy; 0x21uy; 0xE6uy; 0x1Euy; 0x3Cuy; 0x07uy; 0x01uy; 0x00uy; 0x00uy; 0x00uy; 0x0Cuy ;0x78uy ;0x93uy ;0x22uy ;0x54uy ;0x21uy ;0x0Cuy ;0x13uy ;0x57uy ;0x04uy ;0x00uy ;0x00uy ;0x17uy ;0x16uy |]
    let result = parseValidFrame buf
    match result with
    | Frame.LongFrame frame ->
        frame.CField |> should equal 0x08uy
        frame.PrmAdr |> should equal 0x03uy
        match frame.Tpl with
        | Tpl.Long tpl ->
            tpl.Ala.Mfr |> should equal "GWF"
            tpl.Ala.IdNumber |> should equal 21542293
            tpl.Ala.Version |> should equal 0x3C
            tpl.Ala.DeviceType |> should equal MbusDeviceType.WaterMeter
            tpl.Acc |> should equal 0x01uy
            tpl.Status |> should equal 0x00uy
            tpl.Cnf |> should equal 0x0000us
            tpl.Func |> should equal TplLongFunc.Rsp
        | _ -> failwith "Expected Long TPL"
        match frame.Apl with
        | UserData records ->
            records.Length |> should equal 2
            match records[0] with
            | Data dr ->
                dr.Value|> should equal (MbusValue.Bcd8Digit 21542293u)
                dr.StNum |> should equal StorageNumber.zero
                dr.Fn |> should equal InstValue
                dr.Tariff |> should equal Tariff.zero
                dr.SubUnit |> should equal SubUnit.zero
                dr.Vib |> should equal (Normal { Def = { Val = FabricationNumber; Unit = NoUnit; Scaler = 1m }; Ext = [  ] })
            | _ -> failwith "Expected DataRecord"
            match records[1] with
            | Data dr ->
                dr.Value|> should equal (MbusValue.Bcd8Digit 457u)
                dr.StNum |> should equal StorageNumber.zero
                dr.Fn |> should equal InstValue
                dr.Tariff |> should equal Tariff.zero
                dr.SubUnit |> should equal SubUnit.zero
                dr.Vib |> should equal (Normal { Def = { Val = Volume; Unit = CubicMeters; Scaler = 0.001m }; Ext = [  ] })
            | _ -> failwith "Expected DataRecord"
        | _ -> failwith "Expected Data APL"
    | _ -> failwith "Expected Long frame"