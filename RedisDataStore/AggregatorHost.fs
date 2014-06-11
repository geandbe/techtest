namespace BlurocketTest
module AggregatorHost =

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
        use __ = DataStoreUpdater.Store
        use context = new ZMQ.Context()
        use socket = context.Socket ZMQ.SocketType.SUB

        // for now - just dumb sync queue read
        let rec consumeDataTick() : unit =
            let topic = socket.Recv(Encoding.Unicode)
            let objectData = socket.Recv()
            if objectData <> null then
                let dataItem = SerializationHelper.DeserializeFromBinary<DataItem> objectData
//                if dataItem.MessageId % 100L = 0L then
//                    printfn "Data Item with Id %i has being received" dataItem.MessageId
                DataStoreUpdater.Aggregate dataItem
//            let spinWait = new SpinWait()
//            spinWait.SpinOnce()
            if Console.KeyAvailable then
                match Console.ReadKey().Key with
                | ConsoleKey.Q -> ()
                | _ -> consumeDataTick()
            else
                consumeDataTick()

        DataStoreUpdater.InitAnalytics (argv.Length = 0 || argv.[0].ToLower().CompareTo("forcedflush") = 0)
        printfn "Subscribing to 0MQ ORDERS stream... Press 'Q' to Quit"
        socket.Connect @"tcp://127.0.0.1:4567"
        socket.Subscribe("ORDERS", Encoding.Unicode)

        // 25 frames per sec. is close to classic cinema 1/24
        let persistFreqMs = 40.
        let stopwatch = Observable.Interval(TimeSpan.FromMilliseconds(persistFreqMs), Scheduler.Default)
        use handle = stopwatch.Subscribe(fun _ -> DataStoreUpdater.Persist())
        consumeDataTick()
        // Provision here to persist last Cache!!!
        DataStoreUpdater.Persist()

        0


