module App.State

open Elmish
open Elmish.Browser.Navigation
open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack.Fetch
open Fable.Remoting.Client
open App.Types
open Pages
open Shared
open App

let urlUpdate (result: Page option) (model: App.Types.Model) =
    match result with
    | None ->
        Browser.console.error("Error parsing url: " + Browser.window.location.href)
        model, Navigation.modifyUrl (toPageUrl model.CurrentPage) 
    | Some Page.Home ->
        let m, cmd = Home.State.init()
        { model with PageModel = HomeModel m }, Cmd.map HomeMsg cmd
    | Some Page.Counter ->
        let m, cmd = Counter.State.init()
        { model with PageModel = CounterModel m }, Cmd.map CounterMsg cmd
    | Some Page.Janken ->
        let m, cmd = Janken.State.init()
        { model with PageModel = JankenModel m }, Cmd.map JankenMsg cmd
    | Some Page.Taxonomies ->
        let m, cmd = Taxonomies.State.init()
        { model with PageModel = TaxonomiesModel m }, Cmd.map TaxonomiesMsg cmd

let init result =
    let (home, _) = Home.State.init()
    let (model, cmd) =
      urlUpdate result
        { Note = ""
          PageModel = HomeModel home   }
    model, cmd

let update msg model =
    match msg, model.PageModel with
    | HomeMsg msg, HomeModel m ->
        let (model', cmd) = Home.State.update msg m
        { model with PageModel = HomeModel model' }, Cmd.map HomeMsg cmd
    | HomeMsg _, _ ->
        model, Cmd.none
    
    | CounterMsg msg, CounterModel m ->
        let (model', cmd) = Counter.State.update msg m
        { model with PageModel = CounterModel model' }, Cmd.map CounterMsg cmd
    | CounterMsg _, _ ->
        model, Cmd.none

    | JankenMsg msg, JankenModel m ->
        let (model', cmd) = Janken.State.update msg m
        { model with PageModel = JankenModel model' }, Cmd.map JankenMsg cmd
    | JankenMsg _, _ ->
        model, Cmd.none

    | TaxonomiesMsg msg, TaxonomiesModel m ->
        match msg with
        | Taxonomies.Types.Msg.ApiError exn -> 
            match exn with
            | :? ProxyRequestException as ex -> 
                match ex.StatusCode with
                | _ -> 
                    { model with Note = ex.Message } , Cmd.none
            | _ ->
                { model with Note = exn.Message } , Cmd.none
        | _ ->
            let (model', cmd) = Taxonomies.State.update msg m
            { model with PageModel = TaxonomiesModel model' }, Cmd.map TaxonomiesMsg cmd
    | TaxonomiesMsg _, _ ->
        model, Cmd.none
    