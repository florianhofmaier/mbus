namespace Mbus.Messages

open Mbus
open Mbus.BaseWriters.Core
open Mbus.Frames
open Mbus.Records

type RspUdBuilder =
    { CField: uint8
      PrmAdr: uint8
      Tpl: TplLong
      Apl: Record list }

module RspUdBuilder =

    let init ala =
        { CField = CField.response
          PrmAdr = 0uy
          Tpl = { Func = TplLongFunc.Rsp; Ala = ala; Acc = 0uy; Status = MbusStatusField.CreateEmpty; Cnf = 0us }
          Apl = [] }

    let withDfcSet builder : RspUdBuilder =
        { builder with CField = CField.setDfc builder.CField }

    let withAcdSet builder : RspUdBuilder =
        { builder with CField = CField.setAcd builder.CField }

    let withPrimaryAddress addr builder : RspUdBuilder =
        { builder with PrmAdr = addr }

    let addDataRecord record builder : RspUdBuilder =
        { builder with Apl = builder.Apl @ [ Data record ] }

    let withStatus status builder : RspUdBuilder =
        { builder with Tpl.Status = status }

    let withConfigField cnf builder : RspUdBuilder =
        { builder with Tpl.Cnf = cnf }

    let withAccessNumber acc builder : RspUdBuilder =
        { builder with Tpl.Acc = acc }

    let build builder : Writer<unit> =
        let longFrame: LongFrame =
            { CField = builder.CField
              PrmAdr = builder.PrmAdr
              Tpl = builder.Tpl |> Tpl.Long
              Apl = builder.Apl |> Apl.UserData }
        writer {
            do! FrameWriter.writeLongFrame longFrame
        }
