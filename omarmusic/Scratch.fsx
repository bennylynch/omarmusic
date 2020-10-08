#r"/Users/benlynch/.nuget/packages/fsharp.data/3.3.3/lib/netstandard2.0/FSharp.Data.dll"
#r"System.Web"
open FSharp.Data
open System.Web

let url = "https://www.youtube.com/get_video_info?video_id=dOudW5KN1Z4"

let getInfo = async {
    let! resp = Http.AsyncRequestString (url = url)
    return resp
} 

let decode s = 
    HttpUtility.UrlDecode s

let a = getInfo |> Async.RunSynchronously

let dict' = a.Split("&") 
            |> Array.map(fun ntry -> let bits = ntry.Split("=")
                                     bits.[0], bits.[1])
            |> dict

let stuff = dict'.["player_response"] |> decode

type json = JsonProvider<"./json/playerResponse.json">

let j = json.Parse(stuff)

j.StreamingData.Formats |> Array.iter (fun fmt -> printfn "url %s" fmt.Url )


//System.IO.File.WriteAllText("./out.txt", stuff)