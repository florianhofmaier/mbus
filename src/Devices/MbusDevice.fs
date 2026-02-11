namespace Mbus

open System.IO
open System.Threading
open System.Collections.Generic
open Mbus.Devices
open Mbus.Records

type MbusDevice(stream: Stream, address: byte) =
    let mutable _dataRecords : Record list = []
    let _lock = obj()

    member this.SetRecords(records: IEnumerable<Record>) =
        let recordsList = List.ofSeq records
        lock _lock (fun () ->
            _dataRecords <- recordsList
        )

    member this.RunAsync(ct: CancellationToken) =
        let initialState = DeviceLogic.initialState address
        let getData () = lock _lock (fun () -> _dataRecords)
        DeviceRuntime.runAsync stream getData initialState ct