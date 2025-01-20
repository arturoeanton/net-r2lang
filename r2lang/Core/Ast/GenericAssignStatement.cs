#nullable disable
namespace R2Lang.Core.Ast;

 public class GenericAssignStatement : INode
    {
        public INode Left { get; set; }
        public INode Right { get; set; }

        public object Eval(Environment env)
        {
            var val = Right.Eval(env);
            if (Left is Identifier ident)
            {
                env.Set(ident.Name, val);
                return val;
            }
            else if
                (Left is AccessExpression ae) // si es AccessExpression => set al property en un objeto (ObjectInstance) o dictionary
            {
                var objVal = ae.Object.Eval(env);
                // 1) si es un ObjectInstance, actualizamos su Environment interno
                if (objVal is ObjectInstance inst)
                {
                    inst.Env.Set(ae.Member, val);
                    return val;
                }

                // 2) si es un dictionary (map<string,object>)
                if (objVal is Dictionary<string, object> dict)
                {
                    dict[ae.Member] = val;
                    return val;
                }

                throw new Exception("Cannot assign property on this type: " + objVal?.GetType().Name);
            }
            else if (Left is IndexExpression ie) // si es IndexExpression => set al array/dict en la posición [índice]
            {
                var containerVal = ie.Left.Eval(env);
                var indexVal = ie.Index.Eval(env);

                // 1) si es Dictionary<string,object>, la "clave" es la string del indexVal
                if (containerVal is Dictionary<string, object> map)
                {
                    var key = indexVal?.ToString() ?? "";
                    map[key] = val;
                    return val;
                }

                // 2) si es List<object>, la "posición" es un int
                if (containerVal is List<object> list)
                {
                    int idx = (int)BuiltinOps.ToFloat(indexVal);
                    if (idx < 0 || idx >= list.Count)
                        throw new Exception("Index out of range in assignment");
                    list[idx] = val;
                    return val;
                }

                throw new Exception("Cannot assign to index on this type: " + containerVal?.GetType().Name);
            }

            // si IndexExpression => ...
            throw new Exception("Cannot assign to that expression");
        }
    }
