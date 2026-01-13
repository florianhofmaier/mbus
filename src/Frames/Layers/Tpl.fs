namespace Mbus.Frames.Layers

open Mbus
open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Core
open Mbus.Frames

type TplShortFunc = | Rsp
type TplShort = { Func: TplShortFunc; Acc: uint8; Status: uint8; Cnf: uint16; }

type TplLongFunc =
    | Rsp
    | Alarm

type TplLong =
    { Func : TplLongFunc
      Ala: MbusAddress
      Acc: uint8
      Status: uint8
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

    let private aplSelect = 0x50uy, CiOnly
    let private command = 0x51uy, CiOnly
    let private devSelect = 0x52uy, CiOnly
    let private rspShort = 0x7Auy, Short
    let private rspLong = 0x72uy, Long
    let private alarm = 0x75uy, Long

    let private (|IsFunc|_|) tag b=
        if b = fst tag then Some () else None

    let private ciOnlyCodes = [| fst aplSelect; fst command; fst devSelect |]
    let private shortCodes  = [| fst rspShort |]
    let private longCodes   = [| fst rspLong; fst alarm |]

    let private mem xs b = System.Array.Exists(xs, fun x -> x = b)

    let private (|CiOnlyKind|ShortKind|LongKind|UnknownCi|) (b: byte) =
        if mem ciOnlyCodes b then CiOnlyKind
        elif mem shortCodes b then ShortKind
        elif mem longCodes b then LongKind
        else UnknownCi b

    let parseCiOnly : P<TplCiOnlyFunc> = parser {
       let! ci = parseU8
       match ci with
       | IsFunc aplSelect -> return AplSelect
       | IsFunc command -> return Command
       | IsFunc devSelect -> return DevSelect
       | _ -> return! failBefore $"unexpected CI for TPL None: 0x{ci:X2}"
    }

    let private parseCiForShort : P<TplShortFunc> = parser {
        let! ci = parseU8
        match ci with
        | IsFunc rspShort -> return TplShortFunc.Rsp
        | _ -> return! failBefore $"unexpected CI for TPL Short: 0x{ci:X2}"
    }

    let parseShort : P<TplShort> = parser {
        let! ci = parseCiForShort
        let! acc = parseU8
        let! status = parseU8
        let! cnf = parseU16
        return { Func = ci; Acc = acc; Status = status; Cnf = cnf }
    }

    let private parseCiForLong : P<TplLongFunc> = parser {
        let! ci = parseU8
        match ci with
        | IsFunc rspLong -> return TplLongFunc.Rsp
        | IsFunc alarm -> return TplLongFunc.Alarm
        | _ -> return! failBefore $"unexpected CI for TPL Long: 0x{ci:X2}"
    }

    let parseLong : P<TplLong> = parser {
        let! ci = parseCiForLong
        let! ala = Address.parseAla
        let! acc = parseU8
        let! status = parseU8
        let! cnf = parseU16
        return { Func = ci; Ala = ala; Acc = acc; Status = status; Cnf = cnf }
    }

    let parse : P<Tpl> = parser {
       let! b = peekU8
       match b with
       | CiOnlyKind -> return! parseCiOnly |>> CiOnly
       | ShortKind -> return! parseShort |>> Short
       | LongKind -> return! parseLong |>> Long
       | UnknownCi b -> return! fail $"unexpected CI for TPL: 0x{b:X2}"
    }