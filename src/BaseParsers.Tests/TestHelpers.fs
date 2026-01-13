module Mbus.BaseParsers.Tests.TestHelpers

open FsUnit.Xunit
open Mbus.BaseParsers.Core

let runParser p st value pos  =
    let st0 = st
    match p st0 with
    | Ok (v, st1) ->
        v |> should equal value
        st1.Off |> should equal pos
    | Error e -> failwithf $"Unexpected: %A{e}"

let runFailingParser p st msg pos =
    let st0 = st
    match p st0 with
    | Error e ->
        e.Msg |> should equal msg
        e.Pos |> should equal pos
    | Ok _ -> failwith "Expected failure"
