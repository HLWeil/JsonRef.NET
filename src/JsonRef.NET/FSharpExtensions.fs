namespace System.Text.Json

open System.Collections.Generic

[<AutoOpen>]
module FSharpExtensions = 

    let (|Object|_|) (input : Nodes.JsonNode) = 
        try input.AsObject() |> Some with 
        | _ -> None

    let (|Value|_|) (input : Nodes.JsonNode) = 
        try input.AsValue() |> Some with 
        | _ -> None

    let (|Array|_|) (input : Nodes.JsonNode) = 
        try input.AsArray() |> Some with 
        | _ -> None

    module JsonObject =

        let hasField (k : string) (o : Nodes.JsonObject) =
            o.ContainsKey k

        let getFields (o : Nodes.JsonObject) =
            o
            |> Seq.toList

        let tryGetField (k : string) (o : Nodes.JsonObject) =
            let b,v = o.TryGetPropertyValue(k)
            if b then Some v else None
    
        let ofFields (fields : list<KeyValuePair<string,Nodes.JsonNode>>) =
            fields
            |> List.map (fun kv ->
                let v = Nodes.JsonNode.Parse(kv.Value.ToJsonString())
                KeyValuePair.Create(kv.Key,v))
            |> Nodes.JsonObject

        let mergeDistinct (o1 : Nodes.JsonObject) (o2 : Nodes.JsonObject) =
            getFields o1 
            |> List.append (getFields o2)
            |> List.distinctBy (fun kv -> kv.Key)
            |> ofFields

        let mapFieldValues (mapping : Nodes.JsonNode -> Nodes.JsonNode) (o : Nodes.JsonObject) =
            getFields o 
            |> List.map (fun kv -> 
                let v = 
                    mapping kv.Value
                    |> fun v -> v.ToJsonString()
                    |> Nodes.JsonNode.Parse
                KeyValuePair.Create(kv.Key,v))
            |> Nodes.JsonObject

        let mapFieldValuesIf (predicate : string -> bool) (mapping : Nodes.JsonNode -> Nodes.JsonNode) (o : Nodes.JsonObject) =
            getFields o 
            |> List.map (fun kv -> 
                let v = 
                    if predicate kv.Key then
                        mapping kv.Value                    
                    else 
                        kv.Value
                    |> fun v -> v.ToJsonString()
                    |> Nodes.JsonNode.Parse
                KeyValuePair.Create(kv.Key,v))
            |> Nodes.JsonObject


    module JsonValue = 
    
        let asString (v : Nodes.JsonValue) =
            let b,v = v.TryGetValue()
            string v

        let tryAsString (v : Nodes.JsonValue) =
            let b,v = v.TryGetValue()
            try v |> string |> Some with | _ -> None

        let tryAsFloat (v : Nodes.JsonValue) =
            let b,v = v.TryGetValue()
            try v |> float |> Some with | _ ->None

        let tryAsInt (v : Nodes.JsonValue) =
            let b,v = v.TryGetValue()
            try v |> int |> Some with | _ -> None

    module JsonArray = 

        let getElements (a : Nodes.JsonArray) =
            a
            |> Seq.toList

        let map (mapping : Nodes.JsonNode -> Nodes.JsonNode) (a : Nodes.JsonArray) = 
            a
            |> Seq.map mapping
            |> Seq.toArray
            |> Nodes.JsonArray

    [<AutoOpen>]
    module JsonNode =
        let inline node (n : #Nodes.JsonNode) : Nodes.JsonNode =
            n :> Nodes.JsonNode