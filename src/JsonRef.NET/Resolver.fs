namespace JsonRef.NET

open System.Collections.Generic
open System.Text.Json

/// Functions for resolving Json-LD style @id references for serialized json strings and System.Text.Json JsonNode objects
type JsonRefResolver =

    /// Collects all child objects of the given JsonNode, which contain a field with the key @id. Stores them as values in a dictionary, with the keys being the values of their @id fields
    static member collectObjects (n : Nodes.JsonNode,options : JsonRefResolverOptions) = 
        let mapping : Dictionary<string,Nodes.JsonObject> = Dictionary()
        let rec collect (n : Nodes.JsonNode) =
            match n with
            | Object o -> 
                let f = JsonObject.tryGetField options.SourceField o
                match f with
                | Some v ->
                    let k = 
                        v.AsValue()
                        |> JsonValue.asString                   
                    Dictionary.addMerge options.ObjectMerger k o mapping
                | None -> ()
                JsonObject.getFields o
                |> List.iter (fun kv -> collect kv.Value)
            | Array a -> 
                JsonArray.getElements a
                |> List.iter collect
            | _ ->  ()
        collect n
        mapping

    /// Replaces all child objects of the given jsonNode that contain a field with the key @id with the objects that contain the same key in the given mappings
    static member fillObjects (mappings : Dictionary<string,Nodes.JsonObject>,n : Nodes.JsonNode,options : JsonRefResolverOptions) : Nodes.JsonNode = 
        let predicate s = Array.contains s options.IgnoreFields |> not
        let rec fill (n : Nodes.JsonNode) =
            match n with
            | Object o -> 
                let f = JsonObject.tryGetField options.TargetField o
                match f with
                | Some v ->
                    let k = 
                        v.AsValue()
                        |> JsonValue.asString  
                    match Dictionary.tryGet k mappings with
                    | Some newO -> newO
                    | None -> o
                | None ->
                    o
                |> JsonObject.mapFieldValuesIf predicate fill
                |> node
            | Array a -> 
                JsonArray.map fill a
                |> node
            | Value v ->  
                v
                |> node
            | n -> n
        fill n

    /// Resolves all Json-LD style @id references in the given JsonNode by replacing all referencing objects with the objects they reference.
    static member resolve(n : Nodes.JsonNode,?Options : JsonRefResolverOptions) =
        let options = Options |> Option.defaultValue JsonRefResolverOptions.defaultOptions
        let mapping = JsonRefResolver.collectObjects(n,options)
        JsonRefResolver.fillObjects(mapping,n,options)

    /// Resolves all Json-LD style @id references in the given serialzed json string by replacing all referencing objects with the objects they reference.
    static member resolve(n : string,?Options : JsonRefResolverOptions) =
        let options = Options |> Option.defaultValue JsonRefResolverOptions.defaultOptions
        let n = Nodes.JsonObject.Parse(n)
        JsonRefResolver.resolve(n,options)
        |> fun n -> n.ToJsonString()

    /// Recursively removes all fields with empty values from the node and its subnodes
    static member filterEmptyNodes (n : Nodes.JsonNode) : Nodes.JsonNode = 
        let rec filter (n : Nodes.JsonNode) =
            match n with
            | Object o -> 
                JsonObject.getFields o 
                |> List.choose (fun kv -> 
                    if JsonNode.isEmpty kv.Value then
                        None
                    else 
                        KeyValuePair.Create(kv.Key,filter kv.Value)
                        |> Some
                    )
                |> JsonObject.ofFields
                |> node
            | Array a -> 
                JsonArray.map filter a
                |> node
            | Value v ->  
                v
                |> node
            | n -> n
        filter n

    /// Recursively removes all fields with empty values from the node and its subnodes
    static member filterEmptyNodes (n : string) : string = 
        Nodes.JsonObject.Parse(n)
        |> JsonRefResolver.filterEmptyNodes
        |> fun n -> n.ToJsonString()