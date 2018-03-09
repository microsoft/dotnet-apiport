public class TestClass
{
    public static void ArglistMethod(int i, __arglist)
    {
    }

    public static void ArglistMethod2(__arglist)
    {
    }

    public static void Main(string[] args)
    {
        ArglistMethod(5, __arglist(1, 2, 3));
        ArglistMethod2(__arglist());
    }
}
