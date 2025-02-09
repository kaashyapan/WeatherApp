module WeatherApp.Models

open System
open System.Text.Json
open System.Text.Json.Serialization

type WeatherForecast =
    { Date: DateOnly
      TemperatureC: int
      Summary: string | null }

    member this.TemperatureF = 32 + int (float this.TemperatureC / 0.5556)

[<CLIMutable>]
type HomeSignal =
    { [<JsonPropertyName "delay">]
      Delay: float }

type HomeSignalNull = HomeSignal | null

type CounterAction =
    | Incr
    | Decr

[<CLIMutable>]
type CounterSignal =
    { [<JsonPropertyName "counter">]
      Count: int }

type CounterSignalNull = CounterSignal | null
