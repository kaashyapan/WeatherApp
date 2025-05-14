open System
open System.IO
open System.Collections.Generic
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
open System.IO.Compression
open Microsoft.AspNetCore.ResponseCompression
open System.Threading.Channels

[<Literal>]
let Message = "Hello world "

let channel =
    Channel.CreateUnbounded<string>(
        UnboundedChannelOptions(SingleWriter = true, SingleReader = true, AllowSynchronousContinuations = true)
    )

let printCtx (ctx: HttpContext) =
    ctx.Request.Headers
    |> Seq.map (fun x -> printfn "Headers %A - %A" x.Key x.Value)
    |> Seq.toList
    |> ignore

    ctx.GetRequestUrl() |> printfn "Url - %A"
    printfn "Secret cookie - %A" (ctx.TryGetCookieValue("secret"))

let jsonOptions =
    JsonFSharpOptions
        .Default()
        .WithUnionUnwrapFieldlessTags()
        .WithSkippableOptionFields(SkippableOptionFields.Always, deserializeNullAsNone = true)
        .ToJsonSerializerOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)

let htmlView' f (ctx: HttpContext) = f ctx |> layout.html ctx |> ctx.WriteHtmlView

let messageView' (ctx: HttpContext) =
    let datastar = Datastar(ctx)

    let htmlopts = { MergeFragmentsOptions.defaults with MergeMode = Append }
    let reader = channel.Reader
    let writer = channel.Writer

    //Write async messages into the channel. Returns at the end of the loop
    let _readTask =
        task {
            let! signals = datastar.Signals.ReadSignalsOrFail<HomeSignal>(jsonOptions)

            for i = 1 to Message.Length do
                let str = $"""{Message.Substring(0, Message.Length - i)}"""
                do! writer.WriteAsync(str)
                printfn "%A" "Wrote channel"
                do! Task.Delay(TimeSpan.FromMilliseconds(signals.Delay))

            return ()
        }
        :> Task

    //Read async messages from the channel. Returns when the channel has no more msgs
    let _writeTask =
        task {
            while true do
                printfn "%A" "Awaiting channel"
                let! avble = reader.WaitToReadAsync()
                printfn "%A" "Readinging channel"

                match reader.TryRead() with
                | true, str ->
                    let html = str |> home.msgFragment
                    do! datastar.WriteHtmlFragment(html, htmlopts)
                | _ -> return! datastar.WriteHtmlFragment("Done" |> home.msgFragment, htmlopts)

        }
        :> Task

    // Return only when both tasks have completed
    [| _readTask; _writeTask |] |> Task.WhenAll

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

let loginView' (ctx: HttpContext) =

    let datastar = Datastar(ctx)

    task {
        let! r = ctx.BindForm<LoginForm>()
        // ctx |> printCtx
        // let ms = new MemoryStream()
        // ctx.Request.EnableBuffering()
        // do! ctx.Request.Body.CopyToAsync(ms)
        // ms.ToArray() |> System.Text.Encoding.UTF8.GetString |> printfn "Body - %A"
        // let! form = ctx.Request.ReadFormAsync()
        // form.Keys |> Seq.map (fun x -> printfn "%A" x) |> Seq.toList |> ignore
        let validateEmail (r: LoginForm) =
            if r.Email.ToLower().Contains("gmail") then
                r
            else
                { r with EmailError = Some "Email does not exist. Try a gmail address" }

        let validatePassword (r: LoginForm) =
            if r.Password.Length <= 6 then
                { r with PasswordError = Some "Password is too small. Minimum 6 chars" }
            else
                r

        let _r =
            { r with FormError = None; PasswordError = None; EmailError = None }
            |> validateEmail
            |> validatePassword

        printfn "%A" _r

        if (_r.EmailError.IsNone && _r.FormError.IsNone && _r.PasswordError.IsNone) then
            return! ctx |> login.ok |> datastar.WriteHtmlFragment
        else
            let r' = { _r with FormError = Some "Please fix form errors" }
            printfn "%A" r'
            return! ctx |> login.showForm r' |> datastar.WriteHtmlFragment
    }
    :> Task

let weatherView' (ctx: HttpContext) =

    let datastar = Datastar(ctx)

    task {
        // Simulate asynchronous loading to demonstrate long rendering
        do! Task.Delay(500)

        let startDate = DateOnly.FromDateTime(DateTime.Now)

        let summaries =
            [
                "Freezing"
                "Bracing"
                "Chilly"
                "Cool"
                "Mild"
                "Warm"
                "Balmy"
                "Hot"
                "Sweltering"
                "Scorching"
            ]

        let forecasts =
            [|
                for index in 1..5 do
                    {
                        Date = startDate.AddDays(index)
                        TemperatureC = Random.Shared.Next(-20, 55)
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    }
            |]

        return! forecasts |> weather.data |> datastar.WriteHtmlFragment
    }
    :> Task

let endpoints =
    [
        GET
            [
                route "/" <| htmlView' home.html
                route "/messages" <| messageView'
                route "/error" <| htmlView' error.html
            ]
        subRoute
            "/counter"
            [
                GET [ route "" <| htmlView' counter.html ]
                POST [ route "/incr" <| counterView' Incr; route "/decr" <| counterView' Decr ]
            ]
        subRoute
            "/login"
            [
                GET [ route "" <| htmlView' (login.html LoginForm.make) ]
                POST [ route "" <| loginView' ]

            ]
        subRoute "/weather" [ GET [ route "" <| htmlView' weather.html ]; GET [ route "/data" <| weatherView' ] ]
    ]

let configureApp (appBuilder: WebApplication) =
    if appBuilder.Environment.IsDevelopment() then
        appBuilder.UseDeveloperExceptionPage() |> ignore
    else
        appBuilder.UseExceptionHandler("/error", true) |> ignore

    appBuilder.UseResponseCompression().UseStaticFiles().UseAntiforgery().UseRouting().UseOxpecker(endpoints)
    |> ignore

let configureServices (services: IServiceCollection) =

    services
        .AddRouting()
        .AddLogging(fun builder -> builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning) |> ignore)
        .AddAntiforgery()
        .AddOxpecker()
        .AddSingleton<IJsonSerializer>(SystemTextJsonSerializer(jsonOptions))
        .AddResponseCompression(fun opts ->
            opts.EnableForHttps <- true

            opts.MimeTypes <-
                ResponseCompressionDefaults.MimeTypes
                |> Seq.append (
                    seq {
                        "image/svg+xml"
                        "text/event-stream"
                    }
                )

            opts.Providers.Add<BrotliCompressionProvider>()
            opts.Providers.Add<GzipCompressionProvider>()
        )

    |> ignore

    services.Configure<BrotliCompressionProviderOptions>(fun (opts: BrotliCompressionProviderOptions) ->
        opts.Level <- CompressionLevel.Fastest
    )
    |> ignore

    services.Configure<GzipCompressionProviderOptions>(fun (opts: GzipCompressionProviderOptions) ->
        opts.Level <- CompressionLevel.SmallestSize
    )
    |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
