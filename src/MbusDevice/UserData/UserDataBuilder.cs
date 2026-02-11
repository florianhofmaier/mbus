using Mbus.Records;

namespace Mbus.UserData;

internal class UserDataBuilder(List<DataRecord> userData) : IUserDataBuilder
{
    public IUserDataBuilder AddRecord(Func<MbusRecordBuilder, RecordBuilderWithData> recordBuilderFunc)
    {
        var record = recordBuilderFunc(new MbusRecordBuilder()).Record;
        userData.Add(record);
        return this;
    }
}