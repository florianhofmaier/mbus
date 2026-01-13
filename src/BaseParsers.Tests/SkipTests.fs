module Mbus.BaseParsers.Tests.SkipTests
//
//     open Xunit
//     open Mbus.BaseParsers.Tests
//
//     let runSkip n  = TestHelpers.runParser (skip n)
//     let runPU32Err n = TestHelpers.runFailingParser (skip n)
//
//     [<Fact>]
//     let ``skip one at first advances by one`` () =
//         runSkip 1 TestState.create () TestState.secondPos
//
//     [<Fact>]
//     let ``skip four at first advances by four`` () =
//         runSkip 4 TestState.create () TestState.fifthPos
//
//     [<Fact>]
//     let ``skip two at second advances by two`` () =
//         runSkip 2 TestState.createAtSecond () TestState.fourthPos
//
//     [<Fact>]
//     let ``skip zero at first does not advance`` () =
//         runSkip 0 TestState.create () TestState.firstPos
//
//     [<Fact>]
//     let ``skip one at last advances by one`` () =
//         runSkip 1 TestState.createAtLast () (TestState.lastPos + 1)
//
//     [<Fact>]
//     let ``skip on empty buffer fails`` () =
//         runPU32Err 1 TestState.empty "unexpected end of buffer" 0
//
//     [<Fact>]
//     let ``skip at end of buffer fails`` () =
//         runPU32Err 1 TestState.createAtEnd "unexpected end of buffer" (TestState.lastPos + 1)
//
//     [<Fact>]
//     let ``skip too many fails`` () =
//         runPU32Err 100 TestState.create "unexpected end of buffer" (TestState.lastPos + 1)
