#nullable disable
using R2Lang.Core;
using R2Lang.Core.Ast;


public class ForStatement : INode
{
    public INode Init { get; set; }
    public INode Condition { get; set; }
    public INode Post { get; set; }
    public BlockStatement Body { get; set; }

    // flag in => for(... in array|dict)
    public bool inFlag = false;

    // nombre de la variable que vamos a iterar (ej: "miArreglo")
    public string inArray;

    // nombre de la variable que usaremos como índice/clave (ej: "i" o "k")
    public string inIndexName;

    public object Eval(R2Lang.Core.Environment env)
    {
        // 1) if es "for(... in ...)"
        if (inFlag)
        {
            // Obtenemos el valor de la variable inArray
            object raw;
            if (inArray != null)
            {
                bool found = false;
                (raw, found) = env.Get(inArray);
                if (!found)
                    throw new Exception($"Variable '{inArray}' no existe en el entorno.");
            }
            else
            {
                var inner1 = new R2Lang.Core.Environment(env);
                raw = Init?.Eval(inner1);
            }

            object result = null;
            var list1 = raw is Array ? ((Array)raw).Cast<object>().ToList() : raw;
            // a) si es List<object> => iterar con índice
            if (list1 is List<object> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    // asignar la variable "inIndexName" = i
                    env.Set(inIndexName, i);
                    env.Set("$k", i);

                    // también podrías asignar env.Set("$v", list[i]) si lo deseas
                    env.Set("$v", list[i]);

                    var val = Body.Eval(env);
                    if (val is ReturnValue rv)
                        return rv; // si hay un return dentro del for

                    result = val;
                }

                return result;
            }
            // b) si es Dictionary<string, object> => iterar con claves
            else if (raw is Dictionary<string, object> map)
            {
                object resultDict = null;
                foreach (var kv in map)
                {
                    // "inIndexName" = clave (string)
                    env.Set(inIndexName, kv.Key);

                    // también podrías: env.Set("$v", kv.Value);

                    var val = Body.Eval(env);
                    if (val is ReturnValue rv)
                        return rv;

                    resultDict = val;
                }

                return resultDict;
            }
            else
            {
                throw new Exception(
                    $"for(... in ...) solo soporta listas o diccionarios. '{inArray}' es {raw?.GetType().Name}");
            }
        }

        // 2) si no es "for(... in ...)", hacemos for(init; cond; post)
        var inner = new R2Lang.Core.Environment(env);
        Init?.Eval(inner);

        object resultNormal = null;
        while (true)
        {
            if (Condition != null)
            {
                var c = R2Lang.Core.BuiltinOps.ToBool(Condition.Eval(inner));
                if (!c) break;
            }

            var val = Body.Eval(inner);
            if (val is ReturnValue rv)
                return rv;
            resultNormal = val;

            Post?.Eval(inner);
        }

        return resultNormal;
    }
}