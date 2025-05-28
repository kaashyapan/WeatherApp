module WeatherApp.templates.weather

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open WeatherApp.Models
open Oxpecker.Datastar

let data (forecasts: WeatherForecast[]) =
    Fragment() {
        section (id = "weather-data", class' = "w-3/4 lg:w-1/2 py-4 overflow-hidden") {
            div (class' = "container px-4 mx-auto") {
                div (class' = "py-6 bg-neutral-50 border border-neutral-100 rounded-xl") {
                    div (class' = "px-6") {
                        div (class' = "w-full overflow-x-auto border rounded-lg") {
                            div () {
                                table (class' = "w-full min-w-max") {
                                    thead () {
                                        tr (class' = "text-left") {
                                            th (class' = "py-3.5 px-6 bg-light border-b rounded-tl-lg") {
                                                a (class' = "inline-flex items-center", href = "#") {
                                                    span (class' = "mr-3 text-sm font-semibold") { @"Date" }
                                                }
                                            }

                                            th (class' = "py-3.5 px-6 bg-light border-b") {
                                                a (class' = "inline-flex items-center", href = "#") {
                                                    span (class' = "mr-3 text-sm font-semibold") { @"Temp. (C)" }
                                                }
                                            }

                                            th (class' = "py-3.5 px-6 bg-light border-b") {
                                                a (class' = "inline-flex items-center", href = "#") {
                                                    span (class' = "mr-3 text-sm font-semibold") { @"Temp. (F)" }
                                                }
                                            }

                                            th (class' = "py-3.5 px-6 bg-light border-b rounded-tr-lg") {
                                                a (class' = "inline-flex items-center", href = "#") {
                                                    span (class' = "mr-3 text-sm font-semibold") { @"Summary" }
                                                }
                                            }
                                        }
                                    }

                                    tbody () {
                                        for forecast in forecasts do
                                            tr () {
                                                td (class' = "py-4 px-6 border-b") {
                                                    div (class' = "flex flex-wrap items-center") {
                                                        span (class' = "font-semibold") {
                                                            forecast.Date.ToShortDateString()
                                                        }
                                                    }
                                                }

                                                td (class' = "py-4 px-6 border-b") {
                                                    span (class' = "text-sm") { string forecast.TemperatureC }
                                                }

                                                td (class' = "py-4 px-6 border-b") {
                                                    span (class' = "text-sm") { string forecast.TemperatureF }
                                                }

                                                td (class' = "py-4 px-6 border-b") {
                                                    span (class' = "block text-sm font-medium") { forecast.Summary }
                                                }
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Weather"

    Fragment() {
        div (class' = "p-2") {
            div (class' = "mb-10") {
                h1 (class' = "text-5xl font-bold font-heading mb-6 max-w-2xl") { @"Weather" }
                p (class' = "text-lg mb-2 max-w-xl") { "Show a loading page quickly and load data via sse." }
                hr (class' = "border-gray-200")
            }

            p (id = "weather-data", dsOnLoad = DsExec [ SseRqst(SseOptions(DsGet, "/weather/data")) ]) {
                em () { "Loading..." }
            }
        }
    }
