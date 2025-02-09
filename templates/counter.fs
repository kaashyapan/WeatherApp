module WeatherApp.templates.counter

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Counter"

    Fragment() {
        div (class' = "") {
            div (class' = "p-2") {
                h1 () { "Datastar Counter Demo" }
                p () { "Demonstrates counter as a signal" }
            }

            div(class' = "col col-lg-3 p-2 m-5").data ("signals-counter", "0") {
                div (class' = "d-flex justify-content-center gap-3") {
                    button(class' = "btn btn-primary").data ("on-click", "@get('/counter/decr')") { @"-" }
                    p(id = "count", class' = "display-4").data ("text", "$counter")
                    button(class' = "btn btn-primary").data ("on-click", "@get('/counter/incr')") { @"+" }
                }

            }
        }
    }
