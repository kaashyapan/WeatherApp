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
            form(id = "login-form-id", class' = "p-4").data ("on-submit", "@post('/signin', {contentType: 'form'})") {
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

                    button(
                        class' =
                            "h-10 inline-flex items-center justify-center py-2 px-4 text-white font-bold font-heading rounded-full bg-red-500 text-center border border-red-600 shadow hover:bg-red-600 focus:ring focus:ring-red-200 transition duration-200 mb-8"
                    )
                        .data ("on-click__prevent", "resetForm('login-form-id')") {
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
                p (class' = "text-lg mb-2 max-w-xl") { "Form post and errors" }
                hr (class' = "border-gray-200")
            }

            showForm r ctx
        }
    }
