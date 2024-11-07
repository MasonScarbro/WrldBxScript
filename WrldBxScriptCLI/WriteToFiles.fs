module WriteToFiles
open System.IO
open System.Xml.Linq
let files = [
        @"C:\Users\Admin\source\repos\WrldBxScript\WrldBxScript\Tokenizer.cs";
        @"C:\Users\Admin\source\repos\WrldBxScript\WrldBxScript\TokenType.cs";
        @"C:\Users\Admin\source\repos\WrldBxScript\WrldBxScript\Parser.cs";
    ]

let fileExists (filePath: string) = 
    if File.Exists(filePath) then
        true
    else
        printf "Something went Wrong couldnt find that file?"
        false

let searchForFile (fileName: string) (root: string) = 
    try
        let foundFiles = Directory.GetFiles(root, fileName, SearchOption.AllDirectories)
        if foundFiles.Length > 0 then
            Some foundFiles.[0] 
        else
            None
    with
        | :? DirectoryNotFoundException -> None // Handle permission issues
        | _ -> None

let getMajorFormat (filePath: string) (majorName: string) =
    if fileExists filePath then
        if filePath = files.[0] then
            sprintf "{ \"%s\", TokenType.%s },\n//NEW_MAJOR_HERE" majorName majorName
        else if filePath = files.[1] then
            sprintf "%s, //NEW_MAJOR_HERE" majorName
        else
            sprintf ", TokenType.%s //NEW_MAJOR_HERE" majorName
    else
         "//NEW_MAJOR_HERE"
        
        
        

let replacePlaceHolderInFile (filePath: string) (majorName: string) (replacement)=
    let fileContent = File.ReadAllText(filePath)
    let newContent = fileContent |> (fun content ->
        if not (majorName.Equals("")) then
            content.Replace("//NEW_MINORS_HERE", replacement)
                   .Replace("//NEW_MAJOR_HERE", getMajorFormat filePath majorName)
        else
            content.Replace("//NEW_MINORS_HERE", replacement)
    )
    File.WriteAllText(filePath, newContent)
    printfn "File updated and saved to: %s" filePath

let rewriteSrcFiles(replacements: string list) (majorName: string) =
    
    let missingFiles = 
        files 
        |> List.filter (not << fileExists)

    if missingFiles.Length > 0 then
        let foundFiles = 
            missingFiles
            |> List.choose(fun missingFile -> 
                let fileName = Path.GetFileName(missingFile)
                searchForFile fileName @"C:\"
            )
        if foundFiles.Length = missingFiles.Length then 
            printfn "All missing files found: %A" foundFiles
            let updatedFileList = (List.append (files |> List.filter fileExists) foundFiles)
            List.iter2 (fun filePath replacement ->
                replacePlaceHolderInFile (filePath) (majorName) (replacement)
            ) updatedFileList replacements
            else
            printfn "Some files could not be found"
    else
        List.iter2 (fun filePath replacement ->
            replacePlaceHolderInFile (filePath) (majorName) (replacement)
        ) files replacements
        printfn "All files exist."

let getParaCase (str: string) = 
    str.ToLower().Replace(str.Substring(0, 1), str.Substring(0, 1).ToUpper())

let writeObjectToFile (majorName: string) (src: string)=
        let filePath = sprintf @"C:\Users\Admin\source\repos\WrldBxScript\WrldBxScript\Objects\WrldBx%s.cs" (getParaCase (majorName.ToLower()))
        File.WriteAllText(filePath, src)
        
        printfn "File added to project successfully!"

let writeToCompilerFile (codeGenSrc: string) (majorName: string) =
    File.WriteAllText(sprintf @"C:\Users\Admin\source\repos\WrldBxScript\WrldBxScript\Generators\%sCodeGenerator.cs" majorName, codeGenSrc)

let writeGenSrc (path: string) (src: string) = 
    File.WriteAllText(path, src)