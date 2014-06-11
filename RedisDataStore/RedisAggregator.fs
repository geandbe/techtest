namespace BlurocketTest

open System
open NServiceKit.Redis
open NServiceKit.Redis.Generic
open System.Collections.Generic
open Blurocket.TechTestShared
    
module RedisAggregator =

    [<CLIMutable>]
    type Analytics = { LastId: int64; Total: int;  Max: int; Mean: double; Variance: double; MeanLastMinute: double}

    let windowMeanSeconds = 60.

    // Avoid boxing when comparing DateTimes
    let inline cmp<'a when 'a :> IComparable<'a>> (x:'a) (y:'a) = x.CompareTo y
    let inline (=.) x y = cmp x y = 0
    let inline (>.) x y = cmp x y > 0
    let inline (<.) x y = cmp x y < 0
    let inline (>=.) x y = cmp x y >= 0
    let inline (<=.) x y = cmp x y <= 0

    type DataStoreUpdater private () =
        static let client = new RedisClient()
        static let window = new List<DataItem>() 
                
        static member Store = client
        static member MeanWindow = window

        static member private FindMovingAverage (observation: DataItem) =
            let cutOff = observation.Created.AddSeconds(- windowMeanSeconds)
            let firstToStay = DataStoreUpdater.MeanWindow.FindIndex(fun x -> x.Created >. cutOff) in
                if firstToStay <> -1 then DataStoreUpdater.MeanWindow.RemoveRange(0, firstToStay)
            DataStoreUpdater.MeanWindow.Add observation
            DataStoreUpdater.MeanWindow |> List.ofSeq |> List.averageBy (fun x -> (float)x.Amount)

        static member InitAnalytics (forceFlush: bool) =
            if forceFlush then
                DataStoreUpdater.Store.FlushAll()
                DataStoreUpdater.Store.Set<Analytics>("urn:blurocket:analytics", { LastId=0L; Total=0;  Max=0; Mean=0.0; Variance=0.0; MeanLastMinute=0.0}) |> ignore
            elif not (DataStoreUpdater.Store.ContainsKey "urn:blurocket:analytics") then
                failwith ("Underlying Redis DataStore is uninitialized; rerun with forcedFlush argument")

        static member Aggregate (dataItem: DataItem) =
            let currentAnalytics = DataStoreUpdater.Store.Get<Analytics> "urn:blurocket:analytics"
            if dataItem.MessageId <> currentAnalytics.LastId + 1L then
                failwith (String.Format("Stream sequence mismatch: expected {0} vs. arrived {1}", currentAnalytics.LastId + 1L, dataItem.MessageId))
            
            // Mean and Variance are computed below following numerically stable algorithm described in
            // Donald E. Knuth (1998). The Art of Computer Programming, vol 2: Seminumerical Algorithms, 3rd ed., p. 232
            let floatAmount = float dataItem.Amount in
            let delta = floatAmount - currentAnalytics.Mean in
            let newMean = currentAnalytics.Mean + delta / (float dataItem.MessageId) in
            DataStoreUpdater.Store.Set<Analytics>("urn:blurocket:analytics", {
                LastId = dataItem.MessageId;
                Total = currentAnalytics.Total + dataItem.Amount;
                Max = max currentAnalytics.Max dataItem.Amount;
                Mean = newMean;
                Variance = currentAnalytics.Variance + delta*(floatAmount - newMean);
                MeanLastMinute = DataStoreUpdater.FindMovingAverage dataItem }) |> ignore
