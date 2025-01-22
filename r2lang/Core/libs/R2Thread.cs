#nullable disable
namespace R2Lang.Core.Libs;

public class R2Thread
{
    //poner un waitgroup para que no se cierre el programa antes de que termine la tarea
    private static readonly WaitGroup WaitGroup1 = new();

    public static void RegisterAll(Environment env)
    {
        env.Set("r2", new BuiltinFunction(RunThread));
        env.Set("r2_wait_all", new BuiltinFunction(WaitAll));
        env.Set("r2_atomic", new BuiltinFunction(Atomic));
        env.Set("r2_sleep", new BuiltinFunction(Sleep));


        env.Set("thread", new BuiltinFunction(RunThread));
        env.Set("atomic", new BuiltinFunction(Atomic));
        env.Set("wait_all", new BuiltinFunction(WaitAll));
        env.Set("sleep", new BuiltinFunction(Sleep));
    }

    private static object Sleep(params object[] args)
    {
        if (args.Length < 1) throw new Exception("sleep() requires at least 1 argument");

        if (args.Length == 1)
        {
            if (args[0] is not int ms) throw new Exception("sleep() requires an integer as first argument");
            Thread.Sleep(ms);
        }

        if (args.Length == 2)
        {
            if (args[0] is not int ms) throw new Exception("sleep() requires an integer as first argument");
            if (args[1] is not string unitTime) throw new Exception("sleep() requires an integer as second argument");

            var unit = unitTime switch
            {
                "ms" => 1,
                "s" => 1000,
                "m" => 60000,
                "h" => 3600000,
                _ => throw new Exception("sleep() requires a valid unit time")
            };

            Thread.Sleep(ms * unit);
        }

        throw new Exception("sleep() requires 1 or 2 arguments");
    }


    private static object Atomic(params object[] args)
    {
        if (args.Length < 1) throw new Exception("r2_atomic() requires at least 1 argument");
        var rest = args.Skip(1).ToArray();

        // 1) primer argumento: UserFunction
        if (args[0] is UserFunction fn)
        {
            // 2) resto de argumentos
            object out1;
            lock (WaitGroup1)
            {
                out1 = fn.Call(rest);
            }

            return out1;
        }

        if (args[0] is BuiltinFunction bf)
        {
            object out1;
            lock (WaitGroup1)
            {
                out1 = bf.Invoke(rest);
            }

            return out1;
        }

        throw new Exception("r2_atomic() requires a function as first argument");
        return null;
    }

    public static object WaitAll(params object[] args)
    {
        WaitGroup1.Wait();
        return null;
    }


    private static object RunThread(params object[] args)
    {
        if (args.Length < 1) throw new Exception("r2() requires at least 1 argument");
        // 1) primer argumento: UserFunction
        if (args[0] is UserFunction fn)
        {
            // 2) resto de argumentos
            var rest = args.Skip(1).ToArray();

            // 3) ejecutar la función en un hilo 
            WaitGroup1.Add();
            new Thread(() =>
            {
                try
                {
                    fn.Call(rest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] " + ex.Message);
                }
                finally
                {
                    WaitGroup1.Done();
                }
            }).Start();
        }
        else
        {
            throw new Exception("r2() requires a function as first argument");
        }

        return null;
    }

    public class WaitGroup
    {
        private readonly SemaphoreSlim _semaphore = new(0);
        private int _counter;

        public void Add(int count = 1)
        {
            Interlocked.Add(ref _counter, count);
        }

        public void Done()
        {
            if (Interlocked.Decrement(ref _counter) == 0) _semaphore.Release();
        }

        public async Task WaitAsync()
        {
            await _semaphore.WaitAsync();
        }

        public void Wait()
        {
            _semaphore.Wait();
        }
    }
}