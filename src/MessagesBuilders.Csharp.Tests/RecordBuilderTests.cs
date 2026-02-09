using LambdaTale;
using Shouldly;

namespace Mbus.MbusData.Test;

public class RecordBuilderTests
{
    [Scenario]
    public void Build_WhenInt32AndEnergyWattHours_ShouldReturnExpectedBytes()
    {
        var builder = new MbusRecordBuilder()
            .WithInt32(1234567890)
            .AsWattHours(WattHoursScaler.Exp0);

        var bytes = builder.Build();

        bytes.ToArray().ShouldBe([ 0x04, 0x03, 0xD2, 0x02, 0x96, 0x49 ]);
    }
}