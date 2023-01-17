namespace JsonRef.NET

open System.Text.Json

/// Options for Json-LD reference resolver
type JsonRefResolverOptions() = 
    
    let mutable ignoreFields : string [] = [||]
    let mutable objectMerger = JsonObject.mergeDistinct
    let mutable sourceField = "@id"
    let mutable targetField = "@id"

    /// Exclusion list for field names to ignore: Any referencing object, that is a value for a field with a key in this exclusion list will not be resolved with the full object
    member this.IgnoreFields 
        with get() = ignoreFields 
        and set(fieldsToIgnore) = ignoreFields <- fieldsToIgnore

    /// Rule specifying how to merge two objects sharing the same reference key
    member this.ObjectMerger 
        with get() = objectMerger 
        and set(merger) = objectMerger <- merger

    /// Name of the key specifying the identity of a referenced object
    member this.SourceField 
        with get() = sourceField 
        and set(source) = sourceField <- source

    /// Name of the key specifying the identity of a referencing object
    member this.TargetField 
        with get() = targetField 
        and set(target) = targetField <- target

    /// Default options for Json-LD reference resolver with no exclusion list, basic union object merger and @id as both source and target field name
    static member defaultOptions = JsonRefResolverOptions()