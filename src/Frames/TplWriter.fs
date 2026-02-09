module Mbus.Frames.TplWriter

open Mbus.BaseWriters.BinaryWriters
open Mbus.BaseWriters.Core
open Mbus.Frames
open Mbus.Frames.StatusField
open Mbus.Frames.Tpl

let writeCiOnly tplCiOnly : Writer<unit> =
    writer {
        let b =
            match tplCiOnly with
            | TplCiOnlyFunc.AplSelect -> ciAplSelect
            | TplCiOnlyFunc.Command -> ciCommand
            | TplCiOnlyFunc.DevSelect -> ciDevSelect
        do! writeU8 (fst b)
    }

let private writeCiShort ciShortFunc : Writer<unit> =
    writer {
        match ciShortFunc with
        | TplShortFunc.Rsp -> do! writeU8 (fst ciRspShort)
    }

let writeShort (tplShort: TplShort) : Writer<unit> =
    writer {
        do! writeCiShort tplShort.Func
        do! writeU8 tplShort.Acc
        do! writeStatusField tplShort.Status
        do! writeU16 tplShort.Cnf
    }

let writeCiLong ciLongFunc : Writer<unit> =
    writer {
        match ciLongFunc with
        | TplLongFunc.Rsp -> do! writeU8 (fst ciRspLong)
        | TplLongFunc.Alarm -> do! writeU8 (fst ciAlarm)
    }

let writeLong (tplLong: TplLong) : Writer<unit> =
    writer {
        do! writeCiLong tplLong.Func
        do! AddressWriter.writeAla tplLong.Ala
        do! writeU8 tplLong.Acc
        do! writeStatusField tplLong.Status
        do! writeU16 tplLong.Cnf
    }

let write tpl: Writer<unit> =
    writer {
        match tpl with
        | Tpl.CiOnly ciOnly -> do! writeCiOnly ciOnly
        | Tpl.Short short -> do! writeShort short
        | Tpl.Long long -> do! writeLong long
    }

