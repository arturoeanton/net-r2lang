#nullable disable
namespace R2Lang.Core;

using R2Lang.Core.Libs;
public static class Builtins
{
    public static void RegisterAll(Environment env)
    {
        // en Go tenías RegisterLib, RegisterStd, etc.
        // Aquí puedes definir lo que gustes. 
        // Ejemplo:
        env.Set("print", (BuiltinFunction)Print);
        env.Set("println", (BuiltinFunction)Print);
        
        
        
    }

    private static object Print(params object[] args)
    {
        // Por cada argumento, invocamos PrintItem (recursivo)
        foreach (var a in args)
        {
            PrintItem(a);
            Console.Write(" ");  // separador
        }
        Console.WriteLine();    // salto de línea al final
        return null;            // tu builtin retorna "null"
    }

    private static void PrintItem(object item)
    {
        // 1) List<object>
        if (item is List<object> list)
        {
            Console.Write("[");
            for (int i = 0; i < list.Count; i++)
            {
                PrintItem(list[i]);  // recursivo
                if (i < list.Count - 1)
                    Console.Write(", ");
            }
            Console.Write("]");
            return;
        }
        
        if ( item is Array arr)
        {
            Console.Write("[");
            for (int i = 0; i < arr.Length; i++)
            {
                PrintItem(arr.GetValue(i));  // recursivo
                if (i < arr.Length - 1)
                    Console.Write(", ");
            }
            Console.Write("]");
            return;
        }
        // 3) Dictionary<string,object>
        if (item is Dictionary<string, object> dict)
        {
            Console.Write("{");
            bool first = true;
            foreach (var kv in dict)
            {
                if (!first) Console.Write(", ");
                Console.Write(kv.Key + ": ");
                PrintItem(kv.Value); // recursivo
                first = false;
            }
            Console.Write("}");
            return;
        }
        // 4) Si no es lista ni diccionario => imprimir tal cual
        Console.Write(item);
    }
    
   
    

    // si quieres mas builtins => expandir
}
