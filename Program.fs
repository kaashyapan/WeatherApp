﻿open System
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
open Oxpecker.Datastar
open System.IO.Compression
open Microsoft.AspNetCore.ResponseCompression
open System.Threading
open System.Threading.Tasks
open System.Threading.Channels
open IcedTasks
open type Microsoft.AspNetCore.Http.TypedResults

[<Literal>]
let Message = "Hello world"

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

    let htmlopts =
        { MergeFragmentsOptions.defaults with
            MergeMode = Append
            Selector = (ValueSome(Selector "#remote-text"))
            UseViewTransition = true
        }

    let reader = channel.Reader
    let writer = channel.Writer

    // Write async messages to the channel after delay
    let _readTask ctx =
        cancellableTask {
            return!
                // Use lambda to get cancellation token inside cancellableTask
                fun (ct: CancellationToken) ->
                    task {
                        let! signals = datastar.Signals.ReadSignalsOrFail<HomeSignal>(jsonOptions)
                        let mutable i = 0

                        while i < Message.Length && not ct.IsCancellationRequested do
                            let str = $"""{Message.Substring(0, Message.Length - i)}"""
                            do! writer.WriteAsync(str)
                            i <- i + 1
                            do! Task.Delay(TimeSpan.FromMilliseconds(signals.Delay))

                        return! writer.WriteAsync("Done!")
                    }
        }

    // Read async messages from the channel
    // Returns when the channel has no more msgs
    // Writes to html
    let _writeTask ctx =
        cancellableTask {
            return!
                // Use lambda to get cancellation token inside cancellableTask
                fun (ct: CancellationToken) ->
                    task {
                        let mutable avble = true

                        while avble && not ct.IsCancellationRequested do
                            try
                                let! _avble = reader.WaitToReadAsync(ct)
                                avble <- _avble

                                match reader.TryRead() with
                                | true, str ->
                                    let html = str |> home.msgFragment
                                    do! datastar.WriteHtmlFragment(html, htmlopts)
                                | _ -> avble <- false
                            with ex ->
                                avble <- false

                    }
        }

    // Return only when both tasks have completed
    [| _readTask ctx ctx.RequestAborted; _writeTask ctx ctx.RequestAborted |]
    |> Task.WhenAny
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
            return! datastar.ExecuteScript "submitLoginForm(`login-form-id`);"
        else
            let r' = { _r with FormError = Some "Please fix form errors" }
            printfn "%A" r'
            return! ctx |> login.showForm r' |> datastar.WriteHtmlFragment
    }
    :> Task

let loginAndRedirect (ctx: HttpContext) =
    task {
        let! r = ctx.BindForm<LoginForm>()
        // Create login Jwt / Cookie
        printfn "%A" r
        do! Task.Delay(2000)
        ctx.Response.Redirect("/", false)
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

#if DEBUG
let reloadHandler' (id: string) = if id = WeatherApp.Models.reloadToken then %StatusCode(200) else %StatusCode(205)
#endif

let endpoints =
    [
        GET
            [
                route "/" <| htmlView' home.html
                route "/messages" <| messageView'
                route "/error" <| htmlView' error.html
#if DEBUG
                routef "/pagereload/{%s}" <| reloadHandler'
#endif
            ]
        subRoute
            "/counter"
            [
                GET [ route "" <| htmlView' counter.html ]
                POST [ route "/incr" <| counterView' Incr; route "/decr" <| counterView' Decr ]
            ]
        subRoute
            "/signin"
            [
                GET [ route "" <| htmlView' (login.html LoginForm.make) ]
                PATCH [ route "" <| loginView' ]
                POST [ route "" <| loginAndRedirect ]
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
