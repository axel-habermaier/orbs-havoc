// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

module Registry

open System
open System.Linq
open System.IO
open System.Text.RegularExpressions
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp.Syntax
open Utilities

let private getRuntimeName (name : string) = 
    let nameSeq = seq {
        yield Char.ToLower(name.[0])

        for c in name.Skip(1) do
            if Char.IsUpper c then yield '_'
            yield Char.ToLower c
    }
    String.Join("", nameSeq.Select(fun c -> c.ToString()))

let private getSummaryText (summary : string) =
    let lines = summary.Split([|"\n"|], StringSplitOptions.RemoveEmptyEntries)
    let comment = String.Join(" ", lines.Select(fun c -> c.Replace("///", "").Trim()))
    let regexMatch = Regex.Match(comment, "<summary>(.*)</summary>");

    regexMatch.Groups.[1].Value.Replace("\\", "\\\\").Replace("\"", "\\\"").Trim();

let private getParameterTag (parameter : string) (summary : string) =
    let lines = summary.Split([|"\n"|], StringSplitOptions.RemoveEmptyEntries)
    let comment = String.Join(" ", lines.Select(fun c -> c.Replace("///", "").Trim()))
    let regexMatch = Regex.Match(comment, String.Format("<param name=\"{0}\">(.*?)</param>", parameter));

    regexMatch.Groups.[1].Value.Replace("\\", "\\\\").Replace("\"", "\\\"").Trim();

let private getSummary (docComment : string) = seq {
    yield " <summary>"
    yield sprintf "   %s" (getSummaryText docComment)
    yield " </summary>"
}

let private writeDocumentation (writer : CodeWriter) (docComment : string seq) = 
    for line in docComment do
        writer.AppendLine "///%s" line

let hasAttribute (node : SyntaxNode) (attribute : string) =
    node.DescendantNodes().OfType<AttributeSyntax>().Any(fun a -> a.Name.ToString() = attribute)

let getValidators (node : SyntaxNode) =
    node.DescendantNodes().OfType<AttributeSyntax>().Where(fun a -> 
        let name = a.Name.ToString() 
        name <> "Cvar" && name <> "Command" && name <> "SystemOnly" && name <> "Persistent"
    ).ToArray()

let private writeValidators (writer : CodeWriter) (node : SyntaxNode) =
    let validators = getValidators node
    if validators.Length > 0 then writer.Append ", "
    writer.AppendRepeated validators (fun validator ->
        writer.Append "new %sAttribute" (validator.Name.ToString()) 
        if validator.ArgumentList = null then writer.Append "()"
        else writer.Append "%A" (validator.ArgumentList)
    ) (fun () -> writer.Append ", ")

let generateCvars () =
    let syntaxTree = SyntaxFactory.ParseSyntaxTree (File.ReadAllText "../../Source/Scripting/Cvars.cs")
    let root = syntaxTree.GetRoot()
    let writer = CodeWriter ()

    writer.AppendLine("namespace PointWars.Scripting")
    writer.AppendBlockStatement (fun () ->
        writer.AppendLine "using System;"
        writer.AppendLine "using System.Diagnostics;"
        writer.AppendLine "using Utilities;"
        for using in root.DescendantNodes().OfType<UsingDirectiveSyntax>() do
            writer.AppendLine "%A" using

        writer.NewLine()
        writer.AppendLine "public static class Cvars"
        writer.AppendBlockStatement (fun () ->
            let cvars = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().OrderBy(fun c -> c.Identifier.ToString()).ToArray()
            for cvar in cvars do
                writer.AppendLine "%s" (cvar.GetLeadingTrivia().ToString().Trim())
                writer.AppendLine "public static Cvar<%A> %ACvar { get; private set; }" cvar.Type cvar.Identifier
                writer.NewLine()

            for cvar in cvars do
                writer.AppendLine "%s" (cvar.GetLeadingTrivia().ToString().Trim())
                writer.AppendLine "public static %A %A" cvar.Type cvar.Identifier
                writer.AppendBlockStatement (fun () -> 
                    writer.AppendLine "[DebuggerHidden]"
                    writer.AppendLine "get { return %ACvar.Value; }" cvar.Identifier
                    writer.AppendLine "[DebuggerHidden]"
                    writer.AppendLine "set"
                    writer.AppendBlockStatement (fun () ->
                        writer.AppendLine "Assert.ArgumentNotNull((object)value, nameof(value));"
                        writer.AppendLine "%ACvar.Value = value;" cvar.Identifier
                    )
                )
                writer.NewLine()

            for cvar in cvars do
                writer.AppendLine "/// <summary>"
                writer.AppendLine "///  Raised when the '%A' cvar is changed." cvar.Identifier
                writer.AppendLine "/// </summary>"
                writer.AppendLine "public static event Action %AChanged" cvar.Identifier
                writer.AppendBlockStatement (fun () ->
                    writer.AppendLine "add { %ACvar.Changed += value; }" cvar.Identifier
                    writer.AppendLine "remove { %ACvar.Changed -= value; }" cvar.Identifier
                )
                writer.NewLine()

            writer.AppendLine "/// <summary>"
            writer.AppendLine "///  Initializes the declared cvars."
            writer.AppendLine "/// </summary>"
            writer.AppendLine "public static void Initialize()"
            writer.AppendBlockStatement (fun () ->
                for cvar in cvars do
                    let runtimeName = getRuntimeName (cvar.Identifier.ToString())
                    let defaultValue = cvar.DescendantNodes().OfType<AttributeSyntax>().First().ArgumentList.Arguments.First().ToString().Trim()
                    let defaultValue = 
                        if cvar.Type.ToString() <> "string" && defaultValue.StartsWith("\"") then 
                            defaultValue.Substring(1, defaultValue.Length - 2)
                        else 
                            defaultValue

                    writer.Append "%ACvar = new Cvar<%A>(" cvar.Identifier cvar.Type
                    writer.Append "\"%s\", %s, \"%s\", " runtimeName defaultValue (getSummaryText(cvar.GetLeadingTrivia().ToString()))
                    writer.Append "%b, %b" (hasAttribute cvar "Persistent") (hasAttribute cvar "SystemOnly")
                    writeValidators writer cvar
                    writer.AppendLine ");"

                writer.NewLine()
                for cvar in cvars do
                    writer.AppendLine "CvarRegistry.Register(%ACvar);" cvar.Identifier
            )
        )
    )

    File.WriteAllText("../../Source/Scripting/Cvars.gen.cs", writer.ToString())

let generateCommands () =
    let syntaxTree = SyntaxFactory.ParseSyntaxTree (File.ReadAllText "../../Source/Scripting/Commands.cs")
    let root = syntaxTree.GetRoot()
    let writer = CodeWriter ()

    let getGenericTypes (cmd : MethodDeclarationSyntax) =
        if cmd.ParameterList.Parameters.Count = 0 then ""
        else sprintf "<%s>" (String.Join(", ", cmd.ParameterList.Parameters.Select(fun p -> p.Type.ToString())))

    writer.AppendLine("namespace PointWars.Scripting")
    writer.AppendBlockStatement (fun () ->
        writer.AppendLine "using System;"
        writer.AppendLine "using System.Diagnostics;"
        writer.AppendLine "using Utilities;"
        for using in root.DescendantNodes().OfType<UsingDirectiveSyntax>() do
            writer.AppendLine "%A" using

        writer.NewLine()
        writer.AppendLine "public static class Commands"
        writer.AppendBlockStatement (fun () ->
            let commands = root.DescendantNodes().OfType<MethodDeclarationSyntax>().OrderBy(fun c -> c.Identifier.ToString()).ToArray()
            for command in commands do
                writeDocumentation writer (getSummary(command.GetLeadingTrivia().ToString().Trim()))
                writer.AppendLine "public static Command%s %ACommand { get; private set; }" (getGenericTypes command) command.Identifier
                writer.NewLine()

            for command in commands do
                let parameters = String.Join(", ", command.ParameterList.Parameters.Select(fun p -> sprintf "%A %A" p.Type p.Identifier))
                writer.AppendLine "%s" (command.GetLeadingTrivia().ToString().Trim())
                writer.AppendLine "[DebuggerHidden]"
                writer.AppendLine "public static %A %A(%s)" command.ReturnType command.Identifier parameters
                writer.AppendBlockStatement (fun () ->
                    for parameter in command.ParameterList.Parameters do
                        writer.AppendLine "Assert.ArgumentNotNull((object)%A, nameof(%A));" parameter.Identifier parameter.Identifier
                    let arguments = String.Join(", ", command.ParameterList.Parameters.Select(fun p -> p.Identifier.ToString()))
                    writer.AppendLine "%ACommand.Invoke(%s);" command.Identifier arguments
                )
                writer.NewLine()

            for command in commands do
                writer.AppendLine "/// <summary>"
                writer.AppendLine "///   Raised when the '%A' command is invoked." command.Identifier
                writer.AppendLine "/// </summary>"
                writer.AppendLine "public static event Action%s On%A" (getGenericTypes command) command.Identifier
                writer.AppendBlockStatement(fun () ->
                    writer.AppendLine "add { %ACommand.Invoked += value; }" command.Identifier
                    writer.AppendLine "remove { %ACommand.Invoked -= value; }" command.Identifier
                )
                writer.NewLine()

            writer.AppendLine "/// <summary>"
            writer.AppendLine "///  Initializes the declared commands."
            writer.AppendLine "/// </summary>"
            writer.AppendLine "public static void Initialize()"
            writer.AppendBlockStatement (fun () ->
                for command in commands do
                    let runtimeName = getRuntimeName (command.Identifier.ToString())
                    let parameters = command.ParameterList.Parameters
                    writer.Append "%ACommand = new Command%s(" command.Identifier (getGenericTypes command)
                    writer.Append "\"%s\", \"%s\", " runtimeName (getSummaryText(command.GetLeadingTrivia().ToString()))
                    writer.Append "%b" (hasAttribute command "SystemOnly")
                    if command.ParameterList.Parameters.Count > 0 then writer.Append ", "
                        
                    writer.IncreaseIndent()
                    writer.AppendRepeated command.ParameterList.Parameters (fun parameter ->
                        let parameterDescription = getParameterTag (parameter.Identifier.ToString()) (command.GetLeadingTrivia().ToString())
                        writer.NewLine()
                        
                        writer.Append "new CommandParameter(\"%A\", typeof(%A), " parameter.Identifier parameter.Type
                        writer.Append "%b, " (parameter.Default <> null)
                        if parameter.Default = null then writer.Append "default(%A)" parameter.Type
                        else writer.Append "%A" parameter.Default.Value
                        writer.Append ", \"%s\"" parameterDescription
                        writeValidators writer parameter
                        writer.Append ")"
                    ) (fun () -> writer.Append ", ")
                    writer.DecreaseIndent()
                    writer.AppendLine ");"

                writer.NewLine()
                for command in commands do
                    writer.AppendLine "CommandRegistry.Register(%ACommand);" command.Identifier
            )
        )
    )

    File.WriteAllText("../../Source/Scripting/Commands.gen.cs", writer.ToString())