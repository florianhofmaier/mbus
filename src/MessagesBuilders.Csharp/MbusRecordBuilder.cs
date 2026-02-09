using Mbus.Messages;
using Mbus.Records;
using Microsoft.FSharp.Core;

namespace Mbus;

public class MbusRecordBuilder
{
    public RecordBuilderWithValue WithInt32(int value) =>
        new (RecordsBuilders.Values.withInt32<int>(value).ResultValue);

    public RecordBuilderWithValue WithBcd8Digit(int value) =>
        new (RecordsBuilders.Values.withBcd8Digit(value).ResultValue);
}

public class RecordBuilderWithValue(MbusValue value)
{
    public RecordBuilderWithData AsWattHours(WattHoursScaler scaler) =>
        new (RecordsBuilders.EnergyRecords.asWattHours(scaler, value));

    public RecordBuilderWithData AsCubicMeters(CubicMetersScaler scaler) =>
        new (RecordsBuilders.VolumeRecords.asCubicMeters(scaler, value));

    public RecordBuilderWithData AsFabricationNumber() =>
        new (RecordsBuilders.FabricationNumbers.asFabNum(value));
}

public class RecordBuilderWithData(DataRecord record)
{
    internal DataRecord Record => record;

    public RecordBuilderWithData WithFunction(MbusFunctionField function) =>
        new (RecordsBuilders.withFunction<DataRecord>(function, record).ResultValue);

    public RecordBuilderWithData WithStorageNumber(int storageNumber) =>
        RecordsBuilders.withStNum(storageNumber, record)
            .MapOrThrow(
                r => new RecordBuilderWithData(r),
                e => new MbusError(e));

    public RecordBuilderWithData WithSubUnit(int subUnit) =>
        RecordsBuilders.withSubUnit(subUnit, record)
            .MapOrThrow(
                r => new RecordBuilderWithData(r),
                e => new MbusError(e));

    public RecordBuilderWithData WithTariff(int tariff) =>
        RecordsBuilders.withTariff(tariff, record)
            .MapOrThrow(
                r => new RecordBuilderWithData(r),
                e => new MbusError(e));

    internal FSharpFunc
        <BaseWriters.Core.WState, FSharpResult<Tuple<Unit, BaseWriters.Core.WState>, BaseWriters.Core.WError>>
        BuildWriter() =>
        FuncConvert.FromFunc((BaseWriters.Core.WState s) => RecordsBuilders.build(record).Invoke(s));

    public ReadOnlySpan<byte> Build() =>
        RecordsBuilders.build(record)
        .Invoke(BaseWriters.Core.WStateModule.create)
        .ValueOrThrow(e => e.ToMbusError())
        .Item2
        .AsSpan();
}