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


module VidPage =
    type Vid =
        { Title : string
          Description : string
          Url : string
          Thumbnail : string
        }

    type Model = 
       {
        PlayerUrl : string option
        UChoobs : Vid []
        Others  : Vid []
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
            //let url = "https=//www.googleapis.com/youtube/v3/playlists?part=contentDetails%2Csnippet&maxResults=50&id=UUM-KzHZFb4Pds3ZT4QR_u3w"
            let url = "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails%2C%20snippet&playlistId=PL_iUvPl89dqP4qYEsjVbqvGAbNAeYtW6y"
            let! resp = Http.AsyncRequestString (url = url, headers = ["Authorization", sprintf "Bearer %s" accesstoken])
            return PlaylistRequestComplete (playlistResp.Parse resp)
        } |> Cmd.ofAsyncMsg
    (* https=//www.dropbox.com/s/df2d2gf1dvnr5uj/Sample_1280x720_mp4.mp4 *)
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
        
    let initModel = { UChoobs = [||]; Others = [||]; PlayerUrl = None }

    (*
       { Description = "Big Buck Bunny tells the story of a giant rabbit with a heart bigger than himself. When one sunny day three rodents rudely harass him, something snaps... and the rabbit ain't no bunny anymore! In the typical cartoon tradition he prepares the nasty rodents a comical revenge.\n\nLicensed under the Creative Commons Attribution license\nhttp=//www.bigbuckbunny.org",
          Url = [ "http=//commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4" ],
          Thumbnail = "images/BigBuckBunny.jpg",
          Title = "Big Buck Bunny"
        },
        { Description = "The first Blender Open Movie from 2006",
          Url = [ "http=//commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4" ],
          Thumbnail = "images/ElephantsDream.jpg",
          Title = "Elephant Dream"
        },
        { Description = "HBO GO now works with Chromecast -- the easiest way to enjoy online video on your TV. For when you want to settle into your Iron Throne to watch the latest episodes. For $35.\nLearn how to use Chromecast with HBO GO and more at google.com/chromecast.",
          Url = [ "http=//commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" ],
          Thumbnail = "images/ForBiggerBlazes.jpg",
          Title = "For Bigger Blazes"
        },
        { Description = "Introducing Chromecast. The easiest way to enjoy online video and music on your TV—for when Batman's escapes aren't quite big enough. For $35. Learn how to use Chromecast with Google Play Movies and more at google.com/chromecast.",
          Url = [ "http=//commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4" ],
          Thumbnail = "images/ForBiggerEscapes.jpg",
          Title = "For Bigger Escape"
        }
    *)
    let others = [|
                    {
                        Description = "Big Buck Bunny tells the story of a giant rabbit with a heart bigger than himself. When one sunny day three rodents rudely harass him, something snaps... and the rabbit ain't no bunny anymore! In the typical cartoon tradition he prepares the nasty rodents a comical revenge.\n\nLicensed under the Creative Commons Attribution license\nhttp=//www.bigbuckbunny.org";
                        Url =  "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4" ;
                        Thumbnail = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/images/BigBuckBunny.jpg";
                        Title = "Big Buck Bunny"
                    }
                    {
                        Description = "The first Blender Open Movie from 2006";
                        Url =  "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4" ;
                        Thumbnail = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/images/ElephantsDream.jpg";
                        Title = "Elephant Dream"
                    }
                    {
                        Description = "HBO GO now works with Chromecast -- the easiest way to enjoy online video on your TV. For when you want to settle into your Iron Throne to watch the latest episodes. For $35.\nLearn how to use Chromecast with HBO GO and more at google.com/chromecast.";
                        Url = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" ;
                        Thumbnail = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/images/ForBiggerBlazes.jpg";
                        Title = "For Bigger Blazes"
                    }
                    {
                        Description = "Introducing Chromecast. The easiest way to enjoy online video and music on your TV—for when Batman's escapes aren't quite big enough. For $35. Learn how to use Chromecast with Google Play Movies and more at google.com/chromecast.";
                        Url =  "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4" ;
                        Thumbnail = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/images/ForBiggerEscapes.jpg";
                        Title = "For Bigger Escape"
                    }
                |]
    let init () = initModel, getPlayList 
    
    let update msg model =
        match msg with
        | PlaylistRequestComplete  playlistResp ->
            let vidids = playlistResp.Items |> Array.map (fun itm -> itm.ContentDetails.VideoId)
            model,getUrls vidids
        | VideoInfoRequestComplete vidList ->
            { model with UChoobs = vidList; Others = others }, Cmd.none
        | VidSelected url ->
            { model with PlayerUrl = Some url }, Cmd.none
        |_ -> model, Cmd.none

    let view (model : Model) dispatch =
        let vidlist vids =
                View.ListView(
                    hasUnevenRows = true,
                    margin = Thickness 10.,
                    items = [ for vid in vids ->
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
        View.TabbedPage(
            title = "Videos",
            created = (fun target -> target.On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom) |> ignore ),
            children = [
                View.ContentPage(
                    title = "Youtube",
                    content =
                        View.StackLayout (
                            children =
                                match model.PlayerUrl with
                                | None ->
                                    [ vidlist model.UChoobs]
                                | Some _ ->
                                    [ View.VideoView(
                                            source = model.PlayerUrl.Value,
                                            showControls = true,
                                            height = 200.,
                                            autoPlay = true
                                        )
                                      (vidlist model.UChoobs)
                                    ]
                        )
                ).IconImageSource(Image.fromPath "youtube")
                View.ContentPage(
                    title = "Others",
                    content =
                        View.StackLayout (
                            children =
                                match model.PlayerUrl with
                                | None ->
                                    [ vidlist model.Others ]
                                | Some _ ->
                                    [ View.VideoView(
                                            source = model.PlayerUrl.Value,
                                            showControls = true,
                                            height = 200.,
                                            autoPlay = true
                                        )
                                      vidlist model.Others
                                    ]
                        )
                ).IconImageSource(Image.fromPath "video")
            ]
        ).HasNavigationBar(true).HasBackButton(true)