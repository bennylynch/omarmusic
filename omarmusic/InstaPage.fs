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


module InstaPage =

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

    let view (model: Model) dispatch =
        View.ContentPage(
            View.WebView(source = WebViewSource.op_Implicit "https://www.instagram.com/omar_lyefook/")
        ).HasNavigationBar(true).HasBackButton(true)