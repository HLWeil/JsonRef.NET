namespace JsonRef.NET

open System.Text.Json

type JsonRefResolverOptions() = 
    
    let mutable ignoreFields : string [] = [||]
    let mutable objectMerger = JsonObject.mergeDistinct
    let mutable sourceField = "@id"
    let mutable targetField = "@id"

    member this.IgnoreFields 
        with get() = ignoreFields 
        and set(fieldsToIgnore) = ignoreFields <- fieldsToIgnore

    member this.ObjectMerger 
        with get() = objectMerger 
        and set(merger) = objectMerger <- merger

    member this.SourceField 
        with get() = sourceField 
        and set(source) = sourceField <- source

    member this.TargetField 
        with get() = targetField 
        and set(target) = targetField <- target

    static member defaultOptions = JsonRefResolverOptions()