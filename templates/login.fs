module WeatherApp.templates.login

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Oxpecker.Datastar
open WeatherApp.Models

let ok (ctx: HttpContext) =
    Fragment() { div (id = "login-form", class' = "w-full") { p (class' = "mx-auto w-32") { @"Login successful" } } }

let showForm (r: LoginForm) (ctx: HttpContext) =
    let passwdError = r.PasswordError |> Option.defaultValue ""
    let emailError = r.EmailError |> Option.defaultValue ""

    div (id = "login-form", class' = "w-full h-full") {
        div (class' = "w-3/4 lg:w-1/2 mx-auto bg-neutral-50 border border-neutral-50 rounded-xl") {
            form (
                id = "login-form-id",
                class' = "p-4",
                dsOnSubmit = SseRqst(SseOptions(DsPatch, "/signin").WithContentType("form"))
            ) {
                h1 (class' = "text-3xl font-bold font-heading mb-4") { @"Signin" }

                a (class' = "flex gap-4 inline-block text-gray-500 hover: transition duration-200 mb-8") {
                    span () { @"New around here?" }
                    span (class' = "mx-1") { }
                    span (class' = "font-bold font-heading") { @"Create new account" }
                }

                div (class' = "mb-4") {
                    label (class' = "block text-sm font-medium mb-2", for' = "textInput1") { @"Email" }

                    input (
                        class' =
                            "form-input w-full rounded-full p-4 outline-none border border-gray-100 shadow placeholder-gray-300 focus:ring focus:ring-orange-200 transition duration-200 mb-4",
                        id = "textInput1",
                        value = r.Email,
                        type' = "email",
                        name = "Email",
                        required = true,
                        placeholder = "john@email.com"
                    )

                    match r.EmailError with
                    | Some err -> p (class' = "form-error block ml-4 text-red-800 text-xs text-left") { err }
                    | None -> Fragment()
                }

                div (class' = "mb-4") {
                    label (class' = "block text-sm font-medium mb-2", for' = "textInput2") { @"Password" }

                    input (
                        class' =
                            "form-input w-full rounded-full p-4 outline-none border border-gray-100 shadow placeholder-gray-300 focus:ring focus:ring-orange-200 transition duration-200 mb-4",
                        id = "textInput2",
                        value = r.Password,
                        type' = "password",
                        name = "Password",
                        required = true,
                        placeholder = "Enter password"
                    )

                    match r.PasswordError with
                    | Some err -> div (class' = "form-error block ml-4 text-red-800 text-xs text-left") { err }
                    | None -> Fragment()
                }

                div (class' = "mb-8 flex justify-end") {
                    a (
                        class' =
                            "inline-block text-orange-500 hover:text-orange-600 transition duration-200 text-sm font-semibold"
                    ) {
                        @"Forgot Password?"
                    }
                }

                div (class' = "grid grid-cols-4 gap-8 justify-end") {
                    button (
                        class' =
                            "col-start-2 h-10 inline-flex items-center justify-center py-2 px-4 text-white font-bold font-heading rounded-full bg-orange-500 text-center border border-orange-600 shadow hover:bg-orange-600 focus:ring focus:ring-orange-200 transition duration-200 mb-8",
                        type' = "submit"
                    ) {
                        @"Login"
                    }

                    button (
                        type' = "reset",
                        class' =
                            "h-10 inline-flex items-center justify-center py-2 px-4 text-white font-bold font-heading rounded-full bg-red-500 text-center border border-red-600 shadow hover:bg-red-600 focus:ring focus:ring-red-200 transition duration-200 mb-8",
                        dsOnClick = DsExec [ SseRqst(SseOptions(DsGet, "/signin")) ]
                    ) {
                        @"Clear"
                    }
                }

                match r.FormError with
                | Some err -> p (class' = "form-error block pb-2 text-red-800 text-xs text-center") { err }
                | None -> div ()
            }

        }
    }

let html (r: LoginForm) (ctx: HttpContext) =
    ctx.Items["Title"] <- "Login"

    Fragment() {
        div (class' = "p-2") {
            div (class' = "mb-10") {
                h1 (class' = "text-5xl font-bold font-heading mb-6 max-w-2xl") { @"Login" }
                p (class' = "text-lg mb-2 max-w-xl") { "Form post and errors. See notes." }
                hr (class' = "border-gray-200")
            }

            showForm r ctx

            ol (class' = "py-4") {
                li (class' = "text-sm text-gray-600 py-1") { "Initially load an Oxpecker html page/form" }

                li (class' = "text-sm text-gray-600 py-1") {
                    "Submitting a form will trigger a PATCH request to datastar and errors will be shown via datastar-merge-fragments"
                }

                li (class' = "text-sm text-gray-600 py-1") {
                    "If form validation was successful datastar will trigger datastar-execute-script"
                }

                li (class' = "text-sm text-gray-600 py-1") {
                    "The script will trigger a normal POST form data to Oxpecker. This can be used to set client auth credentials in cookies and redirect to a authenticated route"
                }

                li (class' = "text-sm text-gray-600 py-1") {
                    "For forms that are not login forms, the data can be processed by datastar in one request(avoid a POST) and respond with a datastar-execute-script to initiate a redirect from client"
                }

                li (class' = "text-sm text-gray-600 py-1") {
                    "The gap between the PATCH and POST is a potential vulnerability and can be addressed with another set of validations. The price to pay for SPA like form validation"
                }
            }
        }
    }
