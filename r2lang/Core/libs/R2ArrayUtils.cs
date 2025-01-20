#nullable disable
namespace R2Lang.Core.Libs;

public class R2ArrayUtils
{
    
    public static void RegisterAll(Environment env)
    {
        env.Set("range", new BuiltinFunction(RangeToArray));
    }
    
    
    private static double[] RangeToArray(params object[] args)
    {
        var step = 0.0;
        //return array of longs beetwen args[0] and args[1]
        if (args.Length == 2)
        {
            step = 1.0;
        }
        else if (args.Length == 3)
        {
             step = (double)args[2];

        }
        else
        {
            throw new Exception("Range function takes 2 arguments");
        }
        
        var  start = (double)args[0];
        var end = (double)args[1];
        var dif = (int) Math.Ceiling ((Math.Abs(end - start)/step));
        // round up
            
        var result = new double[dif];

        var flagReverse = false;
        if (start > end)
        {
            (start, end) = (end, start);
            flagReverse = true;
        }

        for (double i = 0; i < dif; i++)
        {
            var index = (int)i;
            result[index] = start + i * step;
        }
        
        if (flagReverse)
        {
            Array.Reverse(result);
        }
        
        
       

        return result;


    }
}