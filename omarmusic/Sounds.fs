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
        Model : string option
       }

    type Msg =
        | EventsRequestComplete of eventListEntry.Root []
        | RowClicked of string

    let getSounds =
        async {
            do! Async.SwitchToThreadPool()
            let url = "https://raw.githubusercontent.com/bennylynch/omarmusic/main/omarmusic/json/sounds.json"
            let! resp = Http.AsyncRequestString (url = url)
            return EventsRequestComplete (eventListEntry.Parse resp)
        } |> Cmd.ofAsyncMsg

    let init () = {Model = None} , getSounds 
    
    let update msg model =
        match msg with
        | EventsRequestComplete evts ->
            {model with Model = Some "" }, Cmd.none
        |_ -> model, Cmd.none

    let view (model : Model) dispatch =
        View.ContentPage (
            content = View.MediaElement (
                        source = Media.fromPath "https://file-examples-com.github.io/uploads/2017/11/file_example_MP3_700KB.mp3"
            
                      )
        )