namespace Mbus

open System
open Mbus.BaseParsers.Core
open Mbus.Frames
open Mbus.Frames.Layers
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
        | Record.DataRecord dr -> handleDataRecord dr
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
        let acd = frame.CField &&& 0x40uy <> 0uy
        let dfc = frame.CField &&& 0x20uy <> 0uy
        let sndAdr = tpl.Ala
        let numData, txtData, mfrData, moreFollows = getRecords apl
        prmAdr, acd, dfc, sndAdr, numData, txtData, mfrData.ToArray(), moreFollows

    let parse =
        parser {
            let! frame = LongFrame.parse
            match frame.Tpl with
            | Tpl.Long tpl ->
                match tpl.Func with
                | TplLongFunc.Rsp ->
                    match frame.Apl with
                    | Data apl -> return createMsg frame tpl apl
                    | _ -> return! fail "Expected APL with data records"
                | _ -> return! fail "Expected Ci field: 0x72"
            | _ -> return! fail "Expected long TPL"
        }

type RspUd =
    { PrimaryAddress: int
      Acd: bool
      Afc: bool
      SecondaryAddress: MbusAddress
      NumericalRecords: System.Collections.Generic.IReadOnlyList<MbusNumericalRecord>
      TextRecords: System.Collections.Generic.IReadOnlyList<MbusTextRecord>
      MfrData: System.Collections.Generic.IReadOnlyCollection<byte>
      MoreDataFollows: bool }

    with
        static member FromBytes(buf: ReadOnlyMemory<byte>) : RspUd * int =
            let st = { Off = 0; Buf = buf }
            match RspUdParser.parse st with
            | Ok ((prmAdr, acd, dfc, sndAdr, numRecords, txtRecords, mfrData, moreFollows), st') ->
                let response =
                    { PrimaryAddress = prmAdr
                      Acd = acd
                      Afc = dfc
                      SecondaryAddress = sndAdr
                      NumericalRecords = numRecords
                      TextRecords = txtRecords
                      MfrData = mfrData
                      MoreDataFollows = moreFollows }
                (response, st'.Off)
            | Error e -> raise (MbusParserError.create e)

