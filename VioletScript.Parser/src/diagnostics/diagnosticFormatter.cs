namespace VioletScript.Parser.Diagnostic;

using VioletScript.Parser.Source;
using VioletScript.Parser.Tokenizer;
using VioletScript.Parser.Ast;
using static VioletScript.Parser.Ast.AnnotatableDefinitionModifierMethods;
using static VioletScript.Parser.Ast.AnnotatableDefinitionAccessModifierMethods;

using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IDiagnosticFormatter {
    string Format(Diagnostic p) {
        var msg = FormatArguments(p);
        msg = msg.Substring(0, 1).ToUpper() + msg.Substring(1);
        String file = p.Span.Script.FilePath;
        if (file.StartsWith("/"))
        {
            file = file.Substring(1);
        }
        file = new System.Uri("file://" + file).AbsoluteUri;
        if (file.EndsWith('/'))
        {
            file = file.Substring(0, file.Count() - 1);
        }
        var line = p.Span.FirstLine.ToString();
        var column = (p.Span.FirstColumn + 1).ToString();
        var k = p.KindString;
        var k2 = p.IsWarning ? "Warning" : "Error";
        return $"{file}:{line}:{column}: {k}: {k2} #{p.Id.ToString()}: {msg}";
    }

    string FormatArguments(Diagnostic p) {
        var regex = new Regex(@"\$(\$|[a-z0-9_\-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return regex.Replace(ResolveId(p.Id), delegate(Match m) {
            var s = m.Captures[0].ToString().Substring(1);
            if (s == "$") {
                return "$";
            }
            if (!p.FormatArguments.ContainsKey(s)) {
                return "undefined";
            }
            return FormatArgument(p.FormatArguments[s]);
        });
    }

    string FormatArgument(object arg) {
        if (arg is TokenData) {
            var td = (TokenData) arg;
            if (td.Type == Token.Operator) {
                return FormatArgument(td.Operator);
            }
            if (td.Type == Token.CompoundAssignment) {
                return FormatArgument(td.Operator) + "=";
            }
            if (td.Type == Token.Keyword) {
                return "'" + td.StringValue + "'";
            }
            return FormatArgument(td.Type);
        }
        if (arg is AnnotatableDefinitionModifier) {
            return ((AnnotatableDefinitionModifier) arg).Name();
        }
        if (arg is AnnotatableDefinitionAccessModifier) {
            return ((AnnotatableDefinitionAccessModifier) arg).Name();
        }
        if (arg is Token) {
            return DefaultDiagnosticFormatterStatics.TokenTypesAsArguments.ContainsKey((Token) arg) ? DefaultDiagnosticFormatterStatics.TokenTypesAsArguments[(Token) arg] : "undefined";
        }
        return (string) arg;
    }

    string ResolveId(int id) {
        return DefaultDiagnosticFormatterStatics.Messages.ContainsKey(id) ? DefaultDiagnosticFormatterStatics.Messages[id] : "undefined";
    }
}

public static class DefaultDiagnosticFormatterStatics {
    public static IDiagnosticFormatter Formatter = new DefaultDiagnosticFormatter();
    public class DefaultDiagnosticFormatter : IDiagnosticFormatter {
    }
    public static readonly Dictionary<int, string> Messages = new Dictionary<int, string> {
        [0] = "Unexpected $t",
        [1] = "Invalid or unexpected token",
        [2] = "Keyword must not contain escaped characters",
        [3] = "String must not contain line breaks",
        [4] = "Required parameter must not follow optional parameter",
        [5] = "Invalid destructuring assignment target",
        [6] = "\'await\' must not appear in generator",
        [7] = "\'yield\' must not appear in asynchronous function",
        [8] = "\'throws\' clause must not repeat",
        [9] = "Undefined label \'$label\'",
        [10] = "Try statement must have at least one \'catch\' or \'finally\' clause",
        [11] = "Mal-formed for..in binding",
        [12] = "Unexpected super statement",
        [13] = "Token must be inline",
        [14] = "Enum variant declarations must be constant",
        [15] = "Enum variant must be simple",
        [16] = "Definition appears in unallowed context",
        [17] = "\'yield\' operator unexpected here",
        [18] = "Definition must not have modifier \'$m\'",
        [19] = "Native function must not have body",
        [20] = "Function must have body",
        [21] = "Interface method must have no attributes",
        [22] = "Proxy method has wrong number of parameters",
        [23] = "Unrecognized proxy \'$p\'",
        [24] = "\'extends\' clause must not duplicate",
        [25] = "Failed to resolve include source",
        [26] = "Function must not use \'await\'",
        [27] = "Function must not use \'yield\'",
        [28] = "Invalid arrow function parameter",
        [29] = "Rest parameter must not have initializer",
        [30] = "Value class must not have \'extends\' clause",
        [31] = "",
        [32] = "Illegal break statement",
        [33] = "Illegal continue statement: no surrounding iteration statement",

        // verifier-only messages
        [128] = "'$name' is undefined",
        [129] = "Ambiguous reference to '$name'",
        [130] = "'$name' is private",
        [131] = "'$name' is not a type constant",
        [132] = "'$name' is a generic type or function, therefore must be argumented",
        [133] = "'$t' is not a generic type",
        [134] = "Expression is not a type constant",
        [135] = "Wrong number of arguments: expected $expectedN, got $gotN",
        [136] = "Argument type must be a subtype of $t",
        [137] = "Typed type expression must not be used here",
        [138] = "Binding must have type annotation",
        [139] = "Duplicate definition for '$name'",
        [140] = "Inferred type and type annotation must match: $i is not equals to $a",
        [141] = "Cannot apply array destructuring to type '$t'",
        [142] = "Tuple destructuring cannot have more than $limit elements",
        [143] = "Tuple destructuring pattern must not have a rest element",
        [144] = "Rest element must be last element",
        [145] = "Record pattern key must be an identifier",
        [146] = "'$name' is a generic function, therefore must be argumented",
        [147] = "Assigning read-only property '$name'",
        [148] = "Destructuring assignment target must be lexical",
        [149] = "Target reference must be of type '$e'",
        [150] = "Expression not supported as a compile-time expression",
        [151] = "'$name' is not a compile-time constant",
        [152] = "Identifier must not be typed here",
        [153] = "Operation not supported as a compile-time expression: $op",
        [154] = "Operand must be numeric",
        [155] = "Compile-time \"in\" operation is only supported for flags",
        [156] = "Left operand must not be undefined or null",
    };

    public static readonly Dictionary<Token, string> TokenTypesAsArguments = new Dictionary<Token, string> {
        [Token.Eof] = "end of program",
        [Token.Identifier] = "identifier",
        [Token.StringLiteral] = "string literal",
        [Token.NumericLiteral] = "numeric literal",
        [Token.RegExpLiteral] = "regular expression",
        [Token.Keyword] = "keyword",
        [Token.Operator] = "operator",
        [Token.CompoundAssignment] = "assignment",
        [Token.LCurly] = "{",
        [Token.RCurly] = "}",
        [Token.LParen] = "(",
        [Token.RParen] = ")",
        [Token.LSquare] = "[",
        [Token.RSquare] = "]",
        [Token.Dot] = ".",
        [Token.QuestionDot] = "?.",
        [Token.Ellipsis] = "...",
        [Token.Semicolon] = ";",
        [Token.Comma] = ",",
        [Token.QuestionMark] = "?",
        [Token.ExclamationMark] = "!",
        [Token.Colon] = ":",
        [Token.Assign] = "=",
        [Token.Arrow] = "->",
        [Token.LtSlash] = "</",
    };
}