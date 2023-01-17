namespace System.Text.Json

open System.Collections.Generic

[<AutoOpen>]
/// FSharp style syntactic sugar helpers for System.Text.Json
module FSharpExtensions = 

    /// Nodes.JsonNode match case if it is a Nodes.JsonObject
    let (|Object|_|) (input : Nodes.JsonNode) = 
        try input.AsObject() |> Some with 
        | _ -> None

    /// Nodes.JsonNode match case if it is a Nodes.JsonValue
    let (|Value|_|) (input : Nodes.JsonNode) = 
        try input.AsValue() |> Some with 
        | _ -> None

    /// Nodes.JsonNode match case if it is a Nodes.JsonArray
    let (|Array|_|) (input : Nodes.JsonNode) = 
        try input.AsArray() |> Some with 
        | _ -> None

    /// Helper functions for JsonObject
    module JsonObject =

        /// Returns true, if the given JsonObject has a field with the given key
        let hasField (k : string) (o : Nodes.JsonObject) =
            o.ContainsKey k

        /// Returns a list of the fields of JsonObject as KeyValuePairs
        let getFields (o : Nodes.JsonObject) =
            o
            |> Seq.toList

        /// Returns the value, if the given JsonObject has a field with the given key, else returns None
        let tryGetField (k : string) (o : Nodes.JsonObject) =
            let b,v = o.TryGetPropertyValue(k)
            if b then Some v else None
    
        /// Creates a JsonObject from a list of fields
        let ofFields (fields : list<KeyValuePair<string,Nodes.JsonNode>>) =
            fields
            |> List.map (fun kv ->
                let v = Nodes.JsonNode.Parse(kv.Value.ToJsonString())
                KeyValuePair.Create(kv.Key,v))
            |> Nodes.JsonObject

        /// Creates a new JsonObject, containing the union of the fields of both given JsonObjects, but with any key appearing at most once
        let mergeDistinct (o1 : Nodes.JsonObject) (o2 : Nodes.JsonObject) =
            getFields o1 
            |> List.append (getFields o2)
            |> List.distinctBy (fun kv -> kv.Key)
            |> ofFields

        /// Creates a new JsonObject, with the mapping operation applied on each field value of the given JsonObject
        let mapFieldValues (mapping : Nodes.JsonNode -> Nodes.JsonNode) (o : Nodes.JsonObject) =
            getFields o 
            |> List.map (fun kv -> 
                let v = 
                    mapping kv.Value
                    |> fun v -> v.ToJsonString()
                    |> Nodes.JsonNode.Parse
                KeyValuePair.Create(kv.Key,v))
            |> Nodes.JsonObject

        /// Creates a new JsonObject, with the mapping operation applied on only those field value of the given JsonObject for whichs keys the predicate returned true 
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

    /// Helper functions for JsonValue
    module JsonValue = 
    
        /// Returns the value as string
        let asString (v : Nodes.JsonValue) =
            let b,v = v.TryGetValue()
            string v

        /// Returns the value as string if possible, else returns None
        let tryAsString (v : Nodes.JsonValue) =
            let b,v = v.TryGetValue()
            try v |> string |> Some with | _ -> None

        /// Returns the value as float if possible, else returns None
        let tryAsFloat (v : Nodes.JsonValue) =
            let b,v = v.TryGetValue()
            try v |> float |> Some with | _ ->None

        /// Returns the value as int if possible, else returns None
        let tryAsInt (v : Nodes.JsonValue) =
            let b,v = v.TryGetValue()
            try v |> int |> Some with | _ -> None

    /// Helper functions for JsonArray
    module JsonArray = 

        /// Returns all the elements of the JsonArray
        let getElements (a : Nodes.JsonArray) =
            a
            |> Seq.toList

        /// Returns a new JsonArray with the mapping applied to all the elements of the given JsonArray
        let map (mapping : Nodes.JsonNode -> Nodes.JsonNode) (a : Nodes.JsonArray) = 
            a
            |> Seq.map mapping
            |> Seq.toArray
            |> Nodes.JsonArray

    [<AutoOpen>]
    module JsonNode =
        /// Cast any child object of JsonNode to a JsonNode
        let inline node (n : #Nodes.JsonNode) : Nodes.JsonNode =
            n :> Nodes.JsonNode