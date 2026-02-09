namespace Mbus.Frames

open Mbus

type TplShortFunc =
    | Rsp

type TplShort =
    { Func: TplShortFunc
      Acc: uint8
      Status: MbusStatusField
      Cnf: uint16 }

type TplLongFunc =
    | Rsp
    | Alarm

type TplLong =
    { Func : TplLongFunc
      Ala: MbusAddress
      Acc: uint8
      Status: MbusStatusField
      Cnf: uint16 }

type TplCiOnlyFunc = | AplSelect | Command | DevSelect

type Tpl =
    | CiOnly of TplCiOnlyFunc
    | Short of TplShort
    | Long of TplLong

module Tpl =
    let getLen tpl =
        match tpl with
        | Tpl.CiOnly _ -> 1
        | Tpl.Short _ -> 4
        | Tpl.Long _ -> 13

    let ciAplSelect = 0x50uy, CiOnly
    let ciCommand = 0x51uy, CiOnly
    let ciDevSelect = 0x52uy, CiOnly
    let ciRspShort = 0x7Auy, Short
    let ciRspLong = 0x72uy, Long
    let ciAlarm = 0x75uy, Long

    let getCi tpl =
        match tpl with
        | CiOnly AplSelect -> fst ciAplSelect
        | CiOnly Command -> fst ciCommand
        | CiOnly DevSelect -> fst ciDevSelect
        | Short { Func = TplShortFunc.Rsp } -> fst ciRspShort
        | Long { Func = TplLongFunc.Rsp } -> fst ciRspLong
        | Long { Func = TplLongFunc.Alarm } -> fst ciAlarm
