namespace Oxpecker.Datastar

open System.Diagnostics.CodeAnalysis
open Oxpecker.ViewEngine

type HttpVerb =
    | Get of string
    | Post of string
    | Put of string
    | Patch of string
    | Delete of string

type HttpOption =
    | Headers
    | ContentType
    | IncludeLocal

type HttpVerbWithOptions = GetWith of string seq

[<AutoOpen>]
module CoreAttributes =
    type HtmlTag with
        member this.dsOnClick
            with set (verb: HttpVerb | null) =
                match verb with
                | Get value -> this.data ("on-click", $"@get('{value}')") |> ignore
                | Post value -> this.data ("on-click", $"@post('{value}')") |> ignore
                | Put value -> this.data ("on-click", $"@put('{value}')") |> ignore
                | Patch value -> this.data ("on-click", $"@patch('{value}')") |> ignore
                | Delete value -> this.data ("on-click", $"@delete('{value}')") |> ignore
