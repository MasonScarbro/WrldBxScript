module HandleKeyword
open System
open System.Collections.Generic
open CheckTokenTypes
open WriteToFiles

let inQoutes (str: string) = 
    sprintf "\"%s\"" str

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
                if index >= 1 then 
                    
                    if not (Char.IsDigit(trimmed.[index - 1])) then
                        trimmed.Substring(index + 1).Trim() // Return everything after the period
                    else
                    trimmed
                else
                trimmed
                )
            |> String.concat " " 
            
            
            
            
        let finalSnippet = cleanSnippet (readSnippet ())    
        printfn "You entered the following cleaned snippet:\n%s" finalSnippet

        let typeFromValue (value: string) =
                    match value with
                    | v when v.StartsWith("\"") && v.EndsWith("\"") -> "string"
                    | v when v.Contains("f") -> "double"
                    | v when v.Contains(".") -> "double"
                    | v when v.Contains("true") or v.Contains("false") -> "bool"
                    | _ -> 
                        let mutable intResult = 0
                        if Int32.TryParse(value, &intResult) then "double"
                        else "unknown"

        let createDict (input: string) = 
            let dict = Dictionary<string, string>()
            let parts = input.Split([| ','; ';' |], StringSplitOptions.RemoveEmptyEntries)

            for part in parts do
                let trimmedPart = part.Trim()
                let keyValue = trimmedPart.Split('=')
                if keyValue.Length = 2 then
                    let key = keyValue.[0].Trim()
                    let value = keyValue.[1].Trim()
                    let propType = typeFromValue value
                    if propType <> "unknown" && not (containsToken (key)) then
                        dict.[key] <- value
            dict



        let buildSrc (dict: Dictionary<string, string>) = 

            let tokenizerSrc = System.Text.StringBuilder()
            let tokenTypeSrc = System.Text.StringBuilder()
            let isMinorSrc = System.Text.StringBuilder()

            
            
            dict.Keys
            |> Seq.iteri (fun i key ->
                let upperKey = key.ToString().ToUpper()

                // Format and append the first type of output
                tokenizerSrc.Append(sprintf "{ \"%s\", TokenType.%s }" upperKey upperKey) |> ignore
                if i < dict.Count - 1 then tokenizerSrc.Append(",\n") |> ignore else tokenizerSrc.Append("\n") |> ignore

                // Format and append the second type of output
                
                tokenTypeSrc.AppendFormat(sprintf "%s, " upperKey) |> ignore
                

                // Format and append the third type of output
                if i > 0 then isMinorSrc.Append(",") |> ignore
                isMinorSrc.AppendFormat(sprintf "TokenType.%s" upperKey) |> ignore
                if i = dict.Count - 1 then isMinorSrc.Append(" ") |> ignore
            )
            tokenizerSrc.Append("\n//NEW_MINORS_HERE") |> ignore
            tokenTypeSrc.Append("\n//NEW_MINORS_HERE") |> ignore
            isMinorSrc.Append("\n//NEW_MINORS_HERE") |> ignore
            [ tokenizerSrc.ToString(); tokenTypeSrc.ToString(); isMinorSrc.ToString() ]



        let dict = createDict (finalSnippet)

        

        printfn "Resulting Dictionary:"
        dict |> Seq.iter (fun kvp -> printfn "Key: %s, Value: %s" kvp.Key kvp.Value)


        let keywordsSrcList = buildSrc (dict)
        rewriteSrcFiles (keywordsSrcList) (majorName)

        
        let keywordsArray = List.toArray(keywordsSrcList)
        keywordsArray  |> Array.iter (printf "%s\n")
        
           

        

        printfn "Do You Plan to make an object with this? (experimental) [Y/n]"
        let yorn = Console.ReadLine()
        if yorn.Equals("Y") then

            let buildObjectSrc (majorName: string) (dict: Dictionary<string, string>) =
                let buildSwitchCases ()= 
                    let switchBuilder = System.Text.StringBuilder()

                    
                    dict |> Seq.iter (fun kvp ->
                        let key = kvp.Key
                        let value = kvp.Value
                        let upperKey = key.ToUpper()
                        let propType = typeFromValue value // Get type from the actual value

                        switchBuilder.AppendLine(sprintf "    case TokenType.%s:" upperKey) |> ignore
                        switchBuilder.AppendLine(sprintf "        %s = %s.Parse(value.ToString());" (key) propType) |> ignore
                        switchBuilder.AppendLine("        break;") |> ignore
                    )
                    switchBuilder.AppendLine("    default:") |> ignore
                    switchBuilder.AppendLine("        throw new CompilerError(type,") |> ignore
                    switchBuilder.AppendLine(sprintf "            $\"The Keyword {type.lexeme} does not exist within the block\");") |> ignore
                    switchBuilder.ToString()


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
                        public class WrldBx{getParaCase (majorName)} : IWrldBxObject
                        {{
                            public string id {{ get; set; }}
                            {props}
                            public WrldBx{getParaCase (majorName)}(string id)
                            {{
                                this.id = id;
                            }}

                            public void UpdateStats(Token type, object value)
                            {{
                                switch (type.type)
                                {{

                                {buildSwitchCases()}
                                }}
                            }}
                        }}
                    }}
                    """
                objectSrc

            let buildCompilerSrc (dict: Dictionary<string, string>) = 
                
                let generateCodeSrc =  System.Text.StringBuilder()

                let buildEach () =
                    let codeGen = System.Text.StringBuilder()
                    dict.Keys
                    |> Seq.iter (fun key ->
                        ignore (codeGen.AppendLine($"{key} = {{{majorName.ToLower()}.{key}}},"))
                    )
                    codeGen.ToString()  // Return the generated code

                
                generateCodeSrc.Append (
                    $"""
                    using System;
                    using System.Collections.Generic;
                    using System.Linq;
                    using System.Text;
                    using System.Threading.Tasks;

                    namespace WrldBxScript
                    {{
                        public  class {getParaCase(majorName)}CodeGenerator : ICodeGenerator
                        {{
                            private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;

                            // Constructor that accepts repositories
                            public ProjectilesCodeGenerator(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories)
                            {{
                                _repositories = repositories;
                            }}
                            public void GenerateCode(StringBuilder src, string modname)
                            {{
                                src.AppendLine("\tpublic static void init() \n\t{{");

                                // Add effects-specific generation logic here
                                foreach (WrldBxProjectile projectile in _repositories[\"{majorName.ToUpper()}\"].GetAll.Cast<WrldBxProjectile>())
                                {{
                                    AddBlockId(src, projectile.id);
                                }}  

                            public void AddBlockId(StringBuilder src, object name)
                            {{
                                
                            }}

                            public void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null)
                            {{
                                
                            }}

                            
                        }}
                    }}
                    """) |> ignore
                (generateCodeSrc.ToString())

            // Call the function to build the object source code
            let sourceCode = buildObjectSrc majorName dict
            let generateCodeSrc = buildCompilerSrc dict
            
            printfn "Generated Source Code:\n%s" sourceCode
            writeToCompilerFile (generateCodeSrc) (majorName)
            writeObjectToFile (majorName) (sourceCode)
                
        

        printfn "Press Enter to exit."
        Console.ReadLine() |> ignore
    else
        printfn "Please Enter Each Minor Keyword: "
        let mutable minorKeywords = new List<string>()

        let mutable input = Console.ReadLine() // Initialize with the first input
        while not (String.IsNullOrWhiteSpace(input)) do // Loop until the input is blank
            minorKeywords.Add(input)
            input <- Console.ReadLine()

        //TODO: create a non developer version where it sends a PR to your github or something
