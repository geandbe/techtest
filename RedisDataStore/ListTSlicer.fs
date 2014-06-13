// Nice to have ability to slice List<T>

[<AutoOpen>]
module BlurocketTest.ListTSlicer

type System.Collections.Generic.List<'a> with
    member this.GetSlice(lower : int option, upper : int option) =
        match lower, upper with
        | Some(lower), Some(upper) -> this.GetRange(lower,(upper - lower + 1))
        | Some(lower), None -> this.GetRange(lower,(this.Count - lower))
        | None, Some(upper) -> this.GetRange(0, (upper+1))
        | None, None -> this
