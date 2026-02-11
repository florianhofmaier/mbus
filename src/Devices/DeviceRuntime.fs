namespace Mbus.Devices

open System.IO
open System.Threading
open Mbus.Io
open Mbus.Records

module DeviceRuntime =

    let runAsync (stream: Stream) (getData: unit -> Record list) (initialState: DeviceState) (ct: CancellationToken) =
        task {
            let reader = StreamReader.create stream 1024
            let writer = StreamWriter.create stream

            let mutable currentState = initialState

            while not ct.IsCancellationRequested do
                let! frame = reader ct

                let currentRecords = getData()
                let stateForLogic = { currentState with Data = currentRecords }

                // 3. Logic: Compute next step (Pure)
                let newState, action = DeviceLogic.handleFrame stateForLogic frame

                // Update internal state (Address, FCB, etc.)
                // Note: We might want to persist only protocol state, not Data if it comes from getData
                currentState <- newState

                // 4. IO: Execute side effects
                match action with
                | SendFrame response ->
                    do! writer response ct
                | NoAction ->
                    ()
        }

