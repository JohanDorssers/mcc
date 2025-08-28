using System;
using System.Collections.Generic;

public abstract class AstNode { }

public class ProgramNode : AstNode
{
    public List<ConstantNode> Constants = new();
    public List<VariableNode> Variables = new();
    public List<SubroutineNode> Subroutines = new();
}

public class ConstantNode : AstNode
{
    public string Type, Name, Value;
}

public class VariableNode : AstNode
{
    public List<string> Names = new();
    public string Type;
}

public class SubroutineNode : AstNode
{
    public string Name;
    public CompoundStatementNode Body;
    public bool IsStart;
}

public class CompoundStatementNode : AstNode
{
    public List<AstNode> Statements = new();
}

public class AssignmentNode : AstNode
{
    public string Name;
    public ExpressionNode Expr;
}

public class IfNode : AstNode
{
    public ExpressionNode Condition;
    public AstNode Then;
    public AstNode? Else;
}

public class WhileNode : AstNode
{
    public ExpressionNode Condition;
    public AstNode Body;
}

public class ReturnNode : AstNode { }

public class PrintNode : AstNode
{
    public ExpressionNode Expr;
}

public class JumpNode : AstNode
{
    public string Target;
}

public abstract class ExpressionNode : AstNode { }

public class LiteralNode : ExpressionNode
{
    public string Value;
}

public class IdentifierNode : ExpressionNode
{
    public string Name;
}

public class BinaryOpNode : ExpressionNode
{
    public string Op;
    public ExpressionNode Left, Right;
}

public class UnaryOpNode : ExpressionNode
{
    public string Op;
    public ExpressionNode Expr;
}

public class Parser
{
    private readonly Scanner _scanner;
    private Token _current;

    public Parser(Scanner scanner)
    {
        _scanner = scanner;
        _current = _scanner.NextToken();
    }

    private void Eat(TokenType type)
    {
        if (_current.Type == type) _current = _scanner.NextToken();
        else throw new Exception($"(line: {_current.Line}) Expected {type}, got {_current.Type}");
    }

    public ProgramNode ParseProgram()
    {
        var program = new ProgramNode();
        while (_current.Type == TokenType.Const)
            program.Constants.Add(ParseConstant());
        while (_current.Type == TokenType.Var)
            program.Variables.Add(ParseVariable());
        program.Subroutines.Add(ParseStartFunction());
        while (_current.Type == TokenType.Sub)
            program.Subroutines.Add(ParseSubroutine());
        return program;
    }

    private ConstantNode ParseConstant()
    {
        Eat(TokenType.Const);
        var type = ParseSimpleType();
        var name = _current.Value; Eat(TokenType.Identifier);
        Eat(TokenType.Assign);
        var value = ParseLiteral();
        Eat(TokenType.Semicolon);
        return new ConstantNode { Type = type, Name = name, Value = value };
    }

    private string ParseLiteral()
    {
        string type = "";
        if (_current.Type == TokenType.CharLiteral)
        {
            type = _current.Value;
            Eat(TokenType.CharLiteral);
            return type;
        }
        if (_current.Type == TokenType.StringLiteral)
        {
            type = _current.Value;
            Eat(TokenType.StringLiteral);
            return type;
        }
        if (_current.Type == TokenType.Number)
        {
            type = _current.Value;
            Eat(TokenType.Number);
            return type;
        }
        throw new Exception($"(line: {_current.Line}) Expected Literal, got {_current.Type}");
    }

    private string ParseSimpleType()
    {
        string type= "";
        if (_current.Type == TokenType.Byte)
        {
            type = _current.Value;
            Eat(TokenType.Byte);
            return type;
        }
        if (_current.Type == TokenType.Char)
        {
            type = _current.Value;
            Eat(TokenType.Char);
            return type;
        }
        if (_current.Type == TokenType.String)
        {
            type = _current.Value;
            Eat(TokenType.String);
            return type;
        }
        if (_current.Type == TokenType.Integer)
        {
            type = _current.Value;
            Eat(TokenType.Integer);
            return type;
        }
        if (_current.Type == TokenType.Bool)
        {
            type = _current.Value;
            Eat(TokenType.Bool);
            return type;
        }

        throw new Exception($"(line: {_current.Line}) Expected SimpleType, got {_current.Type}");
    }

    private VariableNode ParseVariable()
    {
        Eat(TokenType.Var);
        var names = new List<string> { _current.Value }; Eat(TokenType.Identifier);
        while (_current.Type == TokenType.Comma)
        {
            Eat(TokenType.Comma);
            names.Add(_current.Value); Eat(TokenType.Identifier);
        }
        Eat(TokenType.Colon);
        var type = ParseSimpleType();
        Eat(TokenType.Semicolon);
        return new VariableNode { Names = names, Type = type };
    }

    private SubroutineNode ParseStartFunction()
    {
        Eat(TokenType.Start);
        var body = ParseCompoundStatement();
        return new SubroutineNode { Name = "start", Body = body, IsStart = true };
    }

    private SubroutineNode ParseSubroutine()
    {
        Eat(TokenType.Sub);
        var name = _current.Value; Eat(TokenType.Identifier);
        var body = ParseCompoundStatement();
        return new SubroutineNode { Name = name, Body = body, IsStart = false };
    }

    private CompoundStatementNode ParseCompoundStatement()
    {
        Eat(TokenType.LBrace);
        var statements = new List<AstNode>();
        while (_current.Type != TokenType.RBrace)
        {
            if (_current.Type == TokenType.Semicolon)
            {
                Eat(TokenType.Semicolon); // empty statement
                continue;
            }
            statements.Add(ParseStatement());
        }
        Eat(TokenType.RBrace);
        return new CompoundStatementNode { Statements = statements };
    }

    private AstNode ParseStatement()
    {
        switch (_current.Type)
        {
            case TokenType.Identifier:
                return ParseAssignmentStatement();
            case TokenType.If:
                return ParseIfStatement();
            case TokenType.While:
                return ParseWhileStatement();
            case TokenType.Return:
                return ParseReturnStatement();
            case TokenType.Print:
                return ParsePrintStatement();
            case TokenType.Jump:
                return ParseJumpStatement();
            case TokenType.LBrace:
                return ParseCompoundStatement();
            case TokenType.Semicolon:
                Eat(TokenType.Semicolon);
                return null!;
            default:
                throw new Exception($"(line: {_current.Line}) Unexpected token in statement: {_current.Type}");
        }
    }

    private AssignmentNode ParseAssignmentStatement()
    {
        var name = _current.Value; Eat(TokenType.Identifier);
        Eat(TokenType.Assign);
        var expr = ParseExpression();
        Eat(TokenType.Semicolon);
        return new AssignmentNode { Name = name, Expr = expr };
    }

    private IfNode ParseIfStatement()
    {
        Eat(TokenType.If);
        Eat(TokenType.LParen);
        var cond = ParseExpression();
        Eat(TokenType.RParen);
        var thenStmt = ParseStatement();
        AstNode? elseStmt = null;
        if (_current.Type == TokenType.Else)
        {
            Eat(TokenType.Else);
            elseStmt = ParseStatement();
        }
        return new IfNode { Condition = cond, Then = thenStmt, Else = elseStmt };
    }

    private WhileNode ParseWhileStatement()
    {
        Eat(TokenType.While);
        Eat(TokenType.LParen);
        var cond = ParseExpression();
        Eat(TokenType.RParen);
        var body = ParseStatement();
        return new WhileNode { Condition = cond, Body = body };
    }

    private ReturnNode ParseReturnStatement()
    {
        Eat(TokenType.Return);
        Eat(TokenType.Semicolon);
        return new ReturnNode();
    }

    private PrintNode ParsePrintStatement()
    {
        Eat(TokenType.Print);
        var expr = ParseExpression();
        Eat(TokenType.Semicolon);
        return new PrintNode { Expr = expr };
    }

    private JumpNode ParseJumpStatement()
    {
        Eat(TokenType.Jump);
        var target = _current.Value; Eat(TokenType.Identifier);
        Eat(TokenType.Semicolon);
        return new JumpNode { Target = target };
    }

    // Expression parser (recursive descent, left-associative, no precedence for brevity)
    private ExpressionNode ParseExpression()
    {
        // Handle unary
        if (_current.Type == TokenType.UnaryOperator)
        {
            var op = _current.Value; Eat(TokenType.UnaryOperator);
            var expr = ParseExpression();
            return new UnaryOpNode { Op = op, Expr = expr };
        }
        // Handle parenthesis
        if (_current.Type == TokenType.LParen)
        {
            Eat(TokenType.LParen);
            var expr = ParseExpression();
            Eat(TokenType.RParen);
            return expr;
        }
        // Handle literal
        if (_current.Type == TokenType.Number || _current.Type == TokenType.CharLiteral ||
            _current.Type == TokenType.StringLiteral || _current.Type == TokenType.BoolLiteral)
        {
            var value = _current.Value; Eat(_current.Type);
            return new LiteralNode { Value = value };
        }
        // Handle identifier
        if (_current.Type == TokenType.Identifier)
        {
            var name = _current.Value; Eat(TokenType.Identifier);
            // Check for binary operator
            if (_current.Type == TokenType.BinaryOperator)
            {
                var op = _current.Value; Eat(TokenType.BinaryOperator);
                var right = ParseExpression();
                return new BinaryOpNode { Op = op, Left = new IdentifierNode { Name = name }, Right = right };
            }
            return new IdentifierNode { Name = name };
        }
        throw new Exception($"(line: {_current.Line}) Invalid expression, got {_current.Type}");
    }
}