using Mbus.Devices;
using Mbus.Records;
using Mbus.UserData;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace Mbus;

public class MbusDevice
{
    private readonly Lock _lock = new();
    private readonly List<DataRecord> _userData = [];

    public void UpdateUserData(Action<IUserDataBuilder> updateAction)
    {
        lock (_lock)
        {
            _userData.Clear();
            var builder = new UserDataBuilder(_userData);
            updateAction(builder);
        }
    }

    public Task RunAsync(
        Stream stream,
        int idNumber,
        string mfrCode,
        MbusDeviceType deviceType,
        byte version,
        byte primaryAddress,
        CancellationToken ct)
    {
        var address = MbusAddress.Create(idNumber, mfrCode, version, deviceType);

        if (address.IsError)
            throw new ArgumentException(address.ErrorValue);

        var initialState = DeviceLogic.initialState(primaryAddress, address.ResultValue);

        return DeviceRuntime.runAsync(stream, GetDataFunc, initialState, ct);
    }

    private FSharpFunc<Unit, FSharpList<Record>> GetDataFunc =>
        FuncConvert.FromFunc(() =>
        {
            lock (_lock)
            {
                return ListModule.OfSeq(_userData.Select(Record.NewData));
            }
        });
}