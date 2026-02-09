namespace Mbus.Io

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open Mbus.Frames
open Mbus.BaseWriters.Core

type StreamWriter(stream: Stream) =
    let lock = new SemaphoreSlim(1, 1)
    let writeBuffer = Array.zeroCreate<byte> 4096

    let serialize frame =
        let wState = { WState.Buf = writeBuffer; Pos = 0 }
        let writer =
            match frame with
            | Confirmation -> FrameWriter.writeConfirmationFrame
            | ShortFrame sf -> FrameWriter.writeShortFrame sf
            | LongFrame lf -> FrameWriter.writeLongFrame lf

        match writer wState with
        | Ok (_, endState) -> endState.Pos
        | Error e -> failwithf "Serialization failed: %s" e.Msg

    member this.WriteAsync(frame: Frame, ct: CancellationToken) = task {
        let len = serialize frame

        do! lock.WaitAsync(ct)
        try
            do! stream.WriteAsync(ReadOnlyMemory(writeBuffer, 0, len), ct)
            do! stream.FlushAsync(ct)
        finally
            lock.Release() |> ignore
    }
