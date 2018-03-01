using System;

namespace WithGenericsAndReference
{
    class WithGenericsAndReferenceTest
    {
        static void Main(string[] args)
        {
            // Reference EmptyProject
            TestAssembly1.EmptyProject.Main(new[] { "hello" });

            Handler += Program_Handler;
        }

        static void Program_Handler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        static void TestGeneric<T>(T item)
        {
            Console.WriteLine(item);

            Handler(null, new EventArgs());
        }

        public static event EventHandler Handler;
    }
}
