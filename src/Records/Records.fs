namespace Mbus.Records

open System
open Mbus
open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Core
open Mbus.Records.DataInfoBlocks

type DataRecord = {
    Value: MbusValue
    StNum: StorageNumber
    Fn: MbusFunctionField
    Tariff: Tariff
    SubUnit: SubUnit
    Vib: Vib
}

type SpecialFunction =
    | IdleFiller
    | MfrData of ReadOnlyMemory<byte>
    | MfrDataMoreFollows of ReadOnlyMemory<byte>

type Record =
    | DataRecord of DataRecord
    | SpecialFunction of SpecialFunction

module Record =
    let maskSpecFn = 0x70uy
    let shiftSpecFn = 4
    let mfrData = 0x00uy
    let mfrDataMoreFollows = 0x01uy
    let idleFiller = 0x02uy
    let globReadReq = 0x07uy
    let isSpecFn b = (b &&& 0x0Fuy) = 0x0Fuy

    let takeMem (n:int) : P<ReadOnlyMemory<uint8>> =
        fun st ->
            if st.Off + n > st.Buf.Length then
                errBufOverflow st
            else
                let mem = st.Buf.Slice(st.Off, n)
                ok mem { st with Off = st.Off + n }

    let (|IsSpecFn|_|) expected b =
        if (b &&& maskSpecFn) >>> shiftSpecFn = expected then Some () else None

    let parseSpecFn b : P<SpecialFunction> = parser {
        match b with
        | IsSpecFn idleFiller -> return IdleFiller
        | IsSpecFn mfrData ->
            let! data = takeAllMem
            return MfrData data
        | IsSpecFn mfrDataMoreFollows ->
            let! data = takeAllMem
            return MfrDataMoreFollows data
        | IsSpecFn globReadReq -> return! failBefore "global readout request not supported"
        | _ -> return! failBefore $"invalid special function code: 0x{b:X2}"
    }

    let parseDataRec dif : P<DataRecord> = parser {
        let! fn, stNum, tariff, subUnit = DibParser.parse dif
        let! vib = Vib.parse
        let! value = MbusValue.parse dif
        return { Fn = fn; StNum = stNum; Tariff = tariff; SubUnit = subUnit; Vib = vib; Value = value; }
    }

    let parseRecord: P<Record> = parser {
        let! dif = parseU8
        if isSpecFn dif then
         let! record = parseSpecFn dif |> withCtx "special function record"
         return SpecialFunction record
        else
         let! record = parseDataRec dif |> withCtx "data record"
         return DataRecord record
    }
