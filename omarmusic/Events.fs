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


module EventsPage =

    type Model = 
       {
        Events : eventListEntry.Root []
       }

    type Msg =
        | EventsRequestComplete of eventListEntry.Root []
        | RowClicked of string

    let getEvents =
        async {
            do! Async.SwitchToThreadPool()
            let url = "https://raw.githubusercontent.com/bennylynch/omarmusic/main/omarmusic/json/events.json"
            let! resp = Http.AsyncRequestString (url = url)
            return EventsRequestComplete (eventListEntry.Parse resp)
        } |> Cmd.ofAsyncMsg

    let init () = {Events = [||]} , getEvents 
    
    let update msg model =
        match msg with
        | EventsRequestComplete evts ->
            {model with Events = evts }, Cmd.none
        |_ -> model, Cmd.none

    let view (model : Model) dispatch =
        View.ContentPage(
            content =
                View.ListView(
                    hasUnevenRows = true,
                    margin = Thickness 10.,
                    items = [ for event in model.Events ->
                                    View.ViewCell (
                                        height = 100.,
                                        view =
                                            View.Grid(
                                                height = 100.,
                                                coldefs = [ Absolute 100.; Star ],
                                                rowdefs = [ Absolute 15.; Absolute 50.; Star ],
                                                children = [
                                                    View.Label(text = event.Title, fontAttributes = FontAttributes.Bold).Column(1).Row(0)
                                                    View.Label(text = event.Description).Column(1).Row(1).RowSpan(2)
                                                    View.Label(text = event.TicketUrl ).Column(1).Row(2).RowSpan(2)
                                                    View.Image(source = ImagePath event.Thumbnail, backgroundColor = Color.Black, verticalOptions = LayoutOptions.FillAndExpand, horizontalOptions = LayoutOptions.FillAndExpand).Column(0).RowSpan(2)
                                                ]
                                            ),
                                        tapped = (fun _ -> dispatch ( RowClicked event.TicketUrl ))
                                    )
                            ]
                    )
        )