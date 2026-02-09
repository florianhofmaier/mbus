module Mbus.Records.DataInfoBlocks.DibWriter

open Mbus
open Mbus.BaseWriters.BinaryWriters
open Mbus.BaseWriters.Core
open Mbus.Records

let extPayload (stNum: int, tariff: int, subUnit: int) (i: int) : byte =
    StorageNumber.toDife stNum i
    ||| Tariff.toDife tariff i
    ||| SubUnit.toDife subUnit i

let private neededExtBytes (stNum: int) (tariff: int) (subUnit: int) : int =
    let rec loop i lastNeeded =
        if i >= InfoBlock.maxExtBytes then lastNeeded
        else
            let payload = extPayload (stNum, tariff, subUnit) i
            let needed = if (payload &&& 0x7Fuy) <> 0uy then i + 1 else lastNeeded
            loop (i + 1) needed
    loop 0 0

type private DibSpec =
    { FuncField: MbusFunctionField
      StNum: int
      Tariff: int
      SubUnit: int
      ExtCount: int
      Val: MbusValue }

let private writeDif dibSpec: Writer<unit> =
    fun st0 ->
        let extBit = if dibSpec.ExtCount > 0 then 0x80uy else 0uy
        let difByte =
            extBit |||
            StorageNumber.toDif dibSpec.StNum |||
            FunctionField.toDif dibSpec.FuncField |||
            MbusValue.toDif dibSpec.Val

        writeU8 difByte st0

let private writeExtByte (dibSpec: DibSpec) (i: int) : byte =
    let moreFollows = i < dibSpec.ExtCount - 1
    let payload = extPayload (dibSpec.StNum, dibSpec.Tariff, dibSpec.SubUnit) i
    if moreFollows then payload ||| 0x80uy
    else payload &&& 0x7Fuy

let private writeDife dibSpec : Writer<unit> =
    fun st0 ->
        if dibSpec.ExtCount = 0 then
            Ok ((), st0)
        else
            let seed = dibSpec
            InfoBlock.writeExt seed writeExtByte st0

let writeDib fn stNum tariff subUnit value  : Writer<unit> =
    writer {
        let sn, tn, su = StorageNumber.value stNum, Tariff.value tariff, SubUnit.value subUnit
        let extByteCount = neededExtBytes sn tn su
        let dibSpec = { FuncField = fn; StNum = sn; Tariff = tn; SubUnit = su; ExtCount = extByteCount; Val = value }
        do! writeDif dibSpec
        do! writeDife dibSpec
    }
