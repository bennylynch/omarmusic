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


module UChoobPage =
    type Vid =
        { Title : string
          Description : string
          Url : string
          Thumbnail : string
        }

    type Model = 
       {
        PlayerUrl : string option
        Vids : Vid []
       }

    type Msg =
        | PlaylistRequestComplete of playlistResp.Root
        | VideoInfoRequestComplete of Vid []
        | VidSelected of string

    // OAuth2 authentication using service account JSON file
    let credentials =
        let jsonServiceAccount = sprintf "./service_account.json" //(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles))  
        use stream = new FileStream(jsonServiceAccount, FileMode.Open, FileAccess.Read)
        
        GoogleCredential.FromStream(stream).CreateScoped([|"https://www.googleapis.com/auth/youtube.readonly"|])
    
    let getPlayList  = 
        async {
            do! Async.SwitchToThreadPool()
            let! accesstoken = credentials.UnderlyingCredential.GetAccessTokenForRequestAsync() |> Async.AwaitTask //"AIzaSyAOlscWKPYKk4tZbjMzjA_zwP_8Bfmx9u4"
            //let url = "https://www.googleapis.com/youtube/v3/playlists?part=contentDetails%2Csnippet&maxResults=50&id=UUM-KzHZFb4Pds3ZT4QR_u3w"
            let url = "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails%2C%20snippet&playlistId=PL_iUvPl89dqP4qYEsjVbqvGAbNAeYtW6y"
            let! resp = Http.AsyncRequestString (url = url, headers = ["Authorization", sprintf "Bearer %s" accesstoken])
            return PlaylistRequestComplete (playlistResp.Parse resp)
        } |> Cmd.ofAsyncMsg

    let getUrls (vidIds : string []) =
        try
            let urls = vidIds |> Array.map (fun vidId ->
                                    async {
                                        let url = sprintf "https://www.youtube.com/get_video_info?video_id=%s" vidId
                                        let! resp = Http.AsyncRequestString (url = url)
                                        let dict' = resp.Split([|'&'|])
                                                    |> Array.map(fun ntry -> let bits = ntry.Split([|'='|])
                                                                             bits.[0], bits.[1])
                                                    |> dict
                                        let stuff = dict'.["player_response"] |> HttpUtility.UrlDecode
                                        let j = videoInfo.Parse(stuff)
                                        return j //{Title = j.VideoDetails.Title; Url = j.StreamingData.Formats.[0].Url}
                                        }) |> Async.Parallel
            let jsons = urls |> Async.RunSynchronously |> Array.filter (fun j -> j.PlayabilityStatus.Status <> "UNPLAYABLE")


            let vids = jsons |> Array.map (fun j -> {
                    Title = j.VideoDetails.Title
                    Url = j.StreamingData.Formats.[0].Url
                    Thumbnail  = j.VideoDetails.Thumbnail.Thumbnails.[0].Url
                    Description = j.VideoDetails.ShortDescription }
            )
            Cmd.ofMsg (VideoInfoRequestComplete vids)
            //urls |> Cmd.ofAsyncMsg |> Cmd.map VideoInfoRequestComplete
        with
        |e -> failwith e.Message
        

    let initModel = { Vids = [||]; PlayerUrl = None }

    let init () = initModel, getPlayList 
    
    let update msg model =
        match msg with
        | PlaylistRequestComplete  playlistResp ->
            let vidids = playlistResp.Items |> Array.map (fun itm -> itm.ContentDetails.VideoId)
            model,getUrls vidids
        | VideoInfoRequestComplete vidList ->
            { model with Vids = vidList }, Cmd.none
        | VidSelected url ->
            { model with PlayerUrl = Some url }, Cmd.none
        |_ -> model, Cmd.none

    let view (model: Model) dispatch =
        let vidlist = View.ListView(
                        hasUnevenRows = true,
                        margin = Thickness 10.,
                        
                        items = [ for vid in model.Vids ->
                                        View.ViewCell (
                                            height = 100.,
                                            view =
                                                View.Grid(
                                                    height = 100.,
                                                    coldefs = [ Absolute 100.; Star ],
                                                    rowdefs = [ Absolute 15.; Star ],
                                                    children = [
                                                        View.Label(text = vid.Title, fontAttributes = FontAttributes.Bold).Column(1).Row(0)
                                                        View.Label(text = vid.Description).Column(1).Row(1).RowSpan(2)
                                                        View.Image(source = ImagePath vid.Thumbnail, verticalOptions = LayoutOptions.FillAndExpand, horizontalOptions = LayoutOptions.FillAndExpand).Column(0).RowSpan(2)
                                                    ]
                                                ),
                                            tapped = (fun _ -> dispatch (VidSelected vid.Url))
                                        )
                                ]
                        )
        View.ContentPage(
            title = "Vids",
            content =
                View.StackLayout (
                    children =
                        match model.PlayerUrl with
                        | None ->
                            [ vidlist ]
                        | Some _ ->
                            [ View.VideoView(
                                    source = model.PlayerUrl.Value,
                                    showControls = true,
                                    height = 200.,
                                    autoPlay = true
                                )
                              vidlist
                            ]
                )
        ).HasNavigationBar(true).HasBackButton(true)//.BackgroundImageSource(ImagePath "background.jpeg")