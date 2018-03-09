/// In some cases, multiple entries known to contain System.Object can be encoded. SystemObjectFinder needs to find the best match,
/// which is taken to be the highest version of possible values

.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )
  .ver 4:0:0:0
}

// 2nd mscorlib that is retargetable
.assembly extern retargetable mscorlib as mscorlib_2
{
  .publickeytoken = (7C EC 85 D7 BE A7 79 8E )
  .ver 2:0:5:0
}

.module 'multiple-mscorlib.dll'
.subsystem 0x0003       // WINDOWS_CUI 
.corflags 0x00000009    //  ILONLY 