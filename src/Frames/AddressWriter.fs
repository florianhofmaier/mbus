namespace Mbus.Frames

open System
open Mbus
open Mbus.BaseWriters.BcdWriters
open Mbus.BaseWriters.BinaryWriters
open Mbus.BaseWriters.Core

module AddressWriter =

    let private encodeMfr (mfr: string) : uint16 =
        if String.IsNullOrEmpty(mfr) || mfr.Length < 3 then 0us
        else
            let c1 = uint16 (Char.ToUpperInvariant mfr[0]) - 64us
            let c2 = uint16 (Char.ToUpperInvariant mfr[1]) - 64us
            let c3 = uint16 (Char.ToUpperInvariant mfr[2]) - 64us
            (c1 <<< 10) ||| (c2 <<< 5) ||| c3

    let writeAla (adr: MbusAddress) : Writer<unit> =
        writer {
            do! writeBcdU32 (uint32 adr.IdNumber)
            do! writeU16 (encodeMfr adr.Mfr)
            do! writeU8 (uint8 adr.Version)
            do! writeU8 (uint8 adr.DeviceType)
        }

