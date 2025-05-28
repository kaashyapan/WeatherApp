module WeatherApp.templates.counter

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Oxpecker.Datastar

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Counter"

    Fragment() {
        div (class' = "p-2") {
            div (class' = "mb-10") {
                h1 (class' = "text-5xl font-bold font-heading mb-6 max-w-2xl") { @"Datastar Counter Demo" }
                p (class' = "text-lg mb-2 max-w-xl") { "Demonstrates counter as a signal" }
                hr (class' = "border-gray-200")
            }

            div (
                class' = "w-3/4 lg:w-1/2 grid h-56 grid-cols-3 items-center place-items-center",
                dsSignals = [ ("counter", "0") ]
            ) {
                div (class' = "w-14") {
                    button (
                        class' =
                            "h-14 items-center justify-center text-white font-bold fond-heading bg-orange-500 w-full text-center border rounded-full border-orange-600 shadow hover:bg-orange-600 focus:ring focus:ring-orange-200 transition duration-200",
                        dsOnClick = SseRqst(SseOptions(DsPost, "/counter/decr"))
                    ) {
                        @"-"
                    }
                }

                div (class' = "self-center") { p (class' = "text-5xl", dsText = "$counter") }

                div (class' = "w-14") {
                    button (
                        class' =
                            "h-14 items-center justify-center text-white font-bold fond-heading bg-orange-500 w-full text-center border rounded-full border-orange-600 shadow hover:bg-orange-600 focus:ring focus:ring-orange-200 transition duration-200",
                        dsOnClick = SseRqst(SseOptions(DsPost, "/counter/incr"))
                    ) {
                        @"+"
                    }
                }
            }
        }
    }
