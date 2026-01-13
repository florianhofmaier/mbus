module Frames.Tests.Layers.TplTests

open FsUnit.Xunit
open Mbus.Frames.Tests.Layers.TestHelpers
open Xunit
open Mbus
open Mbus.Frames.Layers

[<Fact>]
let ``parse Tpl with AplSelect bytes return correct Tpl`` () =
    let testState = createState [| 0x50uy |]
    let res, pos = runParserOk Tpl.parse testState
    res |> should equal (CiOnly AplSelect)
    pos |> should equal 1

[<Fact>]
let ``parse specific Tpl None with AplSelect bytes return correct Tpl`` () =
    let testState = createState[| 0x50uy |]
    let res, pos = runParserOk Tpl.parseCiOnly testState
    res |> should equal AplSelect
    pos |> should equal 1

[<Fact>]
let ``parse Tpl Command return correct Tpl`` () =
    let testState = createState [| 0x51uy |]
    let res, pos = runParserOk Tpl.parse testState
    res |> should equal (CiOnly Command)
    pos |> should equal 1

[<Fact>]
let ``parse Tpl specific None Command return correct Tpl`` () =
    let testState = createState [| 0x51uy |]
    let res, pos = runParserOk Tpl.parseCiOnly testState
    res |> should equal Command
    pos |> should equal 1

[<Fact>]
let ``parse Tpl None Select Device return correct Tpl`` () =
    let testState = createState [| 0x52uy |]
    let res, pos = runParserOk Tpl.parse testState
    res |> should equal (CiOnly DevSelect)
    pos |> should equal 1

[<Fact>]
let ``parse specific Tpl None Select Device return correct Tpl`` () =
    let testState = createState [| 0x52uy |]
    let res, pos = runParserOk Tpl.parseCiOnly testState
    res |> should equal DevSelect
    pos |> should equal 1

[<Fact>]
let ``parse specific Tpl None with invalid ci return error`` () =
    let testState = createState [| 0x7Auy |]
    let pos, msg, ctx = runParserErr Tpl.parseCiOnly testState
    pos |> should equal 0
    msg |> should equal "unexpected CI for TPL None: 0x7A"
    ctx |> should be Empty

[<Fact>]
let ``parse Tpl Short Rsp return correct Tpl`` () =
    let testState = createState[| 0x7Auy; 0x01uy; 0x02uy; 0x34uy; 0x12uy |]
    let expectedResult = Tpl.Short { Func = TplShortFunc.Rsp; Acc = 0x01uy; Status = 0x02uy; Cnf = 0x1234us }
    let res, pos = runParserOk Tpl.parse testState
    res |> should equal expectedResult
    pos |> should equal 5

[<Fact>]
let ``parse specific Tpl Short Rsp return correct Tpl`` () =
    let testState = createState [| 0x7Auy; 0x01uy; 0x02uy; 0x34uy; 0x12uy |]
    let expectedResult = { Func = TplShortFunc.Rsp; Acc = 0x01uy; Status = 0x02uy; Cnf = 0x1234us }
    let res, pos = runParserOk Tpl.parseShort testState
    res |> should equal expectedResult
    pos |> should equal 5

[<Fact>]
let ``parse specific Tpl Short with invalid ci return error`` () =
    let testState = createState [| 0x50uy |]
    let pos, msg, ctx = runParserErr Tpl.parseShort testState
    pos |> should equal 0
    msg |> should equal "unexpected CI for TPL Short: 0x50"
    ctx |> should be Empty

[<Fact>]
let ``parse Tpl Short with incomplete buffer return error`` () =
    let testState = createState [| 0x7Auy; 0x01uy; 0x02uy |]
    let pos, msg, ctx = runParserErr Tpl.parseShort testState
    pos |> should equal 3
    msg |> should equal "unexpected end of buffer"
    ctx |> should be Empty

[<Fact>]
let ``parse Tpl Long Rsp return correct Tpl`` () =
    let testState =
        createState [| 0x72uy; 0x78uy; 0x56uy; 0x34uy; 0x12uy; 0xE6uy; 0x1Euy; 0x01uy; 0x02uy; 0x03uy; 0x04uy; 0x34uy; 0x12uy |]

    let expectedResult =
        Tpl.Long
            { Func = TplLongFunc.Rsp
              Ala = MbusAddress.Create 12345678 "GWF" 1 MbusDeviceType.ElectricityMeter
              Acc = 0x03uy
              Status = 0x04uy
              Cnf = 0x1234us }

    let res, pos = runParserOk Tpl.parse testState
    res |> should equal expectedResult
    pos |> should equal 13

[<Fact>]
let ``parse specific Tpl  Long Rsp return correct Tpl`` () =
    let testState =
        createState [| 0x72uy; 0x78uy; 0x56uy; 0x34uy; 0x12uy; 0xE6uy; 0x1Euy; 0x01uy; 0x02uy; 0x03uy; 0x04uy; 0x34uy; 0x12uy |]

    let expectedResult =
        { Func = TplLongFunc.Rsp
          Ala = MbusAddress.Create 12345678 "GWF" 1 MbusDeviceType.ElectricityMeter
          Acc = 0x03uy
          Status = 0x04uy
          Cnf = 0x1234us }

    let res, pos = runParserOk Tpl.parseLong testState
    res |> should equal expectedResult
    pos |> should equal 13

[<Fact>]
let ``parse specific Tpl Long Alarm return correct Tpl`` () =
    let testState =
        createState [| 0x75uy; 0x78uy; 0x56uy; 0x34uy; 0x12uy; 0xE6uy; 0x1Euy; 0x01uy; 0x02uy; 0x03uy; 0x04uy; 0x34uy; 0x12uy |]

    let expectedResult =
        { Func = TplLongFunc.Alarm
          Ala = MbusAddress.Create 12345678 "GWF" 1 MbusDeviceType.ElectricityMeter
          Acc = 0x03uy
          Status = 0x04uy
          Cnf = 0x1234us }

    let res, pos = runParserOk Tpl.parseLong testState
    res |> should equal expectedResult
    pos |> should equal 13

[<Fact>]
let ``parse specific Tpl Long with invalid ci return error`` () =
    let testState = createState [| 0x50uy |]
    let pos, msg, ctx = runParserErr Tpl.parseLong testState
    pos |> should equal 0
    msg |> should equal "unexpected CI for TPL Long: 0x50"
    ctx |> should be Empty

[<Fact>]
let ``parse specific Tpl Long with incomplete buffer return error`` () =
    let testState = createState [| 0x72uy; 0x78uy; 0x56uy; 0x34uy; 0x12uy; 0xE6uy; 0x1Euy; 0x02uy; |]
    let pos, msg, ctx = runParserErr Tpl.parseLong testState
    pos |> should equal 8
    msg |> should equal "unexpected end of buffer"
    ctx |> should be Empty

[<Fact>]
let ``parse Tpl with empty buffer return error`` () =
    let testState = createState [| |]
    let pos, msg, ctx = runParserErr Tpl.parse testState
    pos |> should equal 0
    msg |> should equal "unexpected end of buffer"
    ctx |> should be Empty

[<Fact>]
let ``parse Tpl with incomplete buffer return error`` () =
    let testState = createState [| 0x7Auy; 0x01uy; 0x02uy; |]
    let pos, msg, ctx = runParserErr Tpl.parse testState
    pos |> should equal 3
    msg |> should equal "unexpected end of buffer"
    ctx |> should be Empty

[<Fact>]
let ``parse Tpl with invalid ci return error`` () =
    let testState =  createState [| 0x00uy |]
    let pos, msg, ctx = runParserErr Tpl.parse testState
    pos |> should equal 0
    msg |> should equal "unexpected CI for TPL: 0x00"
    ctx |> should be Empty
