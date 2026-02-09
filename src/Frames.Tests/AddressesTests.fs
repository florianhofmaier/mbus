module Mbus.Frames.Tests.AddressesTests

    open System
    open Xunit
    open FsUnit.Xunit
    open Mbus.BaseParsers.Core
    open Mbus.Frames
    open Mbus

    [<Fact>]
    let ``parse Lla return correct address`` () =
        let buf = [| 0xE6uy; 0x1Euy; 0x78uy; 0x56uy; 0x34uy; 0x12uy; 0x01uy; 0x03uy |]
        let st0 = { Buf = ReadOnlyMemory(buf); Off = 0 }
        match AddressParser.parseLla st0 with
        | Ok (adr, _) -> adr |> should equal (MbusAddress.Create 12345678 "GWF" 1 MbusDeviceType.GasMeter)
        | Error e -> failwithf $"Unexpected: %A{e}"

    [<Fact>]
    let ``parse Ala return correct address`` () =
        let buf = [| 0x78uy; 0x56uy; 0x34uy; 0x12uy; 0xE6uy; 0x1Euy; 0x01uy; 0x03uy |]
        let st0 = { Buf = ReadOnlyMemory(buf); Off = 0 }
        match AddressParser.parseAla st0 with
        | Ok (adr, _) -> adr |> should equal (MbusAddress.Create 12345678 "GWF" 1 MbusDeviceType.GasMeter)
        | Error e -> failwithf $"Unexpected: %A{e}"