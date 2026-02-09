using Mbus.Messages;

namespace Mbus;

public class MbusRspUdBuilder(MbusAddress secondaryAddress)
{
    private RspUdBuilder _builder = RspUdBuilderModule.init(secondaryAddress);

    public MbusRspUdBuilder WithPrimaryAddress(byte primaryAddress)
    {
        _builder = RspUdBuilderModule.withPrimaryAddress(primaryAddress, _builder);
        return this;
    }

    public MbusRspUdBuilder WithAcd()
    {
        _builder = RspUdBuilderModule.withAcdSet(_builder);
        return this;
    }

    public MbusRspUdBuilder WithDfc()
    {
        _builder = RspUdBuilderModule.withDfcSet(_builder);
        return this;
    }

    public MbusRspUdBuilder WithDataRecord(Func<MbusRecordBuilder, RecordBuilderWithData> recordBuilderFunc)
    {
        var record = recordBuilderFunc(new MbusRecordBuilder()).Record;
        _builder = RspUdBuilderModule.addDataRecord(record, _builder);
        return this;
    }
    
    public MbusRspUdBuilder WithAccessNumber(byte accessNumber)
    {
        _builder = RspUdBuilderModule.withAccessNumber(accessNumber, _builder);
        return this;
    }

    public ReadOnlySpan<byte> Build() =>
        RspUdBuilderModule.build(_builder)
        .Invoke(BaseWriters.Core.WStateModule.create)
        .ValueOrThrow(e => e.ToMbusError())
        .Item2
        .AsSpan();
}