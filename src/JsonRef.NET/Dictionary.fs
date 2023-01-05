namespace JsonRef.NET

open System.Collections.Generic

module Dictionary =

    let tryGet (key : 'Key) (dict : Dictionary<'Key,'Value>) =
        let b,v = dict.TryGetValue(key)
        if b then Some v else None

    let addMerge (mergeRule : 'Value -> 'Value -> 'Value) (key : 'Key) (value : 'Value) (dict : Dictionary<'Key,'Value>) =
        match tryGet key dict with
        | Some oldValue -> 
            let mergeValue = mergeRule oldValue value
            dict.Remove(key) |> ignore
            dict.Add(key,mergeValue)
        | None ->
            dict.Add(key,value)