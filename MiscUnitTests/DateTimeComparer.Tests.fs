namespace UnitTestProject1

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

open BlurocketTest

[<TestClass>]
type DateTimeComparerTests() =
    let mutable earlier = Unchecked.defaultof<DateTime> 
    let mutable later = Unchecked.defaultof<DateTime> 
    
    member val TestContext = Unchecked.defaultof<TestContext> with get,set

    [<TestInitialize>]
    member x.TestInit() =
        earlier <- DateTime.Now
        later <- earlier.AddMilliseconds(1.0)
    
    [<TestMethod>]
    member x.``Check_Equality``() =
        Assert.IsTrue(earlier =. earlier)
        Assert.IsFalse(earlier =. later)

    [<TestMethod>]
    member x.``Check_Greater``() =
        Assert.IsTrue(later >. earlier)
        Assert.IsFalse(earlier >. later)

    [<TestMethod>]
    member x.``Check_Less``() =
        Assert.IsTrue(earlier <. later)
        Assert.IsFalse(later <. earlier)

    [<TestMethod>]
    member x.``Check_Greater_Or_Eqial``() =
        Assert.IsTrue(later >=. earlier)
        Assert.IsFalse(earlier >=. later)
        Assert.IsTrue(later >=. later)

    [<TestMethod>]
    member x.``Check_Less_Or_Equal``() =
        Assert.IsTrue(earlier <=. later)
        Assert.IsFalse(later <=. earlier)
        Assert.IsTrue(earlier <=. earlier)
