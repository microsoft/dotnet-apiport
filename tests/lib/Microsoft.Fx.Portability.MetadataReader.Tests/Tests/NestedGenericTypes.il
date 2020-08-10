.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly NestedGenericTypes
{}

.module NestedGenericTypes.dll

.corflags 0x00000001    //  ILONLY

.class public auto ansi beforefieldinit OuterClass`2<a,b>
{
  .class auto ansi nested public beforefieldinit InnerClass`2<a,b,c,d>
  {
    .method public hidebysig static void 
            InnerMethod(class OuterClass`2/InnerClass`2<!c,!c> param1) cil managed
    {
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  call       void class OuterClass`2/InnerClass`2<!a,!b,!c,!d>::InnerMethod(class OuterClass`2/InnerClass`2<!2,!2>)
      IL_0007:  nop
      IL_0008:  ret
    } // end of method InnerClass`2::InnerMethod
  } // end of class InnerClass`2

  .method public hidebysig static void  OuterMethod(!a param1,
                                                    class OuterClass`2/InnerClass`2<!b,!a,!b,!a> param2) cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  call       void class OuterClass`2<!a,!b>::OuterMethod(!0,
                                                                     class OuterClass`2/InnerClass`2<!1,!0,!1,!0>)
    IL_0008:  nop
    IL_0009:  ret
  } // end of method OuterClass`2::OuterMethod
} // end of class OuterClass`2
