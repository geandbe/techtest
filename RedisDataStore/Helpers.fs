namespace BlurocketTest

open System
// Nice to have ability to slice List<T>, in this case List<DataItemProc>;
// allows for fancy list.[[<lower>]..[<upper>]] notation without perf penalty
[<AutoOpen>]
module ListTSlicer =

    type System.Collections.Generic.List<'a> with
        member this.GetSlice(lower : int option, upper : int option) =
            match lower, upper with
            | Some(lower), Some(upper) -> this.GetRange(lower,(upper - lower + 1))
            | Some(lower), None -> this.GetRange(lower,(this.Count - lower))
            | None, Some(upper) -> this.GetRange(0, (upper+1))
            | None, None -> this

[<AutoOpen>]
module Helpers =
    // Avoid boxing when comparing DateTimes
    let inline cmp<'a when 'a :> IComparable<'a>> (x:'a) (y:'a) = x.CompareTo y
    let inline (=.) x y = cmp x y = 0
    let inline (>.) x y = cmp x y > 0
    let inline (<.) x y = cmp x y < 0
    let inline (>=.) x y = cmp x y >= 0
    let inline (<=.) x y = cmp x y <= 0
