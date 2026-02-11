module Mbus.Io.Tests.StreamReaderTests

open Xunit
open System.IO
open System.Threading
open Mbus.Io
open Mbus.Frames

[<Fact>]
let ``ReadAsync WhenStreamContainsConfirmation ShouldReturnConfirmationFrame`` () =
    task {
        let data = [| 0xE5uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms 1024

        let! frame = reader CancellationToken.None

        match frame with
        | Frame.Confirmation -> ()
        | _ -> failwith "Expected Confirmation"
    }

[<Fact>]
let ``ReadAsync WhenStreamContainsShortFrame ShouldReturnShortFrame`` () =
    task {
        let data = [| 0x10uy; 0x40uy; 0x01uy; 0x41uy; 0x16uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms 1024

        let! frame = reader CancellationToken.None

        match frame with
        | Frame.ShortFrame _ -> ()
        | _ -> failwith "Expected ShortFrame"
    }

[<Fact>]
let ``ReadAsync WhenStreamContainsGarbageThenConfirmation ShouldReturnConfirmation`` () =
    task {
        let data = [| 0x00uy; 0xFFuy; 0xE5uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms 1024

        let! frame = reader CancellationToken.None

        match frame with
        | Frame.Confirmation -> ()
        | _ -> failwith "Expected Confirmation"
    }

[<Fact>]
let ``ReadAsync WhenStreamEndsIncomplete ShouldThrowEndOfStreamException`` () =
        let data = [| 0x10uy; 0x40uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms 1024

        Assert.ThrowsAsync<EndOfStreamException>(fun () -> reader CancellationToken.None)

[<Fact>]
let ``ReadAsync WhenInvalidFollowedByValid ShouldRecover`` () =
    task {
        let data = [| 0x10uy; 0x40uy; 0x01uy; 0x00uy; 0x16uy; 0xE5uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms 1024

        let! frame = reader CancellationToken.None

        match frame with
        | Frame.Confirmation -> ()
        | _ -> failwith "Expected Confirmation"
    }

[<Fact>]
let ``ReadAsync WhenStreamContainsLongFrame ShouldReturnLongFrame`` () =
    task {
        let data = [| 0x68uy; 0x03uy; 0x03uy; 0x68uy; 0x08uy; 0x01uy; 0x50uy; 0x59uy; 0x16uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms 1024

        let! frame = reader CancellationToken.None

        match frame with
        | Frame.LongFrame _ -> ()
        | actual -> failwith $"Expected LongFrame but was %A{actual}"
    }

[<Fact>]
let ``ReadAsync WhenLongFrameChecksumInvalid ShouldSkipAndRecover`` () =
    task {
        let data = [|
            0x68uy; 0x03uy; 0x03uy; 0x68uy; 0x08uy; 0x01uy; 0x50uy; 0x00uy; 0x16uy;
            0xE5uy
        |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms 1024

        let! frame = reader CancellationToken.None

        match frame with
        | Frame.Confirmation -> ()
        | _ -> failwith "Expected Confirmation"
    }

[<Fact>]
let ``ReadAsync WhenLongFrameIncomplete ShouldThrowEndOfStreamException`` () =
        let data = [| 0x68uy; 0x03uy; 0x03uy; 0x68uy; 0x08uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms 1024

        Assert.ThrowsAsync<EndOfStreamException>(fun () -> reader CancellationToken.None)

