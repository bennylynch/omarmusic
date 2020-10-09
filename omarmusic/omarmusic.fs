// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace omarmusic

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.InputTypes
open Fabulous.XamarinForms.InputTypes.Image
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open Fabulous.XamarinForms.VideoManager
open System
open System.IO
open System.Security.Cryptography
open FSharp.Data
open omarmusic.Models

module App = 
    //notasecret - password for P12 cert
    type Msg = 
        | VidPageMsg of VidPage.Msg
        | TweetPageMsg of TweetPage.Msg
        | FBPageMsg of WebPage.Msg
        | WebPageMsg of WebPage.Msg
        | InstaPageMsg of InstaPage.Msg
        | EventsPageMsg of EventsPage.Msg
        | NavigationPopped
        | ShowVids
        | ShowTweets
        | ShowInsta
        | ShowFB
        | ShowWeb of string
        | ShowEvents

        

    type Model = 
      { VidsPageModel : VidPage.Model option
        TweetPageModel : TweetPage.Model option
        FBPageModel : WebPage.Model option
        WebPageModel : WebPage.Model option
        InstaPageModel : InstaPage.Model option
        EventsPageModel : EventsPage.Model option
        (*/ Workaround Cmd limitation 
        Cannot pop a page in page stack and send Cmd at the same time
        Otherwise it would pop pages 2 times in NavigationPage 
        Borrowed from FabulousContacts example
        /*)
        WorkaroundNavPageBug: bool
        WorkaroundNavPageBugPendingCmd: Cmd<Msg> 
      }

    type Pages =
        { MainPage: ViewElement
          VidsPage: ViewElement option
          TweetPage: ViewElement option
          FBPage: ViewElement option
          WebPage: ViewElement option
          InstaPage: ViewElement option
          EventsPage: ViewElement option

        }

    let initModel = {
        VidsPageModel = None
        TweetPageModel = None
        FBPageModel = None
        WebPageModel = None
        InstaPageModel = None
        EventsPageModel = None
        WorkaroundNavPageBug = false
        WorkaroundNavPageBugPendingCmd = Cmd.none
    }              
        
    let init () = initModel, Cmd.none
        
    let navigationMapper (model : Model) =
        let vidsModel = model.VidsPageModel
        let tweetsModel = model.TweetPageModel
        let fbModel = model.FBPageModel
        let instaModel = model.InstaPageModel
        let webModel = model.WebPageModel
        let eventsModel = model.EventsPageModel

        match vidsModel,tweetsModel, fbModel, instaModel, webModel, eventsModel  with
        | None, None, None, None, None, None ->
            model
        |Some _,_,_,_,_,_ ->
            {model with VidsPageModel = None}
        |_, Some _,_,_,_,_->
            {model with TweetPageModel = None}
        |_,_, Some _,_,_,_->
            {model with FBPageModel = None}
        |_,_,_, Some _,_,_->
            {model with InstaPageModel = None}
        |_,_,_, _, Some _,_->
            {model with WebPageModel = None}
        |_,_,_, _, _, Some _->
            {model with EventsPageModel = None}

    let update msg model =
        match msg with
        | NavigationPopped ->
            match model.WorkaroundNavPageBug with
            | true ->
                // Do not pop pages if already done manually
                let newModel =
                    { model with
                        WorkaroundNavPageBug = false
                        WorkaroundNavPageBugPendingCmd = Cmd.none }
                newModel, model.WorkaroundNavPageBugPendingCmd
            | false ->
                navigationMapper model, Cmd.none
        | ShowVids ->
            let vidsModel, cmd = VidPage.init()
            { model with VidsPageModel = Some vidsModel}, (Cmd.map VidPageMsg cmd)
        | ShowTweets ->
            let tweetsModel, cmd = TweetPage.init()
            { model with TweetPageModel = Some tweetsModel}, (Cmd.map TweetPageMsg cmd)
        | ShowFB ->
            let fbModel, cmd = WebPage.init "https://m.facebook.com/barry"
            { model with FBPageModel = Some fbModel},  (Cmd.map FBPageMsg cmd)

        | ShowWeb url ->
            let webModel, cmd = WebPage.init url // 
            { model with WebPageModel = Some webModel},  (Cmd.map WebPageMsg cmd)

        | ShowEvents ->
            let evtsModel, cmd = EventsPage.init ()
            { model with EventsPageModel = Some evtsModel },  (Cmd.map EventsPageMsg cmd)
                
        | ShowInsta ->
            let instaModel, cmd = InstaPage.init()
            { model with InstaPageModel = Some instaModel}, (Cmd.map InstaPageMsg cmd)

        | VidPageMsg vidpagemsg ->
            let newModel, cmd = VidPage.update vidpagemsg (model.VidsPageModel.Value)
            { model with VidsPageModel = Some newModel }, (Cmd.map VidPageMsg cmd)

        | TweetPageMsg tweetmsg ->
            let newModel, cmd = TweetPage.update tweetmsg (model.TweetPageModel.Value)
            { model with TweetPageModel = Some newModel }, (Cmd.map TweetPageMsg cmd)
        | EventsPageMsg evtspagemsg ->
            match evtspagemsg with
            | EventsPage.Msg.RowClicked s ->
                let webModel, cmd = WebPage.init s
                { model with WebPageModel = Some webModel},  (Cmd.map WebPageMsg cmd)
            | _ ->
                let newModel, cmd = EventsPage.update evtspagemsg (model.EventsPageModel.Value)
                { model with EventsPageModel = Some newModel }, (Cmd.map EventsPageMsg cmd)

        |_ -> model, Cmd.none

    let getPages allPages =
        let mainPage = allPages.MainPage
        let vidsPage = allPages.VidsPage
        let tweetPage = allPages.TweetPage
        let fbPage = allPages.FBPage
        let webPage = allPages.WebPage
        let instaPage = allPages.InstaPage
        let eventsPage = allPages.EventsPage

        match vidsPage,tweetPage, fbPage, instaPage, webPage, eventsPage with
        | None,None,None,None,None,None       -> [ mainPage ]
        | Some vPage,_,_,_,_,_          -> [ mainPage; vPage ]
        |_, Some tPage,_,_,_,_          -> [ mainPage; tPage ]
        |_,_, Some fPage,_,_,_          -> [ mainPage; fPage ]
        |_,_,_, Some iPage,_,_          -> [ mainPage; iPage ]
        |_,_,_,_, Some wPage,_          -> [ mainPage; wPage ]
        |_,_,_,_,_, Some ePage          -> [ mainPage; ePage ]
        |_                            -> [ mainPage ]

    let view (model: Model) (dispatch : Msg -> unit ) =
        let vidsPage =
            model.VidsPageModel
            |> Option.map (fun eModel -> VidPage.view eModel (VidPageMsg >> dispatch))
        let tweetPage =
            model.TweetPageModel
            |> Option.map (fun eModel -> TweetPage.view eModel (TweetPageMsg >> dispatch))
        let fbPage =
            model.FBPageModel
            |> Option.map (fun eModel -> WebPage.view eModel (FBPageMsg >> dispatch))
        let webPage =
            model.WebPageModel
            |> Option.map (fun eModel -> WebPage.view eModel (WebPageMsg >> dispatch))
        let instaPage =
            model.InstaPageModel
            |> Option.map (fun eModel -> InstaPage.view eModel (InstaPageMsg >> dispatch))
        let eventsPage =
            model.EventsPageModel
            |> Option.map (fun eModel -> EventsPage.view eModel (EventsPageMsg >> dispatch))

        let mainPage =
            View.ContentPage (
                title = "Omar Music",
                backgroundColor = Color.Black,
                content =
                    View.RelativeLayout(
                        children =
                            [ View.Image( source =  ImagePath "background.jpeg", aspect = Aspect.AspectFill )
                                  .XConstraint(Constraint.RelativeToParent(fun parent -> 0.0))
                                  .WidthConstraint(Constraint.RelativeToParent(fun parent -> parent.Width))
                                  .HeightConstraint(Constraint.RelativeToParent(fun parent -> parent.Height))
                              View.Grid(
                                  coldefs = [ Star; Star; Star; Star ],
                                  rowdefs = [ Star; Absolute 60.; Absolute 60.; Absolute 60.; Absolute 50. ],
                                  children = [
                                      
                                      View.ImageButton( source = ImagePath "calendar", command = (fun () -> dispatch (ShowEvents ))).Row(1).Column(1).ColumnSpan(3)
                                      View.ImageButton( source = ImagePath "headphones", command = (fun _ -> dispatch (ShowWeb "https://soundcloud.com/search?q=omar%20lye%20fook") )).Row(2).Column(1).ColumnSpan(3)
                                      View.ImageButton( source = ImagePath "video", command = (fun () -> dispatch ShowVids )).Row(3).Column(1).ColumnSpan(3)

                                      View.ImageButton( source = ImagePath "website", command = (fun () -> dispatch (ShowWeb "https://omarlyefook.bandcamp.com/"))).Row(4).Column(0)
                                      View.ImageButton( source = ImagePath "twitter", command = (fun () -> dispatch (ShowTweets ))).Row(4).Column(1)
                                      View.ImageButton( source = ImagePath "insta",command = (fun () -> dispatch (ShowInsta ))).Row(4).Column(2)
                                      View.ImageButton( source = ImagePath "fb",command = (fun () -> dispatch (ShowFB ))).Row(4).Column(3)
                                ]
                            )
                            .XConstraint(Constraint.RelativeToParent(fun parent -> 0.0))
                            .WidthConstraint(Constraint.RelativeToParent(fun parent -> parent.Width))
                            .HeightConstraint(Constraint.RelativeToParent(fun parent -> parent.Height))
                        ]
                    )
            )
            
        let pages = { MainPage = mainPage
                      VidsPage = vidsPage
                      TweetPage = tweetPage
                      FBPage = fbPage
                      WebPage = webPage
                      InstaPage = instaPage
                      EventsPage = eventsPage
                    } |> getPages

        View.NavigationPage (
            popped = (fun _ -> dispatch NavigationPopped),
            pages = pages
        )
    // Note, this declaration is needed if you enable LiveUpdate
    let program = XamarinFormsProgram.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


