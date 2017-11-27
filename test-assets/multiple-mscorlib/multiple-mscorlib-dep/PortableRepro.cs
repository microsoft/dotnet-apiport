using System;

namespace MultipleMscorlib
{
    public class PortableImpl
    {
        public Func<string> ReturnSomething
        {
            get { return () => string.Empty; }
        }
    }
}
