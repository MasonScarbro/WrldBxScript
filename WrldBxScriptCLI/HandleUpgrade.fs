module HandleUpgrade
open System
open System.Diagnostics
open System.IO
open System.Net

let runCommand (cmd: string) (args: string) =
    let processInfo = new ProcessStartInfo(cmd, args)
    processInfo.RedirectStandardOutput <- true
    processInfo.RedirectStandardError <- true
    processInfo.UseShellExecute <- false
    processInfo.CreateNoWindow <- true

    let processCurr = new Process()
    processCurr.StartInfo <- processInfo
    processCurr.Start() |> ignore

    let output = processCurr.StandardOutput.ReadToEnd()
    let err = processCurr.StandardError.ReadToEnd()

    processCurr.WaitForExit()
    (output, err)

let isGitInstalled () = 
    let (output, err) = runCommand "git" "--version"
    err = ""


let downloadRepoAsZip (repoUrl: string) (destination: string) = 
    let zipUrl = repoUrl + "/archive/refs/heads/main.zip"
    let zipFilePath = Path.Combine(destination, "WrldBxScript.zip")

    try
        printfn "Downloading repository as ZIP from %s..." zipUrl
        use client = new WebClient()
        client.DownloadFile(zipUrl, zipFilePath)
        printfn "Repository downloaded as ZIP to %s" zipFilePath

        // Extract ZIP file
        let extractPath = Path.Combine(destination, "WrldBxScript")

        if Directory.Exists(extractPath) then
            printfn "Directory %s already exists. Replacing it..." extractPath
            Directory.Delete(extractPath, true)  

        System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, extractPath)
        printfn "Repository extracted to %s" extractPath
    with
    | ex -> printfn "Failed to download repository as ZIP. Error: %s" ex.Message
        

let isGitRepo (repoPath: string) = 
    Directory.Exists(Path.Combine(repoPath, ".git"))

let initializeGitRepoIfNot (repourl: string) (repoPath: string) = 
    if isGitInstalled () then
        if not (isGitRepo repoPath) then
            printfn "No git repository found. Cloning the repository..."
            Directory.SetCurrentDirectory(Path.GetDirectoryName(repoPath))
            let (output, err) = runCommand "git" (sprintf "clone %s %s" repourl repoPath)
            if err = "" then
                printfn "Repository cloned successfully."
            else
                printfn "Failed to clone repository. Error: %s" err
        else
            printfn "Git repository found. Proceeding with the update."
    else
        printfn "Git not installed. Downloading repository as ZIP..."
        downloadRepoAsZip repourl repoPath

let upgradeFromGithub (repoPath) = 
    if isGitRepo repoPath then
        Directory.SetCurrentDirectory(repoPath)

        let (output, err) = runCommand "git" "pull"
        if err = "" then
            printfn "Upgrade successful. Latest changes pulled from GitHub."
            printfn "Output: %s" output
        else
            printfn "Failed to pull latest changes. Error: %s" err
    else
        printfn "This directory is not a git repository. Please Contact Humouroussix2799 on discord"


let searchForRepository repoFolderName =
    let commonPaths = [
        @"C:\Program Files"; 
        @"C:\Program Files (x86)";
        @"C:\DefaultPath\";  // This is updated during installation
    ]

    // Try searching in common locations first
    let foundPath = 
        commonPaths
        |> List.tryFind (fun path -> Directory.Exists(Path.Combine(path, repoFolderName)))

    match foundPath with
    | Some path -> path // Return the found path
    | None -> 
        // Last-ditch effort: Search the whole system recursively
        printfn "Searching the entire system for the repository..."
        let rec searchRecursively baseDir =
            try
                let dirs = Directory.GetDirectories(baseDir, repoFolderName, SearchOption.AllDirectories)
                if dirs.Length > 0 then Some(dirs.[0]) else None
            with
                | :? UnauthorizedAccessException -> None // Skip unauthorized folders
                | _ -> None

        // Start the recursive search from the root directories
        let possibleRootDirs = [ @"C:\"; @"D:\" ]
        possibleRootDirs
        |> List.tryPick searchRecursively
        |> function
        | Some path -> path
        | None -> 
            printfn "Repository not found."
            ""
  
let handleUpgrade () =
    
    let repoPath = searchForRepository "WrldBxScript"
    let repoUrl = "https://github.com/MasonScarbro/WrldBxScript.git"

    if repoPath <> "" then
        // Initialize repository if not found, or proceed with updating it
        initializeGitRepoIfNot repoUrl repoPath

        // Upgrade by pulling the latest changes from GitHub
        upgradeFromGithub repoPath
    else
        printfn "Repository not found. Please ensure it is installed correctly."
        