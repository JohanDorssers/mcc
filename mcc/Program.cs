namespace mcc;

internal class Program
{
    static void Main(string[] args)
    {
        string source = File.ReadAllText("example.mc");

        try 
        {
            var scanner = new Scanner(source);
            var parser = new Parser(scanner);
            var ast = parser.ParseProgram();
            var codeGen = new CodeGenerator6502();
            var assembly = codeGen.Generate(ast);

            File.WriteAllText("output.asm", assembly);
            Console.WriteLine("Compilation successful. Output written to output.asm");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
