module Mbus.Frames.AplWriter

open Mbus.BaseWriters.BinaryWriters
open Mbus.BaseWriters.Core
open Mbus.Frames
open Mbus.Records

let writeUserData (records: Record list) : Writer<unit> =
    writer {
        for r in records do
            match r with
            | Record.Data dr -> do! Record.Writer.write dr
            | _ -> failwith "Not implemented"
    }

let writeAlarmBits alarm: Writer<unit> =
    writer {
        do! writeU8 alarm
    }

let writeDeviceSelection (select: DeviceSelection) : Writer<unit> =
    writer {
        do! AddressWriter.writeAla select.Adr
        match select.Data with
        | Some records ->
            for r in records do
                match r with
                | Record.Data dr -> do! Record.Writer.write dr
                | _ -> ()
        | None -> ()
    }

let write (apl: Apl) : Writer<unit> =
    writer {
        match apl with
        | Apl.UserData records -> do! writeUserData records
        | Apl.AlarmBits alarmBits -> do! writeAlarmBits alarmBits
        | Apl.DeviceSelection selection -> do! writeDeviceSelection selection
    }