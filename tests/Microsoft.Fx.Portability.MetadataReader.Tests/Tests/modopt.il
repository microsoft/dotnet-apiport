.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )
  .ver 4:0:0:0
}
.assembly ModOpt
{}

.module ModOpt.dll

.corflags 0x00000001    //  ILONLY

.class public auto ansi beforefieldinit TestClass
{
  .method public hidebysig instance void
          Foo(int32 modopt([mscorlib]System.Runtime.CompilerServices.IsConst) x) cil managed
  {
    ldarg.0
    call void TestClass::Foo(int32 modopt([mscorlib]System.Runtime.CompilerServices.IsConst))
  } // end of method TestClass::Foo 

  .method public hidebysig instance void
          Bar(int8 modreq([mscorlib]System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt([mscorlib]System.Runtime.CompilerServices.IsConst)* x) cil managed
  {
    ldarg.0
    call void TestClass::Bar(int8 modreq([mscorlib]System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt([mscorlib]System.Runtime.CompilerServices.IsConst)*)
  } // end of method TestClass::Bar
} // end of class TestClass
