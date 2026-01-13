module Mbus.Records.InfoBlock

open Mbus
open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Core
open Mbus.BaseWriters

let isExtended b = (b &&& 0x80uy) <> 0uy
let maxExtBytes = 10

let parseExt seed f : P<'a> =
    let rec loop i acc = parser {
        if i >= maxExtBytes then return! fail "too many DIB/VIB bytes (limit 11)"
        let! b = parseU8
        let acc' = f acc i b
        if isExtended b then return! loop (i + 1) acc'
        else return acc'
    }
    loop 0 seed

let writeExt (seed: 'a) (f: 'a -> int -> byte) : Writer =
    let rec loop (i: int) (st: WState) : Result<WState, WError> =
        if i >= maxExtBytes then
            BaseWriters.err st "too many DIB/VIB bytes (limit 11)"
        else
            let b = f seed i
            writeU8 b st
            |> Result.bind (fun st' ->
                if isExtended b then loop (i + 1) st'
                else Ok st')
    fun st0 -> loop 0 st0
