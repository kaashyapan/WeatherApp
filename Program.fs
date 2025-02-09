open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Oxpecker
open Oxpecker.ViewEngine
open WeatherApp.templates
open WeatherApp.Models
open WeatherApp.templates.shared
open StarFederation.Datastar
open StarFederation.Datastar.DependencyInjection
open System.Text.Json
open System.Text.Json.Serialization

[<Literal>]
let Message = "Hello world "

let htmlView' f (ctx: HttpContext) =
    f ctx |> layout.html ctx |> ctx.WriteHtmlView

let messageView' (ctx: HttpContext) =
    let sse = ctx.GetService<IDatastarServerSentEventService>()
    let signals = ctx.GetService<IDatastarSignalsReaderService>()

    let sseopts =
        ServerSentEventMergeFragmentsOptions(MergeMode = FragmentMergeMode.Append)

    task {
        let! mySignals = signals.ReadSignalsAsync<HomeSignalNull>()

        for i = 1 to Message.Length do
            let html = $"""{Message.Substring(0, Message.Length - i)}""" |> home.msgFragment
            do! (html, sseopts) |> sse.MergeFragmentsAsync

            do! Task.Delay(TimeSpan.FromMilliseconds(mySignals.Delay))

        return! ((home.msgFragment "Done"), sseopts) |> sse.MergeFragmentsAsync
    }
    :> Task


let counterView' (action: CounterAction) (ctx: HttpContext) =
    let sse = ctx.GetService<IDatastarServerSentEventService>()
    let signals = ctx.GetService<IDatastarSignalsReaderService>()


    task {
        let! mySignals = signals.ReadSignalsAsync<CounterSignalNull>()

        let counter =
            match action with
            | Incr -> mySignals.Count + 1
            | Decr -> mySignals.Count - 1

        return!
            { CounterSignal.Count = counter }
            |> JsonSerializer.Serialize
            |> sse.MergeSignalsAsync
    }
    :> Task


let weatherView' (ctx: HttpContext) =
    let sse = ctx.GetService<IDatastarServerSentEventService>()

    task {
        // Simulate asynchronous loading to demonstrate long rendering
        do! Task.Delay(500)

        let startDate = DateOnly.FromDateTime(DateTime.Now)

        let summaries =
            [ "Freezing"
              "Bracing"
              "Chilly"
              "Cool"
              "Mild"
              "Warm"
              "Balmy"
              "Hot"
              "Sweltering"
              "Scorching" ]

        let forecasts =
            [| for index in 1..5 do
                   { Date = startDate.AddDays(index)
                     TemperatureC = Random.Shared.Next(-20, 55)
                     Summary = summaries[Random.Shared.Next(summaries.Length)] } |]

        return! forecasts |> weather.data |> sse.MergeFragmentsAsync
    }
    :> Task

let endpoints =
    [ GET
          [ route "/" <| htmlView' home.html
            route "/messages" <| messageView'
            route "/counter" <| htmlView' counter.html
            route "/counter/incr" <| counterView' Incr
            route "/counter/decr" <| counterView' Decr
            route "/weather" <| htmlView' weather.html
            route "/weather/data" <| weatherView'
            route "/error" <| htmlView' error.html ] ]

let configureApp (appBuilder: WebApplication) =
    if appBuilder.Environment.IsDevelopment() then
        appBuilder.UseDeveloperExceptionPage() |> ignore
    else
        appBuilder.UseExceptionHandler("/error", true) |> ignore

    appBuilder.UseStaticFiles().UseAntiforgery().UseRouting().UseOxpecker(endpoints)
    |> ignore

let configureServices (services: IServiceCollection) =
    let options =
        JsonFSharpOptions
            .Default()
            .WithSkippableOptionFields(SkippableOptionFields.Always, deserializeNullAsNone = true)
            .ToJsonSerializerOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)

    services
        .AddRouting()
        .AddLogging(fun builder -> builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning) |> ignore)
        .AddDatastar()
        .AddAntiforgery()
        .AddOxpecker()
    |> ignore


[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
