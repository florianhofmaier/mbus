namespace Mbus

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open Mbus.BaseParsers.Core
open Mbus.Frames
open Mbus.Records
open Mbus.Messages.Converters
open Mbus.Records.DataInfoBlocks

module RspUdParser =

    open System.Collections.Generic

    let getBaseFields dr =
        let unit = MbusUnit.fromRecord dr
        let fn = dr.Fn
        let storageNum = StorageNumber.value dr.StNum
        let tariff = dr.Tariff |> Tariff.value
        let subUnit = dr.SubUnit |> SubUnit.value
        unit, fn, storageNum, tariff, subUnit

    type RecordType =
        | Num of MbusNumericalRecord
        | Txt of MbusTextRecord
        | Mfr of ReadOnlyMemory<byte> * bool
        | Filler

    let vibToTxt vib =
        match vib with
        | Text txt -> txt
        | Normal normalVib -> normalVib.Def.Val |> MbusValueType.toString
        | Vib.Mfr mfrVib -> mfrVib.ToArray() |> BitConverter.ToString |> sprintf "Manufacturer specific VIB: %s"
        | Invalid inv -> inv.ToArray() |> BitConverter.ToString |> sprintf "Invalid VIB: %s"

    let failIfNotNormalVib vib =
        match vib with
        | Normal normalVib -> normalVib
        | _ -> failwith "Expected normal VIB for numerical record"

    let getValueType vib =
        vib.Def.Val

    let createTxtRec dr =
        let unit, fn, storageNum, tariff, subUnit = getBaseFields dr
        let valueType = vibToTxt dr.Vib
        let value = MbusValue.getTextValue dr
        Txt (MbusTextRecord(unit, fn, storageNum, tariff, subUnit, value, valueType))

    let createNumRec dr =
        let unit, fn, storageNum, tariff, subUnit = getBaseFields dr
        let vib = dr.Vib |> failIfNotNormalVib |> getValueType
        let value = MbusValue.getNumericValue dr
        Num (MbusNumericalRecord(unit, fn, storageNum, tariff, subUnit, value, vib))

    let handleDataRecord dr =
        match dr.Value, dr.Vib with
        | VarLen _, _ | _, Text _ | _, Vib.Mfr _ | _, Invalid _ -> createTxtRec dr
        | _ -> createNumRec dr
        |> Some

    let handleSpecialFunction sf =
        match sf with
        | SpecialFunction.MfrData data ->
            Mfr (data, false) |> Some
        | SpecialFunction.MfrDataMoreFollows data ->
            Mfr (data, true) |> Some
        | _ -> None

    let toRecordType r =
        match r with
        | Record.Data dr -> handleDataRecord dr
        | Record.SpecialFunction sf -> handleSpecialFunction sf

    let getRecords recList : IReadOnlyList<MbusNumericalRecord> * IReadOnlyList<MbusTextRecord> * ReadOnlyMemory<byte> * bool =
        let numRecs, txtRecs, mfrData, moreFollows =
            recList
            |> List.map toRecordType
            |> List.choose id
            |> List.fold (fun (nums, txts, mfr, more) record ->
                match record with
                | Num n -> (n :: nums, txts, mfr, more)
                | Txt t -> (nums, t :: txts, mfr, more)
                | Mfr (data, moreData) -> (nums, txts, data, moreData)
                | Filler -> (nums, txts, mfr, more)
            ) ([], [], ReadOnlyMemory.Empty, false)

        numRecs |> List.rev :> IReadOnlyList<MbusNumericalRecord>,
        txtRecs |> List.rev :> IReadOnlyList<MbusTextRecord>,
        mfrData,
        moreFollows

    let createMsg frame tpl apl =
        let prmAdr = int frame.PrmAdr
        let sndAdr = tpl.Ala
        let status = tpl.Status
        let numData, txtData, mfrData, moreFollows = getRecords apl
        prmAdr, sndAdr, status, numData, txtData, mfrData.ToArray(), moreFollows

    let parse =
        parser {
            let! frame = FrameParser.parseLongFrame
            match frame.Tpl with
            | Tpl.Long tpl ->
                match tpl.Func with
                | TplLongFunc.Rsp ->
                    match frame.Apl with
                    | UserData apl -> return createMsg frame tpl apl
                    | _ -> return! fail "Expected APL with data records"
                | _ -> return! fail "Expected Ci field: 0x72"
            | _ -> return! fail "Expected long TPL"
        }

type RspUd =
    { PrimaryAddress: int
      SecondaryAddress: MbusAddress
      StatusField: MbusStatusField
      NumericalRecords: System.Collections.Generic.IReadOnlyList<MbusNumericalRecord>
      TextRecords: System.Collections.Generic.IReadOnlyList<MbusTextRecord>
      MfrData: System.Collections.Generic.IReadOnlyCollection<byte> }

    with
        static member FromBytes(buf: ReadOnlyMemory<byte>) : RspUd * int =
            let st = { Off = 0; Buf = buf }
            match RspUdParser.parse st with
            | Ok ((prmAdr, sndAdr, status, numRecords, txtRecords, mfrData, moreFollows), st') ->
                let response =
                    { PrimaryAddress = prmAdr
                      SecondaryAddress = sndAdr
                      StatusField = status
                      NumericalRecords = numRecords
                      TextRecords = txtRecords
                      MfrData = mfrData }
                (response, st'.Off)
            | Error e -> raise (MbusParserError.create e)

        static member FromStreamAsync(stream: Stream, ct: CancellationToken) : Task<RspUd> =

            let readByteOrEosAsync () =
                task {
                    let buffer = Array.zeroCreate<byte> 1
                    let! n = stream.ReadAsync(buffer.AsMemory(0, 1), ct)
                    if n = 0 then raise (EndOfStreamException())
                    return buffer[0]
                }

            let readExactlyAsync (buffer: byte[]) offset count =
                task {
                    let mutable offset = offset
                    let mutable count = count
                    while count > 0 do
                        let! n = stream.ReadAsync(buffer.AsMemory(offset, count), ct)
                        if n = 0 then raise (EndOfStreamException())
                        offset <- offset + n
                        count <- count - n
                }

            task {
                let! start = readByteOrEosAsync ()
                if start <> Frame.longFrameStartByte then
                    return raise (InvalidDataException("Expected long frame start byte"))

                let! len1 = readByteOrEosAsync ()
                let! len2 = readByteOrEosAsync ()
                let! start2 = readByteOrEosAsync ()

                if len1 <> len2 || start2 <> Frame.longFrameStartByte then
                    return raise (InvalidDataException("Invalid long frame header"))

                let totalLen = int len1 + 6
                let buf = Array.zeroCreate<byte> totalLen
                buf[0] <- start
                buf[1] <- len1
                buf[2] <- len2
                buf[3] <- start2

                do! readExactlyAsync buf 4 (totalLen - 4)

                let response, _ = RspUd.FromBytes(ReadOnlyMemory buf)
                return response
            }
