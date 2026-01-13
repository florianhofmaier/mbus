module Mbus.BaseParsers.Core

open System

type PState = { Buf: ReadOnlyMemory<uint8>; Off:int }
module PState =
    let init (buf: uint8[]) : PState =
        { Buf = ReadOnlyMemory<byte> buf; Off = 0 }

type PError = { Pos: int; Msg: string; Ctx: string list }
module PError =
    let toString (e: PError) : string =
        let ctxStr =
            match e.Ctx with
            | [] -> ""
            | ctx -> " Context: " + (String.concat "." (List.rev ctx))
        $"Error while parsing {ctxStr} at position {e.Pos}: {e.Msg}"

type P<'a> = PState -> Result<'a * PState, PError>


let mapP f p = fun st ->
    match p st with
    | Ok (a, st2) -> Ok (f a, st2)
    | Error e -> Error e

let (|>>) (p: P<'a>) (f: 'a -> 'b) : P<'b> =
    mapP f p

let inline ok v st = Ok (v, st)
let inline err st msg = Error { Pos = st.Off; Msg = msg; Ctx = [] }
let inline fail msg : P<'a> = fun st -> err st msg
let inline failBefore msg : P<'a> = fun st -> err { st with Off = st.Off - 1 } msg
let inline errBufOverflow st =
    Error { Pos = st.Buf.Length; Msg = "unexpected end of buffer"; Ctx = [] }
let getBuffer : P<ReadOnlyMemory<uint8>> = fun st -> ok st.Buf st

type PBuilder() =
    member _.Bind(p:P<'a>, k:'a -> P<'b>) : P<'b> =
        fun st ->
            match p st with
            | Ok (a, st2) -> k a st2
            | Error e     -> Error e
    member _.Return(x:'a) : P<'a> = fun st -> ok x st
    member _.ReturnFrom(p:P<'a>) : P<'a> = p
    member _.Zero() : P<unit> = fun st -> ok () st
    member _.Delay(f: unit -> P<'a>) : P<'a> = fun st -> f() st
    member _.Combine(p1:P<unit>, p2:P<'a>) : P<'a> = fun st ->
        match p1 st with
        | Ok ((), st2) -> p2 st2
        | Error e      -> Error e

let parser = PBuilder()

let withCtx ctx p : P<'a> = fun st ->
    match p st with
    | Ok r -> Ok r
    | Error e -> Error { e with Ctx = ctx :: e.Ctx }

let pos : P<int> = fun st -> ok st.Off st

let parseByte : P<uint8> =
    fun st ->
        if st.Off >= st.Buf.Length then errBufOverflow st
        else ok st.Buf.Span[st.Off] { st with Off = st.Off + 1 }

let peekByte : P<uint8> =
    fun st ->
        if st.Off >= st.Buf.Length then errBufOverflow st
        else ok st.Buf.Span[st.Off] st

let takeMem (n:int) : P<ReadOnlyMemory<uint8>> =
    fun st ->
        if st.Off + n > st.Buf.Length then errBufOverflow st
        else
            let mem = st.Buf.Slice(st.Off, n)
            ok mem { st with Off = st.Off + n }

let takeAllMem : P<ReadOnlyMemory<uint8>> =
    fun st ->
        let mem = st.Buf.Slice(st.Off)
        ok mem { st with Off = st.Buf.Length }

let remainder : P<int> =
    fun st -> ok (st.Buf.Length - st.Off) st

let runOnSubSlice (n:int) (p: P<'a>) : P<'a> =
    fun st ->
        if st.Off + n > st.Buf.Length then errBufOverflow st
        else
            let subSt = { Buf = st.Buf.Slice(st.Off, n); Off = 0 }
            match p subSt with
            | Ok (a, _) ->
                 ok a { st with Off = st.Off + n }
            | Error e ->
                 Error { e with Pos = st.Off + e.Pos }

let parseAll (p: P<'a>) : P<'a list> =
    fun st ->
        let rec loop st acc =
             if st.Off >= st.Buf.Length then
                 ok (List.rev acc) st
             else
                 match p st with
                 | Ok (x, stNext) -> loop stNext (x::acc)
                 | Error e -> Error e
        loop st []
