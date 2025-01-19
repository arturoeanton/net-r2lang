namespace R2Lang.Core.Ast;

public class TestCase : INode
{
    public string Name { get; set; }
    public List<TestStep> Steps { get; set; } = new List<TestStep>();

    public object Eval(Environment env)
    {
        Console.WriteLine($"[TestCase] Iniciando '{Name}'");
        object result = null;
        string previousStepType = null;

        foreach (var step in Steps)
        {
            var stype = step.Type;
            if (stype == "And" && previousStepType != null)
            {
                // se reutiliza el step anterior
                stype = previousStepType;
            }
            else
            {
                previousStepType = stype;
            }

            Console.Write($"  {stype}: ");
            // si es un call
            if (step.Command is CallExpression ce)
            {
                var calVal = ce.Callee.Eval(env);
                var argVals = new List<object>();
                foreach (var a in ce.Args)
                {
                    argVals.Add(a.Eval(env));
                }

                if (calVal is UserFunction uf)
                {
                    var outVal = uf.CallStep(env, argVals.ToArray());
                    Console.WriteLine(outVal ?? "OK");
                }
                else
                {
                    var val = ce.Eval(env);
                    Console.WriteLine(val ?? "OK");
                }
            }
            else if (step.Command is FunctionLiteral fl)
            {
                var uf = fl.Eval(env) as UserFunction;
                var outVal = uf.CallStep(env);
                Console.WriteLine(outVal ?? "OK");
            }
            else
            {
                var val = step.Command.Eval(env);
                Console.WriteLine(val ?? "OK");
            }
        }

        Console.WriteLine($"[TestCase] Finalizado '{Name}'");
        return result;
    }
}