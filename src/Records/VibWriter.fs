namespace Mbus.Records

open System
open Mbus.BaseWriters
open Mbus.Records.VifDef

module VibWriter =

    let private mask = 0x7Fuy
    let private setExt (b: byte) = b ||| 0x80uy
    let private clearExt (b: byte) = b &&& mask

    let private firstExtension = 0xFBuy
    let private textVif = 0x7Cuy
    let private secondExtension = 0xFDuy

    let private writeRaw (mem: ReadOnlyMemory<byte>) : Writer =
        fun st0 ->
            let span = mem.Span
            let mutable res = Ok st0
            let mutable i = 0
            while i < span.Length && res.IsOk do
                match res with
                | Error e -> res <- Error e
                | Ok st ->
                    res <- writeU8 span[i] st
                    i <- i + 1
            res

    let private findKeyByValue (table: System.Collections.Generic.IDictionary<byte, VifDef>) (target: VifDef) : byte option =
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

    let private writeVifBytes (enc: VifEncoding) : Writer =
        match enc with
        | Prim vif ->
            writeU8 (clearExt vif)
        | FirstExt code ->
            fun st0 ->
                writeU8 firstExtension st0
                |> Result.bind (writeU8 (clearExt code))
        | SecondExt code ->
            fun st0 ->
                writeU8 secondExtension st0
                |> Result.bind (writeU8 (clearExt code))

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

    let writeNormalVib (vib: NormalVib) : Writer =
        fun st0 ->
            match encodeVif vib.Def with
            | None -> err st0 "VibWriter: cannot serialize NormalVib.Def (no matching VIF code in tables)"
            | Some enc ->
                match writeVifBytes enc st0 with
                | Error e -> Error e
                | Ok st1 ->
                    // Write VIFE chain (if any)
                    if vib.Ext.IsEmpty then Ok st1
                    else InfoBlock.writeExt vib.Ext writeExtByte st1
