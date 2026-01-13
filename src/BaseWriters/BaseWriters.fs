module Mbus.BaseWriters

type WState = private { Buf: byte[]; Pos: int }

module WState =
    let create n =
        { Buf = Array.zeroCreate<byte> n; Pos = 0 }
    let buf (st: WState) = st.Buf
    let pos (st: WState) = st.Pos

type WError = { Pos: int; Msg: string; Ctx: string list }
module WError =
    let toString (e: WError) : string =
        let ctxStr =
            match e.Ctx with
            | [] -> ""
            | ctx -> " Context: " + (String.concat "." (List.rev ctx))
        $"Error while writing {ctxStr} at position {e.Pos}: {e.Msg}"

type Writer = WState -> Result<WState, WError>

let err (st: WState) msg = Error { Pos = st.Pos; Msg = msg; Ctx = [] }

let inline private ensureSpace (st: WState) n : Result<WState, WError> =
    if st.Pos + n > st.Buf.Length then
        err st $"not enough space in buffer to write {n} bytes"
    else
        Ok st

let inline private checkedWrite (n: int) (writeAt: WState -> unit) : WState -> Result<WState, WError> =
    fun st ->
        ensureSpace st n
        |> Result.map (fun st ->
            writeAt st
            { st with Pos = st.Pos + n })

let inline private writeBytes (byteCount: int) (value: uint64) : WState -> Result<WState, WError> =
    checkedWrite byteCount (fun st ->
        for i = 0 to byteCount - 1 do
            st.Buf[st.Pos + i] <- byte ((value >>> (8 * i)) &&& 0xFFUL))

let inline private u8bits (v: int8) : uint64 = uint64 (uint8 v)
let inline private u16bits (v: int16) : uint64 = uint64 (uint16 v)
let inline private u32bits (v: int32) : uint64 = uint64 (uint32 v)
let inline private u64bits (v: int64) : uint64 = uint64 v

let writeU8 (value: uint8) : WState -> Result<WState, WError> =
    writeBytes 1 (uint64 value)

let writeI8 (value: int8) : WState -> Result<WState, WError> =
    writeBytes 1 (u8bits value)

let writeU16 (value: uint16) : WState -> Result<WState, WError> =
    writeBytes 2 (uint64 value)

let writeI16 (value: int16) : WState -> Result<WState, WError> =
    writeBytes 2 (u16bits value)

let writeU24 (value: uint32) : WState -> Result<WState, WError> =
    writeBytes 3 (uint64 value)

let writeI24 (value: int32) : WState -> Result<WState, WError> =
    writeBytes 3 (u32bits value &&& 0xFFFFFFUL)

let writeU32 (value: uint32) : WState -> Result<WState, WError> =
    writeBytes 4 (uint64 value)

let writeI32 (value: int32) : WState -> Result<WState, WError> =
    writeBytes 4 (u32bits value)

let writeU48 (value: uint64) : WState -> Result<WState, WError> =
    writeBytes 6 value

let writeI48 (value: int64) : WState -> Result<WState, WError> =
    writeBytes 6 (u64bits value &&& 0xFFFFFFFFFFFFUL)

let writeU64 (value: uint64) : WState -> Result<WState, WError> =
    writeBytes 8 value

let writeI64 (value: int64) : WState -> Result<WState, WError> =
    writeBytes 8 (u64bits value)

let inline private bcdByte (d2: uint32) : byte =
    let tens = d2 / 10u
    let ones = d2 % 10u
    byte ((tens <<< 4) ||| ones)

let inline private writeBcdBytes (bcdByteCount: int) (value: uint64) : WState -> Result<WState, WError> =
    checkedWrite bcdByteCount (fun st ->
        let mutable v = value
        for i = 0 to bcdByteCount - 1 do
            let d2 = uint32 (v % 100UL)
            st.Buf[st.Pos + i] <- bcdByte d2
            v <- v / 100UL)

let writeBcdU8 (value: uint8) : WState -> Result<WState, WError> =
    writeBcdBytes 1 (uint64 value)

let writeBcdU16 (value: uint16) : WState -> Result<WState, WError> =
    writeBcdBytes 2 (uint64 value)

let writeBcdU24 (value: uint32) : WState -> Result<WState, WError> =
    writeBcdBytes 3 (uint64 value)

let writeBcdU32 (value: uint32) : WState -> Result<WState, WError> =
    writeBcdBytes 4 (uint64 value)

let writeBcdU48 (value: uint64) : WState -> Result<WState, WError> =
    writeBcdBytes 6 value
