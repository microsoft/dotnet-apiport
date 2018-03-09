///
/// OnlyNetStandardReference mimics the pattern of .NET Standard
/// facade assemblies for .NET Framework which type forward to the 
/// netstandard facade and don't have any of the usual core library
/// references (like mscorlib or system.runtime)
///

// Netstandard reference
.assembly extern netstandard
{
  .publickeytoken = (CC 7B 13 FF CD 2D DD 51 )
  .ver 2:0:0:0
}

// Assembly declaration
.assembly OnlyNetStandardReference
{
  .ver 1:0:0:0
}

// A forwarded type
.class extern forwarder System.Linq.Enumerable
{
  .assembly extern netstandard
}

.module OnlyNetStandardReference.dll
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000009    //  ILONLY
