module Frames.Tests.Layers.EllTests

// open Mbus.Frames.Layers
// open Mbus.Frames.Tests.Layers.Helpers
// open Xunit
//
// [<Fact>]
// let ``parse Ell with EllShort bytes return Some EllShort`` () =
//     runParser Ell.parseOpt ellShort (ellShortParsed |> Short |> Some)
//
// [<Fact>]
// let ``parse Ell with TplLong return None`` () =
//     runParser Ell.parseOpt tplLongRsp None
//
// [<Fact>]
// let ``parse Ell with empty buffer return error`` () =
//     runParserErr Ell.parseOpt emptyBuf 0 "unexpected end of buffer"
//
// [<Fact>]
// let ``parse Ell with incomplete EllShort return error`` () =
//     let buf = ellShort.Slice(0, 2)
//     runParserErr Ell.parseOpt buf 2 "unexpected end of buffer"