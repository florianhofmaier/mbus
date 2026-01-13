module Mbus.Messages.RecordsBuilders

open Mbus
open Mbus.BaseWriters
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


    let withScaler toVifDef scaler value =
        {
            Value = value
            StNum = StorageNumber.zero
            Fn = MbusFunctionField.InstValue
            Tariff = Tariff.zero
            SubUnit = SubUnit.zero
            Vib = { Def = scaler |> toVifDef; Ext = [] } |> Vib.Normal
        }

module EnergyRecords =

    open Values

    type EnergyRecord =
        | WattHours of WattHoursScaler
        | Joules

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

    let withWattHoursScaler scaler value =
        withScaler WattHoursScaler.toVifDef scaler value


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

let private writeVib (vib: Vib) : Writer =
    match vib with
    | Vib.Normal normalVib -> VibWriter.writeNormalVib normalVib
    | _ -> fun st -> err st $"Unsupported Vib type for writing: {vib}"

let build (record: DataRecord) : Writer =
    fun st0 ->
        DibWriter.writeDib record.Fn record.StNum record.Tariff record.SubUnit record.Value st0
        |> Result.bind (writeVib record.Vib)
        |> Result.bind (MbusValue.write record.Value)

let testBuild =
    42
    |> Values.withInt32
    |> Result.map (EnergyRecords.withWattHoursScaler WattHoursScaler.Exp0)
    |> Result.bind (withStNum 1)
    |> Result.map build
