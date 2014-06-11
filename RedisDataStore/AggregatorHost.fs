namespace BlurocketTest
module AggregatorHost =

    open System
    open System.Threading
    open RedisAggregator
    open System.Text
    open Blurocket.TechTestShared
    open ZMQ

    [<EntryPoint>]
    let main argv =
        let isForcedFlush = argv.Length = 0 || argv.[0].ToLowerInvariant().CompareTo("forcedflush") = 0
        use disposable = DataStoreProcessor.Client
        DataStoreProcessor.InitAnalytics isForcedFlush

        // for now - just dumb sync queue read
        printfn "Subscribing to 0MQ ORDERS stream... Press 'Q' to Quit"
        use context = new ZMQ.Context()
        use socket = context.Socket ZMQ.SocketType.SUB
        socket.Connect @"tcp://127.0.0.1:4567"
        socket.Subscribe("ORDERS", Encoding.Unicode)

        let rec consumeDataTick() : unit =
            let topic = socket.Recv(Encoding.Unicode)
            let objectData = socket.Recv()
            if objectData <> null then
                let dataItem = SerializationHelper.DeserializeFromBinary<DataItem> objectData
                if dataItem.MessageId % 100L = 0L then
                    printfn "Data Item with Id %i has being received" dataItem.MessageId
                DataStoreProcessor.ProcessDataItem dataItem
            let spinWait = new SpinWait()
            spinWait.SpinOnce()
            if Console.KeyAvailable then
                match Console.ReadKey().Key with
                | ConsoleKey.Q -> ()
                | _ -> consumeDataTick()
            else
                consumeDataTick()

        consumeDataTick()

        0 // return an integer exit code


