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

    type DataStoreProcessor private () =
        static let client = new RedisClient()
        static let dataItems = client.As<DataItem>()
        static let window: IRedisList<DataItem> = dataItems.Lists.["urn:blurocket:meanWindow"]
                
        static member Client = client
        
        static member MeanWindow = window

        static member private MovingAverage (observation: DataItem) =
            let oldWindow = window.GetAll()
            let cutOff = observation.Created.AddSeconds(- windowMeanSeconds)
            let newWindow,toDelete = oldWindow |> List.ofSeq |> List.partition (fun x -> x.Created >. cutOff)
            for i = 0 to (toDelete.Length - 1) do
                window.Dequeue() |> ignore
            window.Enqueue observation
            observation::newWindow |> List.averageBy (fun x -> (float)x.Amount)

        static member InitAnalytics (forceFlush: bool) =
            if forceFlush then
                DataStoreProcessor.Client.FlushAll()
                DataStoreProcessor.Client.Set<Analytics>("urn:blurocket:analytics", { LastId=0L; Total=0;  Max=0; Mean=0.0; Variance=0.0; MeanLastMinute=0.0}) |> ignore
                DataStoreProcessor.MeanWindow.Clear()
            elif not (DataStoreProcessor.Client.ContainsKey "urn:blurocket:analytics" && DataStoreProcessor.MeanWindow.Count >= 0) then
                failwith ("Underlying Redis DataStore is uninitialized; rerun with forcedFlush argument")

        static member ProcessDataItem (dataItem: DataItem) =
            let currentAnalytics = DataStoreProcessor.Client.Get<Analytics> "urn:blurocket:analytics"
            if dataItem.MessageId <> currentAnalytics.LastId + 1L then
                failwith (String.Format("Stream sequence mismatch: expected {0} vs. arrived {1}", currentAnalytics.LastId + 1L, dataItem.MessageId))
            
            // Mean and Variance are computed below following numerically stable algorithm described in
            // Donald E. Knuth (1998). The Art of Computer Programming, vol 2: Seminumerical Algorithms, 3rd ed., p. 232
            let floatAmount = float dataItem.Amount in
            let delta = floatAmount - currentAnalytics.Mean in
            let newMean = currentAnalytics.Mean + delta / (float dataItem.MessageId) in
            DataStoreProcessor.Client.Set<Analytics>("urn:blurocket:analytics", {
                LastId = dataItem.MessageId;
                Total = currentAnalytics.Total + dataItem.Amount;
                Max = max currentAnalytics.Max dataItem.Amount;
                Mean = newMean;
                Variance = currentAnalytics.Variance + delta*(floatAmount - newMean);
                MeanLastMinute = DataStoreProcessor.MovingAverage dataItem }) |> ignore
