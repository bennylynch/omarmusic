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


module TweetPage =

    type Model = 
       {
        tweet : string
       }

    type Msg =
        | Nothing
        

    let initModel = { tweet = "" }

    let init () = initModel, Cmd.none 
    
    let update msg model =
        match msg with
        |_ -> model, Cmd.none
    let html = """<!DOCTYPE html>
    <html>
    <head>
    </head>
    <body>
        <a class="twitter-timeline" href="https://twitter.com/omarlyefookMBE?ref_src=twsrc%5Etfw">Tweets by omarlyefookMBE</a>
        <script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>
    </body>
    </html>"""

    let view _ dispatch =
        let htmlSrc = new HtmlWebViewSource()
        htmlSrc.Html <- html
        View.ContentPage(
            View.WebView(source = htmlSrc)
        ).HasNavigationBar(true).HasBackButton(true)