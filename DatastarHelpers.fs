module DataStarExtensions

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
    member __.ReadSignalsOrFail<'T>(jsonSerializerOptions: JsonSerializerOptions) : Task<'T> =
        task {
            let! signalsVopt = (__ :> IReadSignals).ReadSignals<'T>(jsonSerializerOptions)

            let signals =
                match signalsVopt with
                | ValueSome signals -> signals
                | ValueNone -> failwith $"Unable to deserialize {typeof<'T>} from signals"

            return signals
        }

type Datastar(ctx: HttpContext) =
    let _sse = ServerSentEventHttpHandlers ctx.Response
    do _sse.StartResponse() |> ignore

    member __.Signals = SignalsHttpHandlers ctx.Request

    member __.WriteHtmlFragment(htmlView, ?options) =
        let fragment = htmlView |> Oxpecker.ViewEngine.Render.toString
        ServerSentEventGenerator.mergeFragments (_sse, fragment, ?options = options)

    member __.RemoveHtmlFragment(selector, ?options) =
        ServerSentEventGenerator.removeFragments (_sse, selector, ?options = options)

    member __.MergeSignal(signals, ?options, ?jsonSerializerOptions: JsonSerializerOptions) =
        let json =
            match jsonSerializerOptions with
            | Some opts -> JsonSerializer.Serialize(signals, opts)
            | None -> JsonSerializer.Serialize(signals)

        ServerSentEventGenerator.mergeSignals (_sse, json, ?options = options)

    member __.RemoveSignal(paths, ?options) = ServerSentEventGenerator.removeSignals (_sse, paths, ?options = options)

    member __.ExecuteScript(script, ?options) =
        ServerSentEventGenerator.executeScript (_sse, script, ?options = options)
