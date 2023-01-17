# JsonRef.NET
Library for resolving JSON-Ref and JSON-LD style object references. Based on System.Text.Json.

```fsharp
#r "nuget: JsonRef.NET"

open JsonRef.NET

JsonRefResolver.resolve jsonString
```

## Background

JSON-LD supports references to objects with a reserved key @id which can be used either as an object identifier or a reference to an object identifier. The value of @id must be a string. E.g. The following object: 

```json
{
      "@id" : "TheID",
      "value" : "MyValue"
}
```
might be referenced in another part of the json file as follows:

```json
{
      "@id" : "TheID"
}
```
----
## Usage

This kind of reference can be resolved with a single function call, either with a `serialized json string` as input or on a deserialized `System.Text.Json JsonNode`.

E.g. the following json object based on the example above:

```fsharp
let simpleJson = 
"""
{
    "source": {
        "@id" : "TheID",
        "value" : "MyValue"
    },
    "target": {
        "@id" : "TheID"
    }
}
"""
```

can be resolved as follows:

```fsharp
#r "nuget: JsonRef.NET"

open JsonRef.NET

JsonRefResolver.resolve simpleJson
```

resulting in: 

```json
{
    "source": {
      "@id" : "TheID",
      "value" : "MyValue"
    },
    "target": {
      "@id" : "TheID",
      "value" : "MyValue"
    }
}
```

or alternatively working on a `System.Text.Json` `JsonNode`:

```fsharp
#r "nuget: JsonRef.NET"

open JsonRef.NET
open System.Text.Json

let jsonNode = Nodes.JsonObject.Parse(simpleJson)

JsonRefResolver.resolve jsonNode
```

----
## Recursive references

Currently there is no support for autodetection of recursive references. Instead, you can give a exclusion lists for fields that should not be resolved:

```fsharp

#r "nuget: JsonRef.NET"

open JsonRef.NET

let options = JsonRefResolverOptions(IgnoreFields = [|"ignoreValue"|])

JsonRefResolver.resolve(jsonString,options)

```

----
## Resolve references across files

If you want to insert JSON-LD objects of one json string into referencing objects of another string, you can use the following approach:

```fsharp

#r "nuget: JsonRef.NET"

open JsonRef.NET
open System.Text.Json

let referencejsonNode = Nodes.JsonObject.Parse(referenceJsonString)
let targetjsonNode = Nodes.JsonObject.Parse(targetJsonString)
let options = JsonRefResolverOptions.defaultOptions

let referenceObjects = JsonRefResolver.collectObjects(referencejsonNode,options)

JsonRefResolver.fillObjects(referenceObjects,targetjsonNode,options)
|> fun s -> s.ToJsonString()
```