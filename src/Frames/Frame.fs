namespace Mbus.Frames

open System

type ShortFrame = { CField: uint8; PrmAdr: uint8 }

type LongFrame = { CField: uint8; PrmAdr: uint8; Tpl: Tpl; Apl: Apl }

type Frame =
    | Confirmation
    | ShortFrame of ShortFrame
    | LongFrame of LongFrame

module Frame =
    let confirmationStartByte = 0xE5uy
    let shortFrameStartByte = 0x10uy
    let longFrameStartByte = 0x68uy
    let stopByte = 0x16uy

    let internal calcCrc (mem: ReadOnlyMemory<byte>) =
        let span = mem.Span
        let mutable s = 0
        for i = 0 to span.Length - 1 do
            s <- (s + int span[i]) &&& 0xFF
        byte s