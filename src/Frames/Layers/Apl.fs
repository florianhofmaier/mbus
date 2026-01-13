namespace Mbus.Frames.Layers
open Mbus
open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Core
open Mbus.Records
open Mbus.Frames.Address

type Select = { Adr: MbusAddress; Data: Record list option }

type Apl =
    | Data of Record list
    | Alarm of uint8
    | Select of Select

module Apl =

    let parseRecords l : P<Record list> = parser {
        let p = parseAll Record.parseRecord
        return! runOnSubSlice l p
    }

    let parseAlarm : P<uint8> = parser {
        return! parseU8
    }

    let parseSelect l : P<Select> = parser {
        let! adr = parseAla
        let! rem = remainder
        if rem > 0 then
            let lenAdr = 8
            let! records = parseRecords (l - lenAdr)
            return { Adr = adr; Data = Some records }
        else
            return { Adr = adr; Data = None }
    }

    let parse l ci: P<Apl> =
        (parser {
            match ci with
            | 0x52uy ->
                let! apl = parseSelect l |> withCtx "secondary selection"
                return Select apl
            | 0x75uy ->
                let! apl = parseAlarm |> withCtx "alarms"
                return Alarm apl
            | _ ->
                let! apl = parseRecords l |> withCtx "records"
                return Data apl
        }) |> withCtx "application layer"