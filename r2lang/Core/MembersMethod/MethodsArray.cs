namespace R2Lang.Core.MembersMethod;

public class MethodsArray
{
    public static Dictionary<string, Func<List<object>, BuiltinFunction>> Methods = new();

    public static void RegisterMethods()
    {
        Methods.Add("len", Len);
        Methods.Add("length", Len);
        Methods.Add("sort",Sort);
        Methods.Add("add",Add);
        Methods.Add("append",Add);
        Methods.Add("push",Add);
        Methods.Add("pop",Pop);
        Methods.Add("del",Del);
        Methods.Add("delete",Del);
        Methods.Add("remove",Del);

        Methods.Add("find",Find);
        Methods.Add("index",Find);
        Methods.Add("join",Join);
        Methods.Add("reverse",Reverse);
        Methods.Add("map",Map);
        Methods.Add("filter",Filter);
        Methods.Add("reduce",Reduce);
    }


    private static BuiltinFunction Len(List<object> list)
    {
        return args =>
        {
            // 1) Validar que no se pase ningún argumento
            if (args.Length != 0)
                throw new Exception("len() no admite argumentos");

            // 2) Retornar la longitud como double (o int). 
            return (double)list.Count;
        };
    }
    private static BuiltinFunction Sort(List<object> list)
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

            if (args.Length == 1)
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
                        if (result is bool bres) return bres ? -1 : 1;

                        // si devuelven algo distinto => error
                        throw new Exception("sort(fn): la función de comparación debe retornar bool");
                    });
                    return newList;
                }

                if (comparer is BuiltinFunction bf)
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

                throw new Exception("sort: el argumento debe ser una función");
            }

            throw new Exception("sort: argumentos inválidos (0 o 1 esperado)");
        });
    }
    private static BuiltinFunction Join(List<object> list)
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
    private static BuiltinFunction Add(List<object> list) 
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
    private static BuiltinFunction Pop(List<object> list) //if (Member == "pop")
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
    private static BuiltinFunction Del(List<object> list) //if (Member == "remove" || Member == "delete" || Member == "del")
    {
        return (BuiltinFunction)(args =>
        {
            // 1) Validar que se pase un solo argumento
            if (args.Length != 1) throw new Exception("remove() espera un argumento");

            // 2) castear el argumento a int
            var index = (int)BuiltinOps.ToFloat(args[0]);

            // 3) validar index
            if (index < 0 || index >= list.Count) throw new Exception("remove() índice fuera de rango");

            // 4) Remover el elemento de la lista
            list.RemoveAt(index);
            return list;
        });
    }

    // implement find can recive a function or a value
    private static BuiltinFunction Find(List<object> list) //if (Member == "find")
    {
        return (BuiltinFunction)(args =>
        {
            // 1) Validar que se no mas de dos argumentos
            if (args.Length > 2)
                throw new Exception("find() espera uno o dos argumentos");

            if (args.Length == 1)
            {
                // validar que el argumento sea un elemento o una UserFunction
                if (args[0] is UserFunction uf)
                {
                    // 2) Buscar el primer elemento que cumpla con la condición
                    var i = 0;
                    foreach (var item in list)
                    {
                        if (uf.Call(item) is bool bres && bres) return i;
                        i++;
                    }
                }
                else
                {
                    // 2) Buscar el primer elemento que sea igual al argumento
                    return list.IndexOf(args[0]);
                }
            }

            if (args.Length == 2)
            {
                // validar que el primer argumento sea un elemento o una UserFunction
                if (args[0] is UserFunction uf)
                {
                    // 2) Buscar el primer elemento que cumpla con la condición
                    var i = 0;
                    foreach (var item in list)
                    {
                        if (uf.Call(item, args[1]) is bool bres && bres) return i;
                        i++;
                    }
                }
                else
                {
                    throw new Exception("find() espera un UserFunction o un valor y un UserFunction");
                }
            }


            // 4) Si no se encontró, retornar null
            return null;
        });
    }

    // implement reverser
    private static BuiltinFunction Reverse(List<object> list) //if (Member == "reverse")
    {
        return (BuiltinFunction)(args =>
        {
            // 1) Validar que no se pase ningún argumento
            if (args.Length != 0)
                throw new Exception("reverse() no admite argumentos");

            // 2) Revertir la lista
            list.Reverse();
            return list;
        });
    }

    // implement map
    private static BuiltinFunction Map(List<object> list) //if (Member == "map")
    {
        return (BuiltinFunction)(args =>
        {
            // 1) Validar que se pase un solo argumento
            if (args.Length != 1)
                throw new Exception("map() espera un argumento");

            // 2) Validar que el argumento sea una UserFunction
            if (!(args[0] is UserFunction uf))
                throw new Exception("map() espera una función");

            // 3) Mapear la lista
            var newList = new List<object>();
            foreach (var item in list) newList.Add(uf.Call(item));
            return newList;
        });
    }

    // implement filter
    private static BuiltinFunction Filter(List<object> list) //if (Member == "filter")
    {
        return (BuiltinFunction)(args =>
        {
            // 1) Validar que se pase un solo argumento
            if (args.Length != 1)
                throw new Exception("filter() espera un argumento");

            // 2) Validar que el argumento sea una UserFunction
            if (!(args[0] is UserFunction uf))
                throw new Exception("filter() espera una función");

            // 3) Filtrar la lista
            var newList = new List<object>();
            foreach (var item in list)
                if (uf.Call(item) is bool bres && bres)
                    newList.Add(item);

            return newList;
        });
    }

    // implement reduce
    private static BuiltinFunction Reduce(List<object> list) //if (Member == "reduce")
    {
        return (BuiltinFunction)(args =>
        {
            // 1) Validar que se pase un solo argumento
            if (args.Length != 1)
                throw new Exception("reduce() espera un argumento");

            // 2) Validar que el argumento sea una UserFunction
            if (!(args[0] is UserFunction uf))
                throw new Exception("reduce() espera una función");

            // 3) Reducir la lista
            if (list.Count == 0)
                throw new Exception("reduce() en lista vacía");

            var acum = list[0];
            for (var i = 1; i < list.Count; i++) acum = uf.Call(acum, list[i]);
            return acum;
        });
    }
}