module WeatherApp.templates.weather

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open WeatherApp.Models

let data (forecasts: WeatherForecast[]) =
    Fragment() {
        div (id = "weather-data") {
            table (class' = "table") {
                thead () {
                    tr () {
                        th () { "Date" }
                        th () { "Temp. (C)" }
                        th () { "Temp. (F)" }
                        th () { "Summary" }
                    }
                }

                tbody () {
                    for forecast in forecasts do
                        tr () {
                            td () { forecast.Date.ToShortDateString() }
                            td () { string forecast.TemperatureC }
                            td () { string forecast.TemperatureF }
                            td () { forecast.Summary }
                        }
                }
            }
        }
    }
    |> Oxpecker.ViewEngine.Render.toString

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Weather"

    Fragment() {
        h1 () { "Weather" }
        p () { "Show a loading page quickly and load data via sse." }
        p(id = "weather-data").data ("on-load", "@get('/weather/data')") { em () { "Loading..." } }
    }
