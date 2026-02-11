namespace Mbus.Devices

open Mbus
open Mbus.Frames
open Mbus.Records

type DeviceState = {
    PrimaryAddress: byte
    SecondaryAddress: MbusAddress
    AccessNumber: byte
    Data: Record list
}

type DeviceAction =
    | NoAction
    | ChangeAddress of byte
    | SendFrame of Frame

module DeviceLogic =

    let initialState prmAdr secAdr = {
        PrimaryAddress = prmAdr
        SecondaryAddress = secAdr
        AccessNumber = 0uy
        Data = []
    }

    let private createRspUd state =
        let tpl = {
            Func = TplLongFunc.Rsp
            Ala = state.SecondaryAddress
            Acc = 0uy
            Status = MbusStatusField.CreateEmpty
            Cnf = 0us
        }
        Frame.LongFrame {
            CField = 0x08uy // RSP_UD
            PrmAdr = state.PrimaryAddress
            Tpl = Tpl.Long tpl
            Apl = Apl.UserData state.Data
        }

    let handleFrame (state: DeviceState) (frame: Frame) : DeviceState * DeviceAction =
        match frame with
        | Frame.ShortFrame sf when sf.CField = 0x40uy && sf.PrmAdr = state.PrimaryAddress ->
             let newState = { state with AccessNumber = state.AccessNumber + 1uy }
             (newState, SendFrame Frame.Confirmation)

        | Frame.ShortFrame sf when (sf.CField = 0x5Buy || sf.CField = 0x7Buy) && sf.PrmAdr = state.PrimaryAddress ->
             let response = createRspUd state
             state, SendFrame response

        | _ ->
            state, NoAction

