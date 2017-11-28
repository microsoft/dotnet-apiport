namespace MultipleMscorlib
{
   class Program
   {
      static void Main(string[] args)
      {
         System.Console.WriteLine("{0}", new PortableImpl().ReturnSomething());
      }
   }
}
