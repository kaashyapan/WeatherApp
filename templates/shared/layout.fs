namespace WeatherApp.templates.shared

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open WeatherApp.Models

#nowarn "3391"

module layout =

    let navLink (attrs: {| Href: string; Class: string; Ctx: HttpContext |}) =
        let finalClass = if attrs.Href = attrs.Ctx.Request.Path then attrs.Class + " bg-purple-900" else attrs.Class

        a (href = attrs.Href, class' = finalClass)

    let navMenu (ctx: HttpContext) =
        Fragment() {
            div (class' = "w-min-3xl") {
                div (class' = "flex flex-wrap items-center justify-between px-7 py-6 pb-0") {
                    div (class' = "w-auto lg:w-min-3xl mx-auto flex gap-4 py-10") {
                        a (class' = "inline-block w-min-24", href = "#") {
                            img (
                                src = "https://github.com/Lanayx/Oxpecker/raw/develop/images/oxpecker.png",
                                class' = "w-24"
                            )
                        }

                        a (class' = "inline-block w-min-24", href = "#") {
                            img (
                                src =
                                    "https://data-star.dev/static/images/rocket-304e710dde0b42b15673e10937623789adf72cae569c0e0defe7ec21c0bdf293.webp",
                                class' = "w-24"
                            )
                        }

                    }

                }

                h1 (class' = "text-xl text-white font-bold font-heading text-center") { @"Oxpecker + Datastar" }

                div (class' = "flex-1 flex flex-col justify-between py-7 overflow-x-hidden overflow-y-auto") {
                    div (class' = "flex flex-col flex-wrap px-7 mb-8") {
                        div (class' = "w-auto") {
                            navLink
                                {|
                                    Class =
                                        "flex flex-wrap items-center p-3 text-neutral-50 hover:text-neutral-100 hover:bg-purple-900 rounded-lg"
                                    Href = "/"
                                    Ctx = ctx
                                |} {
                                p (class' = "font-medium") { @"Signals" }
                            }
                        }

                        div (class' = "w-auto") {
                            navLink
                                {|
                                    Class =
                                        "flex flex-wrap items-center p-3 text-neutral-50 hover:text-neutral-100 hover:bg-purple-900 rounded-lg"
                                    Href = "/counter"
                                    Ctx = ctx
                                |} {
                                p (class' = "font-medium") { @"Counter" }
                            }
                        }

                        div (class' = "w-auto") {
                            navLink
                                {|
                                    Class =
                                        "flex flex-wrap items-center p-3 text-neutral-50 hover:text-neutral-100 hover:bg-purple-900 rounded-lg"
                                    Href = "/weather"
                                    Ctx = ctx
                                |} {
                                p (class' = "font-medium") { @"Weather Data" }
                            }
                        }

                        div (class' = "w-auto") {
                            navLink
                                {|
                                    Class =
                                        "flex flex-wrap items-center p-3 text-neutral-50 hover:text-neutral-100 hover:bg-purple-900 rounded-lg"
                                    Href = "/signin"
                                    Ctx = ctx
                                |} {
                                p (class' = "font-medium") { @"Login Form" }
                            }
                        }
                    }
                }
            }
        }

    let mainLayout (ctx: HttpContext) (content: HtmlElement) =
        section (class' = "relative") {
            div (class' = "w-full flex flex-row") {
                div (class' = "inset-0 max-w-xss h-screen bg-purple-950") { navMenu ctx }
                div (class' = "w-full ml-xs p-4 bg-white") { content }
            }
        }

    let html (ctx: HttpContext) (content: HtmlElement) =
        html (lang = "en") {
            head () {
                title () {
                    match ctx.Items.TryGetValue "Title" with
                    | true, title -> string title
                    | false, _ -> "F# + Datastar"
                }

                meta (charset = "utf-8")
                meta (name = "viewport", content = "width=device-width, initial-scale=1.0")
                base' (href = "/")
                link (rel = "icon", type' = "image/png", href = "/favicon.png")

#if DEBUG
                link (rel = "stylesheet", href = $"/app.min.css?randomizer={reloadToken}")

                script (type' = "module", src = $"/app.min.js?randomizer={reloadToken}")

                raw
                    $$"""
                  <script type="module">
                        import Pollinator from 'https://cdn.jsdelivr.net/npm/pollinator@0.3.2/dist/index.module.js';

                        const handlePoll = (response, status) => {
                            if (response.status === 205) {
                                console.log("Reloading page...");
                                window.location.reload();
                            } else {
                               // console.log("Skip reload...");
                            }
                        }

                        const pollingFunction = () => fetch('/pagereload/{{reloadToken}}');

                        // Change 100 to some number that depends on how long the project takes to compile
                        // Add 'delay: 5000' to options to control the polling frequency 
                        const poller = new Pollinator(pollingFunction, { failRetryCount: 100 });
                        poller.on(Pollinator.Event.POLL, handlePoll);
                        poller.start();
                    </script>  
                  """
#else
                link (rel = "stylesheet", href = $"/app.min.css")
                script (type' = "module", src = $"/app.min.js")
#endif
            }

            body () { mainLayout ctx content }
        }
