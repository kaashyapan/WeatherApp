namespace Oxpecker.Datastar

open Oxpecker.ViewEngine
open System.Text

[<AutoOpen>]
module Plugins =
    type HtmlTag with

        member this.DsOnEvt(event: string, expression: DsExpression) =
            match expression with
            | SseRqst value ->
                let expression = SseOptions.toDsRequest (value)
                this.data (event, expression) |> ignore
            | DsExp value -> this.data (event, $"{value}") |> ignore
            | DsExec expressions ->
                let stringifyTimeOpts _acc o =
                    let _o =
                        match o with
                        | T500ms -> "500ms"
                        | T1s -> "1s"
                        | Leading -> "leading"
                        | NoLeading -> "noleading"
                        | Trailing -> "trail"
                        | NoTrailing -> "notrail"

                    _acc + "." + _o

                let modifiers =
                    expressions
                    |> List.choose (fun e ->
                        match e with
                        | Mod value -> Some value
                        | _ -> None
                    )
                    |> List.fold
                        (fun finalExp _ex ->
                            let ex =
                                match _ex with
                                | Half -> "half"
                                | Full -> "full"
                                | Once -> "once"
                                | Window -> "window"
                                | Outside -> "outside"
                                | Stop -> "stop"
                                | Prevent -> "prevent"
                                | Passive -> "passive"
                                | Capture -> "capture"
                                | ViewTransition -> "viewtransition"
                                | Debounce opts ->
                                    let _opts = opts |> List.fold stringifyTimeOpts ""
                                    "debounce" + _opts
                                | Throttle opts ->
                                    let _opts = opts |> List.fold stringifyTimeOpts ""
                                    "throttle" + _opts
                                | Delay opts ->
                                    let _opts = opts |> List.fold stringifyTimeOpts ""
                                    "delay" + _opts
                                | Duration opts ->
                                    let _opts = opts |> List.fold stringifyTimeOpts ""
                                    "duration" + _opts
                                | CamelCase -> "case.camel"
                                | SnakeCase -> "case.snake"
                                | KebabCase -> "case.kebab"
                                | PascalCase -> "case.pascal"

                            finalExp + "__" + ex
                        )
                        ""

                let multipleExps =
                    expressions
                    |> List.filter (fun e ->
                        match e with
                        | Mod _ -> false
                        | _ -> true
                    )
                    |> List.fold
                        (fun finalExp _ex ->
                            let ex =
                                match _ex with
                                | SseRqst value -> SseOptions.toDsRequest (value)
                                | DsExp value -> $"{value}"
                                | value -> $"{value}"

                            finalExp + ";" + ex
                        )
                        ""

                this.data (event + modifiers, multipleExps.Trim(' ').Trim(';')) |> ignore
            | someInvalidExp -> someInvalidExp |> ignore

        member this.dsOnEvent
            with set (listOfExpressions: (string * DsExpression) list) =
                listOfExpressions
                |> List.iter (fun (_evt, _exp) -> this.DsOnEvt(_evt, _exp) |> ignore)

        member this.dsOnClick
            with set (expression: DsExpression) = this.DsOnEvt("on-click", expression)

        member this.dsOnLoad
            with set (expression: DsExpression) = this.DsOnEvt("on-load", expression)

        member this.dsOnSubmit
            with set (expression: DsExpression) = this.DsOnEvt("on-submit", expression)

        member this.dsOnHover
            with set (expression: DsExpression) = this.DsOnEvt("on-hover", expression)

        member this.dsOnChange
            with set (expression: DsExpression) = this.DsOnEvt("on-change", expression)

        member this.dsOnInput
            with set (expression: DsExpression) = this.DsOnEvt("on-input", expression)

        member this.dsOnIntersect
            with set (expression: DsExpression) = this.DsOnEvt("on-intersect", expression)

        member this.dsOnInterval
            with set (expression: DsExpression) = this.DsOnEvt("on-interval", expression)

        member this.dsOnRaf
            with set (expression: DsExpression) = this.DsOnEvt("on-raf", expression)

        member this.dsOnSignalChange
            with set (signalName: string, expression: DsExpression) =
                this.DsOnEvt($"on-signal-change-{signalName}", expression)

        member this.dsIndicator
            with set (signalName: string) = this.data ($"indicator", signalName) |> ignore

        member this.dsBind
            with set (signalName: string) = this.data ($"bind", signalName) |> ignore

        member this.dsRef
            with set (signalName: string) = this.data ($"ref", signalName) |> ignore

        member this.dsShow
            with set (dsExpression: string) = this.data ("show", $"{dsExpression}") |> ignore

        member this.dsAttr
            with set (attrs: (string * string) list) =
                let exps = attrs |> List.map (fun (_k, _v) -> _k + ": " + _v)
                let obj = StringBuilder("{").AppendJoin(',', exps).Append("}").ToString()
                this.data ($"attr", obj) |> ignore

        member this.dsClass
            with set (classes: (string * string) list) =
                let exps = classes |> List.map (fun (_k, _v) -> $"'{_k}'" + ": " + _v)
                let obj = StringBuilder("{").AppendJoin(',', exps).Append("}").ToString()
                this.data ($"class", obj) |> ignore

        member this.dsSignals
            with set (signals: (string * string) list) =
                let exps = signals |> List.map (fun (_k, _v) -> _k + ": " + _v)
                let obj = StringBuilder("{").AppendJoin(',', exps).Append("}").ToString()
                this.data ($"signals", obj) |> ignore

        member this.dsComputed
            with set (name: string, dsExpression: string) = this.data ($"computed-{name}", $"{dsExpression}") |> ignore

        member this.dsText
            with set (dsExpression: string) = this.data ("text", $"{dsExpression}") |> ignore

        //TODO
        member this.dsIgnore
            with set (ignoredLibrary: string) = this.data ("star-ignore", ignoredLibrary) |> ignore

        member this.dsIgnoreSelf
            with set (ignoredLibrary: string) = this.data ("star-ignore__self", ignoredLibrary) |> ignore
        //TODO
        member this.dsCustomValidity
            with set (dsExpression: string) = this.data ("custom-validity", $"{dsExpression}") |> ignore

        member this.dsPersist
            with set (signalExpression: string) = this.data ("persist", $"{signalExpression}") |> ignore

        member this.dsPersistSession
            with set (signalExpression: string) = this.data ("persist__session", $"{signalExpression}") |> ignore

        member this.dsReplaceUrl
            with set (url: string) = this.data ("replace-url", $"`{url}`") |> ignore

        member this.dsScrollIntoView
            with set (mods: ScrollModifier list) =

                let modifiers =
                    mods
                    |> List.fold
                        (fun finalExp _ex ->
                            let ex =
                                match _ex with
                                | Smooth -> "smooth"
                                | Instant -> "instant"
                                | Auto -> "auto"
                                | Hstart -> "hstart"
                                | Hend -> "hend"
                                | HCenter -> "hcenter"
                                | HNearest -> "hnearest"
                                | Vstart -> "vstart"
                                | Vend -> "vend"
                                | VCenter -> "vcenter"
                                | VNearest -> "vnearest"
                                | Focus -> "focus"

                            finalExp + "__" + ex
                        )
                        ""

                this.data ("scroll-into-view" + modifiers, "") |> ignore

        member this.dsViewTransition
            with set (dsExpression: string) = this.data ("view-transition", $"{dsExpression}") |> ignore
