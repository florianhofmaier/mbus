using Mbus.Messages;
using Mbus.Records;

namespace Mbus;

public class MbusRecordBuilder
{
    public static RecordBuilderWithValue WithInt32(int value) =>
        new (RecordsBuilders.Values.withInt32<int>(value).ResultValue);
}

public class RecordBuilderWithValue(MbusValue value)
{
    public RecordBuilderWithData AsEnergyWattHours(WattHoursScaler scaler) =>
        new (RecordsBuilders.EnergyRecords.withWattHoursScaler(scaler, value));
}

public class RecordBuilderWithData(DataRecord record)
{
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

    public ReadOnlySpan<byte> Build()
    {
        var wState = RecordsBuilders.build(record, BaseWriters.WStateModule.create(256))
            .ValueOrThrow(e => new MbusError(BaseWriters.WErrorModule.toString(e)));

        return new ReadOnlySpan<byte>(
            BaseWriters.WStateModule.buf(wState), 0, BaseWriters.WStateModule.pos(wState));
    }
}