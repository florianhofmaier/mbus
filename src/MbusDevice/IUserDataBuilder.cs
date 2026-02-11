using Mbus.UserData;

namespace Mbus;

public interface IUserDataBuilder
{
    IUserDataBuilder AddRecord(Func<MbusRecordBuilder, RecordBuilderWithData> recordBuilderFunc);
}