namespace ViewHubHost

open System

open Owin
open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs
open Microsoft.Owin.Hosting
open Microsoft.Owin.Cors

open ServiceStack.Redis

open EkonBenefits.FSharp.Dynamic

open BlurocketTest.RedisAggregator

type ViewHub() =
    inherit Hub()


module Kick_In =

    let startup (app: IAppBuilder) =
            app.UseCors CorsOptions.AllowAll |> ignore
            app.MapSignalR() |> ignore

    let updateA newAnalytics =
        GlobalHost.ConnectionManager.GetHubContext<ViewHub>().Clients.All?UpdateAnalytics newAnalytics

    let updateM newMax =
        GlobalHost.ConnectionManager.GetHubContext<ViewHub>().Clients.All?UpdateMax newMax

    [<EntryPoint>]
    let main argv =
        let hostUrl = @"http://localhost:8080"
        use app = WebApp.Start(hostUrl,startup)
        use redisClient = new RedisClient()
        let typedClient = redisClient.As<String>()
        let eventQ = typedClient.Lists.["urn:blurocket:events"]
        printfn "Press a key to continue..."
        Console.ReadLine() |> ignore
        printfn "SignalR F# ViewHub is running on %s" hostUrl
        updateM (redisClient.Get<int> "urn:blurocket:max") // for restart

        while true do
           match (typedClient.BlockingDequeueItemFromList(eventQ, Nullable())) with
           | "analytics" -> updateA (redisClient.Get<Analytics> "urn:blurocket:analytics")
           | "max" -> updateM (redisClient.Get<int> "urn:blurocket:max")
           | _ -> failwith "Unknown EventStore descriptor"

        0 // return an integer exit code
