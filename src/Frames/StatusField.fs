module Mbus.Frames.StatusField

open Mbus
open Mbus.BaseParsers.BinaryParsers
open Mbus.BaseParsers.Core
open Mbus.BaseWriters.BinaryWriters
open Mbus.BaseWriters.Core

let private maskApplicationError = 0x03uy
let private maskPowerLow = 0x04uy
let private maskPermanentError = 0x08uy
let private maskTemporaryError = 0x10uy

let getApplicationError statusField =
    statusField &&& maskApplicationError |> LanguagePrimitives.EnumOfValue

let hasPowerLow statusField =
    (statusField &&& maskPowerLow) <> 0uy

let hasPermanentError statusField =
    (statusField &&& maskPermanentError) <> 0uy

let hasTemporaryError statusField =
    (statusField &&& maskTemporaryError) <> 0uy

let parseStatusField : Parser<MbusStatusField> =
    parser {
        let! b = parseU8
        let appError = getApplicationError b
        let powerLow = hasPowerLow b
        let permError = hasPermanentError b
        let tempError = hasTemporaryError b
        return { ApplicationError = appError
                 PowerLow = powerLow
                 PermanentError = permError
                 TemporaryError = tempError }
    }

let shiftPowerLow = 2
let shiftPermanentError = 3
let shiftTemporaryError = 4

let writeStatusField (statusField: MbusStatusField) : Writer<unit> =
    writer {
        let mutable b = byte (LanguagePrimitives.EnumToValue statusField.ApplicationError)
        if statusField.PowerLow then
            b <- b ||| (maskPowerLow <<< shiftPowerLow)
        if statusField.PermanentError then
            b <- b ||| (maskPermanentError <<< shiftPermanentError)
        if statusField.TemporaryError then
            b <- b ||| (maskTemporaryError <<< shiftTemporaryError)
        do! writeU8 b
    }