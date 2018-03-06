public class OuterClass<a, b>
{
    public class InnerClass<c, d>
    {
        public class InnerInnerClass
        {
            public static void InnerInnerMethod(OuterClass<d,c>.InnerClass<int,a>.InnerInnerClass param1)
            {
                InnerInnerMethod(param1);
            }
        }
        public static void InnerMethod(OuterClass<c, c>.InnerClass<b, b> param1)
        {
            InnerMethod(param1);
        }
    }
    public static void OuterMethod(a param1, OuterClass<b, a>.InnerClass<b, a> param2)
    {
        OuterMethod(param1, param2);
    }
}

public class EntryClass
{
    public static void Main(string[] args)
    {
    }
}
