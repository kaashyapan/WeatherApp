module WeatherApp.templates.home

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine

let msgFragment (message: string) = Fragment() { p (class' = "") { message } }

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Signals"

    Fragment() {
        div (class' = "p-2") {
            div (class' = "mb-10") {
                h1 (class' = "text-5xl font-bold font-heading mb-6 max-w-2xl") { @"Datastar SDK Demo" }

                p (class' = "text-lg mb-2 max-w-xl") {
                    @"SSE events will be streamed from the backend to the frontend."
                }

                hr (class' = "border-gray-200")
            }

            div(class' = "w-3/4 lg:w-1/2").data ("signals-delay", "400") {
                div (class' = "w-full gap-10 mb-4") {

                    label (class' = "block text-nowrap text-md mb-2 font-medium", for' = "delay") { @"Delay in ms" }

                    input(
                        class' =
                            "w-full rounded-full p-4 outline-none border border-gray-100 shadow placeholder-gray-500 focus:ring focus:ring-orange-200 transition duration-200 mb-4",
                        id = "delay",
                        type' = "number",
                        step = "100",
                        min = "0"
                    )
                        .data ("bind", "delay")

                }

                button(
                    class' =
                        "h-14 max-w-32 items-center justify-center py-4 px-6 text-white font-bold font-heading rounded-full bg-orange-500 w-full text-center border border-orange-600 shadow hover:bg-orange-600 focus:ring focus:ring-orange-200 transition duration-200 mb-8"
                )
                    .data ("on-click", "@get('/messages')") {
                    @"Start"
                }

                div (id = "remote-text", class' = "text-center text-lg mb-10")
            }
        }
    }
