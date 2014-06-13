namespace BlurocketTest

open System
open System.Threading
open NServiceKit.Redis
open NServiceKit.Redis.Generic
open System.Collections.Generic
open MathNet.Numerics
open MathNet.Numerics.Statistics 

open Blurocket.TechTestShared

[<AutoOpen>]
module RedisAggregator =

    [<CLIMutable>]
    type DataItemProc = { Created: DateTime; AmountMod: double } // sliding window item type

    [<CLIMutable>]
    type Analytics = { LastId: int64; Total: int;  Max: int; WindowMean: double; WindowStdDev: double; }

    [<Literal>] // Hardcoded for simplicity
    let meanWindowWidthSeconds = 60.

    let zeroState = { LastId=0L; Total=0;  Max=0; WindowMean=0.0; WindowStdDev=0.0 }

    // Avoid boxing when comparing DateTimes
    let inline cmp<'a when 'a :> IComparable<'a>> (x:'a) (y:'a) = x.CompareTo y
    let inline (=.) x y = cmp x y = 0
    let inline (>.) x y = cmp x y > 0
    let inline (<.) x y = cmp x y < 0
    let inline (>=.) x y = cmp x y >= 0
    let inline (<=.) x y = cmp x y <= 0
    
    // Avoid costly type conversions in tight loop
    let inline intern (observation: DataItem) : DataItemProc = { Created = observation.Created; AmountMod = double observation.Amount }
    
    let monitor = new Object() // sync data window access over this lock from Rx-driven and main threads

    type DataStoreUpdater private () =
        static let client = new RedisClient()
        static let window = new List<DataItemProc>()
        static let mutable cache =  zeroState
        static let mutable lastPersisted = -1L
                
        static member Handle = client
        static member MeanWindow = window
        static member Cache with get() = cache // keeps analytics have being persisted into Redis
                            and set(v) = cache <- v

        static member private Statistics idxLast =
            // Mean and StandardDeviation of a period sliding window may be computed using a numerically stable algorithm described,
            // for example, in Donald E. Knuth (1998). The Art of Computer Programming, vol 2: Seminumerical Algorithms, 3rd ed., p. 232,
            // also http://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#On-line_algorithm
            //
            //let statistics (ss: double list) =
            //    match ss.Length with
            //    | 0 -> failwith "Empty argument"
            //    | 1 -> ((List.head ss), 0.0)
            //    | _ -> ss.Tail |> List.fold (fun (m, v, k) x ->
            //                                        let mk = m + (x - m)/k in (mk, v + (x - m)*(x - mk), k + 1.)) ((List.head ss), 0.0, 2.0)
            //                        |> fun (a,b,c) -> (a,sqrt(b/(c - 1.0)))
            //
            // However, it came out using MathNet.Numerics library is substantially superior performance-wise, so the above was dropped

            // time pivot btw potentially expired window items and more recent ones
            let cutOff = DataStoreUpdater.MeanWindow.[idxLast].Created.AddSeconds(- meanWindowWidthSeconds)
            let firstToStay = DataStoreUpdater.MeanWindow.FindIndex(fun x -> x.Created >. cutOff)
            // always exists at least one such item, hence firstToStay <> -1 is invariant
            let result =
                if firstToStay = idxLast then
                    (DataStoreUpdater.MeanWindow.[idxLast].AmountMod, 0.0) // (Mean, StdDev) for single element window
                elif firstToStay > idxLast then
                    failwith "Wrong state of MeanWindow" // kinda Assertion, should NEVER happend!
                else // firstToStay < idxLast
                    let ds = DescriptiveStatistics(DataStoreUpdater.MeanWindow.[firstToStay..idxLast].ConvertAll(fun x -> x.AmountMod)) in
                    (ds.Mean, ds.StandardDeviation)
            if firstToStay > 0 then // indicates a range of one or more expired observations exists near left 
                lock monitor (fun () -> DataStoreUpdater.MeanWindow.RemoveRange(0, firstToStay))    
            result
                
        static member InitAnalytics forceFlush =
            if forceFlush then
                DataStoreUpdater.Handle.FlushAll()
                DataStoreUpdater.Handle.Set<Analytics>("urn:blurocket:analytics", zeroState) |> ignore
            elif not (DataStoreUpdater.Handle.ContainsKey "urn:blurocket:analytics") then
                failwith ("Underlying Redis DataStore is uninitialized; rerun with forcedFlush argument")
            // For both forced Flush and Restart recover last persisted cache
            DataStoreUpdater.Cache <- DataStoreUpdater.Handle.Get<Analytics> "urn:blurocket:analytics"
            lastPersisted <- DataStoreUpdater.Cache.LastId

        static member Append (nextOrder: DataItem) = // happens on each data item arrival
            //Console.WriteLine("Append order {0} to window of {1} length", nextOrder.MessageId, DataStoreUpdater.MeanWindow.Count);
            if nextOrder.MessageId <> DataStoreUpdater.Cache.LastId + 1L then // ensure we do not miss pieces of data stream
                failwith (String.Format("Stream sequence mismatch: expected {0} vs. arrived {1}", DataStoreUpdater.Cache.LastId + 1L, nextOrder.MessageId))

            lock monitor (fun() -> DataStoreUpdater.MeanWindow.Add (intern nextOrder))
            
            if nextOrder.Amount > DataStoreUpdater.Cache.Max then
                //!!! Signal Max Change !!!
                ()

            DataStoreUpdater.Cache <- {
                DataStoreUpdater.Cache with
                    LastId = nextOrder.MessageId;
                    Total = DataStoreUpdater.Cache.Total + nextOrder.Amount;
                    Max = max DataStoreUpdater.Cache.Max nextOrder.Amount; }
        
        static member Aggregate() = // happens with a frequency suitable for UI
            let lastIdx = ref 0
            let cacheCopy = ref zeroState

            if lastPersisted < DataStoreUpdater.Cache.LastId then
                lock monitor (fun () ->
                            cacheCopy :=  DataStoreUpdater.Cache
                            lastIdx := DataStoreUpdater.MeanWindow.Count - 1)
                //Console.WriteLine("Aggregate lastPersisted Id {0} vs Cache.LastId {1}", lastPersisted, DataStoreUpdater.Cache.LastId);
                lastPersisted <- DataStoreUpdater.Cache.LastId
                match !lastIdx with
                | _ when !lastIdx < 0 -> () // Empty mean window, zilch to aggregate
                | _ -> let mean, stddev = DataStoreUpdater.Statistics !lastIdx
                       cacheCopy := { !cacheCopy  with WindowMean=mean; WindowStdDev=stddev }
                DataStoreUpdater.Handle.Set<Analytics>("urn:blurocket:analytics", !cacheCopy) |> ignore
                //Console.WriteLine("***Persisted: {0}", lastPersisted)
                