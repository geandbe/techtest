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
    type DataItemProc = { Created: DateTime; AmountMod: double }

    [<CLIMutable>]
    type Analytics = { LastId: int64; Total: int;  Max: int; WindowMean: double; WindowStdDev: double; }

    [<Literal>] // Hardcoded for simplicity
    let windowMeanSeconds = 60.

    let zeroState = { LastId=0L; Total=0;  Max=0; WindowMean=0.0; WindowStdDev=0.0 }

    // Avoid boxing when comparing DateTimes
    let inline cmp<'a when 'a :> IComparable<'a>> (x:'a) (y:'a) = x.CompareTo y
    let inline (=.) x y = cmp x y = 0
    let inline (>.) x y = cmp x y > 0
    let inline (<.) x y = cmp x y < 0
    let inline (>=.) x y = cmp x y >= 0
    let inline (<=.) x y = cmp x y <= 0
    
    let inline raw2Proc (observation: DataItem) : DataItemProc = { Created = observation.Created; AmountMod = double observation.Amount }
    
    let monitor = new Object() // sync window access over it

    type DataStoreUpdater private () =
        static let client = new RedisClient()
        static let window = new List<DataItemProc>()
        static let mutable cache =  zeroState
        static let mutable lastPersisted = -1L
                
        static member Store = client
        static member MeanWindow = window
        static member Cache with get() = cache
                            and set(v) = cache <- v

        static member private Statistics idxLast : double*double =
            // Mean and StandardDeviation of a window may be computed with the numerically stable algorithm described in
            // Donald E. Knuth (1998). The Art of Computer Programming, vol 2: Seminumerical Algorithms, 3rd ed., p. 232,
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
            // However, it came out using MathNet.Numerics library is superior performance-wise

            // find potential expired heading
            let cutOff = DataStoreUpdater.MeanWindow.[idxLast].Created.AddSeconds(- windowMeanSeconds)
            let firstToStay = DataStoreUpdater.MeanWindow.FindIndex(fun x -> x.Created >. cutOff)
            // always exists at least one such item, so firstToStay <> -1
            let result =
                if firstToStay = idxLast then
                    (DataStoreUpdater.MeanWindow.[idxLast].AmountMod, 0.0) // (Mean, StdDev) for single element window
                elif firstToStay > idxLast then
                    failwith "Wrong state of MeanWindow"
                else // firstToStay < idxLast
                    let ds = DescriptiveStatistics(DataStoreUpdater.MeanWindow.[firstToStay..idxLast].ConvertAll(fun x -> x.AmountMod)) in
                    (ds.Mean, ds.StandardDeviation)
            if firstToStay > 0 then // there is a range of one or more expired observations on the left
                lock monitor (fun () -> DataStoreUpdater.MeanWindow.RemoveRange(0, firstToStay))    
            result
                

        static member InitAnalytics (forceFlush: bool) =
            if forceFlush then
                DataStoreUpdater.Store.FlushAll()
                DataStoreUpdater.Store.Set<Analytics>("urn:blurocket:analytics", zeroState) |> ignore
            elif not (DataStoreUpdater.Store.ContainsKey "urn:blurocket:analytics") then
                failwith ("Underlying Redis DataStore is uninitialized; rerun with forcedFlush argument")
            // For both forced Flush and Restart
            DataStoreUpdater.Cache <- DataStoreUpdater.Store.Get<Analytics> "urn:blurocket:analytics"
            lastPersisted <- DataStoreUpdater.Cache.LastId

        static member Append (nextOrder: DataItem) =
            Console.WriteLine("Append order {0} to window of {1} length", nextOrder.MessageId, DataStoreUpdater.MeanWindow.Count);
            if nextOrder.MessageId <> DataStoreUpdater.Cache.LastId + 1L then
                failwith (String.Format("Stream sequence mismatch: expected {0} vs. arrived {1}", DataStoreUpdater.Cache.LastId + 1L, nextOrder.MessageId))

            lock monitor (fun() -> DataStoreUpdater.MeanWindow.Add (raw2Proc nextOrder))
            
            if nextOrder.Amount > DataStoreUpdater.Cache.Max then
                //!!! Signal Max Change !!!
                ()

            DataStoreUpdater.Cache <- {
                DataStoreUpdater.Cache with
                    LastId = nextOrder.MessageId;
                    Total = DataStoreUpdater.Cache.Total + nextOrder.Amount;
                    Max = max DataStoreUpdater.Cache.Max nextOrder.Amount; }
        
        static member Aggregate() =
            let tailIdx = ref 0
            let cacheCopy = ref zeroState

            lock monitor (fun () ->
                            cacheCopy :=  DataStoreUpdater.Cache
                            tailIdx := DataStoreUpdater.MeanWindow.Count - 1)
            if lastPersisted < DataStoreUpdater.Cache.LastId then
                Console.WriteLine("Aggregate lastPersisted Id {0} vs Cache.LastId {1}", lastPersisted, DataStoreUpdater.Cache.LastId);
                lastPersisted <- DataStoreUpdater.Cache.LastId
                match !tailIdx with
                | _ when !tailIdx < 0 -> () // Empty window, no analitics
                | _ -> let mean, stddev = DataStoreUpdater.Statistics !tailIdx
                       cacheCopy := { !cacheCopy  with WindowMean=mean; WindowStdDev=stddev }
                DataStoreUpdater.Store.Set<Analytics>("urn:blurocket:analytics", !cacheCopy) |> ignore
            //Console.WriteLine("***Persisted: {0}", lastPersisted)
                