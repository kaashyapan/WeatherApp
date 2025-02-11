open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Oxpecker
open WeatherApp.templates
open WeatherApp.Models
open WeatherApp.templates.shared
open StarFederation.Datastar
open System.Text.Json
open System.Text.Json.Serialization
open DataStarExtensions

[<Literal>]
let Message = "Hello world "

let jsonOptions =
    JsonFSharpOptions
        .Default()
        .WithUnionUnwrapFieldlessTags()
        .WithSkippableOptionFields(SkippableOptionFields.Always, deserializeNullAsNone = true)
        .ToJsonSerializerOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)

let htmlView' f (ctx: HttpContext) =
    f ctx |> layout.html ctx |> ctx.WriteHtmlView

let messageView' (ctx: HttpContext) =
    let datastar = Datastar(ctx)

    let htmlopts =
        { MergeFragmentsOptions.defaults with
            MergeMode = Append }

    task {
        let! signals = datastar.Signals.ReadSignalsOrFail<HomeSignal>(jsonOptions)

        for i = 1 to Message.Length do
            let html = $"""{Message.Substring(0, Message.Length - i)}""" |> home.msgFragment
            do! datastar.WriteHtmlFragment(html, htmlopts)
            do! Task.Delay(TimeSpan.FromMilliseconds(signals.Delay))

        return! datastar.WriteHtmlFragment(home.msgFragment "Done", htmlopts)
    }
    :> Task


let counterView' (action: CounterAction) (ctx: HttpContext) =

    let datastar = Datastar(ctx)

    task {
        let! signals = datastar.Signals.ReadSignalsOrFail<CounterSignal>(jsonOptions)

        let counter =
            match action with
            | Incr -> signals.Count + 1
            | Decr -> signals.Count - 1

        return! { CounterSignal.Count = counter } |> datastar.MergeSignal
    }
    :> Task


let weatherView' (ctx: HttpContext) =

    let datastar = Datastar(ctx)

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

        return! forecasts |> weather.data |> datastar.WriteHtmlFragment
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

    services
        .AddRouting()
        .AddLogging(fun builder -> builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning) |> ignore)
        .AddAntiforgery()
        .AddOxpecker()
        .AddSingleton<IJsonSerializer>(SystemTextJsonSerializer(jsonOptions))
    |> ignore


[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
