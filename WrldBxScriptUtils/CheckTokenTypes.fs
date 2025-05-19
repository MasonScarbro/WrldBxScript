module CheckTokenTypes

open System
open System.IO

let readTokenTypesFromFile (filePath: string) : string list = 
    if File.Exists(filePath) then
        File.ReadAllLines(filePath) |> Array.toList
    else
        printfn "File not found: %s" filePath
        []


let containsToken (tokenToCheck: string) : bool =
    let filePath = @"C:\Users\Admin\source\repos\WrldBxScript\WrldBxScript\TokenType.cs"
    let usedTokenTypes = readTokenTypesFromFile(filePath)
    usedTokenTypes |> List.exists (fun token ->  token.Contains(tokenToCheck.ToUpper()))



