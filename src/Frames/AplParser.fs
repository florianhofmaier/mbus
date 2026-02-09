module Mbus.Frames.AplParser

open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Core
open Mbus.Frames
open Mbus.Records

let parseRecords l : Parser<Record list> = parser {
    let p = parseAll Record.Parser.parseRecord
    return! runOnSubSlice l p
}

let parseAlarm : Parser<uint8> = parser {
    return! parseU8
}

let parseSelect l : Parser<DeviceSelection> =
    parser {
        let! adr = AddressParser.parseAla
        let! rem = remainder
        if rem > 0 then
         let lenAdr = 8
         let! records = parseRecords (l - lenAdr)
         return { Adr = adr; Data = Some records }
        else
         return { Adr = adr; Data = None }
    }

let parseAny l ci: Parser<Apl> =
    (parser {
        match ci with
        | 0x52uy ->
            let! apl = parseSelect l |> withCtx "secondary selection"
            return DeviceSelection apl
        | 0x75uy ->
            let! apl = parseAlarm |> withCtx "alarms"
            return AlarmBits apl
        | _ ->
            let! apl = parseRecords l |> withCtx "records"
            return UserData apl
    }) |> withCtx "application layer"
