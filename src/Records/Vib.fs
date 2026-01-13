namespace Mbus.Records

open System
open Mbus
open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Core
open Mbus.Records.VifDef

type NormalVib = { Def: VifDef; Ext: CombVifExt list; }
type Vib =
    | Invalid of ReadOnlyMemory<uint8>
    | Mfr of ReadOnlyMemory<uint8>
    | Normal of NormalVib
    | Text of string

module Vib =
    open System.Text

    let mask = 0x7Fuy
    let isExt b = b &&& 0x80uy = 0x80uy
    let firstExtension = 0xFBuy
    let textVif = 0x7Cuy
    let secondExtension = 0xFDuy
    let thirdExtension = 0xEFuy
    let anyVif = 0x7Euy
    let mfrVif = 0x7Fuy

    let (|IsSpecial|_|) expected b =
        if b &&& mask = expected then Some () else None

    let (|IsExt|_|) expected b =
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

    let private parseExt b : P<CombVifExt list> = parser {
        let seed = [ ]
        if InfoBlock.isExtended b then
            let! acc = InfoBlock.parseExt seed parseCombVifExtByte
            return acc |> List.rev
        else
            return seed
    }

    let private parseBytes acc _ b : uint8 array =
        Array.append acc [| b |]

    let private parseVibBytes (prefix : uint8 array) : P<ReadOnlyMemory<uint8>> = parser {
        if InfoBlock.isExtended prefix[prefix.Length - 1] then
            let! bytes = InfoBlock.parseExt prefix parseBytes
            return bytes |> ReadOnlyMemory<uint8>
        else
            return prefix |> ReadOnlyMemory<uint8>
    }

    let private parseTextVib : P<string> = parser {
        let! len = parseU8
        let! txtBytes = int len |> takeMem
        return txtBytes.ToArray()
               |> Array.rev
               |> Encoding.ASCII.GetString
    }

    let private parseFirstExt : P<Vib> = parser {
        let! code = parseU8
        match firstExt code with
        | Some def ->
            let! ext = parseExt code
            return { Def = def; Ext = ext } |> Normal
        | None -> return! parseVibBytes [| firstExtension; code |] |>> Invalid
    }

    let private parseSecondExt : P<Vib> = parser {
        let! code = parseU8
        match sndExt code with
        | Some def ->
            let! ext = parseExt code
            return { Def = def; Ext = ext } |> Normal
        | None -> return! parseVibBytes [| secondExtension; code |] |>> Invalid
    }

    let private parsePrim vif : P<Vib> = parser {
        match prim (vif &&& mask) with
        | Some def ->
            let! ext = parseExt vif
            return { Def = def; Ext = ext } |> Normal
        | None -> return! parseVibBytes [| vif |] |>> Invalid
    }

    let parse: P<Vib> = parser {
        let! vif = parseU8
        match vif with
        | IsSpecial mfrVif -> return! parseVibBytes [| vif |] |>> Mfr
        | IsSpecial textVif -> return! parseTextVib |>> Text
        | IsExt firstExtension -> return! parseFirstExt
        | IsExt secondExtension -> return! parseSecondExt
        | _ -> return! parsePrim vif
    }
