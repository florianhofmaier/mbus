namespace Mbus.Frames
open Mbus
open Mbus.Records

type DeviceSelection = { Adr: MbusAddress; Data: Record list option }

type Apl =
    | UserData of Record list
    | AlarmBits of uint8
    | DeviceSelection of DeviceSelection