#pragma warning disable 0067

namespace N
{
    public class SpecTests
    {
        private unsafe void UnsafeTest()
        {
            var array = new[] { 1, 2, 3 };
            var x = new X<int>();
            int F = 5;

            unsafe
            {
                fixed (void* v = &array[0])
                {
                    x.bb("blah", ref F, v);
                }
            }
        }

        public static void Test()
        {
            var x1 = new X<int>();
            var x2 = new X<int>(1);
            var q = x1.q;
            var pi = X<int>.PI;
            var F = x1.f();
            x1.gg(new short[] { }, new int[1, 1]);

            var x3 = x1 + x2;

            var prop = x1.prop;
            x1.prop = prop;

            x1.d += X1_d;

            var @this = x1["blah"];

            var nested = new X<int>.Nested();

            X<int>.D @delegate = _ => { };

            int @int = (int)x1;

        }

        private static void X1_d(int i)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary> 
    /// Enter description here for class X.  
    /// ID string generated is "T:N.X". 
    /// </summary> 
    /// <remarks>
    /// The generic is added so that dependency finder picks it up
    /// </remarks>
    public unsafe class X<T>
    {
        /// <summary> 
        /// Enter description here for the first constructor. 
        /// ID string generated is "M:N.X.#ctor".
        /// </summary> 
        public X() { }


        /// <summary> 
        /// Enter description here for the second constructor. 
        /// ID string generated is "M:N.X.#ctor(System.Int32)".
        /// </summary> 
        /// <param name="i">Describe parameter.</param>
        public X(int i) { }


        /// <summary> 
        /// Enter description here for field q. 
        /// ID string generated is "F:N.X.q".
        /// </summary> 
        public string q;


        /// <summary> 
        /// Enter description for constant PI. 
        /// ID string generated is "F:N.X.PI".
        /// </summary> 
        public const double PI = 3.14;


        /// <summary> 
        /// Enter description for method f. 
        /// ID string generated is "M:N.X.f".
        /// </summary> 
        /// <returns>Describe return value.</returns> 
        public int f() { return 1; }


        /// <summary> 
        /// Enter description for method bb. 
        /// ID string generated is "M:N.X.bb(System.String,System.Int32@,System.Void*)".
        /// </summary> 
        /// <param name="s">Describe parameter.</param>
        /// <param name="y">Describe parameter.</param>
        /// <param name="z">Describe parameter.</param>
        /// <returns>Describe return value.</returns> 
        public int bb(string s, ref int y, void* z) { return 1; }


        /// <summary> 
        /// Enter description for method gg. 
        /// ID string generated is "M:N.X.gg(System.Int16[],System.Int32[0:,0:])". 
        /// </summary> 
        /// <param name="array1">Describe parameter.</param>
        /// <param name="array">Describe parameter.</param>
        /// <returns>Describe return value.</returns> 
        public int gg(short[] array1, int[,] array) { return 0; }


        /// <summary> 
        /// Enter description for operator. 
        /// ID string generated is "M:N.X.op_Addition(N.X,N.X)". 
        /// </summary> 
        /// <param name="x">Describe parameter.</param>
        /// <param name="xx">Describe parameter.</param>
        /// <returns>Describe return value.</returns> 
        public static X<T> operator +(X<T> x, X<T> xx) { return x; }


        /// <summary> 
        /// Enter description for property. 
        /// ID string generated is "P:N.X.prop".
        /// </summary> 
        public int prop { get { return 1; } set { } }


        /// <summary> 
        /// Enter description for event. 
        /// ID string generated is "E:N.X.d".
        /// </summary> 
        public event D d;


        /// <summary> 
        /// Enter description for property. 
        /// ID string generated is "P:N.X.Item(System.String)".
        /// </summary> 
        /// <param name="s">Describe parameter.</param>
        /// <returns></returns> 
        public int this[string s] { get { return 1; } }


        /// <summary> 
        /// Enter description for class Nested. 
        /// ID string generated is "T:N.X.Nested".
        /// </summary> 
        public class Nested { }


        /// <summary> 
        /// Enter description for delegate. 
        /// ID string generated is "T:N.X.D". 
        /// </summary> 
        /// <param name="i">Describe parameter.</param>
        public delegate void D(int i);


        /// <summary> 
        /// Enter description for operator. 
        /// ID string generated is "M:N.X.op_Explicit(N.X)~System.Int32".
        /// </summary> 
        /// <param name="x">Describe parameter.</param>
        /// <returns>Describe return value.</returns> 
        public static explicit operator int (X<T> x) { return 1; }
    }
}