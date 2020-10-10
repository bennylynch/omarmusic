namespace omarmusic

open FSharp.Data

module Models =

    type channelResp = JsonProvider<"./json/channelResponse.json">
    type playlistResp  = JsonProvider<"./json/playlistResponse.json">
    type videoInfo = JsonProvider<"./json/playerResponse.json">
    type vidListEntry = JsonProvider<"./json/vids.json">
    type eventListEntry = JsonProvider<"./json/events.json">
    type soundListEntry = JsonProvider<"./json/sounds.json">
    //type Result = JsonProvider<"./jsons/Result.json",InferTypesFromValues=false>
    //type Request = JsonProvider<"./jsons/Request.json",InferTypesFromValues=false>
    //type Test = JsonProvider<"./jsons/Test.json">