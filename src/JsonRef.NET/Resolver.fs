namespace JsonRef.NET

open System.Collections.Generic
open System.Text.Json

type JsonRefResolver =

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
                | None ->
                    JsonObject.getFields o
                    |> List.iter (fun kv -> collect kv.Value)
            | Array a -> 
                JsonArray.getElements a
                |> List.iter collect
            | _ ->  ()
        collect n
        mapping

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
                    printfn "%s" k
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

    static member resolve(n : Nodes.JsonNode,?Options : JsonRefResolverOptions) =
        let options = Options |> Option.defaultValue JsonRefResolverOptions.defaultOptions
        let mapping = JsonRefResolver.collectObjects(n,options)
        JsonRefResolver.fillObjects(mapping,n,options)

    static member resolve(n : string,?Options : JsonRefResolverOptions) =
        let options = Options |> Option.defaultValue JsonRefResolverOptions.defaultOptions
        let n = Nodes.JsonObject.Parse(n)
        let mapping = JsonRefResolver.collectObjects(n,options)
        JsonRefResolver.fillObjects(mapping,n,options)
        |> fun n -> n.ToJsonString()
