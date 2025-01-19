namespace R2Lang.Core;

  public static class BuiltinOps
    {
        public static double ToFloat(object v)
        {
            if (v == null) return 0;
            if (v is double d) return d;
            if (v is int i) return i;
            if (double.TryParse(v.ToString(), out double dd))
            {
                return dd;
            }

            throw new Exception("Cannot convert to number: " + v);
        }

        public static bool ToBool(object v)
        {
            if (v == null) return false;
            if (v is bool b) return b;
            if (v is double d) return d != 0;
            if (v is string s) return s.Length > 0;
            return true;
        }

        public static bool Equals(object a, object b)
        {
            // num => compare
            if (IsNumeric(a) && IsNumeric(b))
            {
                return Math.Abs(ToFloat(a) - ToFloat(b)) < 1e-9;
            }

            return object.Equals(a, b);
        }

        private static bool IsNumeric(object x)
        {
            return (x is double || x is int);
        }

        public static object AddValues(object a, object b)
        {
            if (IsNumeric(a) && IsNumeric(b))
            {
                return ToFloat(a) + ToFloat(b);
            }

            // array + array => etc
            if (a is string sa) return sa + (b?.ToString() ?? "");
            if (b is string sb) return (a?.ToString() ?? "") + sb;
            // fallback => numeric
            return ToFloat(a) + ToFloat(b);
        }

        public static object SubValues(object a, object b)
        {
            return ToFloat(a) - ToFloat(b);
        }

        public static object MulValues(object a, object b)
        {
            return ToFloat(a) * ToFloat(b);
        }

        public static object DivValues(object a, object b)
        {
            double denom = ToFloat(b);
            if (Math.Abs(denom) < 1e-12) throw new Exception("Division by zero");
            return ToFloat(a) / denom;
        }
    }
    