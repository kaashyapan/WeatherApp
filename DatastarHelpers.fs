namespace Oxpecker.Datastar

open Oxpecker.ViewEngine
open System
open System.Text
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open StarFederation.Datastar
open System.Text.Json

type TimeOpts =
    | T500ms
    | T1s
    | Leading
    | Trailing
    | NoLeading
    | NoTrailing

type Modifier =
    | Once
    | Half
    | Full
    | Duration of TimeOpts list
    | Delay of TimeOpts list
    | Debounce of TimeOpts list
    | Throttle of TimeOpts list
    | Window
    | Outside
    | Stop
    | Prevent
    | Passive
    | Capture
    | ViewTransition
    | KebabCase
    | CamelCase
    | PascalCase
    | SnakeCase

type ScrollModifier =
    | Smooth
    | Instant
    | Auto
    | Hstart
    | Hend
    | HCenter
    | HNearest
    | Vstart
    | Vend
    | VCenter
    | VNearest
    | Focus

type DsExpression =
    | SseRqst of SseOptions
    | DsExp of string
    | DsExec of DsExpression list
    | Mod of Modifier

and HttpMethod =
    | DsGet
    | DsPost
    | DsPut
    | DsPatch
    | DsDelete

and SseOptions(method: HttpMethod, path: string) =
    member private this.Method = method
    member private this.Path = path

    member val private accumulator = Seq.empty with get, set

    member this.WithHeaders(headers: seq<string * string>) =
        let _headers = headers |> Seq.map (fun (k, v) -> $"'{k}':'{v}'")
        let obj = StringBuilder("{").AppendJoin(',', _headers).Append("}").ToString()
        this.accumulator <- $"headers:{obj}" |> Seq.singleton |> Seq.append this.accumulator
        this

    member this.WithContentType(contentType: string) =
        this.accumulator <- $"contentType:\"{contentType}\"" |> Seq.singleton |> Seq.append this.accumulator
        this

    member this.WithOpenWhenHidden(openWhenHidden: bool) =
        let _openWhenHidden = if openWhenHidden then "true" else "false"

        this.accumulator <-
            $"openWhenHidden:{_openWhenHidden}"
            |> Seq.singleton
            |> Seq.append this.accumulator

        this

    member this.WithIncludeLocal(includeLocal: bool) =
        let _includeLocal = if includeLocal then "true" else "false"
        this.accumulator <- $"includeLocal:{_includeLocal}" |> Seq.singleton |> Seq.append this.accumulator
        this

    member this.WithSelector(selector: string) =
        this.accumulator <- $"selector:\"{selector}\"" |> Seq.singleton |> Seq.append this.accumulator
        this

    member this.WithRetryInterval(retryInterval: int) =
        this.accumulator <- $"retryInterval:{retryInterval}" |> Seq.singleton |> Seq.append this.accumulator
        this

    member this.WithRetryScaler(retryScaler: int) =
        this.accumulator <- $"retryScaler:{retryScaler}" |> Seq.singleton |> Seq.append this.accumulator
        this

    member this.WithRetryMaxWaitMs(retryMaxWaitMs: int) =
        this.accumulator <-
            $"retryMaxWaitMs:{retryMaxWaitMs}"
            |> Seq.singleton
            |> Seq.append this.accumulator

        this

    member this.WithRetryMaxCount(retryMaxCount: int) =
        this.accumulator <- $"retryMaxCount:{retryMaxCount}" |> Seq.singleton |> Seq.append this.accumulator
        this

    static member toDsRequest(opts: SseOptions) =
        let obj = StringBuilder("{").AppendJoin(',', opts.accumulator).Append("}").ToString()

        match opts.Method with
        | DsGet -> $"@get('{opts.Path}', {obj})"
        | DsPost -> $"@post('{opts.Path}', {obj})"
        | DsPatch -> $"@patch('{opts.Path}', {obj})"
        | DsPut -> $"@put('{opts.Path}', {obj})"
        | DsDelete -> $"@delete('{opts.Path}', {obj})"

[<AutoOpen>]
module Helpers =
    type SignalsHttpHandlers with

        /// <summary>
        /// Read the client signals from the query string as a Json string and Deserialize
        /// </summary>
        /// <returns>Returns an instance of `'T`.</returns>
        [<Extension>]
        member this.ReadSignalsOrFail<'T>(jsonSerializerOptions: JsonSerializerOptions) : Task<'T> =
            task {
                let! signalsVopt = (this :> IReadSignals).ReadSignals<'T>(jsonSerializerOptions)

                let signals =
                    match signalsVopt with
                    | ValueSome signals -> signals
                    | ValueNone -> failwith $"Unable to deserialize {typeof<'T>} from signals"

                return signals
            }

type Datastar(ctx: HttpContext) =
    let _sse = ServerSentEventHttpHandlers ctx.Response
    do _sse.StartResponse() |> ignore

    member this.Signals = SignalsHttpHandlers ctx.Request

    member this.WriteHtmlFragment(htmlView) =
        let view = htmlView |> Oxpecker.ViewEngine.Render.toString
        ServerSentEventGenerator.MergeFragments(_sse, view)

    member this.WriteHtmlFragment(htmlView, opts: MergeFragmentsOptions) =
        let view = htmlView |> Oxpecker.ViewEngine.Render.toString
        ServerSentEventGenerator.MergeFragments(_sse, view, opts)

    member this.MergeSignal(_signals) =
        let signals = _signals |> JsonSerializer.Serialize
        ServerSentEventGenerator.MergeSignals(_sse, signals)

    member this.MergeSignal(_signals, opts: MergeSignalsOptions) =
        let signals = _signals |> JsonSerializer.Serialize
        ServerSentEventGenerator.MergeSignals(_sse, signals, opts)

    member this.MergeSignal(_signals, jsonopts: JsonSerializerOptions) =
        let signals = JsonSerializer.Serialize(_signals, jsonopts)
        ServerSentEventGenerator.MergeSignals(_sse, signals)

    member this.MergeSignal(_signals, opts: MergeSignalsOptions, jsonopts: JsonSerializerOptions) =
        let signals = JsonSerializer.Serialize(_signals, jsonopts)
        ServerSentEventGenerator.MergeSignals(_sse, signals, opts)

    member this.ExecuteScript(_script) = ServerSentEventGenerator.ExecuteScript(_sse, _script)

    member this.ExecuteScript(_script, opts: ExecuteScriptOptions) =
        ServerSentEventGenerator.ExecuteScript(_sse, _script, opts)

    member this.RemoveFragments(_selector) = ServerSentEventGenerator.RemoveFragments(_sse, _selector)

    member this.RemoveFragments(_selector, opts: RemoveFragmentsOptions) =
        ServerSentEventGenerator.RemoveFragments(_sse, _selector, opts)

    member this.RemoveSignals(_paths) = ServerSentEventGenerator.RemoveSignals(_sse, _paths)

    member this.RemoveSignals(_paths, opts: EventOptions) = ServerSentEventGenerator.RemoveSignals(_sse, _paths, opts)
