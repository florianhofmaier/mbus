module Mbus.Io.Tests.StreamReaderTests

open Xunit
open Shouldly
open System.IO
open System.Threading
open System.Threading.Tasks
open Mbus.Io
open Mbus.Frames

[<Fact>]
let ``ReadAsync_WhenStreamContainsConfirmation_ShouldReturnConfirmationFrame`` () =
    task {
        let data = [| 0xE5uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms None

        let! frame = reader CancellationToken.None

        frame.ShouldBe(Frame.Confirmation)
    }

[<Fact>]
let ``ReadAsync_WhenStreamContainsShortFrame_ShouldReturnShortFrame`` () =
    task {
        // Start(10) C(40) A(01) CS(41) Stop(16)
        // CS = 40 + 01 = 41
        let data = [| 0x10uy; 0x40uy; 0x01uy; 0x41uy; 0x16uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms None

        let! frame = reader CancellationToken.None

        match frame with
        | Frame.ShortFrame sf ->
            sf.CField.ShouldBe(0x40uy)
            sf.PrmAdr.ShouldBe(0x01uy)
        | _ -> failwith "Expected ShortFrame"
    }

[<Fact>]
let ``ReadAsync_WhenStreamContainsGarbageThenConfirmation_ShouldReturnConfirmation`` () =
    task {
        let data = [| 0x00uy; 0xFFuy; 0xE5uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms None

        let! frame = reader CancellationToken.None

        frame.ShouldBe(Frame.Confirmation)
    }

[<Fact>]
let ``ReadAsync_WhenStreamEndsIncomplete_ShouldThrowEndOfStreamException`` () =
    task {
        // Incomplete short frame
        let data = [| 0x10uy; 0x40uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms None

        // Use Assert.ThrowsAsync implicitly by calling in task block
        let! _ = Assert.ThrowsAsync<EndOfStreamException>(fun () -> reader CancellationToken.None)
        ()
    }

[<Fact>]
let ``ReadAsync_WhenInvalidFollowedByValid_ShouldRecover`` () =
    task {
        // 0x10 + 5 bytes that are invalid (bad checksum)
        // 10 40 01 00 16 (CS should be 41, is 00)
        // Then followed by E5
        let data = [|
            0x10uy; 0x40uy; 0x01uy; 0x00uy; 0x16uy;
            0xE5uy
        |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms None

        let! frame = reader CancellationToken.None
        frame.ShouldBe(Frame.Confirmation)
        // The first 5 bytes should be consumed/skipped one by one until E5 matches
    }

[<Fact>]
let ``ReadAsync_WhenStreamContainsLongFrame_ShouldReturnLongFrame`` () =
    task {
        // Long Frame: L=3
        // 68 03 03 68 08 01 50 59 16
        // C=08, A=01, CI=50 -> Tpl=CiOnly(AplSelect)
        // Apl -> UserData([])
        let data = [| 0x68uy; 0x03uy; 0x03uy; 0x68uy; 0x08uy; 0x01uy; 0x50uy; 0x59uy; 0x16uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms None

        let! frame = reader CancellationToken.None

        match frame with
        | Frame.LongFrame lf ->
            lf.CField.ShouldBe(0x08uy)
            lf.PrmAdr.ShouldBe(0x01uy)
            match lf.Tpl with
            | Tpl.CiOnly func -> func.ShouldBe(TplCiOnlyFunc.AplSelect)
            | _ -> failwith "Expected CiOnly TPL"

            match lf.Apl with
            | Apl.UserData records -> records.ShouldBeEmpty()
            | _ -> failwith "Expected UserData APL"
        | _ -> failwith "Expected LongFrame"
    }

[<Fact>]
let ``ReadAsync_WhenLongFrameChecksumInvalid_ShouldSkipAndRecover`` () =
    task {
         // Long Frame: L=3 but Checksum 00 (should be 59)
        // 68 03 03 68 08 01 50 00 16
        // Followed by E5
        let data = [|
            0x68uy; 0x03uy; 0x03uy; 0x68uy; 0x08uy; 0x01uy; 0x50uy; 0x00uy; 0x16uy;
            0xE5uy
        |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms None

        let! frame = reader CancellationToken.None
        frame.ShouldBe(Frame.Confirmation)
    }

[<Fact>]
let ``ReadAsync_WhenLongFrameIncomplete_ShouldThrowEndOfStreamException`` () =
    task {
        // Header ok, but missing data
        // 68 03 03 68 08 (missing remaining bytes)
        let data = [| 0x68uy; 0x03uy; 0x03uy; 0x68uy; 0x08uy |]
        use ms = new MemoryStream(data)
        let reader = StreamReader.create ms None

        // Use Assert.ThrowsAsync implicitly by calling in task block
        let! _ = Assert.ThrowsAsync<EndOfStreamException>(fun () -> reader CancellationToken.None)
        ()
    }
