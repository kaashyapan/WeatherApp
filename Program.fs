open System
open System.IO.Compression
open System.Text.Json.Serialization
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.ResponseCompression
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Routing.Internal
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Oxpecker
open Frank.Builder
open WeatherApp.templates
open WeatherApp.Extensions
open WeatherApp.Models
open WeatherApp.templates.shared
open StarFederation.Datastar
open DataStarExtensions

[<Literal>]
let Message = "Hello world "

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

let home =
    resource "/" {
        name "Home"
        get (htmlView' home.html)
    }

let messages =
    resource "messages" {
        name "Messages"
        get messageView'
    }

let errorPage =
    resource "error" {
        name "Error"
        get (htmlView' error.html)
    }

let counter =
    resource "counter/{action:regex(^(incr|decr)$)?}" {
        name "Counter"
        get (htmlView' counter.html)

        post (fun (ctx: HttpContext) ->
            match ctx.TryGetRouteValue("action") with
            | Some "incr" -> counterView' Incr ctx
            | Some "decr" -> counterView' Decr ctx
            | _ -> htmlView' error.html ctx
        )
    }

let login =
    resource "/login" {
        name "Login"
        get (htmlView' (login.html LoginForm.make))
        post loginView'
    }

let weather =
    resource "/weather" {
        name "Weather"
        get (htmlView' weather.html)
    }

let weatherData =
    resource "/weather/data" {
        name "Weather Data"
        get weatherView'
    }

let graph =
    resource "graph" {
        name "Graph"

        get (fun (ctx: HttpContext) ->
            let graphWriter = ctx.RequestServices.GetRequiredService<DfaGraphWriter>()

            let endpointDataSource = ctx.RequestServices.GetRequiredService<EndpointDataSource>()

            use sw = new IO.StringWriter()
            graphWriter.Write(endpointDataSource, sw)
            ctx.Response.WriteAsync(sw.ToString())
        )
    }

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

    services

[<EntryPoint>]
let main args =
    webHost args {
        useDefaults

        service configureServices

        plugWhen isDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage
        plugWhenNot isDevelopment (fun app -> ExceptionHandlerExtensions.UseExceptionHandler(app, "/error", true))

        plug ResponseCompressionBuilderExtensions.UseResponseCompression
        plug StaticFileExtensions.UseStaticFiles
        plug AntiforgeryApplicationBuilderExtensions.UseAntiforgery

        resource home
        resource messages
        resource counter
        resource login
        resource weather
        resource weatherData
        resource graph
    }

    0
