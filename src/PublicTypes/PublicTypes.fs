namespace Mbus

open System

type MbusError(msg: string) =
    inherit Exception(msg)

type MbusDeviceType =
    | Other = 0x00
    | OilMeter = 0x01
    | ElectricityMeter = 0x02
    | GasMeter = 0x03
    | HeatMeter = 0x04
    | SteamMeter = 0x05
    | WarmWaterMeter = 0x06
    | WaterMeter = 0x07
    | HeatCostAllocator = 0x08
    | CompressedAir = 0x09
    | CoolingMeterReturn = 0x0A
    | CoolingMeterFlow = 0x0B
    | HeatMeterFlow = 0x0C
    | CombinedHeatCoolingMeter = 0x0D
    | BusSystemComponent = 0x0E
    | Unknown = 0x0F
    | CalorificValue = 0x14
    | HotWaterMeter = 0x15
    | ColdWaterMeter = 0x16
    | DualRegisterWaterMeter = 0x17
    | PressureMeter = 0x18
    | ADConverter = 0x19
    | SmokeDetector = 0x1A
    | RoomSensor = 0x1B
    | GasDetector = 0x1C
    | Breaker = 0x20
    | Valve = 0x21
    | WasteWaterMeter = 0x28
    | Garbage = 0x29
    | CommunicationController = 0x31
    | UnidirectionalRepeater = 0x32
    | BidirectionalRepeater = 0x33

type MbusAddress private (idNumber: int, mfr: string, version: int, deviceType: MbusDeviceType) =
    member this.IdNumber = idNumber
    member this.Mfr = mfr
    member this.Version = version
    member this.DeviceType = deviceType

    override this.Equals(obj) =
        match obj with
        | :? MbusAddress as other ->
            idNumber = other.IdNumber &&
            mfr = other.Mfr &&
            version = other.Version &&
            deviceType = other.DeviceType
        | _ -> false

    override this.GetHashCode() =
        HashCode.Combine(idNumber, mfr, version, deviceType)

    static member TryCreate id mfr version deviceType : Result<MbusAddress, string> =
        if id < 0 || id > 99999999 then
            Error $"Invalid ID number: {id}, must be between 0 and 99999999"
        elif String.length mfr<> 3 then
            Error $"Invalid manufacturer code length: {mfr.Length}, expected 3"
        elif not (mfr |> Seq.forall (fun c -> c >= 'A' && c <= 'Z')) then
            Error $"Invalid manufacturer code: {mfr}, must contain only uppercase letters A-Z"
        elif version < 0 || version > 255 then
            Error $"Invalid version: {version}, must be between 0 and 255"
        else MbusAddress(id, mfr, version, deviceType) |> Ok

    static member Create id mfr version deviceType =
        match MbusAddress.TryCreate id mfr version deviceType with
        | Ok adr -> adr
        | Error msg -> MbusError msg |> raise

type MbusApplicationError =
    | NoError = 0uy
    | ApplicationBusy = 1uy
    | AnyApplicationError = 2uy
    | AbnormalCondition = 3uy

type MbusStatusField =
    { ApplicationError : MbusApplicationError
      PowerLow : bool
      PermanentError : bool
      TemporaryError : bool }

    static member CreateEmpty=
        { ApplicationError = MbusApplicationError.NoError
          PowerLow = false
          PermanentError = false
          TemporaryError = false }

type MbusDataType =
    | NoData
    | Bcd2Digit
    | Bcd4Digit
    | Bcd6Digit
    | Bcd8Digit
    | Bcd12Digit
    | Integer8Bit
    | Integer16Bit
    | Integer24Bit
    | Integer32Bit
    | Integer48Bit
    | Integer64Bit
    | Real32Bit
    | Text

type MbusFunctionField =
    | InstValue
    | MinValue
    | MaxValue
    | ValueInErrorState

type MbusValueType =
    | Energy
    | Date
    | DateAndTime
    | ErrorFlags
    | ExternalTemperature
    | FabricationNumber
    | FlowTemperature
    | ReturnTemperature
    | TemperatureDifference
    | MetrologyVersionNumber
    | OnTime
    | OperatingTime
    | RemainingBatteryLifetime
    | Volume
    | VolumeFlow
    | VolumeFlowExt
    | Address
    | Credit
    | Debit
    | DeviceType
    | UniqueMessageIdentification
    | ActualityDuration
    | AveragingDuration
    | Power
    | HardwareVersionNumber
    | DigitalInput
    | DigitalOutput
    | Customer
    | SpecialSupplierInformation
    | MassFlow
    | Mass
    | Identification
    | Manufacturer
    | ParameterSetIdentification
    | Model
    | OtherSoftwareVersionNumber
    | CustomerLocation
    | AccessCodeUser
    | AccessCodeOperator
    | AccessCodeSystemOperator
    | AccessCodeDeveloper
    | Password
    | ErrorMask
    | SecurityKey
    | BaudRate
    | ResponseDelayTime
    | Retry
    | RemoteControl
    | FirstStorageNumberForCyclicStorage
    | LastStorageNumberForCyclicStorage
    | SizeOfStorageBlock
    | StorageInterval
    | OperatorSpecificData
    | TimePointSecond
    | DurationSinceLastReadout
    | StartOfTariff
    | PeriodOfTariff
    | NoVif
    | DataContainerForWMbusProtocol
    | PeriodOfNominalDataTransmissions
    | Voltage
    | Current
    | ResetCounter
    | AccumulationCounter
    | ControlSignal
    | DayOfWeek
    | WeekNumber
    | TimePointOfDayChange
    | StateOfParameterActivation
    | DurationSinceLastAccumulation
    | OperatingTimeBattery
    | DateAndTimeOfBatteryChange
    | RfLevel
    | DaylightSavings
    | ListeningWindowManagement
    | NumberOfTimesTheMeterWasStopped
    | DataContainerForManufacturerSpecificProtocol
    | ManufacturerSpecific
    | DurationOfTariff
    | ReactiveEnergy
    | ApparentEnergy
    | ReactivePower
    | RelativeHumidity
    | PhaseUToU
    | PhaseUToI
    | Frequency
    | ApparentPower
    | ColdWarmTemperatureLimit
    | CumulativeMaximumOfActivePower
    | UnitsForHca
    | Pressure

type MbusValueTypeExtension =
    | RelativeDeviation = 0x04uy
    | StandardConformDataContent = 0x1Duy
    | CompactProfileWithRegisters = 0x1Euy
    | CompactProfileWithoutRegisters = 0x1Fuy
    | PerSecond = 0x20uy
    | PerMinute = 0x21uy
    | PerHour = 0x22uy
    | PerDay = 0x23uy
    | PerWeek = 0x24uy
    | PerMonth = 0x25uy
    | PerYear = 0x26uy
    | PerRevolutionMeasurement = 0x27uy
    | IncrementPerInputPulseOnChannel0 = 0x28uy
    | IncrementPerInputPulseOnChannel1 = 0x29uy
    | IncrementPerOutputPulseOnChannel0 = 0x2Auy
    | IncrementPerOutputPulseOnChannel1 = 0x2Buy
    | PerLitre = 0x2Cuy
    | PerM3 = 0x2Duy
    | PerKg = 0x2Euy
    | PerK = 0x2Fuy
    | PerKWh = 0x30uy
    | PerGJ = 0x31uy
    | PerKW = 0x32uy
    | PerKL = 0x33uy
    | PerV = 0x34uy
    | PerA = 0x35uy
    | MultipliedByS = 0x36uy
    | MultipliedBySV = 0x37uy
    | MultipliedBySA = 0x38uy
    | StartDateOf = 0x39uy
    | VifContainsUncorrectedUnitOrValue = 0x3Auy
    | AccumulationOnlyIfPositiveContributions = 0x3Buy
    | AccumulationOfAbsValueOnlyIfNegativeContributions = 0x3Cuy
    | ReservedForAlternateNonMetricUnitSystem = 0x3Duy
    | ValueAtBaseConditions = 0x3Euy
    | ObisDeclaration = 0x3Fuy
    | MultiplicativeCorrectionFactorForValue = 0x7Duy
    | FutureValue = 0x7Euy
    | ManufacturerSpecific = 0x7Fuy

type MbusUnit =
    | NoUnit
    | WattHours
    | Watt
    | JoulesPerHour
    | Joules
    | Calories
    | Celsius
    | Kelvin
    | CubicMeters
    | CubicMetersPerSecond
    | CubicMetersPerMinute
    | CubicMetersPerHour
    | Degrees
    | Hertz
    | KiloGrams
    | KiloGramsPerHour
    | Tons
    | Seconds
    | Days
    | Minutes
    | Hours
    | KiloBitsPerSecond
    | Months
    | Years
    | Volts
    | Amps
    | DecibelMilliWatts
    | VoltAmpereReactiveHours
    | VoltAmpereHours
    | VoltAmpereReactive
    | VoltAmpere
    | Percent
    | CubicFeet
    | Bar

// module MbusUnit =
//     let toText unit =
//         match unit with
//         | NoUnit -> ""
//         | WattHours -> "Wh"
//         | Watt -> "W"
//         | JoulesPerHour -> "J/h"
//         | Joules -> "J"
//         | Celsius -> "°C"
//         | CubicMeters -> "m³"
//         | CubicMetersPerSecond -> "m³/s"
//         | CubicMetersPerMinute -> "m³/minutes"
//         | CubicMetersPerHour -> "m³/h"
//         | Kelvin -> "K"
//         | KiloGrams -> "kg"
//         | KiloGramsPerHour -> "kg/h"
//         | Tons -> "t"
//         | Seconds -> "s"
//         | Days -> "d"
//         | Minutes -> "minutes"
//         | Hours -> "h"
//         | KiloBitsPerSecond -> "kbps"
//         | Months -> "months"
//         | Years -> "years"
//         | Volts -> "V"
//         | Amps -> "A"
//         | DecibelMilliWatts -> "dBm"
//         | Calories -> "Cal"
//         | VoltAmpereReactiveHours -> "VARh"
//         | VoltAmpereHours -> "VAh"
//         | VoltAmpereReactive -> "VAR"
//         | VoltAmpere -> "VA"
//         | Percent -> "%"
//         | CubicFeet -> "feet³"
//         | Degrees -> "°"
//         | Hertz -> "Hz"
//         | Bar -> "bar"

[<AbstractClass>]
type MbusRecordBase(unit: MbusUnit, fn: MbusFunctionField, storageNum: int, tariff: int, subUnit: int) =
    member _.Unit = unit
    member _.Function = fn
    member _.StorageNumber = storageNum
    member _.Tariff = tariff
    member _.SubUnit = subUnit

type MbusNumericalRecord(unit: MbusUnit, fn: MbusFunctionField, storageNum: int, tariff: int, subUnit: int, value: double, valueType: MbusValueType) =
    inherit MbusRecordBase(unit, fn, storageNum, tariff, subUnit)
    member _.Value = value
    member _.ValueType = valueType

type MbusTextRecord(unit: MbusUnit, fn: MbusFunctionField, storageNum: int, tariff: int, subUnit: int, value: string, valueType: string) =
    inherit MbusRecordBase(unit, fn, storageNum, tariff, subUnit)
    member _.Value = value
    member _.ValueType = valueType

type WattHoursScaler =
    | ExpMinus3 = 0
    | ExpMinus2 = 1
    | ExpMinus1 = 2
    | Exp0 = 3
    | Exp1 = 4
    | Exp2 = 5
    | Exp3 = 6
    | Exp4 = 7
    | Exp5 = 8
    | Exp6 = 9

type CubicMetersScaler =
    | ExpMinus6 = 0
    | ExpMinus5 = 1
    | ExpMinus4 = 2
    | ExpMinus3 = 3
    | ExpMinus2 = 4
    | ExpMinus1 = 5
    | Exp0 = 6
    | Exp1 = 7
    | Exp2 = 8
    | Exp3 = 9