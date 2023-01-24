module JsonFilterTests

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
    
    let testFileFolder = Path.Combine(__SOURCE_DIRECTORY__,"JsonFilterFiles")

    testList "JsonFilterTests" [
        testCase "BasicExample" (fun () -> 
            let file = Path.Combine(testFileFolder, "filterEmptyFieldsExample.json")
            let expectedResultFile = Path.Combine(testFileFolder, "filterEmptyFieldsExampleResult.json")

            let actualString =
                File.ReadAllText file
                |> JsonRefResolver.filterEmptyNodes

            let expectedString = 
                File.ReadAllText expectedResultFile

            Expect.deepEquals actualString expectedString
        )
    ]