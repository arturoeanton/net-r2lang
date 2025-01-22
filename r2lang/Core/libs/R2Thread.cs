#nullable disable
namespace R2Lang.Core.Libs;

public class R2Thread
{
    
    
    //poner un waitgroup para que no se cierre el programa antes de que termine la tarea
    public static WaitGroup waitGroup = new WaitGroup();
    private readonly object lockObject = new object();

    
    public static void RegisterAll(Environment env)
    {
        env.Set("r2", new BuiltinFunction(RunThread));
        env.Set("r2_wait_all", new BuiltinFunction(WaitAll));
        env.Set("r2_atomic", new BuiltinFunction(Atomic));
    }

    private static object Atomic(params object[] args)
    {
        if (args.Length < 1)
        {
            throw new Exception("r2_atomic() requires at least 1 argument");
        }
        // 1) primer argumento: UserFunction
        if (args[0] is UserFunction fn)
        {
            // 2) resto de argumentos
            var rest = args.Skip(1).ToArray();
            object out1;
            lock (waitGroup)
            {
                out1 = fn.Call(rest);
            }

            return out1;
        }
        else
        {
            throw new Exception("r2_atomic() requires a function as first argument");
        }
        return null;
    }
    public static object WaitAll(params object[] args)
    {
        waitGroup.Wait();
        return null;
    }


    private static object RunThread(params object[] args)
    {
        
        if (args.Length < 1)
        {
            throw new Exception("r2() requires at least 1 argument");
        }
        // 1) primer argumento: UserFunction
        if (args[0] is UserFunction fn)
        {
            // 2) resto de argumentos
            var rest = args.Skip(1).ToArray();
           
            // 3) ejecutar la función en un hilo 
            waitGroup.Add();
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
                    waitGroup.Done();
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
        private int _counter = 0;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        public void Add(int count = 1)
        {
            Interlocked.Add(ref _counter, count);
        }

        public void Done()
        {
            if (Interlocked.Decrement(ref _counter) == 0)
            {
                _semaphore.Release();
            }
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