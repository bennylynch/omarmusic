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
        | SoundssRequestComplete of soundListEntry.Root []
        | SoundSelected of string

    let getSounds =
        async {
            do! Async.SwitchToThreadPool()
            let url = "https://raw.githubusercontent.com/bennylynch/omarmusic/main/omarmusic/json/sounds.json"
            let! resp = Http.AsyncRequestString (url = url)
            return SoundssRequestComplete (soundListEntry.Parse resp)
        } |> Cmd.ofAsyncMsg

    let init () = {PlayerUrl = None; Sounds = [||]} , getSounds 
    
    let update msg model =
        match msg with
        | SoundssRequestComplete sounds ->
            {model with Sounds = sounds }, Cmd.none
        | SoundSelected url ->
            { model with PlayerUrl = Some url }, Cmd.none
        |_ -> model, Cmd.none

    let view (model : Model) dispatch =
        let soundsList (sounds : soundListEntry.Root []) =
                 View.ListView(
                     hasUnevenRows = true,
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
                                                     View.Label(text = sound.Title, fontAttributes = FontAttributes.Bold).Column(1).Row(0)
                                                     View.Label(text = sound.Description).Column(1).Row(1).RowSpan(2)
                                                     View.Image(source = ImagePath sound.Thumbnail, verticalOptions = LayoutOptions.FillAndExpand, horizontalOptions = LayoutOptions.FillAndExpand).Column(0).RowSpan(2)
                                                 ]
                                             ),
                                         tapped = (fun _ -> dispatch (SoundSelected sound.Url))
                                     )
                             ]
                     )
        View.TabbedPage(
            title = "Sounds",
            created = (fun target -> target.On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom) |> ignore ),
            tabIndex = 1,
            children = [
                View.ContentPage(
                    title = "Others",
                    content =
                        View.StackLayout (
                            children =
                                match model.PlayerUrl with
                                | None ->
                                    [ soundsList model.Sounds ]
                                | Some _ ->
                                    [ View.MediaElement(
                                            source = Media.fromPath (model.PlayerUrl.Value),
                                            //showControls = true,
                                            height = 200.,
                                            autoPlay = true
                                        )
                                      soundsList model.Sounds
                                    ]
                        )
                ).IconImageSource(Image.fromPath "vid")
                View.ContentPage(
                    title = "Youtube",
                    content =
                        View.StackLayout (
                            children = [
                                View.Label ( text = "label")
                            ]
                        )
                ).IconImageSource(Image.fromPath "youtube")
                
            ]
        ).HasNavigationBar(true).HasBackButton(true)