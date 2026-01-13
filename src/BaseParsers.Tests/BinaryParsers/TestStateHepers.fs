module Mbus.BaseParsers.Tests.BinaryParsers.TestStateHelpers

open System
open Mbus.BaseParsers.Core

let create buf off =
    { Buf = ReadOnlyMemory buf; Off = off }

let empty =
    { Buf = ReadOnlyMemory([| |]); Off = 0 }