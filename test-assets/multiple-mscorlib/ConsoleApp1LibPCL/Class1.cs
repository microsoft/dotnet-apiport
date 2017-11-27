using System;

namespace ConsoleApp1LibPCL
{
    public class Class1
    {
       public Func<string> ReturnSomething
       {
          get { return () => "I'm returning something. Woot!"; }
       }
    }
}
