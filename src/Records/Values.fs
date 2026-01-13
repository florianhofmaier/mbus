namespace Mbus.Records

open System
open System.Buffers.Binary
open System.Text
open Mbus.BaseParsers.Core
open Mbus.BaseParsers.BcdParsers
open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseWriters

type MbusValue =
    | NoData
    | Int8 of int8
    | Int16 of int16
    | Int24 of int32
    | Int32 of int32
    | Int48 of int64
    | Int64 of int64
    | Real32 of float32
    | Bcd2Digit of uint8
    | Bcd4Digit of uint16
    | Bcd6Digit of uint32
    | Bcd8Digit of uint32
    | Bcd12Digit of uint64
    | VarLen of string

module MbusValue =
    let mask = 0x0Fuy
    let noData = 0x00uy
    let int8 = 0x01uy
    let int16 = 0x02uy
    let int24 = 0x03uy
    let int32 = 0x04uy
    let real32 = 0x05uy
    let int48 = 0x06uy
    let int64 = 0x07uy
    let select = 0x08uy
    let bcd2Digit = 0x09uy
    let bcd4Digit = 0x0Auy
    let bcd6Digit = 0x0Buy
    let bcd8Digit = 0x0Cuy
    let varLen = 0x0Duy
    let bcd12Digit = 0x0Euy

    let (|IsDataType|_|) expected b =
        if b &&& mask = expected then Some () else None

    let parseLvar : P<int> = parser {
        let! l = parseU8
        if l < 0xC0uy then return int l
        else return! fail $"unsupported LVAR: 0x{l:X2}"
    }

    let parseVarLen : P<string> = parser {
        let! n = parseLvar
        let! bytes = takeMem n
        let arr = bytes.ToArray()
        Array.Reverse(arr)
        return Encoding.UTF8.GetString(arr)
    }

    let parseFloat32 : P<float32> = parser {
        let! b = takeMem 4
        return BinaryPrimitives.ReadSingleLittleEndian b.Span
    }

    let parse b : P<MbusValue> = parser {
        match b with
        | IsDataType noData -> return NoData
        | IsDataType int8 -> return! parseI8 |>> Int8
        | IsDataType int16 -> return! parseI16 |>> Int16
        | IsDataType int24 -> return! parseI24 |>> Int24
        | IsDataType int32 -> return! parseI32 |>> Int32
        | IsDataType int48 -> return! parseI48 |>> Int48
        | IsDataType int64 -> return! parseI64 |>> Int64
        | IsDataType real32 -> return! parseFloat32 |>> Real32
        | IsDataType bcd2Digit -> return! parseBcd2Digit |>> Bcd2Digit
        | IsDataType bcd4Digit -> return! parseBcd4Digit |>> Bcd4Digit
        | IsDataType bcd6Digit -> return! parseBcd6Digit |>> Bcd6Digit
        | IsDataType bcd8Digit -> return! parseBcd8Digit |>> Bcd8Digit
        | IsDataType bcd12Digit -> return! parseBcd12Digit |>> Bcd12Digit
        | IsDataType varLen -> return! parseVarLen |>> VarLen
        | IsDataType select -> return! fail "data field 'Selection for readout' not supported"
        | _ -> return invalidOp $"invalid data field: 0x{b:X2}"
    }

    let toDif v =
        match v with
        | NoData -> noData
        | Int8 _ -> int8
        | Int16 _ -> int16
        | Int24 _ -> int24
        | Int32 _ -> int32
        | Int48 _ -> int48
        | Int64 _ -> int64
        | Real32 _ -> real32
        | Bcd2Digit _ -> bcd2Digit
        | Bcd4Digit _ -> bcd4Digit
        | Bcd6Digit _ -> bcd6Digit
        | Bcd8Digit _ -> bcd8Digit
        | Bcd12Digit _ -> bcd12Digit
        | VarLen _ -> varLen

    let writeReal32 (x: float32) : Writer =
        fun st0 ->
            let bytes = BitConverter.GetBytes(x)
            if not BitConverter.IsLittleEndian then Array.Reverse(bytes)
            let mutable res = Ok st0
            for b in bytes do
                res <- res |> Result.bind (writeU8 b)
            res

    let writeVarLen (txt: string) : Writer =
        fun st0 ->
            let raw = Encoding.UTF8.GetBytes(txt)
            if raw.Length > 0xBF then
                err st0 $"VarLen too long for supported LVAR (len={raw.Length})"
            else
                let len = byte raw.Length
                let rev = Array.rev raw
                writeU8 len st0
                |> Result.bind (fun st ->
                    let mutable res = Ok st
                    for b in rev do
                        res <- res |> Result.bind (writeU8 b)
                    res)

    let write (v: MbusValue) : Writer =
        fun st0 ->
            match v with
            | NoData -> Ok st0
            | Int8 x -> writeI8 x st0
            | Int16 x -> writeI16 x st0
            | Int24 x -> writeI24 x st0
            | Int32 x -> writeI32 x st0
            | Int48 x -> writeI48 x st0
            | Int64 x -> writeI64 x st0
            | Real32 x -> writeReal32 x st0
            | Bcd2Digit x -> writeBcdU8 x st0
            | Bcd4Digit x -> writeBcdU16 x st0
            | Bcd6Digit x -> writeBcdU24 x st0
            | Bcd8Digit x -> writeBcdU32 x st0
            | Bcd12Digit x -> writeBcdU48 x st0
            | VarLen txt -> writeVarLen txt st0
