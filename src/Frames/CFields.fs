namespace Mbus.Frames

module CField =
    let response = 0x08uy
    let setAcd cField = cField ||| 0x20uy
    let clearAcd cField = cField &&& 0xDFuy
    let setDfc cField = cField ||| 0x10uy
    let clearDfc cField = cField &&& 0xEFuy

