namespace Mbus.Frames

open System
open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Core
open Mbus.Frames.Layers
open Mbus.Records

type ShortFrame = { CField: uint8; PrmAdr: uint8 }
type LongFrame = { CField: uint8; PrmAdr: uint8; Tpl: Tpl; Apl: Apl }
type Frame =
    | Cnf
    | Short of ShortFrame
    | Long of LongFrame

module Cnf =
    let parse : P<Frame> =
        (parser {
            do! expectU8 0xE5uy "invalid CNF frame"
            return Cnf
        }) |> withCtx "single character frame"

module ShortFrame =
    let parse : P<ShortFrame> =
        (parser {
            do! expectU8 0x10uy "invalid start byte"
            let! cField = parseU8
            let! prmAdr = parseU8
            do! expectU8 (cField + prmAdr) "invalid checksum"
            do! expectU8 0x16uy "invalid stop byte"
            return { CField = cField; PrmAdr = prmAdr }
        }) |> withCtx "short frame"

module LongFrame =
    let private parseLen: P<int> = parser {
        let! l1 = parseU8
        do! expectU8 l1 "length byte mismatch"
        return int l1
    }

    let private parseRecords aplLen : P<Record list> =
        (parser {
            let rec loop acc remaining =
                parser {
                    if remaining = 0 then
                        return List.rev acc
                    else
                        let! startPos = pos
                        let! record = Record.parseRecord
                        let! endPos = pos
                        let consumed = endPos - startPos
                        return! loop (record :: acc) (remaining - consumed)
                }
            return! loop [ ] aplLen
        }) |> (withCtx "parsing APL records")

    let private mbusChecksum (mem: ReadOnlyMemory<byte>) =
        let span = mem.Span
        let mutable s = 0
        for i = 0 to span.Length - 1 do
            s <- (s + int span[i]) &&& 0xFF
        byte s

    let checkCrc start l : P<unit> = parser {
        let! buf = getBuffer
        let expectedCrc = buf.Slice(start, l) |> mbusChecksum
        do! expectU8 expectedCrc "invalid checksum"
    }

    let private parseHeader: P<int * int> = parser {
        let parseStartByte = expectU8 0x68uy "invalid start byte"
        do! parseStartByte
        let! len = parseLen
        do! parseStartByte
        let! pos = pos
        return len, pos
    }

    let private parseApl tpl lenUd cField : P<Apl> = parser {
        let aplLen = lenUd - Tpl.getLen tpl - 2 // minus CField and PrmAdr
        if aplLen < 0 then
            return! fail $"invalid APL length: {aplLen}"
        else
            return! Apl.parse aplLen cField
    }

    let parse : P<LongFrame> =
        (parser {
            let! lenUd, startUd = parseHeader
            let! cField = parseU8
            let! prmAdr = parseU8
            let! tpl = Tpl.parse
            let! apl = parseApl tpl lenUd cField
            do! checkCrc startUd lenUd
            do! expectU8 0x16uy "invalid stop byte"
            return { CField = cField; PrmAdr = prmAdr; Tpl = tpl; Apl = apl }
        }) |> (withCtx "long frame")

module Frame =
    let parse : P<Frame> =
        parser {
            let! start = peekU8
            match start with
            | 0xE5uy -> return! Cnf.parse
            | 0x10uy -> return! ShortFrame.parse |>> Short
            | 0x68uy -> return! LongFrame.parse |>> Long
            | _ -> return! fail $"invalid start byte: 0x{start:X2}"
        }
