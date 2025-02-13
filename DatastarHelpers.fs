module DataStarExtensions

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open StarFederation.Datastar
open System.Text.Json

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
    do
        _sse.StartResponse() |> ignore

    member this.Signals = SignalsHttpHandlers ctx.Request

    member this.WriteHtmlFragment(htmlView) =
        htmlView
        |> Oxpecker.ViewEngine.Render.toString
        |> ServerSentEventGenerator.mergeFragments _sse

    member this.WriteHtmlFragment(htmlView, opts: MergeFragmentsOptions) =
        htmlView
        |> Oxpecker.ViewEngine.Render.toString
        |> ServerSentEventGenerator.mergeFragmentsWithOptions opts _sse

    member this.MergeSignal(_signals) =
        _signals
        |> JsonSerializer.Serialize
        |> ServerSentEventGenerator.mergeSignals _sse

    member this.MergeSignal(_signals, opts: MergeSignalsOptions) =
        _signals
        |> JsonSerializer.Serialize
        |> ServerSentEventGenerator.mergeSignalsWithOptions opts _sse

    member this.MergeSignal(_signals, jsonopts: JsonSerializerOptions) =
        JsonSerializer.Serialize(_signals, jsonopts)
        |> ServerSentEventGenerator.mergeSignals _sse

    member this.MergeSignal(_signals, opts: MergeSignalsOptions, jsonopts: JsonSerializerOptions) =
        JsonSerializer.Serialize(_signals, jsonopts)
        |> ServerSentEventGenerator.mergeSignalsWithOptions opts _sse
