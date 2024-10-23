
module FsCLI 

open System
open System.Collections.Generic
open HandleKeyword
open HandleUpgrade
let commands = ["--help"; "--exit"; "--version"; "--createkeyword"; "--upgrade"]

let commandExists command = 
    List.contains command commands      
        
[<EntryPoint>]
let main args =
    
    printfn "Total arguments received: %d" args.Length
    if args.Length > 0 then
        let mutable i = 0
        while i < args.Length do
            printfn "args: %s" args.[i]
            match args.[i] with
            | "--help" -> printfn "Help: Available commands are --help, --exit, --version, --createkeyword, --upgrade"
            | "--version" -> printfn "0.1.0"
            | "--exit" -> printfn "Exiting..."; Environment.Exit(0)
            | "--createkeyword" -> 
                if i + 1 < args.Length && args.[i + 1] = "-d" then
                    printfn "args: %s" args.[i + 1]
                    
                    handleCreateKeyword (true)
                    i <- i + 1 
                else
                    handleCreateKeyword (false)
            | "--upgrade" -> handleUpgrade()
            | _ -> printfn "Unknown command: %s" args.[i]
            i <- i + 1
    else
        printfn "????"

    0