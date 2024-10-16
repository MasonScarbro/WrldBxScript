module FsCLI

open System
open System.Collections.Generic

let commands = ["--help"; "--exit"; "--version"; "--createkeyword"]

let commandExists command = 
    List.contains command commands


let handleCreateKeyword (dev: bool) = 
    printfn "Is this A Major? If So Please supply its name: (else press enter)"
    let majorName = Console.ReadLine().ToUpper();
    if dev then
        printfn "Please Enter code Snippet to create minors:"
        let mutable snippet = System.Text.StringBuilder()

        //Define
        let rec readSnippet() = 
            let line = Console.ReadLine()
            if line <> "" then
                snippet.Append(line + "\n") |> ignore
                readSnippet ()
            else
                snippet.ToString()


        

        //Define
        let cleanSnippet (input: string) =
            let cleanedInput = 
                input.Replace("]", "")
                    .Replace("[", "")
                    .Replace("}", "")
                    .Replace("{", "")

            cleanedInput.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries)
            |> Array.map (fun s -> 
                let trimmed = s.Trim()
                let index = trimmed.IndexOf('.')
                if index >= 0 then 
                    trimmed.Substring(index + 1).Trim() // Return everything after the period
                else
                trimmed
                )
            |> String.concat " " 
            
            
            
            
        let finalSnippet = cleanSnippet (readSnippet ())    
        printfn "You entered the following cleaned snippet:\n%s" finalSnippet

        let createDict (input: string) = 
            let dict = Dictionary<string, string>()
            let parts = input.Split([| ','; ';' |], StringSplitOptions.RemoveEmptyEntries)

            for part in parts do
                let trimmedPart = part.Trim()
                let keyValue = trimmedPart.Split('=')
                if keyValue.Length = 2 then
                    let key = keyValue.[0].Trim()
                    let value = keyValue.[1].Trim()
                    dict.[key] <- value
            dict

        let dict = createDict (finalSnippet)
        printfn "Resulting Dictionary:"
        dict |> Seq.iter (fun kvp -> printfn "Key: %s, Value: %s" kvp.Key kvp.Value)
        
        
           

        let buildSrc (dict: Dictionary<string, string>) = 
            let formats = [
                (fun key -> sprintf "{\"%s\", TokenType.%s},\n" (key.ToString().ToUpper()) (key.ToString().ToUpper()))
                (fun key -> sprintf "TokenType.%s," (key.ToString().ToUpper()))
                (fun key -> sprintf ",TokenType.%s" (key.ToString().ToUpper()))
                (fun _ -> "NEW_MINORS_HERE")
            ]
            let resultList = System.Collections.Generic.List<string>()
            dict.Keys
            |> Seq.iter (fun key ->
                let srcs = formats |> List.map (fun formatFunc -> formatFunc key)
                resultList.AddRange(srcs)
            )
            resultList

        printfn "Do You Plan to make an object with this? (experimental) [Y/n]"
        let yorn = Console.ReadLine()
        if yorn.Equals("Y") then

            let buildObjectSrc (majorName: string) (dict: Dictionary<string, string>) =
                let typeFromValue (value: string) =
                    match value with
                    | v when v.StartsWith("\"") && v.EndsWith("\"") -> "string"
                    | v when v.Contains("f") -> "double"
                    | v when v.Contains(".") -> "double"
                    | _ -> 
                        let mutable intResult = 0
                        if Int32.TryParse(value, &intResult) then "int"
                        else "unknown"

                let objectSrc = 
                    let props = 
                        dict |> Seq.map (fun kvp -> 
                            let key, value = kvp.Key, kvp.Value
                            let propType = typeFromValue value
                            if propType <> "unknown" then
                                sprintf "        public %s %s { get; set; }" propType key
                            else "")
                        |> Seq.filter (fun prop -> prop <> "")
                        |> String.concat "\n"

                    $"""
                    namespace WrldBxScript
                    {{
                        public class WrldBx{majorName} : IWrldBxObject
                        {{
                            public string id {{ get; set; }}
                            {props}
                            public WrldBx{majorName}(string id)
                            {{
                                this.id = id;
                            }}

                            public void UpdateStats(Token type, object value)
                            {{
                            }}
                        }}
                    }}
                    """
                objectSrc

                

            // Call the function to build the object source code
            let sourceCode = buildObjectSrc majorName dict
            printfn "Generated Source Code:\n%s" sourceCode
                
        
        printfn "Press Enter to exit."
        Console.ReadLine() |> ignore
    else
        printfn "Please Enter Each Minor Keyword: "
        let mutable minorKeywords = new List<string>()

        let mutable input = Console.ReadLine() // Initialize with the first input
        while not (String.IsNullOrWhiteSpace(input)) do // Loop until the input is blank
            minorKeywords.Add(input)
            input <- Console.ReadLine()

            

            
        
[<EntryPoint>]
let main args =
    
    printfn "Total arguments received: %d" args.Length
    if args.Length > 0 then
        let mutable i = 0
        while i < args.Length do
            printfn "args: %s" args.[i]
            match args.[i] with
            | "--help" -> printfn "Help: Available commands are --help, --exit, --version, --createkeyword"
            | "--version" -> printfn "0.1.0"
            | "--exit" -> printfn "Exiting..."; Environment.Exit(0)
            | "--createkeyword" -> 
                if i + 1 < args.Length && args.[i + 1] = "-d" then
                    printfn "args: %s" args.[i + 1]
                    
                    handleCreateKeyword (true)
                    i <- i + 1 // Increment i to skip the -d argument
                else
                    handleCreateKeyword (false)
            
            | _ -> printfn "Unknown command: %s" args.[i]
            i <- i + 1
    else
        printfn "????"

    0
        
    
