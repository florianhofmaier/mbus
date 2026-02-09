module Mbus.Messages.RecordsBuilders

open Mbus
open Mbus.BaseWriters.Core
open Mbus.Records
open Mbus.Records.DataInfoBlocks

module Values =

    let private guardedValue value min max typeName =
        if value >= min && value <= max then
            Ok value
        else
            Error $"Value {value} is out of {typeName} range [{min}..{max}]"

    let withInt32 n =
        Int32 n |> Ok

    let withBcd8Digit n =
        guardedValue n 0 99999999 "Bcd8Digit"
        |> Result.map (fun v -> v |> uint32 |> Bcd8Digit)


    let withScaler toVifDef scaler value =
        {
            Value = value
            StNum = StorageNumber.zero
            Fn = MbusFunctionField.InstValue
            Tariff = Tariff.zero
            SubUnit = SubUnit.zero
            Vib = { Def = scaler |> toVifDef; Ext = [] } |> Vib.Normal
        }

    let withNoScaler vifDef value =
        {
            Value = value
            StNum = StorageNumber.zero
            Fn = MbusFunctionField.InstValue
            Tariff = Tariff.zero
            SubUnit = SubUnit.zero
            Vib = { Def = vifDef; Ext = [] } |> Vib.Normal
        }

module EnergyRecords =

    open Values

    module WattHoursScaler =
        let toVifDef = function
            | WattHoursScaler.ExpMinus3 -> VifDef.primTable[0x00uy]
            | WattHoursScaler.ExpMinus2 -> VifDef.primTable[0x01uy]
            | WattHoursScaler.ExpMinus1 -> VifDef.primTable[0x02uy]
            | WattHoursScaler.Exp0 -> VifDef.primTable[0x03uy]
            | WattHoursScaler.Exp1 -> VifDef.primTable[0x04uy]
            | WattHoursScaler.Exp2 -> VifDef.primTable[0x05uy]
            | WattHoursScaler.Exp3 -> VifDef.primTable[0x06uy]
            | WattHoursScaler.Exp4 -> VifDef.primTable[0x07uy]
            | WattHoursScaler.Exp5 -> VifDef.firstExtTable[0x00uy]
            | WattHoursScaler.Exp6 -> VifDef.firstExtTable[0x01uy]
            | _ -> failwith "Invalid WattHoursScaler"

    let asWattHours scaler value =
        withScaler WattHoursScaler.toVifDef scaler value

module VolumeRecords =

    open Values

    module CubicMetersScaler =
        let toVifDef = function
            | CubicMetersScaler.ExpMinus6 -> VifDef.primTable[0x10uy]
            | CubicMetersScaler.ExpMinus5 -> VifDef.primTable[0x11uy]
            | CubicMetersScaler.ExpMinus4 -> VifDef.primTable[0x12uy]
            | CubicMetersScaler.ExpMinus3 -> VifDef.primTable[0x13uy]
            | CubicMetersScaler.ExpMinus2 -> VifDef.primTable[0x14uy]
            | CubicMetersScaler.ExpMinus1 -> VifDef.primTable[0x15uy]
            | CubicMetersScaler.Exp0 -> VifDef.primTable[0x16uy]
            | CubicMetersScaler.Exp1 -> VifDef.primTable[0x17uy]
            | CubicMetersScaler.Exp2 -> VifDef.firstExtTable[0x10uy]
            | CubicMetersScaler.Exp3 -> VifDef.firstExtTable[0x11uy]
            | _ -> failwith "Invalid CubicMetersScaler"

    let asCubicMeters scaler value =
        withScaler CubicMetersScaler.toVifDef scaler value

module FabricationNumbers =

    open Values

    let asFabNum value =
        withNoScaler VifDef.primTable[0x78uy] value

let withFunction fn record =
    Ok { record with Fn = fn }

let withStNum n record =
    StorageNumber.create n
    |> Result.map (fun stNum -> { record with StNum = stNum })

let withTariff tariff record =
    Tariff.create tariff
    |> Result.map (fun t -> { record with Tariff = t })

let withSubUnit subUnit record =
    SubUnit.create subUnit
    |> Result.map (fun su -> { record with SubUnit = su })

let withVibExtension ext record =
    match record.Vib with
    | Vib.Normal vib -> Ok { record with Vib = Vib.Normal { vib with Ext = vib.Ext @ [ ext ] } }
    | _ -> Error "Cannot add Vib extension to non-normal Vib"

let build (record: DataRecord) : Writer<unit> =
    Record.Writer.write record

let testBuild =
    42
    |> Values.withInt32
    |> Result.map (EnergyRecords.asWattHours WattHoursScaler.Exp0)
    |> Result.bind (withStNum 1)
    |> Result.map build
