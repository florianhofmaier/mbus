namespace Mbus.Records

open System
open Mbus
open Mbus.Records.VifDef

type NormalVib = { Def: VifDef; Ext: CombVifExt list; }
type Vib =
    | Invalid of ReadOnlyMemory<uint8>
    | Mfr of ReadOnlyMemory<uint8>
    | Normal of NormalVib
    | Text of string

module Vib =
    let mask = 0x7Fuy
    let private firstExtension = 0xFBuy
    let private secondExtension = 0xFDuy
    let private textVif = 0x7Cuy

    module Parser =
        open Mbus.BaseParsers.BinaryParsers
        open Mbus.BaseParsers.Core
        open System.Text

        let private isExt b = b &&& 0x80uy = 0x80uy
        let private thirdExtension = 0xEFuy
        let private anyVif = 0x7Euy
        let private mfrVif = 0x7Fuy

        let private (|IsSpecial|_|) expected b =
            if b &&& mask = expected then Some () else None

        let private (|IsExt|_|) expected b =
            if b = expected then Some () else None

        let private prepend (prefix: uint8[]) (tail: ReadOnlyMemory<uint8>) =
            let buf = Array.zeroCreate<uint8> (prefix.Length + tail.Length)
            prefix.CopyTo(buf.AsMemory(0))
            tail.CopyTo(buf.AsMemory(prefix.Length))
            buf.AsMemory()

        let private parseCombVifExtByte acc _ b : CombVifExt list =
            match acc with
            | ValidCombVifExt MbusValueTypeExtension.ManufacturerSpecific :: _
            | MfrSpecCombVifExt _ :: _ -> MfrSpecCombVifExt b :: acc
            | _ ->
                match combVifExt b with
                | Some def -> ValidCombVifExt def :: acc
                | None -> InvalidCombVifExt b :: acc

        let private parseExt b : Parser<CombVifExt list> = parser {
            let seed = [ ]
            if InfoBlock.isExtended b then
                let! acc = InfoBlock.parseExt seed parseCombVifExtByte
                return acc |> List.rev
            else
                return seed
        }

        let private parseBytes acc _ b : uint8 array =
            Array.append acc [| b |]

        let private parseVibBytes (prefix : uint8 array) : Parser<ReadOnlyMemory<uint8>> = parser {
            if InfoBlock.isExtended prefix[prefix.Length - 1] then
                let! bytes = InfoBlock.parseExt prefix parseBytes
                return bytes |> ReadOnlyMemory<uint8>
            else
                return prefix |> ReadOnlyMemory<uint8>
        }

        let private parseTextVib : Parser<string> = parser {
            let! len = parseU8
            let! txtBytes = int len |> takeMem
            return txtBytes.ToArray()
                   |> Array.rev
                   |> Encoding.ASCII.GetString
        }

        let private parseFirstExt : Parser<Vib> = parser {
            let! code = parseU8
            match firstExt code with
            | Some def ->
                let! ext = parseExt code
                return { Def = def; Ext = ext } |> Normal
            | None -> return! parseVibBytes [| firstExtension; code |] |>> Invalid
        }

        let private parseSecondExt : Parser<Vib> = parser {
            let! code = parseU8
            match sndExt code with
            | Some def ->
                let! ext = parseExt code
                return { Def = def; Ext = ext } |> Normal
            | None -> return! parseVibBytes [| secondExtension; code |] |>> Invalid
        }

        let private parsePrim vif : Parser<Vib> = parser {
            match prim (vif &&& mask) with
            | Some def ->
                let! ext = parseExt vif
                return { Def = def; Ext = ext } |> Normal
            | None -> return! parseVibBytes [| vif |] |>> Invalid
        }

        let parse: Parser<Vib> = parser {
            let! vif = parseU8
            match vif with
            | IsSpecial mfrVif -> return! parseVibBytes [| vif |] |>> Mfr
            | IsSpecial textVif -> return! parseTextVib |>> Text
            | IsExt firstExtension -> return! parseFirstExt
            | IsExt secondExtension -> return! parseSecondExt
            | _ -> return! parsePrim vif
        }

    module Writer =
        open Mbus.BaseWriters.Core
        open Mbus.BaseWriters.BinaryWriters
        open System.Collections.Generic

        let private mask = 0x7Fuy
        let private setExt (b: byte) = b ||| 0x80uy
        let private clearExt (b: byte) = b &&& mask

        let private writeRaw (mem: ReadOnlyMemory<byte>) : Writer<unit> =
            fun st0 ->
                let span = mem.Span
                let mutable res = Ok ((), st0)
                let mutable i = 0
                while i < span.Length && (match res with Ok _ -> true | _ -> false) do
                    match res with
                    | Error e -> res <- Error e
                    | Ok (_, st) ->
                        res <- writeU8 span[i] st
                        i <- i + 1
                res

        let private findKeyByValue (table: IDictionary<byte, VifDef>) (target: VifDef) : byte option =
            table
            |> Seq.tryFind (fun kv -> kv.Value = target)
            |> Option.map (fun kv -> kv.Key)

        type private VifEncoding =
            | Prim of byte
            | FirstExt of byte
            | SecondExt of byte

        let private encodeVif (def: VifDef) : VifEncoding option =
            match findKeyByValue primTable def with
            | Some k -> Some (Prim k)
            | None ->
                match findKeyByValue firstExtTable def with
                | Some k -> Some (FirstExt k)
                | None ->
                    match findKeyByValue secExtTable def with
                    | Some k -> Some (SecondExt k)
                    | None -> None

        let private writeVifBytes (enc: VifEncoding) : Writer<unit> =
            match enc with
            | Prim vif ->
                writeU8 (clearExt vif)
            | FirstExt code ->
                 writer {
                    do! writeU8 firstExtension
                    do! writeU8 (clearExt code)
                 }
            | SecondExt code ->
                 writer {
                    do! writeU8 secondExtension
                    do! writeU8 (clearExt code)
                 }

        let private writeExtByte (exts: CombVifExt list) (i: int) : byte =
            let count = exts.Length
            if i >= count then 0uy
            else
                let raw =
                    match exts[i] with
                    | ValidCombVifExt code -> LanguagePrimitives.EnumToValue code
                    | MfrSpecCombVifExt b -> b
                    | InvalidCombVifExt b -> b

                if i < count - 1 then setExt (clearExt raw) else clearExt raw

        let writeNormalVib (vib: NormalVib) : Writer<unit> =
            fun st0 ->
                match encodeVif vib.Def with
                | None -> err st0 "VibWriter: cannot serialize NormalVib.Def (no matching VIF code in tables)"
                | Some enc ->
                    match writeVifBytes enc st0 with
                    | Error e -> Error e
                    | Ok ((), st1) ->
                        // Write VIFE chain (if any)
                        if vib.Ext.IsEmpty then Ok ((), st1)
                        else InfoBlock.writeExt vib.Ext writeExtByte st1