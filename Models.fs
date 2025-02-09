module WeatherApp.Models

open System
open System.Text.Json
open System.Text.Json.Serialization

type WeatherForecast =
    { Date: DateOnly
      TemperatureC: int
      Summary: string | null }

    member this.TemperatureF = 32 + int (float this.TemperatureC / 0.5556)

type HomeSignal =
    { [<JsonPropertyName "delay">]
      Delay: float }


type CounterAction =
    | Incr
    | Decr

type CounterSignal =
    { [<JsonPropertyName "counter">]
      Count: int }
