module FSharp.Bolero.Client.Main

open Elmish
open Bolero
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Bolero.Html
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Components.Web

type Page =
    | [<EndPoint "/">] Home

type ViewModel = { Col1 : string
                   Col2 : string
                   Col3 : string }

type Model =
    { Page : Page
      Data : ViewModel list }

type MyComponent() =
    inherit Component()
          
    override this.Render() =
        div [] [text "This is my component"]
              
    member this.Refresh() =
        printfn("Refreshing this component!");

let init _ =
    { Page = Home
      Data = [ { Col1 = "1.1"
                 Col2 = "1.2"
                 Col3 = "1.3" }
               { Col1 = "2.1"
                 Col2 = "2.2"
                 Col3 = "2.3" }
               { Col1 = "3.1"
                 Col2 = "3.2"
                 Col3 = "3.3" } ] }
    , Cmd.none

type Message =
    | SetPage of Page
    | ExportExcel of MyComponent
    | ExcelExported

let update message model =
    match message with
    | SetPage p -> { model with Page = p }, Cmd.none
    | ExportExcel grid ->
        model,
        Cmd.OfTask.perform (fun () ->
            task {
                printfn("Button Click triggered!")
                grid.Refresh()
            }) () (fun () -> ExcelExported)
    //If the below ExcelExported code(line 63-67) is commented then everything works fine
    | ExcelExported ->
        { model with Data = [ { Col1 = "1.1"
                                Col2 = "1.2"
                                Col3 = "1.3" } ] }
        , Cmd.none

let router = Router.infer SetPage (fun model -> model.Page)

type Main = Template<"wwwroot/main.html">

let view model dispatch =

    let myComponentRef = Ref<MyComponent>()

    Main()
        .Grid(
            comp<MyComponent> [attr.ref myComponentRef] []
        ).ExportToExcel(
            div [] [
                button [on.click (fun _ -> dispatch <| ExportExcel myComponentRef.Value)] [text "Refresh MyComponent"]
            ]
        ).Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        Program.mkProgram init update view
        |> Program.withRouter router