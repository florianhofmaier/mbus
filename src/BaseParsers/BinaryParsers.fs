module Mbus.BaseParsers.BinaryParsers

open Mbus.BaseParsers.Core
open System.Buffers.Binary

let parseU8 : Parser<uint8> = parseByte

let peekU8 : Parser<uint8> = peekByte

let parseI8 : Parser<int8> =
    parseU8 |>> int8

let expectU8 b msg : Parser<unit> =
    parser {
        let! v = parseU8
        if v = b then
            return ()
        else
            let! p = pos
            return! fun _ -> Error { Pos = p-1; Msg = $"{msg}: expect 0x{b:X2}, got 0x{v:X2}"; Ctx = [ ] }
    }

let parseU16: Parser<uint16> =
    parser {
        let! b = takeMem 2
        return BinaryPrimitives.ReadUInt16LittleEndian b.Span
    }

let parseI16: Parser<int16> =
    parseU16 |>> int16

let parseU24: Parser<uint32> =
    parser {
        let! b0 = parseU16
        let! b1 = parseU8
        return uint32 b0 ||| (uint32 b1 <<< 16)
    }

let parseI24: Parser<int32> =
    parseU24
    |>> (fun u ->
        if (u &&& (1u <<< 23)) <> 0u then
            int32 (u - (1u <<< 24))
        else
            int32 u
    )

let parseU32: Parser<uint32> =
    parser {
        let! b = takeMem 4
        return BinaryPrimitives.ReadUInt32LittleEndian b.Span
    }

let parseI32: Parser<int32> =
    parseU32 |>> int32

let parseU48: Parser<uint64> =
    parser {
        let! low = parseU32
        let! high = parseU16
        return uint64 low ||| (uint64 high <<< 32)
    }

let parseI48: Parser<int64> =
    parseU48
    |>> (fun u ->
        if (u &&& (1UL <<< 47)) <> 0UL then
            int64 (u - (1UL <<< 48))
        else
            int64 u
    )

let parseU64: Parser<uint64> =
    parser {
        let! b = takeMem 8
        return BinaryPrimitives.ReadUInt64LittleEndian b.Span
    }

let parseI64: Parser<int64> =
    parseU64 |>> int64
