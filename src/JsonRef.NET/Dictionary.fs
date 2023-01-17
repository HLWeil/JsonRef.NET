namespace JsonRef.NET

open System.Collections.Generic

module Dictionary =

    /// If the dictionary contains a value with the given key, returns it, else returns none
    let tryGet (key : 'Key) (dict : Dictionary<'Key,'Value>) =
        let b,v = dict.TryGetValue(key)
        if b then Some v else None

    /// Tries to add the value with the given key to the dictionary. If the dictionary already contains a value with the given key, applies the given merge operation on the old and the new value and then replaces the old value in the dictionary with the merged value.
    let addMerge (mergeRule : 'Value -> 'Value -> 'Value) (key : 'Key) (value : 'Value) (dict : Dictionary<'Key,'Value>) =
        match tryGet key dict with
        | Some oldValue -> 
            let mergeValue = mergeRule oldValue value
            dict.Remove(key) |> ignore
            dict.Add(key,mergeValue)
        | None ->
            dict.Add(key,value)