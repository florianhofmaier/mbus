module Mbus.Frames.Tests.Layers.TestHelpers

open System
open Mbus.BaseParsers.Core

let createState buf =
    { Buf = ReadOnlyMemory<byte>(buf); Off = 0 }

let runParserErr p st0 =
    match p st0 with
    | Ok (tpl, _) -> failwithf $"Unexpected success: %A{tpl}"
    | Error e -> e.Pos, e.Msg, e.Ctx

let runParserOk p st0 =
    match p st0 with
    | Ok (res, st) -> res, st.Off
    | Error e -> failwithf $"Unexpected error: %A{e}"
