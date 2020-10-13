namespace omarmusic

open System
open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.InputTypes
open Fabulous.XamarinForms.InputTypes.Image
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open Fabulous.XamarinForms.VideoManager
open omarmusic.Models
open FSharp.Data
open System
open System.IO
open System.Web
open Google.Apis.Auth
open Google.Apis
open Google.Apis.Auth.OAuth2
open omarmusic.Models
open Xamarin.Forms.PlatformConfiguration.AndroidSpecific
open Xamarin.Forms.PlatformConfiguration


module SoundsPage =

    type Model = 
       {
        PlayerUrl : string option
        Sounds : soundListEntry.Root []
       }

    type Msg =
        | SoundsRequestComplete of soundListEntry.Root []
        | SoundSelected of string

    let getSounds =
        async {
            do! Async.SwitchToThreadPool()
            let url = "https://raw.githubusercontent.com/bennylynch/omarmusic/main/omarmusic/json/sounds.json"
            let! resp = Http.AsyncRequestString (url = url)
            return SoundsRequestComplete (soundListEntry.Parse resp)
        } |> Cmd.ofAsyncMsg

    let init () = {PlayerUrl = None; Sounds = [||]} , getSounds 
    
    let update msg model =
        match msg with
        | SoundsRequestComplete sounds ->
            {model with Sounds = sounds }, Cmd.none
        | SoundSelected url ->
            { model with PlayerUrl = Some url }, Cmd.none
        |_ -> model, Cmd.none

    let view (model : Model) dispatch =
        let player url =
            (View.MediaElement(
                    source = Media.fromPath url, //(model.PlayerUrl.Value),
                    showsPlaybackControls = true,
                    height = 200.,
                    autoPlay = true
            ))
        let soundsList (sounds : soundListEntry.Root []) =
                 View.ListView(
                     hasUnevenRows = true,
                     backgroundColor = Color.Black,
                     margin = Thickness 10.,
                     items = [ for sound in sounds ->
                                     View.ViewCell (
                                         height = 100.,
                                         view =
                                             View.Grid(
                                                 height = 100.,
                                                 coldefs = [ Absolute 100.; Star ],
                                                 rowdefs = [ Absolute 15.; Star ],
                                                 children = [
                                                     View.Label(text = sound.Title, fontAttributes = FontAttributes.Bold, textColor = Color.White).Column(1).Row(0)
                                                     View.Label(text = sound.Description, textColor = Color.White).Column(1).Row(1).RowSpan(2)
                                                     View.Image(source = ImagePath sound.Thumbnail, verticalOptions = LayoutOptions.FillAndExpand, horizontalOptions = LayoutOptions.FillAndExpand).Column(0).RowSpan(2)
                                                 ]
                                             ),
                                         tapped = (fun _ -> dispatch (SoundSelected sound.Url))
                                     )
                             ]
                     )
        (View.ContentPage (
            backgroundColor = Color.Black,
            content =
                View.RelativeLayout(
                    children =
                        [ View.Image( source =  ImagePath "background", aspect = Aspect.AspectFill )
                              .XConstraint(Constraint.RelativeToParent(fun parent -> 0.0))
                              .WidthConstraint(Constraint.RelativeToParent(fun parent -> parent.Width))
                              .HeightConstraint(Constraint.RelativeToParent(fun parent -> parent.Height))
                          View.StackLayout(
                              
                              children = match model.PlayerUrl with
                                         | None ->
                                             [ soundsList model.Sounds ]
                                         | Some _ ->
                                             [ 
                                               (player model.PlayerUrl.Value)
                                               soundsList model.Sounds
                                             ]
                        )
                        .XConstraint(Constraint.RelativeToParent(fun _ -> 0.0))
                        .WidthConstraint(Constraint.RelativeToParent(fun  parent -> parent.Width))
                        .HeightConstraint(Constraint.RelativeToParent(fun parent -> parent.Height))
                    ]
                )
        )).HasNavigationBar(true).HasBackButton(true)