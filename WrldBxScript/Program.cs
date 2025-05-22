using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FsCLI;

namespace WrldBxScript
{
    class WrldBxScript
    {
        public static Compiler compiler = new Compiler();
        static bool hadError = false;
        static bool hadRuntimeError = false;
        public static void Main(string[] args)
        {
            //args = new string[] { "--createkeyword", "-d" };
            if (args.Length > 0 && args[0].StartsWith("--"))
            {

                //Console.WriteLine("Calling F# CLI with arguments " + args[0] + " " + args[1]);
                FsCLI.main(args);
            }
            if (args.Length == 1 && Directory.Exists(args[0]))
            {
                string dirPath = args[0];
                RunMultipleFiles(dirPath);
            }
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: wrldbx [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(String path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                String SourceAsBytes = Encoding.Default.GetString(bytes);
                Run(SourceAsBytes);
                if (hadError) Environment.Exit(65);
                if (hadRuntimeError) Environment.Exit(70);
            }
            catch (IOException)
            {

            }
        }

        private static void RunPrompt()
        {
            try
            {
                TextReader reader = Console.In;

                for (; ; ) // Infinite Loop
                {
                    Console.Write("> ");
                    String line = reader.ReadLine();
                    if (line == null) break;
                    Run(line);
                }
            }
            catch
            {

            }
        }
        //prints out the tokens the scanner emits!
        private static void Run(String source)
        {
            try
            {
                
                Tokenizer scanner = new Tokenizer(source);
                List<Token> tokens = scanner.scanTokens();

                // For now, just print the tokens.
                foreach (Token token in tokens)
                {
                    Console.WriteLine(token.type.ToString());
                }
                Parser parser = new Parser(tokens);
                List<Stmt> stmts = parser.Parse();
                PrettyPrintStmts(stmts);
                if (hadError) return;
                if (hadRuntimeError) return;

                compiler.Compile(stmts);
                //Console.WriteLine(new AstPrinter().Print(expression));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }


        }

        /// <summary>
        /// Runs Multiple files, if its a folder, 
        /// orders files based on convention of having modname in
        /// main or traits, and then appends them to the src to compile
        /// </summary>
        /// <param name="dirPath"></param>
        private static void RunMultipleFiles(string dirPath)
        {
            try
            {
                // Get all the .wrldbx files in the directory
                var files = Directory.GetFiles(dirPath, "*.wrldbx");

                // Combine their content
                List<string> orderedFiles = new List<string>();

                StringBuilder combinedSource = new StringBuilder();

                
                
                
                foreach (var file in files)
                {
                    if (file.EndsWith("Traits.wrldbx", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith("Main.wrldbx", StringComparison.OrdinalIgnoreCase))
                    {
                        orderedFiles.Insert(0, file);
                    }
                    else
                    {
                        orderedFiles.Add(file); // Add the rest in normal order
                    }
                }

                foreach (var file in orderedFiles)
                {
                    byte[] bytes = File.ReadAllBytes(file);
                    string fileContent = Encoding.Default.GetString(bytes);
                    combinedSource.Append(fileContent);
                    combinedSource.Append(Environment.NewLine); // Ensure files are separated by a newline
                }
                // Run the combined source code
                Run(combinedSource.ToString());

                if (hadError) Environment.Exit(65);
                if (hadRuntimeError) Environment.Exit(70);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading files: " + ex.Message);
                Environment.Exit(71);
            }
        }
        public static void Error(int line, String message)
        {
            report(line, "", message);
        }
        private static void report(int line, String where, String message)
        {
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
        public static void Warning(string msg, int line)
        {
            Console.WriteLine($"WARNING: {msg} \n[line: {line}]");
        }

        public static void Warning(string msg)
        {
            Console.WriteLine($"WARNING: {msg}\n");
        }

        public static void Warning(string msg, IWrldBxObject obj)
        {
            Console.WriteLine($"WARNING: {msg}\n [object: {obj.id} {obj.GetType()}]");
        }

        private static void PrettyPrintStmts(List<Stmt> stmts)
        {
            foreach (Stmt stmt in stmts)
            {
                Console.WriteLine(stmt);
                if (stmt is Stmt.Block b)
                {

                    for (int i = 0; i < b.statements.Count; i++)
                    {
                        if (b.statements[i] is Stmt.Var v)
                        {
                            Console.WriteLine("Type: " + v.type.lexeme + " Value: " + v.value);
                        }
                    }
                }
            }
        }

        public static void CompilerErrorToCons(CompilerError error)
        {
            
            Console.Error.WriteLine(error.ToString() + "\n[line " + error.token.line + "]");
            hadRuntimeError = true;
        }

    }
}
