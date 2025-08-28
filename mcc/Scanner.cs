using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum TokenType
{
    Const, Var, Sub, Start, Identifier, Number, CharLiteral, StringLiteral, BoolLiteral,
    Byte, Char, Integer, String, Bool,
    If, Else, While, Return, Print, Jump,
    LParen, RParen, LBrace, RBrace, Comma, Semicolon, Colon, Assign,
    BinaryOperator, UnaryOperator,
    EOF
}

public class Token
{
    public Token(TokenType type, string value, int line) { Type = type; Value = value; Line = line; }
    public TokenType Type { get; }
    public string Value { get; }
    public int Line { get; }
}

public class Scanner
{
    private static readonly Regex IdentifierRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*");
    private static readonly Regex NumberRegex = new(@"^[+-]?[0-9]+");
    private static readonly Regex StringRegex = new("^\"[^\"]*\"");
    private static readonly Regex CharRegex = new("^'[^']'");
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["const"] = TokenType.Const, ["var"] = TokenType.Var, ["sub"] = TokenType.Sub, ["start"] = TokenType.Start,
        ["byte"] = TokenType.Byte, ["char"] = TokenType.Char, ["integer"] = TokenType.Integer,
        ["string"] = TokenType.String, ["bool"] = TokenType.Bool,
        ["if"] = TokenType.If, ["else"] = TokenType.Else, ["while"] = TokenType.While,
        ["return"] = TokenType.Return, ["print"] = TokenType.Print, ["jump"] = TokenType.Jump,
        ["true"] = TokenType.BoolLiteral, ["false"] = TokenType.BoolLiteral
    };
    private static readonly HashSet<string> BinaryOps = new() { "==", "!=", "<", ">", "<=", ">=", "*", "/", "+", "-" };
    private static readonly HashSet<string> UnaryOps = new() { "-", "!" };

    private readonly string _input;
    private int _pos;
    private int _line;

    public Scanner(string input) { _input = input; _pos = 0; _line = 1; }

    public Token NextToken()
    {
        while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
        {
            if (_input[_pos] == '\n')
                _line++;
            _pos++;
        }
        if (_pos >= _input.Length) return new Token(TokenType.EOF, "", _line);

        char c = _input[_pos];

        // Symbols
        if (c == '{') { _pos++; return new Token(TokenType.LBrace, "{", _line); }
        if (c == '}') { _pos++; return new Token(TokenType.RBrace, "}", _line); }
        if (c == '(') { _pos++; return new Token(TokenType.LParen, "(", _line); }
        if (c == ')') { _pos++; return new Token(TokenType.RParen, ")", _line); }
        if (c == ',') { _pos++; return new Token(TokenType.Comma, ",", _line); }
        if (c == ';') { _pos++; return new Token(TokenType.Semicolon, ";", _line); }
        if (c == ':') { _pos++; return new Token(TokenType.Colon, ":", _line); }
        if (c == '=') {
            if (_pos + 1 < _input.Length && _input[_pos + 1] == '=') {
                _pos += 2; return new Token(TokenType.BinaryOperator, "==", _line);
            }
            _pos++; return new Token(TokenType.Assign, "=", _line);
        }
        if (c == '!') {
            if (_pos + 1 < _input.Length && _input[_pos + 1] == '=') {
                _pos += 2; return new Token(TokenType.BinaryOperator, "!=", _line);
            }
            _pos++; return new Token(TokenType.UnaryOperator, "!", _line);
        }
        if (c == '<') {
            if (_pos + 1 < _input.Length && _input[_pos + 1] == '=') {
                _pos += 2; return new Token(TokenType.BinaryOperator, "<=", _line);
            }
            _pos++; return new Token(TokenType.BinaryOperator, "<", _line);
        }
        if (c == '>') {
            if (_pos + 1 < _input.Length && _input[_pos + 1] == '=') {
                _pos += 2; return new Token(TokenType.BinaryOperator, ">=", _line);
            }
            _pos++; return new Token(TokenType.BinaryOperator, ">", _line);
        }
        if (c == '*' || c == '/' || c == '+' || c == '-') {
            string op = c.ToString();
            _pos++;
            if (BinaryOps.Contains(op)) return new Token(TokenType.BinaryOperator, op, _line);
            if (UnaryOps.Contains(op)) return new Token(TokenType.UnaryOperator, op, _line);
        }

        // String literal
        var strMatch = StringRegex.Match(_input[_pos..]);
        if (strMatch.Success) {
            _pos += strMatch.Length;
            return new Token(TokenType.StringLiteral, strMatch.Value, _line);
        }

        // Char literal
        var charMatch = CharRegex.Match(_input[_pos..]);
        if (charMatch.Success) {
            _pos += charMatch.Length;
            return new Token(TokenType.CharLiteral, charMatch.Value, _line);
        }

        // Number literal
        var numMatch = NumberRegex.Match(_input[_pos..]);
        if (numMatch.Success) {
            _pos += numMatch.Length;
            return new Token(TokenType.Number, numMatch.Value, _line);
        }

        // Identifier or keyword
        var idMatch = IdentifierRegex.Match(_input[_pos..]);
        if (idMatch.Success) {
            _pos += idMatch.Length;
            var value = idMatch.Value;
            if (Keywords.TryGetValue(value, out var type)) return new Token(type, value, _line);
            return new Token(TokenType.Identifier, value, _line);
        }

        throw new Exception($"Unknown token at position {_pos}: '{_input[_pos]}'");
    }
}