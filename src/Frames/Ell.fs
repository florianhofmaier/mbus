namespace Mbus.Frames.Layers

// open Mbus.BaseParsers.BinaryParsers
// open Mbus.BaseParsers.Core
//
// type EllShort = { CcField: uint8; AccField: uint8; }
// type Ell =
//     | Short of EllShort
//
// module Ell =
//     open Mbus.BaseParsers
//
//     let ciShort = 0x8Cuy
//
//     let getLen ell =
//         match ell with
//         | Ell.Short _ -> 3
//
//     let private parseShort : Parser<EllShort> = parser {
//         do! skip 1
//         let! cc = parseU8
//         let! acc = parseU8
//         return { CcField = cc; AccField = acc }
//     }
//
//     let parseOpt : Parser<Ell option> = parser {
//         let! ci = peekU8
//         match ci with
//         | _ when ci = ciShort ->
//             return! parseShort |>> Short |>> Some
//         | _ -> return None
//     }