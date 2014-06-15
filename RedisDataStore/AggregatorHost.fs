module BlurocketTest.AggregatorHost

open System
open System.Threading
open System.Text
open ZMQ
open System.Reactive.Linq
open System.Reactive.Concurrency
open System.Reactive.PlatformServices

open RedisAggregator
open Blurocket.TechTestShared

[<EntryPoint>]
let main argv =
    use __ = EventStoreUpdater.Handle
    use context = new ZMQ.Context()
    use socket = context.Socket ZMQ.SocketType.SUB

    // for now - just dumb sync queue read in a tight loop
    let rec consumeDataItem() : unit =
        let topic = socket.Recv(Encoding.Unicode)
        let objectData = socket.Recv()
        if objectData <> null then
            SerializationHelper.DeserializeFromBinary<DataItem> objectData |> EventStoreUpdater.Append
        let spinWait = new SpinWait() // give others some breath
        spinWait.SpinOnce()
        if Console.KeyAvailable then
            match Console.ReadKey().Key with
            | ConsoleKey.Q -> ()
            | _ -> consumeDataItem()
        else
            consumeDataItem()

    EventStoreUpdater.InitAnalytics (argv.Length = 0 || argv.[0].ToLower().CompareTo("forcedflush") = 0)
    printfn "Subscribing to 0MQ ORDERS stream... Press 'Q' to Quit"
    socket.Connect @"tcp://127.0.0.1:4567"
    socket.Subscribe("ORDERS", Encoding.Unicode)

    // 25 frames per sec. is close to classic cinema 1/24
    let aggregateFreqMsec = 40.
    let stopwatch = Observable.Interval(TimeSpan.FromMilliseconds(aggregateFreqMsec), Scheduler.Default)
    use handle = stopwatch.Subscribe(fun _ -> EventStoreUpdater.Aggregate())
    consumeDataItem()
    // Last chance aggregation before exit with a hope to restart
    EventStoreUpdater.Aggregate()

    0


