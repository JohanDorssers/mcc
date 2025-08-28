using System.Text;

// 6502 code generator for the provided AST
public class CodeGenerator6502
{
    private readonly StringBuilder _sb = new();
    private int _labelCounter = 0;

    public string Generate(ProgramNode program)
    {
        // Emit constants
        foreach (var c in program.Constants)
            EmitConstant(c);

        // Emit variables
        foreach (var v in program.Variables)
            EmitVariable(v);

        // Emit subroutines
        foreach (var s in program.Subroutines)
            EmitSubroutine(s);

        return _sb.ToString();
    }

    private void EmitConstant(ConstantNode node)
    {
        // Example: .byte, .word, .asc, etc.
        switch (node.Type)
        {
            case "byte":
                _sb.AppendLine($"{node.Name}: .byte {node.Value}");
                break;
            case "char":
                _sb.AppendLine($"{node.Name}: .byte {node.Value}");
                break;
            case "integer":
                _sb.AppendLine($"{node.Name}: .word {node.Value}");
                break;
            case "string":
                _sb.AppendLine($"{node.Name}: .asc \"{node.Value.Trim('\"')}\"");
                break;
            case "bool":
                _sb.AppendLine($"{node.Name}: .byte {(node.Value == "true" ? "1" : "0")}");
                break;
            default:
                _sb.AppendLine($"; Unknown constant type: {node.Type}");
                break;
        }
    }

    private void EmitVariable(VariableNode node)
    {
        foreach (var name in node.Names)
        {
            switch (node.Type)
            {
                case "byte":
                case "char":
                case "bool":
                    _sb.AppendLine($"{name}: .byte 0");
                    break;
                case "integer":
                    _sb.AppendLine($"{name}: .word 0");
                    break;
                case "string":
                    _sb.AppendLine($"{name}: .res 32"); // Reserve 32 bytes for string
                    break;
                default:
                    _sb.AppendLine($"; Unknown variable type: {node.Type}");
                    break;
            }
        }
    }

    private void EmitSubroutine(SubroutineNode node)
    {
        var label = node.IsStart ? "start" : node.Name;
        _sb.AppendLine();
        _sb.AppendLine($"{label}:");
        EmitCompoundStatement(node.Body);
        _sb.AppendLine(node.IsStart ? "\tBRK" : "\tRTS");
    }

    private void EmitCompoundStatement(CompoundStatementNode node)
    {
        foreach (var stmt in node.Statements)
        {
            if (stmt == null) continue;
            switch (stmt)
            {
                case AssignmentNode a: EmitAssignment(a); break;
                case IfNode i: EmitIf(i); break;
                case WhileNode w: EmitWhile(w); break;
                case ReturnNode r: EmitReturn(r); break;
                case PrintNode p: EmitPrint(p); break;
                case JumpNode j: EmitJump(j); break;
                case CompoundStatementNode c: EmitCompoundStatement(c); break;
                default: _sb.AppendLine($"; Unknown statement type: {stmt.GetType().Name}"); break;
            }
        }
    }

    private void EmitAssignment(AssignmentNode node)
    {
        EmitExpression(node.Expr, "A");
        _sb.AppendLine($"\tSTA {node.Name}");
    }

    private void EmitIf(IfNode node)
    {
        var elseLabel = $"ELSE_{_labelCounter}";
        var endLabel = $"ENDIF_{_labelCounter}";
        _labelCounter++;

        EmitExpression(node.Condition, "A");
        _sb.AppendLine("\tCMP #0");
        _sb.AppendLine($"\tBEQ {elseLabel}");
        EmitStatement(node.Then);
        _sb.AppendLine($"\tJMP {endLabel}");
        _sb.AppendLine($"{elseLabel}:");
        if (node.Else != null)
            EmitStatement(node.Else);
        _sb.AppendLine($"{endLabel}:");
    }

    private void EmitWhile(WhileNode node)
    {
        var startLabel = $"WHILE_{_labelCounter}";
        var endLabel = $"ENDWHILE_{_labelCounter}";
        _labelCounter++;

        _sb.AppendLine($"{startLabel}:");
        EmitExpression(node.Condition, "A");
        _sb.AppendLine("\tCMP #0");
        _sb.AppendLine($"\tBEQ {endLabel}");
        EmitStatement(node.Body);
        _sb.AppendLine($"\tJMP {startLabel}");
        _sb.AppendLine($"{endLabel}:");
    }

    private void EmitReturn(ReturnNode node)
    {
        _sb.AppendLine("\tRTS");
    }

    private void EmitPrint(PrintNode node)
    {
        EmitExpression(node.Expr, "A");
        _sb.AppendLine("; Print value in A (user must implement print routine)");
        _sb.AppendLine("\tJSR PRINT");
    }

    private void EmitJump(JumpNode node)
    {
        _sb.AppendLine($"\tJMP {node.Target}");
    }

    private void EmitStatement(AstNode node)
    {
        switch (node)
        {
            case AssignmentNode a: EmitAssignment(a); break;
            case IfNode i: EmitIf(i); break;
            case WhileNode w: EmitWhile(w); break;
            case ReturnNode r: EmitReturn(r); break;
            case PrintNode p: EmitPrint(p); break;
            case JumpNode j: EmitJump(j); break;
            case CompoundStatementNode c: EmitCompoundStatement(c); break;
            default: _sb.AppendLine($"; Unknown statement type: {node.GetType().Name}"); break;
        }
    }

    // Expression codegen: result in A
    private void EmitExpression(ExpressionNode expr, string target)
    {
        switch (expr)
        {
            case LiteralNode l:
                _sb.AppendLine($"\tLDA #{l.Value}");
                break;
            case IdentifierNode id:
                _sb.AppendLine($"\tLDA {id.Name}");
                break;
            case BinaryOpNode bin:
                EmitExpression(bin.Left, "A");
                _sb.AppendLine("\tPHA"); // push left
                EmitExpression(bin.Right, "A");
                _sb.AppendLine("\tPLA"); // pop left to A
                switch (bin.Op)
                {
                    case "+": _sb.AppendLine("\tCLC\n\tADC"); break;
                    case "-": _sb.AppendLine("\tSEC\n\tSBC"); break;
                    case "*": _sb.AppendLine("; MUL not native, call MUL routine"); _sb.AppendLine("JSR MUL"); break;
                    case "/": _sb.AppendLine("; DIV not native, call DIV routine"); _sb.AppendLine("JSR DIV"); break;
                    case "==": _sb.AppendLine("\tCMP\n\tBEQ EQ_LABEL"); break;
                    case "!=": _sb.AppendLine("\tCMP\n\tBNE NEQ_LABEL"); break;
                    case "<": _sb.AppendLine("\tCMP\n\tBCC LT_LABEL"); break;
                    case ">": _sb.AppendLine("\tCMP\n\tBCS GT_LABEL"); break;
                    case "<=": _sb.AppendLine("\tCMP\n\tBCC LE_LABEL\n\tBEQ LE_LABEL"); break;
                    case ">=": _sb.AppendLine("\tCMP\n\tBCS GE_LABEL\n\tBEQ GE_LABEL"); break;
                    default: _sb.AppendLine($"; Unknown binary op: {bin.Op}"); break;
                }
                break;
            case UnaryOpNode un:
                EmitExpression(un.Expr, "A");
                switch (un.Op)
                {
                    case "-": _sb.AppendLine("\tEOR #$FF\n\tCLC\n\tADC #1"); break; // two's complement
                    case "!": _sb.AppendLine("; Logical NOT, user must implement"); break;
                    default: _sb.AppendLine($"; Unknown unary op: {un.Op}"); break;
                }
                break;
            default:
                _sb.AppendLine($"; Unknown expression type: {expr.GetType().Name}");
                break;
        }
    }
}