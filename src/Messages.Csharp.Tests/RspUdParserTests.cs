using LambdaTale;
using Shouldly;

namespace Mbus.MbusData.Test;

public class RspUdParserTests
{
    [Scenario]
    public void Test1(
        byte[] buffer,
        RspUd rspUd,
        int bytesRead)
    {
        "Given a buffer containing RspUd of GWF MKP".x(() =>
            buffer = [ 0x68, 0x1B, 0x1B, 0x68, 0x08, 0x03, 0x72, 0x93, 0x22, 0x54,
                       0x21, 0xE6, 0x1E, 0x3C, 0x07, 0x01, 0x00, 0x00, 0x00, 0x0C,
                       0x78, 0x93, 0x22, 0x54, 0x21, 0x0C, 0x13, 0x57, 0x04, 0x00,
                       0x00, 0x17, 0x16 ]);

        "When parsing the buffer".x(() =>
            (rspUd, bytesRead) = RspUd.FromBytes(new ReadOnlyMemory<byte>(buffer)));

        "Then the response should be parsed correctly".x(() =>
            {
                rspUd.PrimaryAddress.ShouldBe(3);
                rspUd.SecondaryAddress.IdNumber.ShouldBe(21542293);
                rspUd.SecondaryAddress.Mfr.ShouldBe("GWF");
                rspUd.SecondaryAddress.DeviceType.ShouldBe(MbusDeviceType.WaterMeter);
                rspUd.SecondaryAddress.Version.ShouldBe(0x3C);
                rspUd.StatusField.ShouldBeEquivalentTo(MbusStatusField.CreateEmpty);
                rspUd.MfrData.ShouldBeEmpty();
                rspUd.NumericalRecords.Count.ShouldBe(2);
                rspUd.NumericalRecords[0].Value.ShouldBe(21542293);
                rspUd.NumericalRecords[0].ValueType.ShouldBe(MbusValueType.FabricationNumber);
                rspUd.NumericalRecords[0].Function.ShouldBe(MbusFunctionField.InstValue);
                rspUd.NumericalRecords[0].StorageNumber.ShouldBe(0);
                rspUd.NumericalRecords[0].SubUnit.ShouldBe(0);
                rspUd.NumericalRecords[0].Tariff.ShouldBe(0);
                rspUd.NumericalRecords[0].Unit.ShouldBe(MbusUnit.NoUnit);
                rspUd.NumericalRecords[1].Value.ShouldBe(0.457);
                rspUd.NumericalRecords[1].ValueType.ShouldBe(MbusValueType.Volume);
                rspUd.NumericalRecords[1].Function.ShouldBe(MbusFunctionField.InstValue);
                rspUd.NumericalRecords[1].StorageNumber.ShouldBe(0);
                rspUd.NumericalRecords[1].SubUnit.ShouldBe(0);
                rspUd.NumericalRecords[1].Tariff.ShouldBe(0);
                rspUd.NumericalRecords[1].Unit.ShouldBe(MbusUnit.CubicMeters);
            });
    }

    [Scenario]
    public void FromStreamAsync_WhenLongFrame_ShouldParse(
        MemoryStream stream,
        RspUd rspUd)
    {
        "Given a stream containing RspUd of GWF MKP".x(() =>
        {
            var buffer = new byte[]
            {
                0x68, 0x1B, 0x1B, 0x68, 0x08, 0x03, 0x72, 0x93, 0x22, 0x54,
                0x21, 0xE6, 0x1E, 0x3C, 0x07, 0x01, 0x00, 0x00, 0x00, 0x0C,
                0x78, 0x93, 0x22, 0x54, 0x21, 0x0C, 0x13, 0x57, 0x04, 0x00,
                0x00, 0x17, 0x16
            };

            stream = new MemoryStream(buffer, writable: false);
        }).Teardown(() => stream.Dispose());

        "When parsing the stream asynchronously".x(async () =>
            rspUd = await RspUd.FromStreamAsync(stream, CancellationToken.None));

        "Then the response should be parsed correctly".x(() =>
        {
            rspUd.PrimaryAddress.ShouldBe(3);
            rspUd.SecondaryAddress.IdNumber.ShouldBe(21542293);
            rspUd.SecondaryAddress.Mfr.ShouldBe("GWF");
            rspUd.SecondaryAddress.DeviceType.ShouldBe(MbusDeviceType.WaterMeter);
            rspUd.SecondaryAddress.Version.ShouldBe(0x3C);
            rspUd.Acd.ShouldBeFalse();
            rspUd.Afc.ShouldBeFalse();
            rspUd.StatusField.ShouldBeEquivalentTo(MbusStatusField.CreateEmpty);
            rspUd.MfrData.ShouldBeEmpty();
            rspUd.MoreDataFollows.ShouldBeFalse();
            rspUd.NumericalRecords.Count.ShouldBe(2);
            rspUd.NumericalRecords[0].Value.ShouldBe(21542293);
            rspUd.NumericalRecords[0].ValueType.ShouldBe(MbusValueType.FabricationNumber);
            rspUd.NumericalRecords[0].Function.ShouldBe(MbusFunctionField.InstValue);
            rspUd.NumericalRecords[0].StorageNumber.ShouldBe(0);
            rspUd.NumericalRecords[0].SubUnit.ShouldBe(0);
            rspUd.NumericalRecords[0].Tariff.ShouldBe(0);
            rspUd.NumericalRecords[0].Unit.ShouldBe(MbusUnit.NoUnit);
            rspUd.NumericalRecords[1].Value.ShouldBe(0.457);
            rspUd.NumericalRecords[1].ValueType.ShouldBe(MbusValueType.Volume);
            rspUd.NumericalRecords[1].Function.ShouldBe(MbusFunctionField.InstValue);
            rspUd.NumericalRecords[1].StorageNumber.ShouldBe(0);
            rspUd.NumericalRecords[1].SubUnit.ShouldBe(0);
            rspUd.NumericalRecords[1].Tariff.ShouldBe(0);
            rspUd.NumericalRecords[1].Unit.ShouldBe(MbusUnit.CubicMeters);
        });
    }
}
