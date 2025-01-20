#nullable disable
namespace R2Lang.Core.Ast;

public class AccessExpression : INode
{
    public INode Object { get; set; }
    public string Member { get; set; }

    public object Eval(Environment env)
    {
        var objVal = Object.Eval(env);

        if (objVal is ObjectInstance inst)
        {
            var (v, found) = inst.Env.Get(Member);
            if (!found)
                throw new Exception($"Object does not have property: {Member}");
            return v;
        }

        if (objVal is Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey(Member))
                throw new Exception($"Map does not have key: {Member}");
            return dict[Member];
        }

        if (objVal is List<object> list)
        {
            //implement len
            if (Member == "len" || Member == "length")
                return (BuiltinFunction)(args =>
                {
                    // 1) Validar que no se pase ningún argumento
                    if (args.Length != 0)
                        throw new Exception("len() no admite argumentos");

                    // 2) Retornar la longitud como double (o int). 
                    return (double)list.Count;
                });

            // implement sort
            if (Member == "sort")
            {
                return (BuiltinFunction)(args =>
                {
                    // 1) Creamos una copia para no tocar la original (o no, a elección)
                    var newList = new List<object>(list);

                    if (args.Length == 0)
                    {
                        // Orden por defecto
                        newList.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
                        return newList;
                    }
                    else if (args.Length == 1)
                    {
                        // asumo el usuario pasa un userFunction (o builtinFunction) que devuelva bool
                        var comparer = args[0];

                        if (comparer is UserFunction uf)
                        {
                            // Sort con custom comparison => si uf.Call(a, b) = true => a < b
                            newList.Sort((x, y) =>
                            {
                                // Llamamos uf con (x, y)
                                var result = uf.Call(x, y);
                                // esperamos que sea bool => si true => x < y => devolvemos -1
                                // sino => +1
                                if (result is bool bres)
                                {
                                    return bres ? -1 : 1;
                                }

                                // si devuelven algo distinto => error
                                throw new Exception("sort(fn): la función de comparación debe retornar bool");
                            });
                            return newList;
                        }
                        else if (comparer is BuiltinFunction bf)
                        {
                            // Similar si el comparer es builtin
                            newList.Sort((x, y) =>
                            {
                                var result = bf(x, y);
                                if (result is bool bres)
                                    return bres ? -1 : 1;
                                throw new Exception("sort(fn): la función de comparación debe retornar bool");
                            });
                            return newList;
                        }
                        else
                        {
                            throw new Exception("sort: el argumento debe ser una función");
                        }
                    }
                    else
                    {
                        throw new Exception("sort: argumentos inválidos (0 o 1 esperado)");
                    }
                });
            }

            // implement join
            if (Member == "join")
            {
                return (BuiltinFunction)(args =>
                {
                    // 1) Validar que se pase un solo argumento
                    if (args.Length != 1)
                        throw new Exception("join() espera un argumento");

                    // 2) Validar que el argumento sea un string
                    if (!(args[0] is string sep))
                        throw new Exception("join() espera un string");

                    // 3) Unir los elementos de la lista con el separador
                    return string.Join(sep, list);
                });
            }
            
            // implement add
            if (Member == "add" || Member == "append" || Member == "push")
            {
                return (BuiltinFunction)(args =>
                {
                    // 1) Validar que se pase un solo argumento
                    if (args.Length != 1)
                        throw new Exception("add() espera un argumento");

                    // 2) Agregar el argumento a la lista
                    list.Add(args[0]);
                    return list;
                });
            }
            
            // implement pop 
            if (Member == "pop")
            {
                return (BuiltinFunction)(args =>
                {
                    // 1) Validar que no se pase ningún argumento
                    if (args.Length != 0)
                        throw new Exception("pop() no admite argumentos");

                    // 2) Validar que la lista no esté vacía
                    if (list.Count == 0)
                        throw new Exception("pop() en lista vacía");

                    // 3) Sacar el último elemento de la lista
                    var last = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    return last;
                });
            }
            
            // implement remove
            if (Member == "remove" || Member == "delete" || Member == "del")
            {
                return (BuiltinFunction)(args =>
                {
                    // 1) Validar que se pase un solo argumento
                    if (args.Length != 1)
                    {
                        throw new Exception("remove() espera un argumento");
                    }

                    // 2) castear el argumento a int
                    var index = (int)BuiltinOps.ToFloat(args[0]);
                    
                    // 3) validar index
                    if (index < 0 || index >= list.Count)
                    {
                        throw new Exception("remove() índice fuera de rango");
                    }
                    
                    // 4) Remover el elemento de la lista
                    list.RemoveAt(index);
                    return list;
                });
            }
            
            
        }

        throw new Exception("AccessExpression: Not supported type => " + objVal?.GetType().Name);
    }
}