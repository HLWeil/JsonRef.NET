module JsonLDTests

open Expecto

open JsonRef.NET
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq

module Expect = 

    let deepEquals (acturalJson : string) (expectedJson : string) =
        let jToken1 = JsonConvert.DeserializeObject<JObject> acturalJson 
        let jToken2 = JsonConvert.DeserializeObject<JObject> expectedJson
        let equals = JToken.DeepEquals(jToken1,jToken2)
        Expect.isTrue equals $"Json expected \"{expectedJson}\", bot got \"{acturalJson}\" did not match."

[<Tests>]
let testJsonRefResolver = 
    
    let testFileFolder = Path.Combine(__SOURCE_DIRECTORY__,"JsonLDFiles")

    testList "JsonRefResolverTests" [
        testCase "BasicExample" (fun () -> 
            let file = Path.Combine(testFileFolder, "basicExample.json")
            let expectedResultFile = Path.Combine(testFileFolder, "basicExampleResult.json")

            let actualString =
                File.ReadAllText file
                |> JsonRefResolver.resolve

            let expectedString = 
                File.ReadAllText expectedResultFile

            Expect.deepEquals actualString expectedString
        )
        testCase "NestedObject" (fun () -> 
            let file = Path.Combine(testFileFolder, "nestedExample.json")
            let expectedResultFile = Path.Combine(testFileFolder, "nestedExampleResult.json")

            let actualString =
                File.ReadAllText file
                |> JsonRefResolver.resolve

            let expectedString = 
                File.ReadAllText expectedResultFile

            Expect.deepEquals actualString expectedString
        )
        testCase "IgnoreFiled" (fun () -> 
            let file = Path.Combine(testFileFolder, "ignoreRecursiveField.json")
            let expectedResultFile = Path.Combine(testFileFolder, "ignoreRecursiveFieldResult.json")

            let options = JsonRefResolverOptions(IgnoreFields = [|"ignoreValue"|])

            let actualString =
                File.ReadAllText file
                |> fun s -> JsonRefResolver.resolve (s,options)

            let expectedString = 
                File.ReadAllText expectedResultFile

            Expect.deepEquals actualString expectedString
        )
    ]
