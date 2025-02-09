module WeatherApp.templates.home

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine

let msgFragment (message: string) =
    Fragment() { div (id = "remote-text") { p (class' = "") { message } } }
    |> Oxpecker.ViewEngine.Render.toString

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Home"

    Fragment() {
        div (class' = "") {
            div (class' = "p-2") {
                h1 () { @"Datastar SDK Demo" }
                p (class' = "") { @"SSE events will be streamed from the backend to the frontend." }
            }

            div(class' = "col col-lg-3 p-2 m-5").data ("signals-delay", "400") {
                div (class' = "mb-3") {

                    label (class' = "form-label", for' = "delay") { @"Delay in milliseconds" }

                    input(id = "delay", type' = "number", step = "100", min = "0", class' = "form-control")
                        .data ("bind", "delay")
                }

                button(class' = "btn btn-primary").data ("on-click", "@get('/messages')") { @"Start" }
                div (id = "remote-text", class' = "form-text")
            }
        }
    }
