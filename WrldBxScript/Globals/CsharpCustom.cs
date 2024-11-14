using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WrldBxScript.Globals
{
    public class CsharpCustom : IGlobal
    {
        public CsharpCustom()
        {
            Type = "Effect_Appendage";
        }
        public List<string> TypeAllowance => new List<string> { "Effects" };

        public string Type { get; set; }

        // we should parse the code with roslyn (maybe even eventually use my lsp tools) to ensure there are no errors
        public string Call(List<object> arguments)
        {
            if (arguments.Count != 0)
            {
                if (ValidateCsharpSyntax(arguments[0].ToString()))
                {
                    return arguments[0].ToString();
                }
                
            }
            //else
            return "";
        }

        private bool ValidateCsharpSyntax(string codeSnippet)
        {
            // Wrap snippet in a method so it can be parsed
            string wrappedCode = $"void TempMethod(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile) {{ {codeSnippet} }}";

            // Parse the code using Roslyn
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(wrappedCode);

            // Check for syntax errors
            var diagnostics = syntaxTree.GetDiagnostics()
                                        .Where(d => d.Severity == DiagnosticSeverity.Error);

            if (diagnostics.Any()) return true;
            //else

            var errorMessages = new List<string>();
            foreach (var diagnostic in diagnostics)
            {
                var lineSpan = syntaxTree.GetLineSpan(diagnostic.Location.SourceSpan);
                int line = lineSpan.StartLinePosition.Line + 1; // Line numbers are zero-based
                int column = lineSpan.StartLinePosition.Character + 1;

                errorMessages.Add($"Error: {diagnostic.GetMessage()} at line {line}, column {column}");
            }
            WrldBxScript.Warning
                ("Your Custom Csharp failed," +
                " it is not valid here are the errors of the Csharp" +
                $"\t{codeSnippet}" +
                $"\t\n{string.Join("\n", errorMessages)}");
            return false;
        }


        public void SetType(string type)
        {
            Type = type;
        }
    }
}
