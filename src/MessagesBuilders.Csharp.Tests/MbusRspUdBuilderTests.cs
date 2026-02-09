using LambdaTale;
using Shouldly;

namespace Mbus.MbusData.Test;

public class MbusRspUdBuilderTests
{
    private static MbusAddress Address =>
        MbusAddress.Create(21542293, "GWF", 0x3C, MbusDeviceType.WaterMeter);

    [Scenario]
    public void BuildRspUd_WhenDeviceIsGwgMkp_ShouldReturnExpectedBytes()
    {
        var builder = new MbusRspUdBuilder(Address)
            .WithPrimaryAddress(3)
            .WithAccessNumber(1)
            .WithDataRecord(record => record
                .WithBcd8Digit(21542293)
                .AsFabricationNumber())
            .WithDataRecord(record => record
                .WithBcd8Digit(457)
                .AsCubicMeters(CubicMetersScaler.ExpMinus3));

        var bytes = builder.Build();

        bytes.ToArray().ShouldBe(
        [
            0x68, 0x1B, 0x1B, 0x68, 0x08, 0x03, 0x72, 0x93, 0x22, 0x54, 0x21, 0xE6, 0x1E, 0x3C, 0x07, 0x01,
            0x00, 0x00, 0x00, 0x0C, 0x78, 0x93, 0x22, 0x54, 0x21, 0x0C, 0x13, 0x57, 0x04, 0x00, 0x00, 0x17,
            0x16
        ]);
    }
}