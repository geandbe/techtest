namespace BlurocketTest

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

open System.Collections.Generic

[<TestClass>]
type GenericListSlicerTests() =
    let mutable ll: List<RedisAggregator.DataItemProc> = null 
    member val TestContext: TestContext = null with get,set

    [<TestInitialize>]
    member x.TestInit() =
        ll <- new List<RedisAggregator.DataItemProc>()
        for i in 1..3 do ll.Add({Created=DateTime.Now; AmountMod=(float)i})
    
    [<TestMethod>]
    member x.``Slice_all_is_invariant``() =
        let slice = ll.[0..2]
        x.TestContext.WriteLine("Before: {0}\nAfter: {1}", (sprintf "%A" ll), (sprintf "%A" slice))
        Assert.IsTrue(ll.TrueForAll (fun x -> slice.Exists(fun s -> s.Created = x.Created)))

    [<TestMethod>]
    member x.``Slice_to_rightmost_is_invariant``() =
        let slice = ll.[..2]
        x.TestContext.WriteLine("Before: {0}\nAfter: {1}", (sprintf "%A" ll), (sprintf "%A" slice))
        Assert.IsTrue(ll.TrueForAll (fun x -> slice.Exists(fun s -> s.Created = x.Created)))

    [<TestMethod>]
    member x.``Slice_from_leftmost_is_invariant``() =
        let slice = ll.[0..]
        x.TestContext.WriteLine("Before: {0}\nAfter: {1}", (sprintf "%A" ll), (sprintf "%A" slice))
        Assert.IsTrue(ll.TrueForAll (fun x -> slice.Exists(fun s -> s.Created = x.Created)))

    [<TestMethod>]
    member x.``Slice_beyond_leftmost_yields_leftmost``() =
        let slice = ll.[..0]
        x.TestContext.WriteLine("Before: {0}\nAfter: {1}", (sprintf "%A" ll), (sprintf "%A" slice))
        Assert.AreEqual(1, slice.Count)
        Assert.IsTrue(slice.Exists (fun x -> x.AmountMod = 1.0))

    [<TestMethod>]
    member x.``Slice_partial_to_right_is_correct``() =
        let slice = ll.[..1]
        x.TestContext.WriteLine("Before: {0}\nAfter: {1}", (sprintf "%A" ll), (sprintf "%A" slice))
        Assert.AreEqual(2, slice.Count)
        Assert.IsTrue(slice.Exists (fun x -> x.AmountMod = 1.0) && slice.Exists (fun x -> x.AmountMod = 2.0))

    [<TestMethod>]
    member x.``Slice_partial_from_left_is_correct``() =
        let slice = ll.[1..]
        x.TestContext.WriteLine("Before: {0}\nAfter: {1}", (sprintf "%A" ll), (sprintf "%A" slice))
        Assert.AreEqual(2, slice.Count)
        Assert.IsTrue(slice.Exists (fun x -> x.AmountMod = 3.0) && slice.Exists (fun x -> x.AmountMod = 2.0))

    [<TestMethod>]
    member x.``Slice_above_rightmost_yields_rightmost``() =
        let slice = ll.[2..]
        x.TestContext.WriteLine("Before: {0}\nAfter: {1}", (sprintf "%A" ll), (sprintf "%A" slice))
        Assert.AreEqual(1, slice.Count)
        Assert.IsTrue(slice.Exists (fun x -> x.AmountMod = 3.0))

    [<TestCleanup>]
    member x.TestCleanup() =
        ll <- null