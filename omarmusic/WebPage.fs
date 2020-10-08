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


module WebPage =

    type Model = { Url : string }
       
    type Msg = None
    let init url = {Url = url} , Cmd.none 
    
    let update msg model =
        match msg with
        |_ -> model, Cmd.none
    
    let view (model: Model) dispatch =
        View.ContentPage(
            View.WebView(source = WebViewSource.op_Implicit model.Url )
        ).HasNavigationBar(true).HasBackButton(true)