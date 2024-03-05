using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class WrldBxScript
    {
        public static readonly Compiler compiler = new Compiler();
        static bool hadError = false;
        static bool hadRuntimeError = false;
        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: scarbro [script]");
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

                compiler.Compile(stmts);
                //Console.WriteLine(new AstPrinter().Print(expression));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
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
