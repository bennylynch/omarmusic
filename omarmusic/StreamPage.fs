namespace omarmusic

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.InputTypes
open Fabulous.XamarinForms.InputTypes.Image
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open omarmusic.Models
open FSharp.Data
open System
open System.IO
open System.Web


module StreamsPage =

    type Model = 
       {
        eventid : string
       }

    type Msg =
        | Nothing
        

    //let initModel = { tweet = "" }

    let init () = {eventid = ""}, Cmd.none 
    
    let update msg model =
        match msg with
        |_ -> model, Cmd.none

    let html = """<!DOCTYPE html>
    <html>
    <head>
    </head>
    <body>
        <embed src='https://vimeo.com/event/31282/embed?autoplay=1' width='100%' height='600px' frameborder = '0'
               allow = 'autoplay' ></embed>
        <!--embed src="https://minnit.chat/TheMagnificentChat?embed&&nickname="
               width='100%' height='600px'  allowTransparency="true"></embed-->
    </body>
    </html>"""
    let view (model) dispatch =
        let htmlSrc = new HtmlWebViewSource()
        htmlSrc.Html <- html
        View.ContentPage(
            View.WebView(source = htmlSrc)
        ).HasNavigationBar(true).HasBackButton(true)