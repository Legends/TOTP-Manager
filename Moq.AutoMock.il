
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.8.3928.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern netstandard
{
  .publickeytoken = (CC 7B 13 FF CD 2D DD 51 )                         // .{...-.Q
  .ver 2:0:0:0
}
.assembly extern Moq
{
  .publickeytoken = (69 F4 91 C3 94 45 E9 20 )                         // i....E. 
  .ver 4:18:0:0
}
.assembly extern NonBlocking
{
  .ver 1:0:0:0
}
.assembly Moq.AutoMock
{
  .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [netstandard]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                                63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [netstandard]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [netstandard]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 02 00 00 00 00 00 ) 

  .custom instance void [netstandard]System.Runtime.CompilerServices.InternalsVisibleToAttribute::.ctor(string) = ( 01 00 12 4D 6F 71 2E 41 75 74 6F 4D 6F 63 6B 2E   // ...Moq.AutoMock.
                                                                                                                    54 65 73 74 73 00 00 )                            // Tests..
  .custom instance void [netstandard]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = ( 01 00 19 2E 4E 45 54 53 74 61 6E 64 61 72 64 2C   // ....NETStandard,
                                                                                                           56 65 72 73 69 6F 6E 3D 76 32 2E 30 01 00 54 0E   // Version=v2.0..T.
                                                                                                           14 46 72 61 6D 65 77 6F 72 6B 44 69 73 70 6C 61   // .FrameworkDispla
                                                                                                           79 4E 61 6D 65 00 )                               // yName.
  .custom instance void [netstandard]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = ( 01 00 24 54 69 6D 20 4B 65 6C 6C 6F 67 67 2C 20   // ..$Tim Kellogg, 
                                                                                                   41 64 61 6D 20 48 65 77 69 74 74 2C 20 4B 65 76   // Adam Hewitt, Kev
                                                                                                   69 6E 20 42 6F 73 74 00 00 )                      // in Bost..
  .custom instance void [netstandard]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = ( 01 00 07 52 65 6C 65 61 73 65 00 00 )             // ...Release..
  .custom instance void [netstandard]System.Reflection.AssemblyCopyrightAttribute::.ctor(string) = ( 01 00 1A 43 6F 70 79 72 69 67 68 74 20 54 69 6D   // ...Copyright Tim
                                                                                                     20 4B 65 6C 6C 6F 67 67 20 32 30 32 33 00 00 )    //  Kellogg 2023..
  .custom instance void [netstandard]System.Reflection.AssemblyFileVersionAttribute::.ctor(string) = ( 01 00 07 33 2E 35 2E 30 2E 30 00 00 )             // ...3.5.0.0..
  .custom instance void [netstandard]System.Reflection.AssemblyInformationalVersionAttribute::.ctor(string) = ( 01 00 2E 33 2E 35 2E 30 2B 63 63 30 35 31 39 36   // ...3.5.0+cc05196
                                                                                                                36 32 30 33 30 38 64 39 61 31 65 31 34 66 38 34   // 620308d9a1e14f84
                                                                                                                62 66 62 36 36 32 34 30 64 33 61 38 36 32 30 34   // bfb66240d3a86204
                                                                                                                39 00 00 )                                        // 9..
  .custom instance void [netstandard]System.Reflection.AssemblyProductAttribute::.ctor(string) = ( 01 00 0C 4D 6F 71 2E 41 75 74 6F 4D 6F 63 6B 00   // ...Moq.AutoMock.
                                                                                                   00 ) 
  .custom instance void [netstandard]System.Reflection.AssemblyTitleAttribute::.ctor(string) = ( 01 00 0C 4D 6F 71 2E 41 75 74 6F 4D 6F 63 6B 00   // ...Moq.AutoMock.
                                                                                                 00 ) 
  .custom instance void [netstandard]System.Reflection.AssemblyMetadataAttribute::.ctor(string,
                                                                                        string) = ( 01 00 0D 52 65 70 6F 73 69 74 6F 72 79 55 72 6C   // ...RepositoryUrl
                                                                                                    25 68 74 74 70 73 3A 2F 2F 67 69 74 68 75 62 2E   // %https://github.
                                                                                                    63 6F 6D 2F 6D 6F 71 2F 4D 6F 71 2E 41 75 74 6F   // com/moq/Moq.Auto
                                                                                                    4D 6F 63 6B 65 72 00 00 )                         // Mocker..
  .custom instance void [netstandard]System.Resources.NeutralResourcesLanguageAttribute::.ctor(string) = ( 01 00 05 65 6E 2D 55 53 00 00 )                   // ...en-US..
  .hash algorithm 0x00008004
  .ver 3:5:0:0
}
.mresource public Moq.AutoMock.Resources.Strings.resources
{
  // Offset: 0x00000000 Length: 0x00000146
  // WARNING: managed resource file Moq.AutoMock.Resources.Strings.resources created
}
.module Moq.AutoMock.dll
// MVID: {AB93F0A8-DD32-4B1B-A3F7-B8DFB61BF044}
.imagebase 0x10000000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x0000027E43C90000


// =============== CLASS MEMBERS DECLARATION ===================

.class private auto ansi sealed beforefieldinit Microsoft.CodeAnalysis.EmbeddedAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void Microsoft.CodeAnalysis.EmbeddedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method EmbeddedAttribute::.ctor

} // end of class Microsoft.CodeAnalysis.EmbeddedAttribute

.class private auto ansi sealed beforefieldinit System.Runtime.CompilerServices.NullableAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void Microsoft.CodeAnalysis.EmbeddedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 84 6B 00 00 02 00 54 02 0D 41 6C 6C 6F 77   // ...k....T..Allow
                                                                                                                               4D 75 6C 74 69 70 6C 65 00 54 02 09 49 6E 68 65   // Multiple.T..Inhe
                                                                                                                               72 69 74 65 64 00 )                               // rited.
  .field public initonly uint8[] NullableFlags
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(uint8 A_1) cil managed
  {
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldc.i4.1
    IL_0008:  newarr     [netstandard]System.Byte
    IL_000d:  dup
    IL_000e:  ldc.i4.0
    IL_000f:  ldarg.1
    IL_0010:  stelem.i1
    IL_0011:  stfld      uint8[] System.Runtime.CompilerServices.NullableAttribute::NullableFlags
    IL_0016:  ret
  } // end of method NullableAttribute::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(uint8[] A_1) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      uint8[] System.Runtime.CompilerServices.NullableAttribute::NullableFlags
    IL_000d:  ret
  } // end of method NullableAttribute::.ctor

} // end of class System.Runtime.CompilerServices.NullableAttribute

.class private auto ansi sealed beforefieldinit System.Runtime.CompilerServices.NullableContextAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void Microsoft.CodeAnalysis.EmbeddedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 4C 14 00 00 02 00 54 02 0D 41 6C 6C 6F 77   // ..L.....T..Allow
                                                                                                                               4D 75 6C 74 69 70 6C 65 00 54 02 09 49 6E 68 65   // Multiple.T..Inhe
                                                                                                                               72 69 74 65 64 00 )                               // rited.
  .field public initonly uint8 Flag
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(uint8 A_1) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      uint8 System.Runtime.CompilerServices.NullableContextAttribute::Flag
    IL_000d:  ret
  } // end of method NullableContextAttribute::.ctor

} // end of class System.Runtime.CompilerServices.NullableContextAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.AllowNullAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 80 09 00 00 01 00 54 02 09 49 6E 68 65 72   // ........T..Inher
                                                                                                                               69 74 65 64 00 )                                  // ited.
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method AllowNullAttribute::.ctor

} // end of class System.Diagnostics.CodeAnalysis.AllowNullAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.DisallowNullAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 80 09 00 00 01 00 54 02 09 49 6E 68 65 72   // ........T..Inher
                                                                                                                               69 74 65 64 00 )                                  // ited.
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method DisallowNullAttribute::.ctor

} // end of class System.Diagnostics.CodeAnalysis.DisallowNullAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 40 00 00 00 01 00 54 02 09 49 6E 68 65 72   // ..@.....T..Inher
                                                                                                                               69 74 65 64 00 )                                  // ited.
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method DoesNotReturnAttribute::.ctor

} // end of class System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 00 08 00 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(bool parameterValue) cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method DoesNotReturnIfAttribute::.ctor

  .method public hidebysig specialname instance bool 
          get_ParameterValue() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldnull
    IL_0001:  throw
  } // end of method DoesNotReturnIfAttribute::get_ParameterValue

  .property instance bool ParameterValue()
  {
    .get instance bool System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute::get_ParameterValue()
  } // end of property DoesNotReturnIfAttribute::ParameterValue
} // end of class System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.MaybeNullAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 80 29 00 00 01 00 54 02 09 49 6E 68 65 72   // ...)....T..Inher
                                                                                                                               69 74 65 64 00 )                                  // ited.
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method MaybeNullAttribute::.ctor

} // end of class System.Diagnostics.CodeAnalysis.MaybeNullAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 00 08 00 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(bool returnValue) cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method MaybeNullWhenAttribute::.ctor

  .method public hidebysig specialname instance bool 
          get_ReturnValue() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldnull
    IL_0001:  throw
  } // end of method MaybeNullWhenAttribute::get_ReturnValue

  .property instance bool ReturnValue()
  {
    .get instance bool System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute::get_ReturnValue()
  } // end of property MaybeNullWhenAttribute::ReturnValue
} // end of class System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.NotNullAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 80 29 00 00 01 00 54 02 09 49 6E 68 65 72   // ...)....T..Inher
                                                                                                                               69 74 65 64 00 )                                  // ited.
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method NotNullAttribute::.ctor

} // end of class System.Diagnostics.CodeAnalysis.NotNullAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 80 28 00 00 02 00 54 02 0D 41 6C 6C 6F 77   // ...(....T..Allow
                                                                                                                               4D 75 6C 74 69 70 6C 65 01 54 02 09 49 6E 68 65   // Multiple.T..Inhe
                                                                                                                               72 69 74 65 64 00 )                               // rited.
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string parameterName) cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method NotNullIfNotNullAttribute::.ctor

  .method public hidebysig specialname instance string 
          get_ParameterName() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldnull
    IL_0001:  throw
  } // end of method NotNullIfNotNullAttribute::get_ParameterName

  .property instance string ParameterName()
  {
    .get instance string System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute::get_ParameterName()
  } // end of property NotNullIfNotNullAttribute::ParameterName
} // end of class System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute

.class private auto ansi sealed beforefieldinit System.Diagnostics.CodeAnalysis.NotNullWhenAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void [netstandard]System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 00 08 00 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(bool returnValue) cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method NotNullWhenAttribute::.ctor

  .method public hidebysig specialname instance bool 
          get_ReturnValue() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldnull
    IL_0001:  throw
  } // end of method NotNullWhenAttribute::get_ReturnValue

  .property instance bool ReturnValue()
  {
    .get instance bool System.Diagnostics.CodeAnalysis.NotNullWhenAttribute::get_ReturnValue()
  } // end of property NotNullWhenAttribute::ReturnValue
} // end of class System.Diagnostics.CodeAnalysis.NotNullWhenAttribute

.class public auto ansi beforefieldinit Moq.AutoMock.AutoMocker
       extends [netstandard]System.Object
       implements [netstandard]System.IServiceProvider
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .interfaceimpl type [netstandard]System.IServiceProvider
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .class auto ansi serializable sealed nested private beforefieldinit '<>c'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public static initonly class Moq.AutoMock.AutoMocker/'<>c' '<>9'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,class [netstandard]System.Type> '<>9__21_0'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,object> '<>9__21_1'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,object> '<>9__28_0'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> '<>9__65_1'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,object> '<>9__74_0'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,int32> '<>9__75_0'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> '<>9__75_1'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> '<>9__75_4'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method private hidebysig specialname rtspecialname static 
            void  .cctor() cil managed
    {
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  newobj     instance void Moq.AutoMock.AutoMocker/'<>c'::.ctor()
      IL_0005:  stsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
      IL_000a:  ret
    } // end of method '<>c'::.cctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c'::.ctor

    .method assembly hidebysig instance class [netstandard]System.Type 
            '<get_ResolvedObjects>b__21_0'(valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> kvp) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  ldarga.s   kvp
      IL_0002:  call       instance !0 valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::get_Key()
      IL_0007:  ret
    } // end of method '<>c'::'<get_ResolvedObjects>b__21_0'

    .method assembly hidebysig instance object 
            '<get_ResolvedObjects>b__21_1'(valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> kvp) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       40 (0x28)
      .maxstack  1
      .locals init (class Moq.AutoMock.MockInstance V_0,
               object V_1)
      IL_0000:  ldarga.s   kvp
      IL_0002:  call       instance !1 valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::get_Value()
      IL_0007:  isinst     Moq.AutoMock.MockInstance
      IL_000c:  stloc.0
      IL_000d:  ldloc.0
      IL_000e:  brfalse.s  IL_0019

      IL_0010:  ldloc.0
      IL_0011:  callvirt   instance class [Moq]Moq.Mock Moq.AutoMock.MockInstance::get_Mock()
      IL_0016:  stloc.1
      IL_0017:  br.s       IL_0026

      IL_0019:  ldarga.s   kvp
      IL_001b:  call       instance !1 valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::get_Value()
      IL_0020:  callvirt   instance object Moq.AutoMock.IInstance::get_Value()
      IL_0025:  stloc.1
      IL_0026:  ldloc.1
      IL_0027:  ret
    } // end of method '<>c'::'<get_ResolvedObjects>b__21_1'

    .method assembly hidebysig instance object 
            '<CreateInstance>b__28_0'(class Moq.AutoMock.IInstance x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance object Moq.AutoMock.IInstance::get_Value()
      IL_0006:  ret
    } // end of method '<>c'::'<CreateInstance>b__28_0'

    .method assembly hidebysig instance bool 
            '<Combine>b__65_1'(class [netstandard]System.Reflection.MethodInfo x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       17 (0x11)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance string [netstandard]System.Reflection.MemberInfo::get_Name()
      IL_0006:  ldstr      "As"
      IL_000b:  call       bool [netstandard]System.String::op_Equality(string,
                                                                        string)
      IL_0010:  ret
    } // end of method '<>c'::'<Combine>b__65_1'

    .method assembly hidebysig instance object 
            '<CreateMock>b__74_0'(class Moq.AutoMock.IInstance x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance object Moq.AutoMock.IInstance::get_Value()
      IL_0006:  ret
    } // end of method '<>c'::'<CreateMock>b__74_0'

    .method assembly hidebysig instance int32 
            '<TryGetConstructorInvocation>b__75_0'(class [netstandard]System.Reflection.ConstructorInfo x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance class [netstandard]System.Reflection.ParameterInfo[] [netstandard]System.Reflection.MethodBase::GetParameters()
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  ret
    } // end of method '<>c'::'<TryGetConstructorInvocation>b__75_0'

    .method assembly hidebysig instance bool 
            '<TryGetConstructorInvocation>b__75_1'(class [netstandard]System.Reflection.ConstructorInfo x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       5 (0x5)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldnull
      IL_0002:  cgt.un
      IL_0004:  ret
    } // end of method '<>c'::'<TryGetConstructorInvocation>b__75_1'

    .method assembly hidebysig instance bool 
            '<TryGetConstructorInvocation>b__75_4'(class [netstandard]System.Reflection.ConstructorInfo x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance class [netstandard]System.Reflection.ParameterInfo[] [netstandard]System.Reflection.MethodBase::GetParameters()
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  ldc.i4.0
      IL_0009:  ceq
      IL_000b:  ret
    } // end of method '<>c'::'<TryGetConstructorInvocation>b__75_4'

  } // end of class '<>c'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass32_0`2'<class TService,class (!TService) TImplementation>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [Moq]Moq.Mock`1<!TImplementation> selfMock
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 00 01 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass32_0`2'::.ctor

    .method assembly hidebysig instance void 
            '<WithSelfMock>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> typeMap) cil managed
    {
      // Code size       60 (0x3c)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldtoken    !TImplementation
      IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [Moq]Moq.Mock`1<!1> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass32_0`2'<!TService,!TImplementation>::selfMock
      IL_0011:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
      IL_0016:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_001b:  ldarg.1
      IL_001c:  ldtoken    !TService
      IL_0021:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
      IL_0026:  ldarg.0
      IL_0027:  ldfld      class [Moq]Moq.Mock`1<!1> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass32_0`2'<!TService,!TImplementation>::selfMock
      IL_002c:  callvirt   instance class [Moq]Moq.Mock`1<!!0> class [Moq]Moq.Mock`1<!TImplementation>::As<!0>()
      IL_0031:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
      IL_0036:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_003b:  ret
    } // end of method '<>c__DisplayClass32_0`2'::'<WithSelfMock>b__0'

  } // end of class '<>c__DisplayClass32_0`2'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass33_0`1'<class T>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [Moq]Moq.Mock`1<!T> selfMock
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 00 01 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass33_0`1'::.ctor

    .method assembly hidebysig instance void 
            '<WithSelfMock>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> typeMap) cil managed
    {
      // Code size       28 (0x1c)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldtoken    !T
      IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [Moq]Moq.Mock`1<!0> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass33_0`1'<!T>::selfMock
      IL_0011:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
      IL_0016:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_001b:  ret
    } // end of method '<>c__DisplayClass33_0`1'::'<WithSelfMock>b__0'

  } // end of class '<>c__DisplayClass33_0`1'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass34_0'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [netstandard]System.Type implementationType
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public class [Moq]Moq.Mock selfMock
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public class [netstandard]System.Type serviceType
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass34_0'::.ctor

    .method assembly hidebysig instance void 
            '<WithSelfMock>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> typeMap) cil managed
    {
      // Code size       58 (0x3a)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::implementationType
      IL_0007:  ldarg.0
      IL_0008:  ldfld      class [Moq]Moq.Mock Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::selfMock
      IL_000d:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
      IL_0012:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_0017:  ldarg.1
      IL_0018:  ldarg.0
      IL_0019:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::serviceType
      IL_001e:  ldarg.0
      IL_001f:  ldfld      class [Moq]Moq.Mock Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::selfMock
      IL_0024:  ldarg.0
      IL_0025:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::serviceType
      IL_002a:  call       class [Moq]Moq.Mock Moq.AutoMock.MockExtensions::As(class [Moq]Moq.Mock,
                                                                               class [netstandard]System.Type)
      IL_002f:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
      IL_0034:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_0039:  ret
    } // end of method '<>c__DisplayClass34_0'::'<WithSelfMock>b__0'

  } // end of class '<>c__DisplayClass34_0'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass35_0'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [netstandard]System.Type implementationType
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public class [Moq]Moq.Mock selfMock
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass35_0'::.ctor

    .method assembly hidebysig instance void 
            '<WithSelfMock>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> typeMap) cil managed
    {
      // Code size       24 (0x18)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0'::implementationType
      IL_0007:  ldarg.0
      IL_0008:  ldfld      class [Moq]Moq.Mock Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0'::selfMock
      IL_000d:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
      IL_0012:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_0017:  ret
    } // end of method '<>c__DisplayClass35_0'::'<WithSelfMock>b__0'

  } // end of class '<>c__DisplayClass35_0'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass39_0'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [netstandard]System.Type 'type'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public object service
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass39_0'::.ctor

    .method assembly hidebysig instance void 
            '<Use>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> typeMap) cil managed
    {
      // Code size       24 (0x18)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::'type'
      IL_0007:  ldarg.0
      IL_0008:  ldfld      object Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::service
      IL_000d:  newobj     instance void Moq.AutoMock.RealInstance::.ctor(object)
      IL_0012:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_0017:  ret
    } // end of method '<>c__DisplayClass39_0'::'<Use>b__0'

  } // end of class '<>c__DisplayClass39_0'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass40_0`1'<class TService>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [Moq]Moq.Mock`1<!TService> mockedService
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 00 01 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass40_0`1'::.ctor

    .method assembly hidebysig instance void 
            '<Use>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> typeMap) cil managed
    {
      // Code size       43 (0x2b)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldtoken    !TService
      IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [Moq]Moq.Mock`1<!0> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass40_0`1'<!TService>::mockedService
      IL_0011:  dup
      IL_0012:  brtrue.s   IL_0020

      IL_0014:  pop
      IL_0015:  ldstr      "mockedService"
      IL_001a:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
      IL_001f:  throw

      IL_0020:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
      IL_0025:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_002a:  ret
    } // end of method '<>c__DisplayClass40_0`1'::'<Use>b__0'

  } // end of class '<>c__DisplayClass40_0`1'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass58_0`1'<class TService>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!TService>> setup
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 03 00 00 00 00 01 01 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass58_0`1'::.ctor

    .method assembly hidebysig instance class [Moq]Moq.Language.Flow.ISetup`1<!TService> 
            '<Setup>b__0'(class [Moq]Moq.Mock`1<!TService> m) cil managed
    {
      // Code size       13 (0xd)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass58_0`1'<!TService>::setup
      IL_0007:  callvirt   instance class [Moq]Moq.Language.Flow.ISetup`1<!0> class [Moq]Moq.Mock`1<!TService>::Setup(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>>)
      IL_000c:  ret
    } // end of method '<>c__DisplayClass58_0`1'::'<Setup>b__0'

  } // end of class '<>c__DisplayClass58_0`1'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass59_0`2'<class TService,TReturn>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!TService,!TReturn>> setup
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 04 00 00 00 00 01 01 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass59_0`2'::.ctor

    .method assembly hidebysig instance class [Moq]Moq.Language.Flow.ISetup`2<!TService,!TReturn> 
            '<Setup>b__0'(class [Moq]Moq.Mock`1<!TService> m) cil managed
    {
      .param [0]
      .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 03 00 00 00 01 01 00 00 00 ) 
      // Code size       13 (0xd)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!1>> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass59_0`2'<!TService,!TReturn>::setup
      IL_0007:  callvirt   instance class [Moq]Moq.Language.Flow.ISetup`2<!0,!!0> class [Moq]Moq.Mock`1<!TService>::Setup<!1>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>)
      IL_000c:  ret
    } // end of method '<>c__DisplayClass59_0`2'::'<Setup>b__0'

  } // end of class '<>c__DisplayClass59_0`2'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass60_0`1'<class TService>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public string methodName
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass60_0`1'::.ctor

    .method assembly hidebysig instance class [Moq]Moq.Language.Flow.ISetup`1<!TService> 
            '<SetupWithAny>b__0'(class [Moq]Moq.Mock`1<!TService> m) cil managed
    {
      // Code size       13 (0xd)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      string class Moq.AutoMock.AutoMocker/'<>c__DisplayClass60_0`1'<!TService>::methodName
      IL_0007:  call       class [Moq]Moq.Language.Flow.ISetup`1<!!0> Moq.AutoMock.MockExtensions::SetupWithAny<!TService>(class [Moq]Moq.Mock`1<!!0>,
                                                                                                                           string)
      IL_000c:  ret
    } // end of method '<>c__DisplayClass60_0`1'::'<SetupWithAny>b__0'

  } // end of class '<>c__DisplayClass60_0`1'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass61_0`2'<class TService,TReturn>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public string methodName
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass61_0`2'::.ctor

    .method assembly hidebysig instance class [Moq]Moq.Language.Flow.ISetup`2<!TService,!TReturn> 
            '<SetupWithAny>b__0'(class [Moq]Moq.Mock`1<!TService> m) cil managed
    {
      .param [0]
      .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 03 00 00 00 01 01 00 00 00 ) 
      // Code size       13 (0xd)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      string class Moq.AutoMock.AutoMocker/'<>c__DisplayClass61_0`2'<!TService,!TReturn>::methodName
      IL_0007:  call       class [Moq]Moq.Language.Flow.ISetup`2<!!0,!!1> Moq.AutoMock.MockExtensions::SetupWithAny<!TService,!TReturn>(class [Moq]Moq.Mock`1<!!0>,
                                                                                                                                        string)
      IL_000c:  ret
    } // end of method '<>c__DisplayClass61_0`2'::'<SetupWithAny>b__0'

  } // end of class '<>c__DisplayClass61_0`2'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass64_0`2'<class TService,TReturn>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!TService,!TReturn>> setup
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 04 00 00 00 00 01 01 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass64_0`2'::.ctor

    .method assembly hidebysig instance class [Moq]Moq.Language.ISetupSequentialResult`1<!TReturn> 
            '<SetupSequence>b__0'(class [Moq]Moq.Mock`1<!TService> m) cil managed
    {
      .param [0]
      .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 01 00 00 00 ) 
      // Code size       13 (0xd)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!1>> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass64_0`2'<!TService,!TReturn>::setup
      IL_0007:  callvirt   instance class [Moq]Moq.Language.ISetupSequentialResult`1<!!0> class [Moq]Moq.Mock`1<!TService>::SetupSequence<!1>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>)
      IL_000c:  ret
    } // end of method '<>c__DisplayClass64_0`2'::'<SetupSequence>b__0'

  } // end of class '<>c__DisplayClass64_0`2'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass76_0'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [netstandard]System.Type 'type'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public class Moq.AutoMock.IInstance 'instance'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass76_0'::.ctor

    .method assembly hidebysig instance void 
            '<EnsureCached>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> typeMap) cil managed
    {
      // Code size       35 (0x23)
      .maxstack  3
      .locals init (class Moq.AutoMock.IInstance V_0)
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass76_0'::'type'
      IL_0007:  ldloca.s   V_0
      IL_0009:  callvirt   instance bool class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::TryGetValue(!0,
                                                                                                                                                                         !1&)
      IL_000e:  brtrue.s   IL_0022

      IL_0010:  ldarg.1
      IL_0011:  ldarg.0
      IL_0012:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass76_0'::'type'
      IL_0017:  ldarg.0
      IL_0018:  ldfld      class Moq.AutoMock.IInstance Moq.AutoMock.AutoMocker/'<>c__DisplayClass76_0'::'instance'
      IL_001d:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_0022:  ret
    } // end of method '<>c__DisplayClass76_0'::'<EnsureCached>b__0'

  } // end of class '<>c__DisplayClass76_0'

  .field private initonly valuetype [Moq]Moq.MockBehavior '<MockBehavior>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private initonly valuetype [Moq]Moq.DefaultValue '<DefaultValue>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private initonly class [Moq]Moq.DefaultValueProvider '<DefaultValueProvider>k__BackingField'
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private initonly bool '<CallBase>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private initonly class [netstandard]System.Collections.Generic.IList`1<class Moq.AutoMock.Resolvers.IMockResolver> '<Resolvers>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  call       instance void Moq.AutoMock.AutoMocker::.ctor(valuetype [Moq]Moq.MockBehavior)
    IL_0007:  ret
  } // end of method AutoMocker::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype [Moq]Moq.MockBehavior mockBehavior) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  call       instance void Moq.AutoMock.AutoMocker::.ctor(valuetype [Moq]Moq.MockBehavior,
                                                                      valuetype [Moq]Moq.DefaultValue)
    IL_0008:  ret
  } // end of method AutoMocker::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype [Moq]Moq.MockBehavior mockBehavior,
                               valuetype [Moq]Moq.DefaultValue defaultValue) cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldc.i4.0
    IL_0004:  call       instance void Moq.AutoMock.AutoMocker::.ctor(valuetype [Moq]Moq.MockBehavior,
                                                                      valuetype [Moq]Moq.DefaultValue,
                                                                      bool)
    IL_0009:  ret
  } // end of method AutoMocker::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype [Moq]Moq.MockBehavior mockBehavior,
                               valuetype [Moq]Moq.DefaultValue defaultValue,
                               bool callBase) cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldnull
    IL_0004:  ldarg.3
    IL_0005:  call       instance void Moq.AutoMock.AutoMocker::.ctor(valuetype [Moq]Moq.MockBehavior,
                                                                      valuetype [Moq]Moq.DefaultValue,
                                                                      class [Moq]Moq.DefaultValueProvider,
                                                                      bool)
    IL_000a:  ret
  } // end of method AutoMocker::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype [Moq]Moq.MockBehavior mockBehavior,
                               valuetype [Moq]Moq.DefaultValue defaultValue,
                               class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                               bool callBase) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       151 (0x97)
    .maxstack  7
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::'<MockBehavior>k__BackingField'
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::'<DefaultValue>k__BackingField'
    IL_0014:  ldarg.0
    IL_0015:  ldarg.3
    IL_0016:  stfld      class [Moq]Moq.DefaultValueProvider Moq.AutoMock.AutoMocker::'<DefaultValueProvider>k__BackingField'
    IL_001b:  ldarg.0
    IL_001c:  ldarg.s    callBase
    IL_001e:  stfld      bool Moq.AutoMock.AutoMocker::'<CallBase>k__BackingField'
    IL_0023:  ldarg.0
    IL_0024:  newobj     instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::.ctor()
    IL_0029:  dup
    IL_002a:  newobj     instance void Moq.AutoMock.Resolvers.CacheResolver::.ctor()
    IL_002f:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_0034:  dup
    IL_0035:  newobj     instance void Moq.AutoMock.Resolvers.SelfResolver::.ctor()
    IL_003a:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_003f:  dup
    IL_0040:  newobj     instance void Moq.AutoMock.Resolvers.ArrayResolver::.ctor()
    IL_0045:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_004a:  dup
    IL_004b:  newobj     instance void Moq.AutoMock.Resolvers.AutoMockerDisposableResolver::.ctor()
    IL_0050:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_0055:  dup
    IL_0056:  newobj     instance void Moq.AutoMock.Resolvers.EnumerableResolver::.ctor()
    IL_005b:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_0060:  dup
    IL_0061:  newobj     instance void Moq.AutoMock.Resolvers.LazyResolver::.ctor()
    IL_0066:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_006b:  dup
    IL_006c:  newobj     instance void Moq.AutoMock.Resolvers.FuncResolver::.ctor()
    IL_0071:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_0076:  dup
    IL_0077:  newobj     instance void Moq.AutoMock.Resolvers.CancellationTokenResolver::.ctor()
    IL_007c:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_0081:  dup
    IL_0082:  ldarg.1
    IL_0083:  ldarg.2
    IL_0084:  ldarg.3
    IL_0085:  ldarg.s    callBase
    IL_0087:  newobj     instance void Moq.AutoMock.Resolvers.MockResolver::.ctor(valuetype [Moq]Moq.MockBehavior,
                                                                                  valuetype [Moq]Moq.DefaultValue,
                                                                                  class [Moq]Moq.DefaultValueProvider,
                                                                                  bool)
    IL_008c:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::Add(!0)
    IL_0091:  stfld      class [netstandard]System.Collections.Generic.IList`1<class Moq.AutoMock.Resolvers.IMockResolver> Moq.AutoMock.AutoMocker::'<Resolvers>k__BackingField'
    IL_0096:  ret
  } // end of method AutoMocker::.ctor

  .method public hidebysig specialname instance valuetype [Moq]Moq.MockBehavior 
          get_MockBehavior() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::'<MockBehavior>k__BackingField'
    IL_0006:  ret
  } // end of method AutoMocker::get_MockBehavior

  .method public hidebysig specialname instance valuetype [Moq]Moq.DefaultValue 
          get_DefaultValue() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::'<DefaultValue>k__BackingField'
    IL_0006:  ret
  } // end of method AutoMocker::get_DefaultValue

  .method public hidebysig specialname instance class [Moq]Moq.DefaultValueProvider 
          get_DefaultValueProvider() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [Moq]Moq.DefaultValueProvider Moq.AutoMock.AutoMocker::'<DefaultValueProvider>k__BackingField'
    IL_0006:  ret
  } // end of method AutoMocker::get_DefaultValueProvider

  .method public hidebysig specialname instance bool 
          get_CallBase() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Moq.AutoMock.AutoMocker::'<CallBase>k__BackingField'
    IL_0006:  ret
  } // end of method AutoMocker::get_CallBase

  .method public hidebysig specialname instance class [netstandard]System.Collections.Generic.IList`1<class Moq.AutoMock.Resolvers.IMockResolver> 
          get_Resolvers() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [netstandard]System.Collections.Generic.IList`1<class Moq.AutoMock.Resolvers.IMockResolver> Moq.AutoMock.AutoMocker::'<Resolvers>k__BackingField'
    IL_0006:  ret
  } // end of method AutoMocker::get_Resolvers

  .method public hidebysig specialname instance class [netstandard]System.Collections.Generic.IReadOnlyDictionary`2<class [netstandard]System.Type,object> 
          get_ResolvedObjects() cil managed
  {
    .param [0]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 03 00 00 00 01 01 02 00 00 ) 
    // Code size       90 (0x5a)
    .maxstack  4
    IL_0000:  ldarg.0
    IL_0001:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.AutoMocker::get_TypeMap()
    IL_0006:  dup
    IL_0007:  brtrue.s   IL_000d

    IL_0009:  pop
    IL_000a:  ldnull
    IL_000b:  br.s       IL_0050

    IL_000d:  ldsfld     class [netstandard]System.Func`2<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,class [netstandard]System.Type> Moq.AutoMock.AutoMocker/'<>c'::'<>9__21_0'
    IL_0012:  dup
    IL_0013:  brtrue.s   IL_002c

    IL_0015:  pop
    IL_0016:  ldsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
    IL_001b:  ldftn      instance class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c'::'<get_ResolvedObjects>b__21_0'(valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_0021:  newobj     instance void class [netstandard]System.Func`2<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,class [netstandard]System.Type>::.ctor(object,
                                                                                                                                                                                                                                             native int)
    IL_0026:  dup
    IL_0027:  stsfld     class [netstandard]System.Func`2<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,class [netstandard]System.Type> Moq.AutoMock.AutoMocker/'<>c'::'<>9__21_0'
    IL_002c:  ldsfld     class [netstandard]System.Func`2<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,object> Moq.AutoMock.AutoMocker/'<>c'::'<>9__21_1'
    IL_0031:  dup
    IL_0032:  brtrue.s   IL_004b

    IL_0034:  pop
    IL_0035:  ldsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
    IL_003a:  ldftn      instance object Moq.AutoMock.AutoMocker/'<>c'::'<get_ResolvedObjects>b__21_1'(valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_0040:  newobj     instance void class [netstandard]System.Func`2<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,object>::.ctor(object,
                                                                                                                                                                                                                     native int)
    IL_0045:  dup
    IL_0046:  stsfld     class [netstandard]System.Func`2<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,object> Moq.AutoMock.AutoMocker/'<>c'::'<>9__21_1'
    IL_004b:  call       class [netstandard]System.Collections.Generic.Dictionary`2<!!1,!!2> [netstandard]System.Linq.Enumerable::ToDictionary<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>,class [netstandard]System.Type,object>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                                                                                                                                                    class [netstandard]System.Func`2<!!0,!!1>,
                                                                                                                                                                                                                                                                                                                    class [netstandard]System.Func`2<!!0,!!2>)
    IL_0050:  dup
    IL_0051:  brtrue.s   IL_0059

    IL_0053:  pop
    IL_0054:  newobj     instance void class [netstandard]System.Collections.Generic.Dictionary`2<class [netstandard]System.Type,object>::.ctor()
    IL_0059:  ret
  } // end of method AutoMocker::get_ResolvedObjects

  .method private hidebysig specialname instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> 
          get_TypeMap() cil managed
  {
    .param [0]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 03 00 00 00 02 01 01 00 00 ) 
    // Code size       28 (0x1c)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance class [netstandard]System.Collections.Generic.IList`1<class Moq.AutoMock.Resolvers.IMockResolver> Moq.AutoMock.AutoMocker::get_Resolvers()
    IL_0006:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::OfType<class Moq.AutoMock.Resolvers.CacheResolver>(class [netstandard]System.Collections.IEnumerable)
    IL_000b:  call       !!0 [netstandard]System.Linq.Enumerable::FirstOrDefault<class Moq.AutoMock.Resolvers.CacheResolver>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0010:  dup
    IL_0011:  brtrue.s   IL_0016

    IL_0013:  pop
    IL_0014:  ldnull
    IL_0015:  ret

    IL_0016:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.Resolvers.CacheResolver::get_TypeMap()
    IL_001b:  ret
  } // end of method AutoMocker::get_TypeMap

  .method private hidebysig instance bool 
          TryResolve(class [netstandard]System.Type serviceType,
                     class Moq.AutoMock.ObjectGraphContext resolutionContext,
                     [out] class Moq.AutoMock.IInstance& 'instance') cil managed
  {
    .param [3]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .custom instance void System.Diagnostics.CodeAnalysis.NotNullWhenAttribute::.ctor(bool) = ( 01 00 01 00 00 ) 
    // Code size       179 (0xb3)
    .maxstack  3
    .locals init (class Moq.AutoMock.Resolvers.MockResolutionContext V_0,
             class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver> V_1,
             int32 V_2,
             class [Moq]Moq.Mock V_3,
             class Moq.AutoMock.IInstance V_4,
             class Moq.AutoMock.IInstance V_5,
             object V_6)
    IL_0000:  ldarg.2
    IL_0001:  callvirt   instance class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> Moq.AutoMock.ObjectGraphContext::get_VisitedTypes()
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance bool class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type>::Contains(!0)
    IL_000c:  brfalse.s  IL_0013

    IL_000e:  ldarg.3
    IL_000f:  ldnull
    IL_0010:  stind.ref
    IL_0011:  ldc.i4.0
    IL_0012:  ret

    IL_0013:  ldarg.2
    IL_0014:  callvirt   instance class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> Moq.AutoMock.ObjectGraphContext::get_VisitedTypes()
    IL_0019:  ldarg.1
    IL_001a:  callvirt   instance bool class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type>::Add(!0)
    IL_001f:  pop
    IL_0020:  ldarg.0
    IL_0021:  ldarg.1
    IL_0022:  ldarg.2
    IL_0023:  newobj     instance void Moq.AutoMock.Resolvers.MockResolutionContext::.ctor(class Moq.AutoMock.AutoMocker,
                                                                                           class [netstandard]System.Type,
                                                                                           class Moq.AutoMock.ObjectGraphContext)
    IL_0028:  stloc.0
    IL_0029:  ldarg.0
    IL_002a:  call       instance class [netstandard]System.Collections.Generic.IList`1<class Moq.AutoMock.Resolvers.IMockResolver> Moq.AutoMock.AutoMocker::get_Resolvers()
    IL_002f:  newobj     instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::.ctor(class [netstandard]System.Collections.Generic.IEnumerable`1<!0>)
    IL_0034:  stloc.1
    IL_0035:  ldc.i4.0
    IL_0036:  stloc.2
    IL_0037:  br.s       IL_0050

    IL_0039:  nop
    .try
    {
      IL_003a:  ldloc.1
      IL_003b:  ldloc.2
      IL_003c:  callvirt   instance !0 class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::get_Item(int32)
      IL_0041:  ldloc.0
      IL_0042:  callvirt   instance void Moq.AutoMock.Resolvers.IMockResolver::Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext)
      IL_0047:  leave.s    IL_004c

    }  // end .try
    catch [netstandard]System.Object 
    {
      IL_0049:  pop
      IL_004a:  leave.s    IL_004c

    }  // end handler
    IL_004c:  ldloc.2
    IL_004d:  ldc.i4.1
    IL_004e:  add
    IL_004f:  stloc.2
    IL_0050:  ldloc.2
    IL_0051:  ldloc.1
    IL_0052:  callvirt   instance int32 class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.Resolvers.IMockResolver>::get_Count()
    IL_0057:  bge.s      IL_0061

    IL_0059:  ldloc.0
    IL_005a:  callvirt   instance bool Moq.AutoMock.Resolvers.MockResolutionContext::get_ValueProvided()
    IL_005f:  brfalse.s  IL_0039

    IL_0061:  ldloc.0
    IL_0062:  callvirt   instance bool Moq.AutoMock.Resolvers.MockResolutionContext::get_ValueProvided()
    IL_0067:  brtrue.s   IL_006e

    IL_0069:  ldarg.3
    IL_006a:  ldnull
    IL_006b:  stind.ref
    IL_006c:  ldc.i4.0
    IL_006d:  ret

    IL_006e:  ldloc.0
    IL_006f:  callvirt   instance object Moq.AutoMock.Resolvers.MockResolutionContext::get_Value()
    IL_0074:  stloc.s    V_6
    IL_0076:  ldloc.s    V_6
    IL_0078:  isinst     [Moq]Moq.Mock
    IL_007d:  stloc.3
    IL_007e:  ldloc.3
    IL_007f:  brtrue.s   IL_0090

    IL_0081:  ldloc.s    V_6
    IL_0083:  isinst     Moq.AutoMock.IInstance
    IL_0088:  stloc.s    V_4
    IL_008a:  ldloc.s    V_4
    IL_008c:  brtrue.s   IL_009a

    IL_008e:  br.s       IL_00a0

    IL_0090:  ldloc.3
    IL_0091:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
    IL_0096:  stloc.s    V_5
    IL_0098:  br.s       IL_00ad

    IL_009a:  ldloc.s    V_4
    IL_009c:  stloc.s    V_5
    IL_009e:  br.s       IL_00ad

    IL_00a0:  ldloc.0
    IL_00a1:  callvirt   instance object Moq.AutoMock.Resolvers.MockResolutionContext::get_Value()
    IL_00a6:  newobj     instance void Moq.AutoMock.RealInstance::.ctor(object)
    IL_00ab:  stloc.s    V_5
    IL_00ad:  ldarg.3
    IL_00ae:  ldloc.s    V_5
    IL_00b0:  stind.ref
    IL_00b1:  ldc.i4.1
    IL_00b2:  ret
  } // end of method AutoMocker::TryResolve

  .method public hidebysig instance !!T  CreateInstance<class T>() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  call       instance !!0 Moq.AutoMock.AutoMocker::CreateInstance<!!0>(bool)
    IL_0007:  ret
  } // end of method AutoMocker::CreateInstance

  .method public hidebysig instance !!T  CreateInstance<class T>(bool enablePrivate) cil managed
  {
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!T
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldarg.1
    IL_000c:  call       instance object Moq.AutoMock.AutoMocker::CreateInstance(class [netstandard]System.Type,
                                                                                 bool)
    IL_0011:  unbox.any  !!T
    IL_0016:  ret
  } // end of method AutoMocker::CreateInstance

  .method public hidebysig instance object 
          CreateInstance(class [netstandard]System.Type 'type') cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  call       instance object Moq.AutoMock.AutoMocker::CreateInstance(class [netstandard]System.Type,
                                                                                 bool)
    IL_0008:  ret
  } // end of method AutoMocker::CreateInstance

  .method public hidebysig instance object 
          CreateInstance(class [netstandard]System.Type 'type',
                         bool enablePrivate) cil managed
  {
    // Code size       147 (0x93)
    .maxstack  5
    .locals init (class Moq.AutoMock.ObjectGraphContext V_0,
             class [netstandard]System.Reflection.ConstructorInfo V_1,
             class Moq.AutoMock.IInstance[] V_2,
             object[] V_3,
             object V_4,
             class [netstandard]System.Reflection.TargetInvocationException V_5)
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "type"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.2
    IL_000f:  newobj     instance void Moq.AutoMock.ObjectGraphContext::.ctor(bool)
    IL_0014:  stloc.0
    IL_0015:  ldarg.0
    IL_0016:  ldarg.1
    IL_0017:  ldloc.0
    IL_0018:  ldloca.s   V_1
    IL_001a:  ldloca.s   V_2
    IL_001c:  call       instance bool Moq.AutoMock.AutoMocker::TryGetConstructorInvocation(class [netstandard]System.Type,
                                                                                            class Moq.AutoMock.ObjectGraphContext,
                                                                                            class [netstandard]System.Reflection.ConstructorInfo&,
                                                                                            class Moq.AutoMock.IInstance[]&)
    IL_0021:  brtrue.s   IL_003e

    IL_0023:  ldstr      "Did not find a best constructor for `{0}`. If any "
    + "type in the hierarchy has a non-public constructor, set the 'enablePriv"
    + "ate' parameter to true for this {1} method."
    IL_0028:  ldarg.1
    IL_0029:  ldstr      "AutoMocker"
    IL_002e:  call       string [netstandard]System.String::Format(string,
                                                                   object,
                                                                   object)
    IL_0033:  ldstr      "type"
    IL_0038:  newobj     instance void [netstandard]System.ArgumentException::.ctor(string,
                                                                                    string)
    IL_003d:  throw

    IL_003e:  nop
    .try
    {
      IL_003f:  ldloc.2
      IL_0040:  ldsfld     class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,object> Moq.AutoMock.AutoMocker/'<>c'::'<>9__28_0'
      IL_0045:  dup
      IL_0046:  brtrue.s   IL_005f

      IL_0048:  pop
      IL_0049:  ldsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
      IL_004e:  ldftn      instance object Moq.AutoMock.AutoMocker/'<>c'::'<CreateInstance>b__28_0'(class Moq.AutoMock.IInstance)
      IL_0054:  newobj     instance void class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,object>::.ctor(object,
                                                                                                                      native int)
      IL_0059:  dup
      IL_005a:  stsfld     class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,object> Moq.AutoMock.AutoMocker/'<>c'::'<>9__28_0'
      IL_005f:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!1> [netstandard]System.Linq.Enumerable::Select<class Moq.AutoMock.IInstance,object>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                             class [netstandard]System.Func`2<!!0,!!1>)
      IL_0064:  call       !!0[] [netstandard]System.Linq.Enumerable::ToArray<object>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
      IL_0069:  stloc.3
      IL_006a:  ldloc.1
      IL_006b:  ldloc.3
      IL_006c:  callvirt   instance object [netstandard]System.Reflection.ConstructorInfo::Invoke(object[])
      IL_0071:  stloc.s    V_4
      IL_0073:  leave.s    IL_0090

    }  // end .try
    catch [netstandard]System.Reflection.TargetInvocationException 
    {
      IL_0075:  stloc.s    V_5
      IL_0077:  ldloc.s    V_5
      IL_0079:  callvirt   instance class [netstandard]System.Exception [netstandard]System.Exception::get_InnerException()
      IL_007e:  dup
      IL_007f:  brtrue.s   IL_0084

      IL_0081:  pop
      IL_0082:  ldloc.s    V_5
      IL_0084:  call       class [netstandard]System.Runtime.ExceptionServices.ExceptionDispatchInfo [netstandard]System.Runtime.ExceptionServices.ExceptionDispatchInfo::Capture(class [netstandard]System.Exception)
      IL_0089:  callvirt   instance void [netstandard]System.Runtime.ExceptionServices.ExceptionDispatchInfo::Throw()
      IL_008e:  rethrow
    }  // end handler
    IL_0090:  ldloc.s    V_4
    IL_0092:  ret
  } // end of method AutoMocker::CreateInstance

  .method public hidebysig instance !!T  CreateSelfMock<class T>() cil managed
  {
    .param type T 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  call       instance !!0 Moq.AutoMock.AutoMocker::CreateSelfMock<!!0>(bool)
    IL_0007:  ret
  } // end of method AutoMocker::CreateSelfMock

  .method public hidebysig instance !!T  CreateSelfMock<class T>(bool enablePrivate) cil managed
  {
    .param type T 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       42 (0x2a)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.0
    IL_0003:  call       instance valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::get_MockBehavior()
    IL_0008:  newobj     instance void valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::.ctor(!0)
    IL_000d:  ldarg.0
    IL_000e:  call       instance valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::get_DefaultValue()
    IL_0013:  newobj     instance void valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::.ctor(!0)
    IL_0018:  ldnull
    IL_0019:  ldarg.0
    IL_001a:  call       instance bool Moq.AutoMock.AutoMocker::get_CallBase()
    IL_001f:  newobj     instance void valuetype [netstandard]System.Nullable`1<bool>::.ctor(!0)
    IL_0024:  call       instance !!0 Moq.AutoMock.AutoMocker::CreateSelfMock<!!0>(bool,
                                                                                   valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>,
                                                                                   valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>,
                                                                                   class [Moq]Moq.DefaultValueProvider,
                                                                                   valuetype [netstandard]System.Nullable`1<bool>)
    IL_0029:  ret
  } // end of method AutoMocker::CreateSelfMock

  .method public hidebysig instance !!T  CreateSelfMock<class T>([opt] bool enablePrivate,
                                                                 [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> mockBehavior,
                                                                 [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> defaultValue,
                                                                 [opt] class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                                                                 [opt] valuetype [netstandard]System.Nullable`1<bool> callBase) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [0]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    .param [1] = bool(false)
    .param [2] = nullref
    .param [3] = nullref
    .param [4] = nullref
    .param [5] = nullref
    // Code size       104 (0x68)
    .maxstack  6
    .locals init (valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> V_0,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> V_1,
             valuetype [netstandard]System.Nullable`1<bool> V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  stloc.0
    IL_0004:  ldloca.s   V_0
    IL_0006:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::get_HasValue()
    IL_000b:  brtrue.s   IL_0015

    IL_000d:  ldarg.0
    IL_000e:  call       instance valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::get_MockBehavior()
    IL_0013:  br.s       IL_001c

    IL_0015:  ldloca.s   V_0
    IL_0017:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::GetValueOrDefault()
    IL_001c:  ldarg.3
    IL_001d:  stloc.1
    IL_001e:  ldloca.s   V_1
    IL_0020:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::get_HasValue()
    IL_0025:  brtrue.s   IL_002f

    IL_0027:  ldarg.0
    IL_0028:  call       instance valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::get_DefaultValue()
    IL_002d:  br.s       IL_0036

    IL_002f:  ldloca.s   V_1
    IL_0031:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::GetValueOrDefault()
    IL_0036:  ldarg.s    defaultValueProvider
    IL_0038:  dup
    IL_0039:  brtrue.s   IL_0042

    IL_003b:  pop
    IL_003c:  ldarg.0
    IL_003d:  call       instance class [Moq]Moq.DefaultValueProvider Moq.AutoMock.AutoMocker::get_DefaultValueProvider()
    IL_0042:  ldarg.s    callBase
    IL_0044:  stloc.2
    IL_0045:  ldloca.s   V_2
    IL_0047:  call       instance bool valuetype [netstandard]System.Nullable`1<bool>::get_HasValue()
    IL_004c:  brtrue.s   IL_0056

    IL_004e:  ldarg.0
    IL_004f:  call       instance bool Moq.AutoMock.AutoMocker::get_CallBase()
    IL_0054:  br.s       IL_005d

    IL_0056:  ldloca.s   V_2
    IL_0058:  call       instance !0 valuetype [netstandard]System.Nullable`1<bool>::GetValueOrDefault()
    IL_005d:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::BuildSelfMock<!!0>(bool,
                                                                                                         valuetype [Moq]Moq.MockBehavior,
                                                                                                         valuetype [Moq]Moq.DefaultValue,
                                                                                                         class [Moq]Moq.DefaultValueProvider,
                                                                                                         bool)
    IL_0062:  callvirt   instance !0 class [Moq]Moq.Mock`1<!!T>::get_Object()
    IL_0067:  ret
  } // end of method AutoMocker::CreateSelfMock

  .method public hidebysig instance !!TImplementation 
          WithSelfMock<class TService,class (!!TService) TImplementation>([opt] bool enablePrivate,
                                                                          [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> mockBehavior,
                                                                          [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> defaultValue,
                                                                          [opt] class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                                                                          [opt] valuetype [netstandard]System.Nullable`1<bool> callBase) cil managed
  {
    .param [1] = bool(false)
    .param [2] = nullref
    .param [3] = nullref
    .param [4] = nullref
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [5] = nullref
    // Code size       140 (0x8c)
    .maxstack  7
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass32_0`2'<!!TService,!!TImplementation> V_0,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> V_1,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> V_2,
             valuetype [netstandard]System.Nullable`1<bool> V_3)
    IL_0000:  newobj     instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass32_0`2'<!!TService,!!TImplementation>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  ldarg.2
    IL_000a:  stloc.1
    IL_000b:  ldloca.s   V_1
    IL_000d:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::get_HasValue()
    IL_0012:  brtrue.s   IL_001c

    IL_0014:  ldarg.0
    IL_0015:  call       instance valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::get_MockBehavior()
    IL_001a:  br.s       IL_0023

    IL_001c:  ldloca.s   V_1
    IL_001e:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::GetValueOrDefault()
    IL_0023:  ldarg.3
    IL_0024:  stloc.2
    IL_0025:  ldloca.s   V_2
    IL_0027:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::get_HasValue()
    IL_002c:  brtrue.s   IL_0036

    IL_002e:  ldarg.0
    IL_002f:  call       instance valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::get_DefaultValue()
    IL_0034:  br.s       IL_003d

    IL_0036:  ldloca.s   V_2
    IL_0038:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::GetValueOrDefault()
    IL_003d:  ldarg.s    defaultValueProvider
    IL_003f:  dup
    IL_0040:  brtrue.s   IL_0049

    IL_0042:  pop
    IL_0043:  ldarg.0
    IL_0044:  call       instance class [Moq]Moq.DefaultValueProvider Moq.AutoMock.AutoMocker::get_DefaultValueProvider()
    IL_0049:  ldarg.s    callBase
    IL_004b:  stloc.3
    IL_004c:  ldloca.s   V_3
    IL_004e:  call       instance bool valuetype [netstandard]System.Nullable`1<bool>::get_HasValue()
    IL_0053:  brtrue.s   IL_005d

    IL_0055:  ldarg.0
    IL_0056:  call       instance bool Moq.AutoMock.AutoMocker::get_CallBase()
    IL_005b:  br.s       IL_0064

    IL_005d:  ldloca.s   V_3
    IL_005f:  call       instance !0 valuetype [netstandard]System.Nullable`1<bool>::GetValueOrDefault()
    IL_0064:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::BuildSelfMock<!!1>(bool,
                                                                                                         valuetype [Moq]Moq.MockBehavior,
                                                                                                         valuetype [Moq]Moq.DefaultValue,
                                                                                                         class [Moq]Moq.DefaultValueProvider,
                                                                                                         bool)
    IL_0069:  stfld      class [Moq]Moq.Mock`1<!1> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass32_0`2'<!!TService,!!TImplementation>::selfMock
    IL_006e:  ldarg.0
    IL_006f:  ldloc.0
    IL_0070:  ldftn      instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass32_0`2'<!!TService,!!TImplementation>::'<WithSelfMock>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_0076:  newobj     instance void class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::.ctor(object,
                                                                                                                                                                                                     native int)
    IL_007b:  call       instance void Moq.AutoMock.AutoMocker::WithTypeMap(class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>)
    IL_0080:  ldloc.0
    IL_0081:  ldfld      class [Moq]Moq.Mock`1<!1> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass32_0`2'<!!TService,!!TImplementation>::selfMock
    IL_0086:  callvirt   instance !0 class [Moq]Moq.Mock`1<!!TImplementation>::get_Object()
    IL_008b:  ret
  } // end of method AutoMocker::WithSelfMock

  .method public hidebysig instance !!T  WithSelfMock<class T>([opt] bool enablePrivate,
                                                               [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> mockBehavior,
                                                               [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> defaultValue,
                                                               [opt] class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                                                               [opt] valuetype [netstandard]System.Nullable`1<bool> callBase) cil managed
  {
    .param [1] = bool(false)
    .param [2] = nullref
    .param [3] = nullref
    .param [4] = nullref
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [5] = nullref
    // Code size       140 (0x8c)
    .maxstack  7
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass33_0`1'<!!T> V_0,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> V_1,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> V_2,
             valuetype [netstandard]System.Nullable`1<bool> V_3)
    IL_0000:  newobj     instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass33_0`1'<!!T>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  ldarg.2
    IL_000a:  stloc.1
    IL_000b:  ldloca.s   V_1
    IL_000d:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::get_HasValue()
    IL_0012:  brtrue.s   IL_001c

    IL_0014:  ldarg.0
    IL_0015:  call       instance valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::get_MockBehavior()
    IL_001a:  br.s       IL_0023

    IL_001c:  ldloca.s   V_1
    IL_001e:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::GetValueOrDefault()
    IL_0023:  ldarg.3
    IL_0024:  stloc.2
    IL_0025:  ldloca.s   V_2
    IL_0027:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::get_HasValue()
    IL_002c:  brtrue.s   IL_0036

    IL_002e:  ldarg.0
    IL_002f:  call       instance valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::get_DefaultValue()
    IL_0034:  br.s       IL_003d

    IL_0036:  ldloca.s   V_2
    IL_0038:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::GetValueOrDefault()
    IL_003d:  ldarg.s    defaultValueProvider
    IL_003f:  dup
    IL_0040:  brtrue.s   IL_0049

    IL_0042:  pop
    IL_0043:  ldarg.0
    IL_0044:  call       instance class [Moq]Moq.DefaultValueProvider Moq.AutoMock.AutoMocker::get_DefaultValueProvider()
    IL_0049:  ldarg.s    callBase
    IL_004b:  stloc.3
    IL_004c:  ldloca.s   V_3
    IL_004e:  call       instance bool valuetype [netstandard]System.Nullable`1<bool>::get_HasValue()
    IL_0053:  brtrue.s   IL_005d

    IL_0055:  ldarg.0
    IL_0056:  call       instance bool Moq.AutoMock.AutoMocker::get_CallBase()
    IL_005b:  br.s       IL_0064

    IL_005d:  ldloca.s   V_3
    IL_005f:  call       instance !0 valuetype [netstandard]System.Nullable`1<bool>::GetValueOrDefault()
    IL_0064:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::BuildSelfMock<!!0>(bool,
                                                                                                         valuetype [Moq]Moq.MockBehavior,
                                                                                                         valuetype [Moq]Moq.DefaultValue,
                                                                                                         class [Moq]Moq.DefaultValueProvider,
                                                                                                         bool)
    IL_0069:  stfld      class [Moq]Moq.Mock`1<!0> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass33_0`1'<!!T>::selfMock
    IL_006e:  ldarg.0
    IL_006f:  ldloc.0
    IL_0070:  ldftn      instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass33_0`1'<!!T>::'<WithSelfMock>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_0076:  newobj     instance void class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::.ctor(object,
                                                                                                                                                                                                     native int)
    IL_007b:  call       instance void Moq.AutoMock.AutoMocker::WithTypeMap(class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>)
    IL_0080:  ldloc.0
    IL_0081:  ldfld      class [Moq]Moq.Mock`1<!0> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass33_0`1'<!!T>::selfMock
    IL_0086:  callvirt   instance !0 class [Moq]Moq.Mock`1<!!T>::get_Object()
    IL_008b:  ret
  } // end of method AutoMocker::WithSelfMock

  .method public hidebysig instance object 
          WithSelfMock(class [netstandard]System.Type serviceType,
                       class [netstandard]System.Type implementationType,
                       [opt] bool enablePrivate,
                       [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> mockBehavior,
                       [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> defaultValue,
                       [opt] class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                       [opt] valuetype [netstandard]System.Nullable`1<bool> callBase) cil managed
  {
    .param [3] = bool(false)
    .param [4] = nullref
    .param [5] = nullref
    .param [6] = nullref
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [7] = nullref
    // Code size       162 (0xa2)
    .maxstack  8
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0' V_0,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> V_1,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> V_2,
             valuetype [netstandard]System.Nullable`1<bool> V_3)
    IL_0000:  newobj     instance void Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.2
    IL_0008:  stfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::implementationType
    IL_000d:  ldloc.0
    IL_000e:  ldarg.1
    IL_000f:  stfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::serviceType
    IL_0014:  ldloc.0
    IL_0015:  ldarg.0
    IL_0016:  ldloc.0
    IL_0017:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::implementationType
    IL_001c:  ldarg.3
    IL_001d:  ldarg.s    mockBehavior
    IL_001f:  stloc.1
    IL_0020:  ldloca.s   V_1
    IL_0022:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::get_HasValue()
    IL_0027:  brtrue.s   IL_0031

    IL_0029:  ldarg.0
    IL_002a:  call       instance valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::get_MockBehavior()
    IL_002f:  br.s       IL_0038

    IL_0031:  ldloca.s   V_1
    IL_0033:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::GetValueOrDefault()
    IL_0038:  ldarg.s    defaultValue
    IL_003a:  stloc.2
    IL_003b:  ldloca.s   V_2
    IL_003d:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::get_HasValue()
    IL_0042:  brtrue.s   IL_004c

    IL_0044:  ldarg.0
    IL_0045:  call       instance valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::get_DefaultValue()
    IL_004a:  br.s       IL_0053

    IL_004c:  ldloca.s   V_2
    IL_004e:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::GetValueOrDefault()
    IL_0053:  ldarg.s    defaultValueProvider
    IL_0055:  dup
    IL_0056:  brtrue.s   IL_005f

    IL_0058:  pop
    IL_0059:  ldarg.0
    IL_005a:  call       instance class [Moq]Moq.DefaultValueProvider Moq.AutoMock.AutoMocker::get_DefaultValueProvider()
    IL_005f:  ldarg.s    callBase
    IL_0061:  stloc.3
    IL_0062:  ldloca.s   V_3
    IL_0064:  call       instance bool valuetype [netstandard]System.Nullable`1<bool>::get_HasValue()
    IL_0069:  brtrue.s   IL_0073

    IL_006b:  ldarg.0
    IL_006c:  call       instance bool Moq.AutoMock.AutoMocker::get_CallBase()
    IL_0071:  br.s       IL_007a

    IL_0073:  ldloca.s   V_3
    IL_0075:  call       instance !0 valuetype [netstandard]System.Nullable`1<bool>::GetValueOrDefault()
    IL_007a:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::BuildSelfMock(class [netstandard]System.Type,
                                                                                             bool,
                                                                                             valuetype [Moq]Moq.MockBehavior,
                                                                                             valuetype [Moq]Moq.DefaultValue,
                                                                                             class [Moq]Moq.DefaultValueProvider,
                                                                                             bool)
    IL_007f:  stfld      class [Moq]Moq.Mock Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::selfMock
    IL_0084:  ldarg.0
    IL_0085:  ldloc.0
    IL_0086:  ldftn      instance void Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::'<WithSelfMock>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_008c:  newobj     instance void class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::.ctor(object,
                                                                                                                                                                                                     native int)
    IL_0091:  call       instance void Moq.AutoMock.AutoMocker::WithTypeMap(class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>)
    IL_0096:  ldloc.0
    IL_0097:  ldfld      class [Moq]Moq.Mock Moq.AutoMock.AutoMocker/'<>c__DisplayClass34_0'::selfMock
    IL_009c:  callvirt   instance object [Moq]Moq.Mock::get_Object()
    IL_00a1:  ret
  } // end of method AutoMocker::WithSelfMock

  .method public hidebysig instance object 
          WithSelfMock(class [netstandard]System.Type implementationType,
                       [opt] bool enablePrivate,
                       [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> mockBehavior,
                       [opt] valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> defaultValue,
                       [opt] class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                       [opt] valuetype [netstandard]System.Nullable`1<bool> callBase) cil managed
  {
    .param [2] = bool(false)
    .param [3] = nullref
    .param [4] = nullref
    .param [5] = nullref
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [6] = nullref
    // Code size       154 (0x9a)
    .maxstack  8
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0' V_0,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior> V_1,
             valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue> V_2,
             valuetype [netstandard]System.Nullable`1<bool> V_3)
    IL_0000:  newobj     instance void Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0'::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0'::implementationType
    IL_000d:  ldloc.0
    IL_000e:  ldarg.0
    IL_000f:  ldloc.0
    IL_0010:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0'::implementationType
    IL_0015:  ldarg.2
    IL_0016:  ldarg.3
    IL_0017:  stloc.1
    IL_0018:  ldloca.s   V_1
    IL_001a:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::get_HasValue()
    IL_001f:  brtrue.s   IL_0029

    IL_0021:  ldarg.0
    IL_0022:  call       instance valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::get_MockBehavior()
    IL_0027:  br.s       IL_0030

    IL_0029:  ldloca.s   V_1
    IL_002b:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.MockBehavior>::GetValueOrDefault()
    IL_0030:  ldarg.s    defaultValue
    IL_0032:  stloc.2
    IL_0033:  ldloca.s   V_2
    IL_0035:  call       instance bool valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::get_HasValue()
    IL_003a:  brtrue.s   IL_0044

    IL_003c:  ldarg.0
    IL_003d:  call       instance valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::get_DefaultValue()
    IL_0042:  br.s       IL_004b

    IL_0044:  ldloca.s   V_2
    IL_0046:  call       instance !0 valuetype [netstandard]System.Nullable`1<valuetype [Moq]Moq.DefaultValue>::GetValueOrDefault()
    IL_004b:  ldarg.s    defaultValueProvider
    IL_004d:  dup
    IL_004e:  brtrue.s   IL_0057

    IL_0050:  pop
    IL_0051:  ldarg.0
    IL_0052:  call       instance class [Moq]Moq.DefaultValueProvider Moq.AutoMock.AutoMocker::get_DefaultValueProvider()
    IL_0057:  ldarg.s    callBase
    IL_0059:  stloc.3
    IL_005a:  ldloca.s   V_3
    IL_005c:  call       instance bool valuetype [netstandard]System.Nullable`1<bool>::get_HasValue()
    IL_0061:  brtrue.s   IL_006b

    IL_0063:  ldarg.0
    IL_0064:  call       instance bool Moq.AutoMock.AutoMocker::get_CallBase()
    IL_0069:  br.s       IL_0072

    IL_006b:  ldloca.s   V_3
    IL_006d:  call       instance !0 valuetype [netstandard]System.Nullable`1<bool>::GetValueOrDefault()
    IL_0072:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::BuildSelfMock(class [netstandard]System.Type,
                                                                                             bool,
                                                                                             valuetype [Moq]Moq.MockBehavior,
                                                                                             valuetype [Moq]Moq.DefaultValue,
                                                                                             class [Moq]Moq.DefaultValueProvider,
                                                                                             bool)
    IL_0077:  stfld      class [Moq]Moq.Mock Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0'::selfMock
    IL_007c:  ldarg.0
    IL_007d:  ldloc.0
    IL_007e:  ldftn      instance void Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0'::'<WithSelfMock>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_0084:  newobj     instance void class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::.ctor(object,
                                                                                                                                                                                                     native int)
    IL_0089:  call       instance void Moq.AutoMock.AutoMocker::WithTypeMap(class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>)
    IL_008e:  ldloc.0
    IL_008f:  ldfld      class [Moq]Moq.Mock Moq.AutoMock.AutoMocker/'<>c__DisplayClass35_0'::selfMock
    IL_0094:  callvirt   instance object [Moq]Moq.Mock::get_Object()
    IL_0099:  ret
  } // end of method AutoMocker::WithSelfMock

  .method private hidebysig instance class [Moq]Moq.Mock`1<!!T> 
          BuildSelfMock<class T>(bool enablePrivate,
                                 valuetype [Moq]Moq.MockBehavior mockBehavior,
                                 valuetype [Moq]Moq.DefaultValue defaultValue,
                                 class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                                 bool callBase) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [0]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       72 (0x48)
    .maxstack  7
    .locals init (class Moq.AutoMock.ObjectGraphContext V_0,
             class [Moq]Moq.Mock`1<!!T> V_1)
    IL_0000:  ldarg.1
    IL_0001:  newobj     instance void Moq.AutoMock.ObjectGraphContext::.ctor(bool)
    IL_0006:  stloc.0
    IL_0007:  ldarg.0
    IL_0008:  ldtoken    !!T
    IL_000d:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0012:  ldarg.2
    IL_0013:  ldarg.3
    IL_0014:  ldarg.s    defaultValueProvider
    IL_0016:  ldarg.s    callBase
    IL_0018:  ldloc.0
    IL_0019:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::CreateMock(class [netstandard]System.Type,
                                                                                          valuetype [Moq]Moq.MockBehavior,
                                                                                          valuetype [Moq]Moq.DefaultValue,
                                                                                          class [Moq]Moq.DefaultValueProvider,
                                                                                          bool,
                                                                                          class Moq.AutoMock.ObjectGraphContext)
    IL_001e:  isinst     class [Moq]Moq.Mock`1<!!T>
    IL_0023:  stloc.1
    IL_0024:  ldloc.1
    IL_0025:  brtrue.s   IL_0046

    IL_0027:  ldstr      "Failed to create self mock of type "
    IL_002c:  ldtoken    !!T
    IL_0031:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0036:  callvirt   instance string [netstandard]System.Type::get_FullName()
    IL_003b:  call       string [netstandard]System.String::Concat(string,
                                                                   string)
    IL_0040:  newobj     instance void [netstandard]System.InvalidOperationException::.ctor(string)
    IL_0045:  throw

    IL_0046:  ldloc.1
    IL_0047:  ret
  } // end of method AutoMocker::BuildSelfMock

  .method private hidebysig instance class [Moq]Moq.Mock 
          BuildSelfMock(class [netstandard]System.Type serviceType,
                        bool enablePrivate,
                        valuetype [Moq]Moq.MockBehavior mockBehavior,
                        valuetype [Moq]Moq.DefaultValue defaultValue,
                        class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                        bool callBase) cil managed
  {
    .param [5]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       50 (0x32)
    .maxstack  7
    .locals init (class Moq.AutoMock.ObjectGraphContext V_0,
             class [Moq]Moq.Mock V_1)
    IL_0000:  ldarg.2
    IL_0001:  newobj     instance void Moq.AutoMock.ObjectGraphContext::.ctor(bool)
    IL_0006:  stloc.0
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  ldarg.3
    IL_000a:  ldarg.s    defaultValue
    IL_000c:  ldarg.s    defaultValueProvider
    IL_000e:  ldarg.s    callBase
    IL_0010:  ldloc.0
    IL_0011:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::CreateMock(class [netstandard]System.Type,
                                                                                          valuetype [Moq]Moq.MockBehavior,
                                                                                          valuetype [Moq]Moq.DefaultValue,
                                                                                          class [Moq]Moq.DefaultValueProvider,
                                                                                          bool,
                                                                                          class Moq.AutoMock.ObjectGraphContext)
    IL_0016:  stloc.1
    IL_0017:  ldloc.1
    IL_0018:  brtrue.s   IL_0030

    IL_001a:  ldstr      "Failed to create self mock of type "
    IL_001f:  ldarg.1
    IL_0020:  callvirt   instance string [netstandard]System.Type::get_FullName()
    IL_0025:  call       string [netstandard]System.String::Concat(string,
                                                                   string)
    IL_002a:  newobj     instance void [netstandard]System.InvalidOperationException::.ctor(string)
    IL_002f:  throw

    IL_0030:  ldloc.1
    IL_0031:  ret
  } // end of method AutoMocker::BuildSelfMock

  .method public hidebysig instance void 
          Use<TService>(!!TService service) cil managed
  {
    .param type TService 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [1]
    .custom instance void System.Diagnostics.CodeAnalysis.DisallowNullAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       44 (0x2c)
    .maxstack  3
    .locals init (!!TService V_0,
             !!TService V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldarg.1
    IL_000c:  stloc.0
    IL_000d:  ldloc.0
    IL_000e:  box        !!TService
    IL_0013:  brtrue.s   IL_0020

    IL_0015:  ldstr      "service"
    IL_001a:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001f:  throw

    IL_0020:  ldloc.0
    IL_0021:  box        !!TService
    IL_0026:  call       instance void Moq.AutoMock.AutoMocker::Use(class [netstandard]System.Type,
                                                                    object)
    IL_002b:  ret
  } // end of method AutoMocker::Use

  .method public hidebysig instance void 
          Use(class [netstandard]System.Type 'type',
              object service) cil managed
  {
    // Code size       117 (0x75)
    .maxstack  3
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0' V_0)
    IL_0000:  newobj     instance void Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::'type'
    IL_000d:  ldloc.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      object Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::service
    IL_0014:  ldloc.0
    IL_0015:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::'type'
    IL_001a:  brtrue.s   IL_0027

    IL_001c:  ldstr      "type"
    IL_0021:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_0026:  throw

    IL_0027:  ldloc.0
    IL_0028:  ldfld      object Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::service
    IL_002d:  brfalse.s  IL_0062

    IL_002f:  ldloc.0
    IL_0030:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::'type'
    IL_0035:  ldloc.0
    IL_0036:  ldfld      object Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::service
    IL_003b:  callvirt   instance bool [netstandard]System.Type::IsInstanceOfType(object)
    IL_0040:  brtrue.s   IL_0062

    IL_0042:  ldstr      "{0} is not of type {1}"
    IL_0047:  ldstr      "service"
    IL_004c:  ldloc.0
    IL_004d:  ldfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::'type'
    IL_0052:  call       string [netstandard]System.String::Format(string,
                                                                   object,
                                                                   object)
    IL_0057:  ldstr      "service"
    IL_005c:  newobj     instance void [netstandard]System.ArgumentException::.ctor(string,
                                                                                    string)
    IL_0061:  throw

    IL_0062:  ldarg.0
    IL_0063:  ldloc.0
    IL_0064:  ldftn      instance void Moq.AutoMock.AutoMocker/'<>c__DisplayClass39_0'::'<Use>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_006a:  newobj     instance void class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::.ctor(object,
                                                                                                                                                                                                     native int)
    IL_006f:  call       instance void Moq.AutoMock.AutoMocker::WithTypeMap(class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>)
    IL_0074:  ret
  } // end of method AutoMocker::Use

  .method public hidebysig instance void 
          Use<class TService>(class [Moq]Moq.Mock`1<!!TService> mockedService) cil managed
  {
    // Code size       32 (0x20)
    .maxstack  3
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass40_0`1'<!!TService> V_0)
    IL_0000:  newobj     instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass40_0`1'<!!TService>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [Moq]Moq.Mock`1<!0> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass40_0`1'<!!TService>::mockedService
    IL_000d:  ldarg.0
    IL_000e:  ldloc.0
    IL_000f:  ldftn      instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass40_0`1'<!!TService>::'<Use>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_0015:  newobj     instance void class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::.ctor(object,
                                                                                                                                                                                                     native int)
    IL_001a:  call       instance void Moq.AutoMock.AutoMocker::WithTypeMap(class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>)
    IL_001f:  ret
  } // end of method AutoMocker::Use

  .method public hidebysig instance void 
          Use<class TService>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!TService,bool>> setup) cil managed
  {
    // Code size       32 (0x20)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "setup"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  ldarg.1
    IL_0010:  call       !!0 [Moq]Moq.Mock::Of<!!0>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!0,bool>>)
    IL_0015:  call       class [Moq]Moq.Mock`1<!!0> [Moq]Moq.Mock::Get<!!0>(!!0)
    IL_001a:  call       instance void Moq.AutoMock.AutoMocker::Use<!!0>(class [Moq]Moq.Mock`1<!!0>)
    IL_001f:  ret
  } // end of method AutoMocker::Use

  .method public hidebysig instance void 
          With<TService,class (!!TService) TImplementation>() cil managed
  {
    .param type TService 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  call       instance !!0 Moq.AutoMock.AutoMocker::CreateInstance<!!1>()
    IL_0007:  box        !!TImplementation
    IL_000c:  unbox.any  !!TService
    IL_0011:  call       instance void Moq.AutoMock.AutoMocker::Use<!!0>(!!0)
    IL_0016:  ret
  } // end of method AutoMocker::With

  .method public hidebysig instance void 
          With<class TImplementation>() cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  call       instance !!0 Moq.AutoMock.AutoMocker::CreateInstance<!!0>()
    IL_0007:  call       instance void Moq.AutoMock.AutoMocker::Use<!!0>(!!0)
    IL_000c:  ret
  } // end of method AutoMocker::With

  .method public hidebysig instance void 
          With(class [netstandard]System.Type serviceType,
               class [netstandard]System.Type implementationType) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.0
    IL_0003:  ldarg.2
    IL_0004:  call       instance object Moq.AutoMock.AutoMocker::CreateInstance(class [netstandard]System.Type)
    IL_0009:  call       instance void Moq.AutoMock.AutoMocker::Use(class [netstandard]System.Type,
                                                                    object)
    IL_000e:  ret
  } // end of method AutoMocker::With

  .method public hidebysig instance void 
          With(class [netstandard]System.Type implementationType) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.0
    IL_0003:  ldarg.1
    IL_0004:  call       instance object Moq.AutoMock.AutoMocker::CreateInstance(class [netstandard]System.Type)
    IL_0009:  call       instance void Moq.AutoMock.AutoMocker::Use(class [netstandard]System.Type,
                                                                    object)
    IL_000e:  ret
  } // end of method AutoMocker::With

  .method public hidebysig instance !!TService 
          Get<TService>() cil managed
  {
    .param type TService 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       44 (0x2c)
    .maxstack  2
    .locals init (!!TService V_0,
             object V_1,
             !!TService V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  call       instance object Moq.AutoMock.AutoMocker::Get(class [netstandard]System.Type)
    IL_0010:  stloc.1
    IL_0011:  ldloc.1
    IL_0012:  isinst     !!TService
    IL_0017:  brfalse.s  IL_0022

    IL_0019:  ldloc.1
    IL_001a:  unbox.any  !!TService
    IL_001f:  stloc.0
    IL_0020:  ldloc.0
    IL_0021:  ret

    IL_0022:  ldloca.s   V_2
    IL_0024:  initobj    !!TService
    IL_002a:  ldloc.2
    IL_002b:  ret
  } // end of method AutoMocker::Get

  .method public hidebysig instance !!TService 
          Get<TService>(bool enablePrivate) cil managed
  {
    .param type TService 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       45 (0x2d)
    .maxstack  3
    .locals init (!!TService V_0,
             object V_1,
             !!TService V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldarg.1
    IL_000c:  call       instance object Moq.AutoMock.AutoMocker::Get(class [netstandard]System.Type,
                                                                      bool)
    IL_0011:  stloc.1
    IL_0012:  ldloc.1
    IL_0013:  isinst     !!TService
    IL_0018:  brfalse.s  IL_0023

    IL_001a:  ldloc.1
    IL_001b:  unbox.any  !!TService
    IL_0020:  stloc.0
    IL_0021:  ldloc.0
    IL_0022:  ret

    IL_0023:  ldloca.s   V_2
    IL_0025:  initobj    !!TService
    IL_002b:  ldloc.2
    IL_002c:  ret
  } // end of method AutoMocker::Get

  .method public hidebysig instance object 
          Get(class [netstandard]System.Type serviceType) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  call       instance object Moq.AutoMock.AutoMocker::Get(class [netstandard]System.Type,
                                                                      bool)
    IL_0008:  ret
  } // end of method AutoMocker::Get

  .method public hidebysig instance object 
          Get(class [netstandard]System.Type serviceType,
              bool enablePrivate) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Moq.AutoMock.ObjectGraphContext::.ctor(bool)
    IL_0008:  call       instance object Moq.AutoMock.AutoMocker::Get(class [netstandard]System.Type,
                                                                      class Moq.AutoMock.ObjectGraphContext)
    IL_000d:  ret
  } // end of method AutoMocker::Get

  .method private hidebysig instance object 
          Get(class [netstandard]System.Type serviceType,
              class Moq.AutoMock.ObjectGraphContext context) cil managed
  {
    // Code size       68 (0x44)
    .maxstack  4
    .locals init (class Moq.AutoMock.IInstance V_0,
             class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldloca.s   V_0
    IL_0005:  call       instance bool Moq.AutoMock.AutoMocker::TryGet(class [netstandard]System.Type,
                                                                       class Moq.AutoMock.ObjectGraphContext,
                                                                       class Moq.AutoMock.IInstance&)
    IL_000a:  brfalse.s  IL_002e

    IL_000c:  ldarg.0
    IL_000d:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.AutoMocker::get_TypeMap()
    IL_0012:  stloc.1
    IL_0013:  ldloc.1
    IL_0014:  brfalse.s  IL_0027

    IL_0016:  ldloc.1
    IL_0017:  ldarg.1
    IL_0018:  callvirt   instance bool class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::ContainsKey(!0)
    IL_001d:  brtrue.s   IL_0027

    IL_001f:  ldloc.1
    IL_0020:  ldarg.1
    IL_0021:  ldloc.0
    IL_0022:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                    !1)
    IL_0027:  ldloc.0
    IL_0028:  callvirt   instance object Moq.AutoMock.IInstance::get_Value()
    IL_002d:  ret

    IL_002e:  ldstr      "{0} could not resolve to an object."
    IL_0033:  ldarg.1
    IL_0034:  call       string [netstandard]System.String::Format(string,
                                                                   object)
    IL_0039:  ldstr      "serviceType"
    IL_003e:  newobj     instance void [netstandard]System.ArgumentException::.ctor(string,
                                                                                    string)
    IL_0043:  throw
  } // end of method AutoMocker::Get

  .method assembly hidebysig instance bool 
          TryGet(class [netstandard]System.Type serviceType,
                 class Moq.AutoMock.ObjectGraphContext context,
                 [out] class Moq.AutoMock.IInstance& service) cil managed
  {
    .param [3]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .custom instance void System.Diagnostics.CodeAnalysis.NotNullWhenAttribute::.ctor(bool) = ( 01 00 01 00 00 ) 
    // Code size       36 (0x24)
    .maxstack  4
    .locals init (class Moq.AutoMock.IInstance V_0)
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "serviceType"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  ldarg.1
    IL_0010:  ldarg.2
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       instance bool Moq.AutoMock.AutoMocker::TryResolve(class [netstandard]System.Type,
                                                                           class Moq.AutoMock.ObjectGraphContext,
                                                                           class Moq.AutoMock.IInstance&)
    IL_0018:  brfalse.s  IL_001f

    IL_001a:  ldarg.3
    IL_001b:  ldloc.0
    IL_001c:  stind.ref
    IL_001d:  ldc.i4.1
    IL_001e:  ret

    IL_001f:  ldarg.3
    IL_0020:  ldnull
    IL_0021:  stind.ref
    IL_0022:  ldc.i4.0
    IL_0023:  ret
  } // end of method AutoMocker::TryGet

  .method private hidebysig newslot virtual final 
          instance object  System.IServiceProvider.GetService(class [netstandard]System.Type serviceType) cil managed
  {
    .param [0]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .override [netstandard]System.IServiceProvider::GetService
    // Code size       26 (0x1a)
    .maxstack  4
    .locals init (class Moq.AutoMock.IInstance V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  newobj     instance void Moq.AutoMock.ObjectGraphContext::.ctor(bool)
    IL_0008:  ldloca.s   V_0
    IL_000a:  call       instance bool Moq.AutoMock.AutoMocker::TryGet(class [netstandard]System.Type,
                                                                       class Moq.AutoMock.ObjectGraphContext,
                                                                       class Moq.AutoMock.IInstance&)
    IL_000f:  brtrue.s   IL_0013

    IL_0011:  ldnull
    IL_0012:  ret

    IL_0013:  ldloc.0
    IL_0014:  callvirt   instance object Moq.AutoMock.IInstance::get_Value()
    IL_0019:  ret
  } // end of method AutoMocker::System.IServiceProvider.GetService

  .method public hidebysig instance class [Moq]Moq.Mock`1<!!TService> 
          GetMock<class TService>() cil managed
  {
    // Code size       22 (0x16)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::GetMock(class [netstandard]System.Type)
    IL_0010:  castclass  class [Moq]Moq.Mock`1<!!TService>
    IL_0015:  ret
  } // end of method AutoMocker::GetMock

  .method public hidebysig instance class [Moq]Moq.Mock`1<!!TService> 
          GetMock<class TService>(bool enablePrivate) cil managed
  {
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldarg.1
    IL_000c:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::GetMock(class [netstandard]System.Type,
                                                                                       bool)
    IL_0011:  castclass  class [Moq]Moq.Mock`1<!!TService>
    IL_0016:  ret
  } // end of method AutoMocker::GetMock

  .method public hidebysig instance class [Moq]Moq.Mock 
          GetMock(class [netstandard]System.Type serviceType) cil managed
  {
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "serviceType"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  ldarg.1
    IL_0010:  ldc.i4.0
    IL_0011:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::GetMock(class [netstandard]System.Type,
                                                                                       bool)
    IL_0016:  ret
  } // end of method AutoMocker::GetMock

  .method public hidebysig instance class [Moq]Moq.Mock 
          GetMock(class [netstandard]System.Type serviceType,
                  bool enablePrivate) cil managed
  {
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "serviceType"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  ldarg.1
    IL_0010:  ldarg.2
    IL_0011:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::GetMockImplementation(class [netstandard]System.Type,
                                                                                                     bool)
    IL_0016:  ret
  } // end of method AutoMocker::GetMock

  .method private hidebysig instance class [Moq]Moq.Mock 
          GetMockImplementation(class [netstandard]System.Type serviceType,
                                bool enablePrivate) cil managed
  {
    // Code size       99 (0x63)
    .maxstack  4
    .locals init (class Moq.AutoMock.IInstance V_0,
             class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Moq.AutoMock.ObjectGraphContext::.ctor(bool)
    IL_0008:  ldloca.s   V_0
    IL_000a:  call       instance bool Moq.AutoMock.AutoMocker::TryResolve(class [netstandard]System.Type,
                                                                           class Moq.AutoMock.ObjectGraphContext,
                                                                           class Moq.AutoMock.IInstance&)
    IL_000f:  brfalse.s  IL_0040

    IL_0011:  ldloc.0
    IL_0012:  callvirt   instance bool Moq.AutoMock.IInstance::get_IsMock()
    IL_0017:  brfalse.s  IL_0040

    IL_0019:  ldarg.0
    IL_001a:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.AutoMocker::get_TypeMap()
    IL_001f:  stloc.1
    IL_0020:  ldloc.1
    IL_0021:  brfalse.s  IL_0034

    IL_0023:  ldloc.1
    IL_0024:  ldarg.1
    IL_0025:  callvirt   instance bool class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::ContainsKey(!0)
    IL_002a:  brtrue.s   IL_0034

    IL_002c:  ldloc.1
    IL_002d:  ldarg.1
    IL_002e:  ldloc.0
    IL_002f:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                    !1)
    IL_0034:  ldloc.0
    IL_0035:  castclass  Moq.AutoMock.MockInstance
    IL_003a:  callvirt   instance class [Moq]Moq.Mock Moq.AutoMock.MockInstance::get_Mock()
    IL_003f:  ret

    IL_0040:  ldstr      "Registered service `{0}` was not a mock"
    IL_0045:  ldarg.0
    IL_0046:  ldarg.1
    IL_0047:  call       instance object Moq.AutoMock.AutoMocker::Get(class [netstandard]System.Type)
    IL_004c:  dup
    IL_004d:  brtrue.s   IL_0053

    IL_004f:  pop
    IL_0050:  ldnull
    IL_0051:  br.s       IL_0058

    IL_0053:  call       instance class [netstandard]System.Type [netstandard]System.Object::GetType()
    IL_0058:  call       string [netstandard]System.String::Format(string,
                                                                   object)
    IL_005d:  newobj     instance void [netstandard]System.ArgumentException::.ctor(string)
    IL_0062:  throw
  } // end of method AutoMocker::GetMockImplementation

  .method public hidebysig instance class [Moq]Moq.Language.Flow.ISetup`1<!!TService> 
          Setup<class TService>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!!TService>> setup) cil managed
  {
    // Code size       32 (0x20)
    .maxstack  3
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass58_0`1'<!!TService> V_0)
    IL_0000:  newobj     instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass58_0`1'<!!TService>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass58_0`1'<!!TService>::setup
    IL_000d:  ldarg.0
    IL_000e:  ldloc.0
    IL_000f:  ldftn      instance class [Moq]Moq.Language.Flow.ISetup`1<!0> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass58_0`1'<!!TService>::'<Setup>b__0'(class [Moq]Moq.Mock`1<!0>)
    IL_0015:  newobj     instance void class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!TService>,class [Moq]Moq.Language.Flow.ISetup`1<!!TService>>::.ctor(object,
                                                                                                                                                                    native int)
    IL_001a:  call       instance !!0 Moq.AutoMock.AutoMocker::Setup<class [Moq]Moq.Language.Flow.ISetup`1<!!0>,!!0>(class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!1>,!!0>)
    IL_001f:  ret
  } // end of method AutoMocker::Setup

  .method public hidebysig instance class [Moq]Moq.Language.Flow.ISetup`2<!!TService,!!TReturn> 
          Setup<class TService,TReturn>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!TService,!!TReturn>> setup) cil managed
  {
    .param type TReturn 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       32 (0x20)
    .maxstack  3
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass59_0`2'<!!TService,!!TReturn> V_0)
    IL_0000:  newobj     instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass59_0`2'<!!TService,!!TReturn>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!1>> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass59_0`2'<!!TService,!!TReturn>::setup
    IL_000d:  ldarg.0
    IL_000e:  ldloc.0
    IL_000f:  ldftn      instance class [Moq]Moq.Language.Flow.ISetup`2<!0,!1> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass59_0`2'<!!TService,!!TReturn>::'<Setup>b__0'(class [Moq]Moq.Mock`1<!0>)
    IL_0015:  newobj     instance void class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!TService>,class [Moq]Moq.Language.Flow.ISetup`2<!!TService,!!TReturn>>::.ctor(object,
                                                                                                                                                                              native int)
    IL_001a:  call       instance !!0 Moq.AutoMock.AutoMocker::Setup<class [Moq]Moq.Language.Flow.ISetup`2<!!0,!!1>,!!0>(class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!1>,!!0>)
    IL_001f:  ret
  } // end of method AutoMocker::Setup

  .method public hidebysig instance class [Moq]Moq.Language.Flow.ISetup`1<!!TService> 
          SetupWithAny<class TService>(string methodName) cil managed
  {
    // Code size       32 (0x20)
    .maxstack  3
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass60_0`1'<!!TService> V_0)
    IL_0000:  newobj     instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass60_0`1'<!!TService>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      string class Moq.AutoMock.AutoMocker/'<>c__DisplayClass60_0`1'<!!TService>::methodName
    IL_000d:  ldarg.0
    IL_000e:  ldloc.0
    IL_000f:  ldftn      instance class [Moq]Moq.Language.Flow.ISetup`1<!0> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass60_0`1'<!!TService>::'<SetupWithAny>b__0'(class [Moq]Moq.Mock`1<!0>)
    IL_0015:  newobj     instance void class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!TService>,class [Moq]Moq.Language.Flow.ISetup`1<!!TService>>::.ctor(object,
                                                                                                                                                                    native int)
    IL_001a:  call       instance !!0 Moq.AutoMock.AutoMocker::Setup<class [Moq]Moq.Language.Flow.ISetup`1<!!0>,!!0>(class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!1>,!!0>)
    IL_001f:  ret
  } // end of method AutoMocker::SetupWithAny

  .method public hidebysig instance class [Moq]Moq.Language.Flow.ISetup`2<!!TService,!!TReturn> 
          SetupWithAny<class TService,TReturn>(string methodName) cil managed
  {
    .param type TReturn 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       32 (0x20)
    .maxstack  3
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass61_0`2'<!!TService,!!TReturn> V_0)
    IL_0000:  newobj     instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass61_0`2'<!!TService,!!TReturn>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      string class Moq.AutoMock.AutoMocker/'<>c__DisplayClass61_0`2'<!!TService,!!TReturn>::methodName
    IL_000d:  ldarg.0
    IL_000e:  ldloc.0
    IL_000f:  ldftn      instance class [Moq]Moq.Language.Flow.ISetup`2<!0,!1> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass61_0`2'<!!TService,!!TReturn>::'<SetupWithAny>b__0'(class [Moq]Moq.Mock`1<!0>)
    IL_0015:  newobj     instance void class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!TService>,class [Moq]Moq.Language.Flow.ISetup`2<!!TService,!!TReturn>>::.ctor(object,
                                                                                                                                                                              native int)
    IL_001a:  call       instance !!0 Moq.AutoMock.AutoMocker::Setup<class [Moq]Moq.Language.Flow.ISetup`2<!!0,!!1>,!!0>(class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!1>,!!0>)
    IL_001f:  ret
  } // end of method AutoMocker::SetupWithAny

  .method private hidebysig instance !!TReturn 
          Setup<TReturn,class TService>(class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!TService>,!!TReturn> returnValue) cil managed
  {
    .param type TReturn 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       30 (0x1e)
    .maxstack  2
    .locals init (class [Moq]Moq.Mock`1<!!TService> V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::GetOrMakeMockFor(class [netstandard]System.Type)
    IL_0010:  castclass  class [Moq]Moq.Mock`1<!!TService>
    IL_0015:  stloc.0
    IL_0016:  ldarg.1
    IL_0017:  ldloc.0
    IL_0018:  callvirt   instance !1 class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!TService>,!!TReturn>::Invoke(!0)
    IL_001d:  ret
  } // end of method AutoMocker::Setup

  .method public hidebysig instance class [Moq]Moq.Mock`1<!!TService> 
          SetupAllProperties<class TService>() cil managed
  {
    // Code size       38 (0x26)
    .maxstack  2
    .locals init (class [Moq]Moq.Mock`1<!!TService> V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::GetOrMakeMockFor(class [netstandard]System.Type)
    IL_0010:  castclass  class [Moq]Moq.Mock`1<!!TService>
    IL_0015:  stloc.0
    IL_0016:  ldarg.0
    IL_0017:  ldloc.0
    IL_0018:  call       instance void Moq.AutoMock.AutoMocker::Use<!!0>(class [Moq]Moq.Mock`1<!!0>)
    IL_001d:  ldloc.0
    IL_001e:  callvirt   instance class [Moq]Moq.Mock`1<!0> class [Moq]Moq.Mock`1<!!TService>::SetupAllProperties()
    IL_0023:  pop
    IL_0024:  ldloc.0
    IL_0025:  ret
  } // end of method AutoMocker::SetupAllProperties

  .method public hidebysig instance class [Moq]Moq.Language.ISetupSequentialResult`1<!!TReturn> 
          SetupSequence<class TService,TReturn>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!TService,!!TReturn>> setup) cil managed
  {
    .param type TReturn 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       51 (0x33)
    .maxstack  3
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass64_0`2'<!!TService,!!TReturn> V_0)
    IL_0000:  newobj     instance void class Moq.AutoMock.AutoMocker/'<>c__DisplayClass64_0`2'<!!TService,!!TReturn>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!1>> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass64_0`2'<!!TService,!!TReturn>::setup
    IL_000d:  ldloc.0
    IL_000e:  ldfld      class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!1>> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass64_0`2'<!!TService,!!TReturn>::setup
    IL_0013:  brtrue.s   IL_0020

    IL_0015:  ldstr      "setup"
    IL_001a:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001f:  throw

    IL_0020:  ldarg.0
    IL_0021:  ldloc.0
    IL_0022:  ldftn      instance class [Moq]Moq.Language.ISetupSequentialResult`1<!1> class Moq.AutoMock.AutoMocker/'<>c__DisplayClass64_0`2'<!!TService,!!TReturn>::'<SetupSequence>b__0'(class [Moq]Moq.Mock`1<!0>)
    IL_0028:  newobj     instance void class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!TService>,class [Moq]Moq.Language.ISetupSequentialResult`1<!!TReturn>>::.ctor(object,
                                                                                                                                                                              native int)
    IL_002d:  call       instance !!0 Moq.AutoMock.AutoMocker::Setup<class [Moq]Moq.Language.ISetupSequentialResult`1<!!1>,!!0>(class [netstandard]System.Func`2<class [Moq]Moq.Mock`1<!!1>,!!0>)
    IL_0032:  ret
  } // end of method AutoMocker::SetupSequence

  .method public hidebysig instance void 
          Combine(class [netstandard]System.Type 'type',
                  class [netstandard]System.Type[] forwardTo) cil managed
  {
    .param [2]
    .custom instance void [netstandard]System.ParamArrayAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       126 (0x7e)
    .maxstack  4
    .locals init (class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> V_0,
             class [Moq]Moq.Mock V_1,
             class [netstandard]System.Collections.Generic.IEnumerator`1<class [netstandard]System.Type> V_2,
             class [netstandard]System.Type V_3)
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "type"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.AutoMocker::get_TypeMap()
    IL_0014:  stloc.0
    IL_0015:  ldloc.0
    IL_0016:  brtrue.s   IL_0023

    IL_0018:  ldstr      "CacheResolver was not found. Cannot combine types "
    + "without resolver."
    IL_001d:  newobj     instance void [netstandard]System.InvalidOperationException::.ctor(string)
    IL_0022:  throw

    IL_0023:  ldarg.2
    IL_0024:  ldarg.0
    IL_0025:  ldarg.1
    IL_0026:  call       instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::GetOrMakeMockFor(class [netstandard]System.Type)
    IL_002b:  ldnull
    IL_002c:  ldftn      class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::'<Combine>g__As|65_0'(class [Moq]Moq.Mock,
                                                                                            class [netstandard]System.Type)
    IL_0032:  newobj     instance void class [netstandard]System.Func`3<class [Moq]Moq.Mock,class [netstandard]System.Type,class [Moq]Moq.Mock>::.ctor(object,
                                                                                                                                                       native int)
    IL_0037:  call       !!1 [netstandard]System.Linq.Enumerable::Aggregate<class [netstandard]System.Type,class [Moq]Moq.Mock>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                !!1,
                                                                                                                                class [netstandard]System.Func`3<!!1,!!0,!!1>)
    IL_003c:  stloc.1
    IL_003d:  ldc.i4.1
    IL_003e:  newarr     [netstandard]System.Type
    IL_0043:  dup
    IL_0044:  ldc.i4.0
    IL_0045:  ldarg.1
    IL_0046:  stelem.ref
    IL_0047:  ldarg.2
    IL_0048:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::Concat<class [netstandard]System.Type>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                      class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_004d:  callvirt   instance class [netstandard]System.Collections.Generic.IEnumerator`1<!0> class [netstandard]System.Collections.Generic.IEnumerable`1<class [netstandard]System.Type>::GetEnumerator()
    IL_0052:  stloc.2
    .try
    {
      IL_0053:  br.s       IL_0069

      IL_0055:  ldloc.2
      IL_0056:  callvirt   instance !0 class [netstandard]System.Collections.Generic.IEnumerator`1<class [netstandard]System.Type>::get_Current()
      IL_005b:  stloc.3
      IL_005c:  ldloc.0
      IL_005d:  ldloc.3
      IL_005e:  ldloc.1
      IL_005f:  newobj     instance void Moq.AutoMock.MockInstance::.ctor(class [Moq]Moq.Mock)
      IL_0064:  callvirt   instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::set_Item(!0,
                                                                                                                                                                      !1)
      IL_0069:  ldloc.2
      IL_006a:  callvirt   instance bool [netstandard]System.Collections.IEnumerator::MoveNext()
      IL_006f:  brtrue.s   IL_0055

      IL_0071:  leave.s    IL_007d

    }  // end .try
    finally
    {
      IL_0073:  ldloc.2
      IL_0074:  brfalse.s  IL_007c

      IL_0076:  ldloc.2
      IL_0077:  callvirt   instance void [netstandard]System.IDisposable::Dispose()
      IL_007c:  endfinally
    }  // end handler
    IL_007d:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          VerifyAll() cil managed
  {
    // Code size       83 (0x53)
    .maxstack  2
    .locals init (class [netstandard]System.Collections.Generic.IEnumerator`1<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>> V_0,
             valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> V_1,
             class Moq.AutoMock.MockInstance V_2)
    IL_0000:  ldarg.0
    IL_0001:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.AutoMocker::get_TypeMap()
    IL_0006:  dup
    IL_0007:  brtrue.s   IL_0014

    IL_0009:  ldstr      "CacheResolver was not found. Cannot verify expecta"
    + "tions without resolver."
    IL_000e:  newobj     instance void [netstandard]System.InvalidOperationException::.ctor(string)
    IL_0013:  throw

    IL_0014:  callvirt   instance class [netstandard]System.Collections.Generic.IEnumerator`1<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<!0,!1>> class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::GetEnumerator()
    IL_0019:  stloc.0
    .try
    {
      IL_001a:  br.s       IL_003e

      IL_001c:  ldloc.0
      IL_001d:  callvirt   instance !0 class [netstandard]System.Collections.Generic.IEnumerator`1<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::get_Current()
      IL_0022:  stloc.1
      IL_0023:  ldloca.s   V_1
      IL_0025:  call       instance !1 valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::get_Value()
      IL_002a:  isinst     Moq.AutoMock.MockInstance
      IL_002f:  stloc.2
      IL_0030:  ldloc.2
      IL_0031:  brfalse.s  IL_003e

      IL_0033:  ldloc.2
      IL_0034:  callvirt   instance class [Moq]Moq.Mock Moq.AutoMock.MockInstance::get_Mock()
      IL_0039:  callvirt   instance void [Moq]Moq.Mock::VerifyAll()
      IL_003e:  ldloc.0
      IL_003f:  callvirt   instance bool [netstandard]System.Collections.IEnumerator::MoveNext()
      IL_0044:  brtrue.s   IL_001c

      IL_0046:  leave.s    IL_0052

    }  // end .try
    finally
    {
      IL_0048:  ldloc.0
      IL_0049:  brfalse.s  IL_0051

      IL_004b:  ldloc.0
      IL_004c:  callvirt   instance void [netstandard]System.IDisposable::Dispose()
      IL_0051:  endfinally
    }  // end handler
    IL_0052:  ret
  } // end of method AutoMocker::VerifyAll

  .method public hidebysig instance void 
          Verify() cil managed
  {
    // Code size       83 (0x53)
    .maxstack  2
    .locals init (class [netstandard]System.Collections.Generic.IEnumerator`1<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>> V_0,
             valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> V_1,
             class Moq.AutoMock.MockInstance V_2)
    IL_0000:  ldarg.0
    IL_0001:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.AutoMocker::get_TypeMap()
    IL_0006:  dup
    IL_0007:  brtrue.s   IL_0014

    IL_0009:  ldstr      "CacheResolver was not found. Cannot verify expecta"
    + "tions without resolver."
    IL_000e:  newobj     instance void [netstandard]System.InvalidOperationException::.ctor(string)
    IL_0013:  throw

    IL_0014:  callvirt   instance class [netstandard]System.Collections.Generic.IEnumerator`1<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<!0,!1>> class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::GetEnumerator()
    IL_0019:  stloc.0
    .try
    {
      IL_001a:  br.s       IL_003e

      IL_001c:  ldloc.0
      IL_001d:  callvirt   instance !0 class [netstandard]System.Collections.Generic.IEnumerator`1<valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::get_Current()
      IL_0022:  stloc.1
      IL_0023:  ldloca.s   V_1
      IL_0025:  call       instance !1 valuetype [netstandard]System.Collections.Generic.KeyValuePair`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::get_Value()
      IL_002a:  isinst     Moq.AutoMock.MockInstance
      IL_002f:  stloc.2
      IL_0030:  ldloc.2
      IL_0031:  brfalse.s  IL_003e

      IL_0033:  ldloc.2
      IL_0034:  callvirt   instance class [Moq]Moq.Mock Moq.AutoMock.MockInstance::get_Mock()
      IL_0039:  callvirt   instance void [Moq]Moq.Mock::Verify()
      IL_003e:  ldloc.0
      IL_003f:  callvirt   instance bool [netstandard]System.Collections.IEnumerator::MoveNext()
      IL_0044:  brtrue.s   IL_001c

      IL_0046:  leave.s    IL_0052

    }  // end .try
    finally
    {
      IL_0048:  ldloc.0
      IL_0049:  brfalse.s  IL_0051

      IL_004b:  ldloc.0
      IL_004c:  callvirt   instance void [netstandard]System.IDisposable::Dispose()
      IL_0051:  endfinally
    }  // end handler
    IL_0052:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T,TResult>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,!!TResult>> expression) cil managed
  {
    .param type TResult 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       27 (0x1b)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<!!1>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>)
    IL_001a:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T,TResult>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,!!TResult>> expression,
                                  valuetype [Moq]Moq.Times times) cil managed
  {
    .param type TResult 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       28 (0x1c)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  ldarg.2
    IL_0016:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<!!1>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>,
                                                                               valuetype [Moq]Moq.Times)
    IL_001b:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T,TResult>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,!!TResult>> expression,
                                  class [netstandard]System.Func`1<valuetype [Moq]Moq.Times> times) cil managed
  {
    .param type TResult 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       42 (0x2a)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.2
    IL_000f:  brtrue.s   IL_001c

    IL_0011:  ldstr      "times"
    IL_0016:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001b:  throw

    IL_001c:  ldarg.0
    IL_001d:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0022:  ldarg.1
    IL_0023:  ldarg.2
    IL_0024:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<!!1>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>,
                                                                               class [netstandard]System.Func`1<valuetype [Moq]Moq.Times>)
    IL_0029:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T,TResult>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,!!TResult>> expression,
                                  string failMessage) cil managed
  {
    .param type TResult 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       28 (0x1c)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  ldarg.2
    IL_0016:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<!!1>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>,
                                                                               string)
    IL_001b:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T,TResult>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,!!TResult>> expression,
                                  valuetype [Moq]Moq.Times times,
                                  string failMessage) cil managed
  {
    .param type TResult 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       43 (0x2b)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.3
    IL_000f:  brtrue.s   IL_001c

    IL_0011:  ldstr      "failMessage"
    IL_0016:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001b:  throw

    IL_001c:  ldarg.0
    IL_001d:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0022:  ldarg.1
    IL_0023:  ldarg.2
    IL_0024:  ldarg.3
    IL_0025:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<!!1>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>,
                                                                               valuetype [Moq]Moq.Times,
                                                                               string)
    IL_002a:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance class [netstandard]System.IDisposable 
          AsDisposable() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance !!0 Moq.AutoMock.AutoMocker::Get<class Moq.AutoMock.IAutoMockerDisposable>()
    IL_0006:  ret
  } // end of method AutoMocker::AsDisposable

  .method assembly hidebysig instance class [Moq]Moq.Mock 
          CreateMock(class [netstandard]System.Type serviceType,
                     valuetype [Moq]Moq.MockBehavior mockBehavior,
                     valuetype [Moq]Moq.DefaultValue defaultValue,
                     class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                     bool callBase,
                     class Moq.AutoMock.ObjectGraphContext objectGraphContext) cil managed
  {
    .param [0]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [4]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       191 (0xbf)
    .maxstack  6
    .locals init (object[] V_0,
             class [netstandard]System.Reflection.ConstructorInfo V_1,
             class Moq.AutoMock.IInstance[] V_2,
             class [Moq]Moq.Mock V_3)
    IL_0000:  ldtoken    [Moq]Moq.Mock`1
    IL_0005:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000a:  ldc.i4.1
    IL_000b:  newarr     [netstandard]System.Type
    IL_0010:  dup
    IL_0011:  ldc.i4.0
    IL_0012:  ldarg.1
    IL_0013:  stelem.ref
    IL_0014:  callvirt   instance class [netstandard]System.Type [netstandard]System.Type::MakeGenericType(class [netstandard]System.Type[])
    IL_0019:  ldarg.1
    IL_001a:  callvirt   instance bool [netstandard]System.Type::get_IsClass()
    IL_001f:  brfalse.s  IL_0036

    IL_0021:  ldtoken    [netstandard]System.Delegate
    IL_0026:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002b:  ldarg.1
    IL_002c:  callvirt   instance bool [netstandard]System.Type::IsAssignableFrom(class [netstandard]System.Type)
    IL_0031:  ldc.i4.0
    IL_0032:  ceq
    IL_0034:  br.s       IL_0037

    IL_0036:  ldc.i4.0
    IL_0037:  call       !!0[] [netstandard]System.Array::Empty<object>()
    IL_003c:  stloc.0
    IL_003d:  brfalse.s  IL_0079

    IL_003f:  ldarg.0
    IL_0040:  ldarg.1
    IL_0041:  ldarg.s    objectGraphContext
    IL_0043:  ldloca.s   V_1
    IL_0045:  ldloca.s   V_2
    IL_0047:  call       instance bool Moq.AutoMock.AutoMocker::TryGetConstructorInvocation(class [netstandard]System.Type,
                                                                                            class Moq.AutoMock.ObjectGraphContext,
                                                                                            class [netstandard]System.Reflection.ConstructorInfo&,
                                                                                            class Moq.AutoMock.IInstance[]&)
    IL_004c:  brfalse.s  IL_0079

    IL_004e:  ldloc.2
    IL_004f:  ldsfld     class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,object> Moq.AutoMock.AutoMocker/'<>c'::'<>9__74_0'
    IL_0054:  dup
    IL_0055:  brtrue.s   IL_006e

    IL_0057:  pop
    IL_0058:  ldsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
    IL_005d:  ldftn      instance object Moq.AutoMock.AutoMocker/'<>c'::'<CreateMock>b__74_0'(class Moq.AutoMock.IInstance)
    IL_0063:  newobj     instance void class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,object>::.ctor(object,
                                                                                                                    native int)
    IL_0068:  dup
    IL_0069:  stsfld     class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,object> Moq.AutoMock.AutoMocker/'<>c'::'<>9__74_0'
    IL_006e:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!1> [netstandard]System.Linq.Enumerable::Select<class Moq.AutoMock.IInstance,object>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                           class [netstandard]System.Func`2<!!0,!!1>)
    IL_0073:  call       !!0[] [netstandard]System.Linq.Enumerable::ToArray<object>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0078:  stloc.0
    IL_0079:  ldc.i4.2
    IL_007a:  newarr     [netstandard]System.Object
    IL_007f:  dup
    IL_0080:  ldc.i4.0
    IL_0081:  ldarg.2
    IL_0082:  box        [Moq]Moq.MockBehavior
    IL_0087:  stelem.ref
    IL_0088:  dup
    IL_0089:  ldc.i4.1
    IL_008a:  ldloc.0
    IL_008b:  stelem.ref
    IL_008c:  call       object [netstandard]System.Activator::CreateInstance(class [netstandard]System.Type,
                                                                              object[])
    IL_0091:  isinst     [Moq]Moq.Mock
    IL_0096:  stloc.3
    IL_0097:  ldloc.3
    IL_0098:  brfalse.s  IL_00bd

    IL_009a:  ldarg.s    defaultValueProvider
    IL_009c:  brfalse.s  IL_00ac

    IL_009e:  ldarg.3
    IL_009f:  ldc.i4.2
    IL_00a0:  bne.un.s   IL_00ac

    IL_00a2:  ldloc.3
    IL_00a3:  ldarg.s    defaultValueProvider
    IL_00a5:  callvirt   instance void [Moq]Moq.Mock::set_DefaultValueProvider(class [Moq]Moq.DefaultValueProvider)
    IL_00aa:  br.s       IL_00b3

    IL_00ac:  ldloc.3
    IL_00ad:  ldarg.3
    IL_00ae:  callvirt   instance void [Moq]Moq.Mock::set_DefaultValue(valuetype [Moq]Moq.DefaultValue)
    IL_00b3:  ldloc.3
    IL_00b4:  ldarg.s    callBase
    IL_00b6:  callvirt   instance void [Moq]Moq.Mock::set_CallBase(bool)
    IL_00bb:  ldloc.3
    IL_00bc:  ret

    IL_00bd:  ldnull
    IL_00be:  ret
  } // end of method AutoMocker::CreateMock

  .method assembly hidebysig instance bool 
          TryGetConstructorInvocation(class [netstandard]System.Type 'type',
                                      class Moq.AutoMock.ObjectGraphContext context,
                                      [out] class [netstandard]System.Reflection.ConstructorInfo& constructor,
                                      [out] class Moq.AutoMock.IInstance[]& arguments) cil managed
  {
    .param [3]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .custom instance void System.Diagnostics.CodeAnalysis.NotNullWhenAttribute::.ctor(bool) = ( 01 00 01 00 00 ) 
    .param [4]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 02 01 00 00 ) 
    .custom instance void System.Diagnostics.CodeAnalysis.NotNullWhenAttribute::.ctor(bool) = ( 01 00 01 00 00 ) 
    // Code size       186 (0xba)
    .maxstack  5
    .locals init (class [netstandard]System.Collections.Generic.IEnumerator`1<class [netstandard]System.Reflection.ConstructorInfo> V_0,
             class [netstandard]System.Reflection.ConstructorInfo V_1,
             class Moq.AutoMock.IInstance[] V_2,
             bool V_3)
    IL_0000:  ldarg.1
    IL_0001:  ldarg.2
    IL_0002:  callvirt   instance valuetype [netstandard]System.Reflection.BindingFlags Moq.AutoMock.ObjectGraphContext::get_BindingFlags()
    IL_0007:  callvirt   instance class [netstandard]System.Reflection.ConstructorInfo[] [netstandard]System.Type::GetConstructors(valuetype [netstandard]System.Reflection.BindingFlags)
    IL_000c:  ldsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,int32> Moq.AutoMock.AutoMocker/'<>c'::'<>9__75_0'
    IL_0011:  dup
    IL_0012:  brtrue.s   IL_002b

    IL_0014:  pop
    IL_0015:  ldsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
    IL_001a:  ldftn      instance int32 Moq.AutoMock.AutoMocker/'<>c'::'<TryGetConstructorInvocation>b__75_0'(class [netstandard]System.Reflection.ConstructorInfo)
    IL_0020:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,int32>::.ctor(object,
                                                                                                                                           native int)
    IL_0025:  dup
    IL_0026:  stsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,int32> Moq.AutoMock.AutoMocker/'<>c'::'<>9__75_0'
    IL_002b:  call       class [netstandard]System.Linq.IOrderedEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::OrderByDescending<class [netstandard]System.Reflection.ConstructorInfo,int32>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                                     class [netstandard]System.Func`2<!!0,!!1>)
    IL_0030:  ldc.i4.1
    IL_0031:  newarr     [netstandard]System.Reflection.ConstructorInfo
    IL_0036:  dup
    IL_0037:  ldc.i4.0
    IL_0038:  ldarg.1
    IL_0039:  call       class [netstandard]System.Reflection.ConstructorInfo Moq.AutoMock.AutoMocker::'<TryGetConstructorInvocation>g__Empty|75_2'(class [netstandard]System.Type)
    IL_003e:  stelem.ref
    IL_003f:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::Concat<class [netstandard]System.Reflection.ConstructorInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                            class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0044:  ldsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> Moq.AutoMock.AutoMocker/'<>c'::'<>9__75_1'
    IL_0049:  dup
    IL_004a:  brtrue.s   IL_0063

    IL_004c:  pop
    IL_004d:  ldsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
    IL_0052:  ldftn      instance bool Moq.AutoMock.AutoMocker/'<>c'::'<TryGetConstructorInvocation>b__75_1'(class [netstandard]System.Reflection.ConstructorInfo)
    IL_0058:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool>::.ctor(object,
                                                                                                                                          native int)
    IL_005d:  dup
    IL_005e:  stsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> Moq.AutoMock.AutoMocker/'<>c'::'<>9__75_1'
    IL_0063:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::Where<class [netstandard]System.Reflection.ConstructorInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                           class [netstandard]System.Func`2<!!0,bool>)
    IL_0068:  ldarg.2
    IL_0069:  callvirt   instance class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> Moq.AutoMock.ObjectGraphContext::get_VisitedTypes()
    IL_006e:  ldarg.1
    IL_006f:  callvirt   instance bool class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type>::Add(!0)
    IL_0074:  pop
    IL_0075:  callvirt   instance class [netstandard]System.Collections.Generic.IEnumerator`1<!0> class [netstandard]System.Collections.Generic.IEnumerable`1<class [netstandard]System.Reflection.ConstructorInfo>::GetEnumerator()
    IL_007a:  stloc.0
    .try
    {
      IL_007b:  br.s       IL_009b

      IL_007d:  ldloc.0
      IL_007e:  callvirt   instance !0 class [netstandard]System.Collections.Generic.IEnumerator`1<class [netstandard]System.Reflection.ConstructorInfo>::get_Current()
      IL_0083:  stloc.1
      IL_0084:  ldarg.0
      IL_0085:  ldloc.1
      IL_0086:  ldarg.2
      IL_0087:  ldloca.s   V_2
      IL_0089:  call       instance bool Moq.AutoMock.AutoMocker::'<TryGetConstructorInvocation>g__TryCreateArguments|75_3'(class [netstandard]System.Reflection.ConstructorInfo,
                                                                                                                            class Moq.AutoMock.ObjectGraphContext,
                                                                                                                            class Moq.AutoMock.IInstance[]&)
      IL_008e:  brfalse.s  IL_009b

      IL_0090:  ldarg.3
      IL_0091:  ldloc.1
      IL_0092:  stind.ref
      IL_0093:  ldarg.s    arguments
      IL_0095:  ldloc.2
      IL_0096:  stind.ref
      IL_0097:  ldc.i4.1
      IL_0098:  stloc.3
      IL_0099:  leave.s    IL_00b8

      IL_009b:  ldloc.0
      IL_009c:  callvirt   instance bool [netstandard]System.Collections.IEnumerator::MoveNext()
      IL_00a1:  brtrue.s   IL_007d

      IL_00a3:  leave.s    IL_00af

    }  // end .try
    finally
    {
      IL_00a5:  ldloc.0
      IL_00a6:  brfalse.s  IL_00ae

      IL_00a8:  ldloc.0
      IL_00a9:  callvirt   instance void [netstandard]System.IDisposable::Dispose()
      IL_00ae:  endfinally
    }  // end handler
    IL_00af:  ldarg.3
    IL_00b0:  ldnull
    IL_00b1:  stind.ref
    IL_00b2:  ldarg.s    arguments
    IL_00b4:  ldnull
    IL_00b5:  stind.ref
    IL_00b6:  ldc.i4.0
    IL_00b7:  ret

    IL_00b8:  ldloc.3
    IL_00b9:  ret
  } // end of method AutoMocker::TryGetConstructorInvocation

  .method private hidebysig instance void 
          EnsureCached(class [netstandard]System.Type 'type',
                       class Moq.AutoMock.IInstance 'instance') cil managed
  {
    // Code size       39 (0x27)
    .maxstack  3
    .locals init (class Moq.AutoMock.AutoMocker/'<>c__DisplayClass76_0' V_0)
    IL_0000:  newobj     instance void Moq.AutoMock.AutoMocker/'<>c__DisplayClass76_0'::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [netstandard]System.Type Moq.AutoMock.AutoMocker/'<>c__DisplayClass76_0'::'type'
    IL_000d:  ldloc.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      class Moq.AutoMock.IInstance Moq.AutoMock.AutoMocker/'<>c__DisplayClass76_0'::'instance'
    IL_0014:  ldarg.0
    IL_0015:  ldloc.0
    IL_0016:  ldftn      instance void Moq.AutoMock.AutoMocker/'<>c__DisplayClass76_0'::'<EnsureCached>b__0'(class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>)
    IL_001c:  newobj     instance void class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::.ctor(object,
                                                                                                                                                                                                     native int)
    IL_0021:  call       instance void Moq.AutoMock.AutoMocker::WithTypeMap(class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>)
    IL_0026:  ret
  } // end of method AutoMocker::EnsureCached

  .method private hidebysig instance class [Moq]Moq.Mock 
          GetOrMakeMockFor(class [netstandard]System.Type 'type') cil managed
  {
    // Code size       59 (0x3b)
    .maxstack  4
    .locals init (class Moq.AutoMock.IInstance V_0,
             class Moq.AutoMock.MockInstance V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  newobj     instance void Moq.AutoMock.ObjectGraphContext::.ctor(bool)
    IL_0008:  ldloca.s   V_0
    IL_000a:  call       instance bool Moq.AutoMock.AutoMocker::TryResolve(class [netstandard]System.Type,
                                                                           class Moq.AutoMock.ObjectGraphContext,
                                                                           class Moq.AutoMock.IInstance&)
    IL_000f:  brfalse.s  IL_002a

    IL_0011:  ldloc.0
    IL_0012:  isinst     Moq.AutoMock.MockInstance
    IL_0017:  stloc.1
    IL_0018:  ldloc.1
    IL_0019:  brfalse.s  IL_002a

    IL_001b:  ldarg.0
    IL_001c:  ldarg.1
    IL_001d:  ldloc.1
    IL_001e:  call       instance void Moq.AutoMock.AutoMocker::EnsureCached(class [netstandard]System.Type,
                                                                             class Moq.AutoMock.IInstance)
    IL_0023:  ldloc.1
    IL_0024:  callvirt   instance class [Moq]Moq.Mock Moq.AutoMock.MockInstance::get_Mock()
    IL_0029:  ret

    IL_002a:  ldstr      "{0} does not resolve to a Mock"
    IL_002f:  ldarg.1
    IL_0030:  call       string [netstandard]System.String::Format(string,
                                                                   object)
    IL_0035:  newobj     instance void [netstandard]System.ArgumentException::.ctor(string)
    IL_003a:  throw
  } // end of method AutoMocker::GetOrMakeMockFor

  .method private hidebysig instance void 
          WithTypeMap(class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>> onTypeMap) cil managed
  {
    // Code size       29 (0x1d)
    .maxstack  2
    .locals init (class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> V_0)
    IL_0000:  ldarg.0
    IL_0001:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.AutoMocker::get_TypeMap()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_0012

    IL_000a:  ldarg.1
    IL_000b:  ldloc.0
    IL_000c:  callvirt   instance void class [netstandard]System.Action`1<class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>>::Invoke(!0)
    IL_0011:  ret

    IL_0012:  ldstr      "CacheResolver was not found. Cannot cache service "
    + "instance without resolver."
    IL_0017:  newobj     instance void [netstandard]System.InvalidOperationException::.ctor(string)
    IL_001c:  throw
  } // end of method AutoMocker::WithTypeMap

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!!T>> expression) cil managed
  {
    // Code size       27 (0x1b)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  callvirt   instance void class [Moq]Moq.Mock`1<!!T>::Verify(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>>)
    IL_001a:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!!T>> expression,
                          valuetype [Moq]Moq.Times times) cil managed
  {
    // Code size       28 (0x1c)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  ldarg.2
    IL_0016:  callvirt   instance void class [Moq]Moq.Mock`1<!!T>::Verify(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>>,
                                                                          valuetype [Moq]Moq.Times)
    IL_001b:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!!T>> expression,
                          class [netstandard]System.Func`1<valuetype [Moq]Moq.Times> times) cil managed
  {
    // Code size       42 (0x2a)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.2
    IL_000f:  brtrue.s   IL_001c

    IL_0011:  ldstr      "times"
    IL_0016:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001b:  throw

    IL_001c:  ldarg.0
    IL_001d:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0022:  ldarg.1
    IL_0023:  ldarg.2
    IL_0024:  callvirt   instance void class [Moq]Moq.Mock`1<!!T>::Verify(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>>,
                                                                          class [netstandard]System.Func`1<valuetype [Moq]Moq.Times>)
    IL_0029:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!!T>> expression,
                          string failMessage) cil managed
  {
    // Code size       28 (0x1c)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  ldarg.2
    IL_0016:  callvirt   instance void class [Moq]Moq.Mock`1<!!T>::Verify(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>>,
                                                                          string)
    IL_001b:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!!T>> expression,
                          valuetype [Moq]Moq.Times times,
                          string failMessage) cil managed
  {
    // Code size       29 (0x1d)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  ldarg.2
    IL_0016:  ldarg.3
    IL_0017:  callvirt   instance void class [Moq]Moq.Mock`1<!!T>::Verify(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>>,
                                                                          valuetype [Moq]Moq.Times,
                                                                          string)
    IL_001c:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!!T>> expression,
                          class [netstandard]System.Func`1<valuetype [Moq]Moq.Times> times,
                          string failMessage) cil managed
  {
    // Code size       43 (0x2b)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.2
    IL_000f:  brtrue.s   IL_001c

    IL_0011:  ldstr      "times"
    IL_0016:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001b:  throw

    IL_001c:  ldarg.0
    IL_001d:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0022:  ldarg.1
    IL_0023:  ldarg.2
    IL_0024:  ldarg.3
    IL_0025:  callvirt   instance void class [Moq]Moq.Mock`1<!!T>::Verify(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Action`1<!0>>,
                                                                          class [netstandard]System.Func`1<valuetype [Moq]Moq.Times>,
                                                                          string)
    IL_002a:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,object>> expression) cil managed
  {
    // Code size       46 (0x2e)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  call       bool Moq.AutoMock.CastChecker::DoesReturnPrimitive<!!0>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!0,object>>)
    IL_001a:  brfalse.s  IL_0027

    IL_001c:  call       string Moq.AutoMock.Resources.Strings::get_VerifyWithValueReturn()
    IL_0021:  newobj     instance void [netstandard]System.NotSupportedException::.ctor(string)
    IL_0026:  throw

    IL_0027:  ldarg.1
    IL_0028:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<object>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>)
    IL_002d:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,object>> expression,
                          valuetype [Moq]Moq.Times times) cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  call       bool Moq.AutoMock.CastChecker::DoesReturnPrimitive<!!0>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!0,object>>)
    IL_001a:  brfalse.s  IL_0027

    IL_001c:  call       string Moq.AutoMock.Resources.Strings::get_VerifyWithValueReturn()
    IL_0021:  newobj     instance void [netstandard]System.NotSupportedException::.ctor(string)
    IL_0026:  throw

    IL_0027:  ldarg.1
    IL_0028:  ldarg.2
    IL_0029:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<object>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>,
                                                                                  valuetype [Moq]Moq.Times)
    IL_002e:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,object>> expression,
                          class [netstandard]System.Func`1<valuetype [Moq]Moq.Times> times) cil managed
  {
    // Code size       61 (0x3d)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.2
    IL_000f:  brtrue.s   IL_001c

    IL_0011:  ldstr      "times"
    IL_0016:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001b:  throw

    IL_001c:  ldarg.0
    IL_001d:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0022:  ldarg.1
    IL_0023:  call       bool Moq.AutoMock.CastChecker::DoesReturnPrimitive<!!0>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!0,object>>)
    IL_0028:  brfalse.s  IL_0035

    IL_002a:  call       string Moq.AutoMock.Resources.Strings::get_VerifyWithValueReturn()
    IL_002f:  newobj     instance void [netstandard]System.NotSupportedException::.ctor(string)
    IL_0034:  throw

    IL_0035:  ldarg.1
    IL_0036:  ldarg.2
    IL_0037:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<object>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>,
                                                                                  class [netstandard]System.Func`1<valuetype [Moq]Moq.Times>)
    IL_003c:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,object>> expression,
                          string failMessage) cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  call       bool Moq.AutoMock.CastChecker::DoesReturnPrimitive<!!0>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!0,object>>)
    IL_001a:  brfalse.s  IL_0027

    IL_001c:  call       string Moq.AutoMock.Resources.Strings::get_VerifyWithValueReturn()
    IL_0021:  newobj     instance void [netstandard]System.NotSupportedException::.ctor(string)
    IL_0026:  throw

    IL_0027:  ldarg.1
    IL_0028:  ldarg.2
    IL_0029:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<object>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>,
                                                                                  string)
    IL_002e:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!T,object>> expression,
                          valuetype [Moq]Moq.Times times,
                          string failMessage) cil managed
  {
    // Code size       48 (0x30)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0014:  ldarg.1
    IL_0015:  call       bool Moq.AutoMock.CastChecker::DoesReturnPrimitive<!!0>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!0,object>>)
    IL_001a:  brfalse.s  IL_0027

    IL_001c:  call       string Moq.AutoMock.Resources.Strings::get_VerifyWithValueReturn()
    IL_0021:  newobj     instance void [netstandard]System.NotSupportedException::.ctor(string)
    IL_0026:  throw

    IL_0027:  ldarg.1
    IL_0028:  ldarg.2
    IL_0029:  ldarg.3
    IL_002a:  callvirt   instance void class [Moq]Moq.Mock`1<!!0>::Verify<object>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!0,!!0>>,
                                                                                  valuetype [Moq]Moq.Times,
                                                                                  string)
    IL_002f:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Verify<class T>() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance class [Moq]Moq.Mock`1<!!0> Moq.AutoMock.AutoMocker::GetMock<!!0>()
    IL_0006:  callvirt   instance void [Moq]Moq.Mock::Verify()
    IL_000b:  ret
  } // end of method AutoMocker::Verify

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       36 (0x24)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.1
    IL_000c:  newarr     [netstandard]System.Type
    IL_0011:  dup
    IL_0012:  ldc.i4.0
    IL_0013:  ldtoken    !!TAsWellAs1
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  stelem.ref
    IL_001e:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_0023:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       49 (0x31)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.2
    IL_000c:  newarr     [netstandard]System.Type
    IL_0011:  dup
    IL_0012:  ldc.i4.0
    IL_0013:  ldtoken    !!TAsWellAs1
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  stelem.ref
    IL_001e:  dup
    IL_001f:  ldc.i4.1
    IL_0020:  ldtoken    !!TAsWellAs2
    IL_0025:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002a:  stelem.ref
    IL_002b:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_0030:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2,TAsWellAs3>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       62 (0x3e)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.3
    IL_000c:  newarr     [netstandard]System.Type
    IL_0011:  dup
    IL_0012:  ldc.i4.0
    IL_0013:  ldtoken    !!TAsWellAs1
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  stelem.ref
    IL_001e:  dup
    IL_001f:  ldc.i4.1
    IL_0020:  ldtoken    !!TAsWellAs2
    IL_0025:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002a:  stelem.ref
    IL_002b:  dup
    IL_002c:  ldc.i4.2
    IL_002d:  ldtoken    !!TAsWellAs3
    IL_0032:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0037:  stelem.ref
    IL_0038:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_003d:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       75 (0x4b)
    .maxstack  6
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.4
    IL_000c:  newarr     [netstandard]System.Type
    IL_0011:  dup
    IL_0012:  ldc.i4.0
    IL_0013:  ldtoken    !!TAsWellAs1
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  stelem.ref
    IL_001e:  dup
    IL_001f:  ldc.i4.1
    IL_0020:  ldtoken    !!TAsWellAs2
    IL_0025:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002a:  stelem.ref
    IL_002b:  dup
    IL_002c:  ldc.i4.2
    IL_002d:  ldtoken    !!TAsWellAs3
    IL_0032:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0037:  stelem.ref
    IL_0038:  dup
    IL_0039:  ldc.i4.3
    IL_003a:  ldtoken    !!TAsWellAs4
    IL_003f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0044:  stelem.ref
    IL_0045:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_004a:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       88 (0x58)
    .maxstack  6
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.5
    IL_000c:  newarr     [netstandard]System.Type
    IL_0011:  dup
    IL_0012:  ldc.i4.0
    IL_0013:  ldtoken    !!TAsWellAs1
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  stelem.ref
    IL_001e:  dup
    IL_001f:  ldc.i4.1
    IL_0020:  ldtoken    !!TAsWellAs2
    IL_0025:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002a:  stelem.ref
    IL_002b:  dup
    IL_002c:  ldc.i4.2
    IL_002d:  ldtoken    !!TAsWellAs3
    IL_0032:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0037:  stelem.ref
    IL_0038:  dup
    IL_0039:  ldc.i4.3
    IL_003a:  ldtoken    !!TAsWellAs4
    IL_003f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0044:  stelem.ref
    IL_0045:  dup
    IL_0046:  ldc.i4.4
    IL_0047:  ldtoken    !!TAsWellAs5
    IL_004c:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0051:  stelem.ref
    IL_0052:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_0057:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       101 (0x65)
    .maxstack  6
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.6
    IL_000c:  newarr     [netstandard]System.Type
    IL_0011:  dup
    IL_0012:  ldc.i4.0
    IL_0013:  ldtoken    !!TAsWellAs1
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  stelem.ref
    IL_001e:  dup
    IL_001f:  ldc.i4.1
    IL_0020:  ldtoken    !!TAsWellAs2
    IL_0025:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002a:  stelem.ref
    IL_002b:  dup
    IL_002c:  ldc.i4.2
    IL_002d:  ldtoken    !!TAsWellAs3
    IL_0032:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0037:  stelem.ref
    IL_0038:  dup
    IL_0039:  ldc.i4.3
    IL_003a:  ldtoken    !!TAsWellAs4
    IL_003f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0044:  stelem.ref
    IL_0045:  dup
    IL_0046:  ldc.i4.4
    IL_0047:  ldtoken    !!TAsWellAs5
    IL_004c:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0051:  stelem.ref
    IL_0052:  dup
    IL_0053:  ldc.i4.5
    IL_0054:  ldtoken    !!TAsWellAs6
    IL_0059:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_005e:  stelem.ref
    IL_005f:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_0064:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6,TAsWellAs7>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       114 (0x72)
    .maxstack  6
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.7
    IL_000c:  newarr     [netstandard]System.Type
    IL_0011:  dup
    IL_0012:  ldc.i4.0
    IL_0013:  ldtoken    !!TAsWellAs1
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  stelem.ref
    IL_001e:  dup
    IL_001f:  ldc.i4.1
    IL_0020:  ldtoken    !!TAsWellAs2
    IL_0025:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002a:  stelem.ref
    IL_002b:  dup
    IL_002c:  ldc.i4.2
    IL_002d:  ldtoken    !!TAsWellAs3
    IL_0032:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0037:  stelem.ref
    IL_0038:  dup
    IL_0039:  ldc.i4.3
    IL_003a:  ldtoken    !!TAsWellAs4
    IL_003f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0044:  stelem.ref
    IL_0045:  dup
    IL_0046:  ldc.i4.4
    IL_0047:  ldtoken    !!TAsWellAs5
    IL_004c:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0051:  stelem.ref
    IL_0052:  dup
    IL_0053:  ldc.i4.5
    IL_0054:  ldtoken    !!TAsWellAs6
    IL_0059:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_005e:  stelem.ref
    IL_005f:  dup
    IL_0060:  ldc.i4.6
    IL_0061:  ldtoken    !!TAsWellAs7
    IL_0066:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_006b:  stelem.ref
    IL_006c:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_0071:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6,TAsWellAs7,TAsWellAs8>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       127 (0x7f)
    .maxstack  6
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.8
    IL_000c:  newarr     [netstandard]System.Type
    IL_0011:  dup
    IL_0012:  ldc.i4.0
    IL_0013:  ldtoken    !!TAsWellAs1
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  stelem.ref
    IL_001e:  dup
    IL_001f:  ldc.i4.1
    IL_0020:  ldtoken    !!TAsWellAs2
    IL_0025:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002a:  stelem.ref
    IL_002b:  dup
    IL_002c:  ldc.i4.2
    IL_002d:  ldtoken    !!TAsWellAs3
    IL_0032:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0037:  stelem.ref
    IL_0038:  dup
    IL_0039:  ldc.i4.3
    IL_003a:  ldtoken    !!TAsWellAs4
    IL_003f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0044:  stelem.ref
    IL_0045:  dup
    IL_0046:  ldc.i4.4
    IL_0047:  ldtoken    !!TAsWellAs5
    IL_004c:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0051:  stelem.ref
    IL_0052:  dup
    IL_0053:  ldc.i4.5
    IL_0054:  ldtoken    !!TAsWellAs6
    IL_0059:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_005e:  stelem.ref
    IL_005f:  dup
    IL_0060:  ldc.i4.6
    IL_0061:  ldtoken    !!TAsWellAs7
    IL_0066:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_006b:  stelem.ref
    IL_006c:  dup
    IL_006d:  ldc.i4.7
    IL_006e:  ldtoken    !!TAsWellAs8
    IL_0073:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0078:  stelem.ref
    IL_0079:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_007e:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6,TAsWellAs7,TAsWellAs8,TAsWellAs9>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       141 (0x8d)
    .maxstack  6
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.s   9
    IL_000d:  newarr     [netstandard]System.Type
    IL_0012:  dup
    IL_0013:  ldc.i4.0
    IL_0014:  ldtoken    !!TAsWellAs1
    IL_0019:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001e:  stelem.ref
    IL_001f:  dup
    IL_0020:  ldc.i4.1
    IL_0021:  ldtoken    !!TAsWellAs2
    IL_0026:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002b:  stelem.ref
    IL_002c:  dup
    IL_002d:  ldc.i4.2
    IL_002e:  ldtoken    !!TAsWellAs3
    IL_0033:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0038:  stelem.ref
    IL_0039:  dup
    IL_003a:  ldc.i4.3
    IL_003b:  ldtoken    !!TAsWellAs4
    IL_0040:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0045:  stelem.ref
    IL_0046:  dup
    IL_0047:  ldc.i4.4
    IL_0048:  ldtoken    !!TAsWellAs5
    IL_004d:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0052:  stelem.ref
    IL_0053:  dup
    IL_0054:  ldc.i4.5
    IL_0055:  ldtoken    !!TAsWellAs6
    IL_005a:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_005f:  stelem.ref
    IL_0060:  dup
    IL_0061:  ldc.i4.6
    IL_0062:  ldtoken    !!TAsWellAs7
    IL_0067:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_006c:  stelem.ref
    IL_006d:  dup
    IL_006e:  ldc.i4.7
    IL_006f:  ldtoken    !!TAsWellAs8
    IL_0074:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0079:  stelem.ref
    IL_007a:  dup
    IL_007b:  ldc.i4.8
    IL_007c:  ldtoken    !!TAsWellAs9
    IL_0081:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0086:  stelem.ref
    IL_0087:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_008c:  ret
  } // end of method AutoMocker::Combine

  .method public hidebysig instance void 
          Combine<TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6,TAsWellAs7,TAsWellAs8,TAsWellAs9,TAsWellAs10>() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       155 (0x9b)
    .maxstack  6
    IL_0000:  ldarg.0
    IL_0001:  ldtoken    !!TService
    IL_0006:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_000b:  ldc.i4.s   10
    IL_000d:  newarr     [netstandard]System.Type
    IL_0012:  dup
    IL_0013:  ldc.i4.0
    IL_0014:  ldtoken    !!TAsWellAs1
    IL_0019:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001e:  stelem.ref
    IL_001f:  dup
    IL_0020:  ldc.i4.1
    IL_0021:  ldtoken    !!TAsWellAs2
    IL_0026:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002b:  stelem.ref
    IL_002c:  dup
    IL_002d:  ldc.i4.2
    IL_002e:  ldtoken    !!TAsWellAs3
    IL_0033:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0038:  stelem.ref
    IL_0039:  dup
    IL_003a:  ldc.i4.3
    IL_003b:  ldtoken    !!TAsWellAs4
    IL_0040:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0045:  stelem.ref
    IL_0046:  dup
    IL_0047:  ldc.i4.4
    IL_0048:  ldtoken    !!TAsWellAs5
    IL_004d:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0052:  stelem.ref
    IL_0053:  dup
    IL_0054:  ldc.i4.5
    IL_0055:  ldtoken    !!TAsWellAs6
    IL_005a:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_005f:  stelem.ref
    IL_0060:  dup
    IL_0061:  ldc.i4.6
    IL_0062:  ldtoken    !!TAsWellAs7
    IL_0067:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_006c:  stelem.ref
    IL_006d:  dup
    IL_006e:  ldc.i4.7
    IL_006f:  ldtoken    !!TAsWellAs8
    IL_0074:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0079:  stelem.ref
    IL_007a:  dup
    IL_007b:  ldc.i4.8
    IL_007c:  ldtoken    !!TAsWellAs9
    IL_0081:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0086:  stelem.ref
    IL_0087:  dup
    IL_0088:  ldc.i4.s   9
    IL_008a:  ldtoken    !!TAsWellAs10
    IL_008f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0094:  stelem.ref
    IL_0095:  call       instance void Moq.AutoMock.AutoMocker::Combine(class [netstandard]System.Type,
                                                                        class [netstandard]System.Type[])
    IL_009a:  ret
  } // end of method AutoMocker::Combine

  .method assembly hidebysig static class [Moq]Moq.Mock 
          '<Combine>g__As|65_0'(class [Moq]Moq.Mock mock,
                                class [netstandard]System.Type forInterface) cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       79 (0x4f)
    .maxstack  5
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance class [netstandard]System.Type [netstandard]System.Object::GetType()
    IL_0006:  callvirt   instance class [netstandard]System.Reflection.MethodInfo[] [netstandard]System.Type::GetMethods()
    IL_000b:  ldsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> Moq.AutoMock.AutoMocker/'<>c'::'<>9__65_1'
    IL_0010:  dup
    IL_0011:  brtrue.s   IL_002a

    IL_0013:  pop
    IL_0014:  ldsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
    IL_0019:  ldftn      instance bool Moq.AutoMock.AutoMocker/'<>c'::'<Combine>b__65_1'(class [netstandard]System.Reflection.MethodInfo)
    IL_001f:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool>::.ctor(object,
                                                                                                                                     native int)
    IL_0024:  dup
    IL_0025:  stsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> Moq.AutoMock.AutoMocker/'<>c'::'<>9__65_1'
    IL_002a:  call       !!0 [netstandard]System.Linq.Enumerable::First<class [netstandard]System.Reflection.MethodInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                         class [netstandard]System.Func`2<!!0,bool>)
    IL_002f:  ldc.i4.1
    IL_0030:  newarr     [netstandard]System.Type
    IL_0035:  dup
    IL_0036:  ldc.i4.0
    IL_0037:  ldarg.1
    IL_0038:  stelem.ref
    IL_0039:  callvirt   instance class [netstandard]System.Reflection.MethodInfo [netstandard]System.Reflection.MethodInfo::MakeGenericMethod(class [netstandard]System.Type[])
    IL_003e:  ldarg.0
    IL_003f:  call       !!0[] [netstandard]System.Array::Empty<object>()
    IL_0044:  callvirt   instance object [netstandard]System.Reflection.MethodBase::Invoke(object,
                                                                                           object[])
    IL_0049:  castclass  [Moq]Moq.Mock
    IL_004e:  ret
  } // end of method AutoMocker::'<Combine>g__As|65_0'

  .method assembly hidebysig static class [netstandard]System.Reflection.ConstructorInfo 
          '<TryGetConstructorInvocation>g__Empty|75_2'(class [netstandard]System.Type 'type') cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .param [0]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       45 (0x2d)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.s   36
    IL_0003:  callvirt   instance class [netstandard]System.Reflection.ConstructorInfo[] [netstandard]System.Type::GetConstructors(valuetype [netstandard]System.Reflection.BindingFlags)
    IL_0008:  ldsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> Moq.AutoMock.AutoMocker/'<>c'::'<>9__75_4'
    IL_000d:  dup
    IL_000e:  brtrue.s   IL_0027

    IL_0010:  pop
    IL_0011:  ldsfld     class Moq.AutoMock.AutoMocker/'<>c' Moq.AutoMock.AutoMocker/'<>c'::'<>9'
    IL_0016:  ldftn      instance bool Moq.AutoMock.AutoMocker/'<>c'::'<TryGetConstructorInvocation>b__75_4'(class [netstandard]System.Reflection.ConstructorInfo)
    IL_001c:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool>::.ctor(object,
                                                                                                                                          native int)
    IL_0021:  dup
    IL_0022:  stsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> Moq.AutoMock.AutoMocker/'<>c'::'<>9__75_4'
    IL_0027:  call       !!0 [netstandard]System.Linq.Enumerable::FirstOrDefault<class [netstandard]System.Reflection.ConstructorInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                       class [netstandard]System.Func`2<!!0,bool>)
    IL_002c:  ret
  } // end of method AutoMocker::'<TryGetConstructorInvocation>g__Empty|75_2'

  .method private hidebysig instance bool 
          '<TryGetConstructorInvocation>g__TryCreateArguments|75_3'(class [netstandard]System.Reflection.ConstructorInfo constructor,
                                                                    class Moq.AutoMock.ObjectGraphContext context,
                                                                    [out] class Moq.AutoMock.IInstance[]& arguments) cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       81 (0x51)
    .maxstack  4
    .locals init (class [netstandard]System.Reflection.ParameterInfo[] V_0,
             int32 V_1,
             class Moq.AutoMock.ObjectGraphContext V_2,
             class Moq.AutoMock.IInstance V_3)
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance class [netstandard]System.Reflection.ParameterInfo[] [netstandard]System.Reflection.MethodBase::GetParameters()
    IL_0006:  stloc.0
    IL_0007:  ldarg.3
    IL_0008:  ldloc.0
    IL_0009:  ldlen
    IL_000a:  conv.i4
    IL_000b:  newarr     Moq.AutoMock.IInstance
    IL_0010:  stind.ref
    IL_0011:  ldc.i4.0
    IL_0012:  stloc.1
    IL_0013:  br.s       IL_0049

    IL_0015:  ldarg.2
    IL_0016:  newobj     instance void Moq.AutoMock.ObjectGraphContext::.ctor(class Moq.AutoMock.ObjectGraphContext)
    IL_001b:  stloc.2
    IL_001c:  ldarg.0
    IL_001d:  ldloc.0
    IL_001e:  ldloc.1
    IL_001f:  ldelem.ref
    IL_0020:  callvirt   instance class [netstandard]System.Type [netstandard]System.Reflection.ParameterInfo::get_ParameterType()
    IL_0025:  ldloc.2
    IL_0026:  ldloca.s   V_3
    IL_0028:  call       instance bool Moq.AutoMock.AutoMocker::TryGet(class [netstandard]System.Type,
                                                                       class Moq.AutoMock.ObjectGraphContext,
                                                                       class Moq.AutoMock.IInstance&)
    IL_002d:  brtrue.s   IL_0031

    IL_002f:  ldc.i4.0
    IL_0030:  ret

    IL_0031:  ldarg.0
    IL_0032:  ldloc.0
    IL_0033:  ldloc.1
    IL_0034:  ldelem.ref
    IL_0035:  callvirt   instance class [netstandard]System.Type [netstandard]System.Reflection.ParameterInfo::get_ParameterType()
    IL_003a:  ldloc.3
    IL_003b:  call       instance void Moq.AutoMock.AutoMocker::EnsureCached(class [netstandard]System.Type,
                                                                             class Moq.AutoMock.IInstance)
    IL_0040:  ldarg.3
    IL_0041:  ldind.ref
    IL_0042:  ldloc.1
    IL_0043:  ldloc.3
    IL_0044:  stelem.ref
    IL_0045:  ldloc.1
    IL_0046:  ldc.i4.1
    IL_0047:  add
    IL_0048:  stloc.1
    IL_0049:  ldloc.1
    IL_004a:  ldloc.0
    IL_004b:  ldlen
    IL_004c:  conv.i4
    IL_004d:  blt.s      IL_0015

    IL_004f:  ldc.i4.1
    IL_0050:  ret
  } // end of method AutoMocker::'<TryGetConstructorInvocation>g__TryCreateArguments|75_3'

  .property instance valuetype [Moq]Moq.MockBehavior
          MockBehavior()
  {
    .get instance valuetype [Moq]Moq.MockBehavior Moq.AutoMock.AutoMocker::get_MockBehavior()
  } // end of property AutoMocker::MockBehavior
  .property instance valuetype [Moq]Moq.DefaultValue
          DefaultValue()
  {
    .get instance valuetype [Moq]Moq.DefaultValue Moq.AutoMock.AutoMocker::get_DefaultValue()
  } // end of property AutoMocker::DefaultValue
  .property instance class [Moq]Moq.DefaultValueProvider
          DefaultValueProvider()
  {
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .get instance class [Moq]Moq.DefaultValueProvider Moq.AutoMock.AutoMocker::get_DefaultValueProvider()
  } // end of property AutoMocker::DefaultValueProvider
  .property instance bool CallBase()
  {
    .get instance bool Moq.AutoMock.AutoMocker::get_CallBase()
  } // end of property AutoMocker::CallBase
  .property instance class [netstandard]System.Collections.Generic.IList`1<class Moq.AutoMock.Resolvers.IMockResolver>
          Resolvers()
  {
    .get instance class [netstandard]System.Collections.Generic.IList`1<class Moq.AutoMock.Resolvers.IMockResolver> Moq.AutoMock.AutoMocker::get_Resolvers()
  } // end of property AutoMocker::Resolvers
  .property instance class [netstandard]System.Collections.Generic.IReadOnlyDictionary`2<class [netstandard]System.Type,object>
          ResolvedObjects()
  {
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 03 00 00 00 01 01 02 00 00 ) 
    .get instance class [netstandard]System.Collections.Generic.IReadOnlyDictionary`2<class [netstandard]System.Type,object> Moq.AutoMock.AutoMocker::get_ResolvedObjects()
  } // end of property AutoMocker::ResolvedObjects
  .property instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>
          TypeMap()
  {
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 03 00 00 00 02 01 01 00 00 ) 
    .get instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.AutoMocker::get_TypeMap()
  } // end of property AutoMocker::TypeMap
} // end of class Moq.AutoMock.AutoMocker

.class private abstract auto ansi sealed beforefieldinit Moq.AutoMock.CastChecker
       extends [netstandard]System.Object
{
  .method public hidebysig static bool  DoesReturnPrimitive<class TService>(class [netstandard]System.Linq.Expressions.Expression`1<class [netstandard]System.Func`2<!!TService,object>> expression) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       63 (0x3f)
    .maxstack  2
    .locals init (class [netstandard]System.Linq.Expressions.LambdaExpression V_0)
    IL_0000:  ldarg.0
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "expression"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.0
    IL_000f:  stloc.0
    IL_0010:  ldloc.0
    IL_0011:  callvirt   instance class [netstandard]System.Linq.Expressions.Expression [netstandard]System.Linq.Expressions.LambdaExpression::get_Body()
    IL_0016:  callvirt   instance valuetype [netstandard]System.Linq.Expressions.ExpressionType [netstandard]System.Linq.Expressions.Expression::get_NodeType()
    IL_001b:  ldc.i4.s   10
    IL_001d:  bne.un.s   IL_003d

    IL_001f:  ldloc.0
    IL_0020:  callvirt   instance class [netstandard]System.Linq.Expressions.Expression [netstandard]System.Linq.Expressions.LambdaExpression::get_Body()
    IL_0025:  callvirt   instance class [netstandard]System.Type [netstandard]System.Linq.Expressions.Expression::get_Type()
    IL_002a:  ldtoken    [netstandard]System.Object
    IL_002f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0034:  call       bool [netstandard]System.Type::op_Equality(class [netstandard]System.Type,
                                                                    class [netstandard]System.Type)
    IL_0039:  brfalse.s  IL_003d

    IL_003b:  ldc.i4.1
    IL_003c:  ret

    IL_003d:  ldc.i4.0
    IL_003e:  ret
  } // end of method CastChecker::DoesReturnPrimitive

} // end of class Moq.AutoMock.CastChecker

.class private abstract auto ansi sealed beforefieldinit Moq.AutoMock.ConstructorSelector
       extends [netstandard]System.Object
{
  .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
  .class auto ansi serializable sealed nested private beforefieldinit '<>c'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public static initonly class Moq.AutoMock.ConstructorSelector/'<>c' '<>9'
    .field public static class [netstandard]System.Func`2<class [netstandard]System.Reflection.ParameterInfo,class [netstandard]System.Type> '<>9__0_3'
    .field public static class [netstandard]System.Func`3<class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo> '<>9__0_1'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 04 00 00 00 00 02 01 02 00 00 ) 
    .field public static class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> '<>9__0_5'
    .method private hidebysig specialname rtspecialname static 
            void  .cctor() cil managed
    {
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  newobj     instance void Moq.AutoMock.ConstructorSelector/'<>c'::.ctor()
      IL_0005:  stsfld     class Moq.AutoMock.ConstructorSelector/'<>c' Moq.AutoMock.ConstructorSelector/'<>c'::'<>9'
      IL_000a:  ret
    } // end of method '<>c'::.cctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c'::.ctor

    .method assembly hidebysig instance class [netstandard]System.Type 
            '<SelectCtor>b__0_3'(class [netstandard]System.Reflection.ParameterInfo parameter) cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance class [netstandard]System.Type [netstandard]System.Reflection.ParameterInfo::get_ParameterType()
      IL_0006:  ret
    } // end of method '<>c'::'<SelectCtor>b__0_3'

    .method assembly hidebysig instance class [netstandard]System.Reflection.ConstructorInfo 
            '<SelectCtor>b__0_1'(class [netstandard]System.Reflection.ConstructorInfo 'value',
                                 class [netstandard]System.Reflection.ConstructorInfo constructor) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
      .param [2]
      .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
      // Code size       27 (0x1b)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  ldarg.2
      IL_0004:  ret

      IL_0005:  ldarg.1
      IL_0006:  callvirt   instance class [netstandard]System.Reflection.ParameterInfo[] [netstandard]System.Reflection.MethodBase::GetParameters()
      IL_000b:  ldlen
      IL_000c:  conv.i4
      IL_000d:  ldarg.2
      IL_000e:  callvirt   instance class [netstandard]System.Reflection.ParameterInfo[] [netstandard]System.Reflection.MethodBase::GetParameters()
      IL_0013:  ldlen
      IL_0014:  conv.i4
      IL_0015:  bge.s      IL_0019

      IL_0017:  ldarg.2
      IL_0018:  ret

      IL_0019:  ldarg.1
      IL_001a:  ret
    } // end of method '<>c'::'<SelectCtor>b__0_1'

    .method assembly hidebysig instance bool 
            '<SelectCtor>b__0_5'(class [netstandard]System.Reflection.ConstructorInfo x) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance class [netstandard]System.Reflection.ParameterInfo[] [netstandard]System.Reflection.MethodBase::GetParameters()
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  ldc.i4.0
      IL_0009:  ceq
      IL_000b:  ret
    } // end of method '<>c'::'<SelectCtor>b__0_5'

  } // end of class '<>c'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass0_0'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [netstandard]System.Type[] existingTypes
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 00 01 00 00 ) 
    .field public class [netstandard]System.Func`2<class [netstandard]System.Type,bool> '<>9__4'
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass0_0'::.ctor

    .method assembly hidebysig instance bool 
            '<SelectCtor>b__0'(class [netstandard]System.Reflection.ConstructorInfo constructor) cil managed
    {
      // Code size       88 (0x58)
      .maxstack  4
      .locals init (class [netstandard]System.Reflection.ParameterInfo[] V_0,
               class [netstandard]System.Func`2<class [netstandard]System.Type,bool> V_1)
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance class [netstandard]System.Reflection.ParameterInfo[] [netstandard]System.Reflection.MethodBase::GetParameters()
      IL_0006:  stloc.0
      IL_0007:  ldloc.0
      IL_0008:  ldlen
      IL_0009:  conv.i4
      IL_000a:  brfalse.s  IL_0056

      IL_000c:  ldloc.0
      IL_000d:  ldsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ParameterInfo,class [netstandard]System.Type> Moq.AutoMock.ConstructorSelector/'<>c'::'<>9__0_3'
      IL_0012:  dup
      IL_0013:  brtrue.s   IL_002c

      IL_0015:  pop
      IL_0016:  ldsfld     class Moq.AutoMock.ConstructorSelector/'<>c' Moq.AutoMock.ConstructorSelector/'<>c'::'<>9'
      IL_001b:  ldftn      instance class [netstandard]System.Type Moq.AutoMock.ConstructorSelector/'<>c'::'<SelectCtor>b__0_3'(class [netstandard]System.Reflection.ParameterInfo)
      IL_0021:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.ParameterInfo,class [netstandard]System.Type>::.ctor(object,
                                                                                                                                                                    native int)
      IL_0026:  dup
      IL_0027:  stsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ParameterInfo,class [netstandard]System.Type> Moq.AutoMock.ConstructorSelector/'<>c'::'<>9__0_3'
      IL_002c:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!1> [netstandard]System.Linq.Enumerable::Select<class [netstandard]System.Reflection.ParameterInfo,class [netstandard]System.Type>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                                                           class [netstandard]System.Func`2<!!0,!!1>)
      IL_0031:  ldarg.0
      IL_0032:  ldfld      class [netstandard]System.Func`2<class [netstandard]System.Type,bool> Moq.AutoMock.ConstructorSelector/'<>c__DisplayClass0_0'::'<>9__4'
      IL_0037:  dup
      IL_0038:  brtrue.s   IL_0050

      IL_003a:  pop
      IL_003b:  ldarg.0
      IL_003c:  ldarg.0
      IL_003d:  ldftn      instance bool Moq.AutoMock.ConstructorSelector/'<>c__DisplayClass0_0'::'<SelectCtor>b__4'(class [netstandard]System.Type)
      IL_0043:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Type,bool>::.ctor(object,
                                                                                                                      native int)
      IL_0048:  dup
      IL_0049:  stloc.1
      IL_004a:  stfld      class [netstandard]System.Func`2<class [netstandard]System.Type,bool> Moq.AutoMock.ConstructorSelector/'<>c__DisplayClass0_0'::'<>9__4'
      IL_004f:  ldloc.1
      IL_0050:  call       bool [netstandard]System.Linq.Enumerable::All<class [netstandard]System.Type>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                         class [netstandard]System.Func`2<!!0,bool>)
      IL_0055:  ret

      IL_0056:  ldc.i4.1
      IL_0057:  ret
    } // end of method '<>c__DisplayClass0_0'::'<SelectCtor>b__0'

    .method assembly hidebysig instance bool 
            '<SelectCtor>b__4'(class [netstandard]System.Type 'type') cil managed
    {
      // Code size       31 (0x1f)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [netstandard]System.Type[] Moq.AutoMock.ConstructorSelector/'<>c__DisplayClass0_0'::existingTypes
      IL_0006:  ldarg.1
      IL_0007:  call       bool [netstandard]System.Linq.Enumerable::Contains<class [netstandard]System.Type>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                              !!0)
      IL_000c:  brtrue.s   IL_001d

      IL_000e:  ldarg.1
      IL_000f:  call       bool Moq.AutoMock.Extensions.TypeExtensions::IsMockable(class [netstandard]System.Type)
      IL_0014:  brtrue.s   IL_001d

      IL_0016:  ldarg.1
      IL_0017:  callvirt   instance bool [netstandard]System.Type::get_IsArray()
      IL_001c:  ret

      IL_001d:  ldc.i4.1
      IL_001e:  ret
    } // end of method '<>c__DisplayClass0_0'::'<SelectCtor>b__4'

  } // end of class '<>c__DisplayClass0_0'

  .method public hidebysig static class [netstandard]System.Reflection.ConstructorInfo 
          SelectCtor(class [netstandard]System.Type 'type',
                     class [netstandard]System.Type[] existingTypes,
                     valuetype [netstandard]System.Reflection.BindingFlags bindingFlags) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       111 (0x6f)
    .maxstack  4
    .locals init (class Moq.AutoMock.ConstructorSelector/'<>c__DisplayClass0_0' V_0)
    IL_0000:  newobj     instance void Moq.AutoMock.ConstructorSelector/'<>c__DisplayClass0_0'::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [netstandard]System.Type[] Moq.AutoMock.ConstructorSelector/'<>c__DisplayClass0_0'::existingTypes
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  callvirt   instance class [netstandard]System.Reflection.ConstructorInfo[] [netstandard]System.Type::GetConstructors(valuetype [netstandard]System.Reflection.BindingFlags)
    IL_0014:  ldloc.0
    IL_0015:  ldftn      instance bool Moq.AutoMock.ConstructorSelector/'<>c__DisplayClass0_0'::'<SelectCtor>b__0'(class [netstandard]System.Reflection.ConstructorInfo)
    IL_001b:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool>::.ctor(object,
                                                                                                                                          native int)
    IL_0020:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::Where<class [netstandard]System.Reflection.ConstructorInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                           class [netstandard]System.Func`2<!!0,bool>)
    IL_0025:  ldnull
    IL_0026:  ldsfld     class [netstandard]System.Func`3<class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo> Moq.AutoMock.ConstructorSelector/'<>c'::'<>9__0_1'
    IL_002b:  dup
    IL_002c:  brtrue.s   IL_0045

    IL_002e:  pop
    IL_002f:  ldsfld     class Moq.AutoMock.ConstructorSelector/'<>c' Moq.AutoMock.ConstructorSelector/'<>c'::'<>9'
    IL_0034:  ldftn      instance class [netstandard]System.Reflection.ConstructorInfo Moq.AutoMock.ConstructorSelector/'<>c'::'<SelectCtor>b__0_1'(class [netstandard]System.Reflection.ConstructorInfo,
                                                                                                                                                    class [netstandard]System.Reflection.ConstructorInfo)
    IL_003a:  newobj     instance void class [netstandard]System.Func`3<class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo>::.ctor(object,
                                                                                                                                                                                                                                               native int)
    IL_003f:  dup
    IL_0040:  stsfld     class [netstandard]System.Func`3<class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo> Moq.AutoMock.ConstructorSelector/'<>c'::'<>9__0_1'
    IL_0045:  call       !!1 [netstandard]System.Linq.Enumerable::Aggregate<class [netstandard]System.Reflection.ConstructorInfo,class [netstandard]System.Reflection.ConstructorInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                       !!1,
                                                                                                                                                                                       class [netstandard]System.Func`3<!!1,!!0,!!1>)
    IL_004a:  dup
    IL_004b:  brtrue.s   IL_006e

    IL_004d:  pop
    IL_004e:  ldarg.0
    IL_004f:  call       class [netstandard]System.Reflection.ConstructorInfo Moq.AutoMock.ConstructorSelector::'<SelectCtor>g__Empty|0_2'(class [netstandard]System.Type)
    IL_0054:  dup
    IL_0055:  brtrue.s   IL_006e

    IL_0057:  pop
    IL_0058:  ldstr      "Did not find a best constructor for `{0}`. If your"
    + " type has a non-public constructor, set the 'enablePrivate' parameter t"
    + "o true for this AutoMocker method."
    IL_005d:  ldarg.0
    IL_005e:  call       string [netstandard]System.String::Format(string,
                                                                   object)
    IL_0063:  ldstr      "type"
    IL_0068:  newobj     instance void [netstandard]System.ArgumentException::.ctor(string,
                                                                                    string)
    IL_006d:  throw

    IL_006e:  ret
  } // end of method ConstructorSelector::SelectCtor

  .method assembly hidebysig static class [netstandard]System.Reflection.ConstructorInfo 
          '<SelectCtor>g__Empty|0_2'(class [netstandard]System.Type 'type') cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       45 (0x2d)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.s   36
    IL_0003:  callvirt   instance class [netstandard]System.Reflection.ConstructorInfo[] [netstandard]System.Type::GetConstructors(valuetype [netstandard]System.Reflection.BindingFlags)
    IL_0008:  ldsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> Moq.AutoMock.ConstructorSelector/'<>c'::'<>9__0_5'
    IL_000d:  dup
    IL_000e:  brtrue.s   IL_0027

    IL_0010:  pop
    IL_0011:  ldsfld     class Moq.AutoMock.ConstructorSelector/'<>c' Moq.AutoMock.ConstructorSelector/'<>c'::'<>9'
    IL_0016:  ldftn      instance bool Moq.AutoMock.ConstructorSelector/'<>c'::'<SelectCtor>b__0_5'(class [netstandard]System.Reflection.ConstructorInfo)
    IL_001c:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool>::.ctor(object,
                                                                                                                                          native int)
    IL_0021:  dup
    IL_0022:  stsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.ConstructorInfo,bool> Moq.AutoMock.ConstructorSelector/'<>c'::'<>9__0_5'
    IL_0027:  call       !!0 [netstandard]System.Linq.Enumerable::FirstOrDefault<class [netstandard]System.Reflection.ConstructorInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                       class [netstandard]System.Func`2<!!0,bool>)
    IL_002c:  ret
  } // end of method ConstructorSelector::'<SelectCtor>g__Empty|0_2'

} // end of class Moq.AutoMock.ConstructorSelector

.class public auto ansi beforefieldinit Moq.AutoMock.ConstructorTestsAttribute
       extends [netstandard]System.Attribute
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .custom instance void [netstandard]System.AttributeUsageAttribute::.ctor(valuetype [netstandard]System.AttributeTargets) = ( 01 00 04 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                               4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .field private class [netstandard]System.Type '<TargetType>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname instance class [netstandard]System.Type 
          get_TargetType() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [netstandard]System.Type Moq.AutoMock.ConstructorTestsAttribute::'<TargetType>k__BackingField'
    IL_0006:  ret
  } // end of method ConstructorTestsAttribute::get_TargetType

  .method public hidebysig specialname instance void 
          set_TargetType(class [netstandard]System.Type 'value') cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      class [netstandard]System.Type Moq.AutoMock.ConstructorTestsAttribute::'<TargetType>k__BackingField'
    IL_0007:  ret
  } // end of method ConstructorTestsAttribute::set_TargetType

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method ConstructorTestsAttribute::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [netstandard]System.Type targetType) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Attribute::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  call       instance void Moq.AutoMock.ConstructorTestsAttribute::set_TargetType(class [netstandard]System.Type)
    IL_000d:  ret
  } // end of method ConstructorTestsAttribute::.ctor

  .property instance class [netstandard]System.Type
          TargetType()
  {
    .get instance class [netstandard]System.Type Moq.AutoMock.ConstructorTestsAttribute::get_TargetType()
    .set instance void Moq.AutoMock.ConstructorTestsAttribute::set_TargetType(class [netstandard]System.Type)
  } // end of property ConstructorTestsAttribute::TargetType
} // end of class Moq.AutoMock.ConstructorTestsAttribute

.class interface private abstract auto ansi Moq.AutoMock.IAutoMockerDisposable
       implements [netstandard]System.IDisposable
{
} // end of class Moq.AutoMock.IAutoMockerDisposable

.class private auto ansi sealed beforefieldinit Moq.AutoMock.AutoMockerDisposable
       extends [netstandard]System.Object
       implements Moq.AutoMock.IAutoMockerDisposable,
                  [netstandard]System.IDisposable
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .interfaceimpl type Moq.AutoMock.IAutoMockerDisposable
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .interfaceimpl type [netstandard]System.IDisposable
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .field private bool _isDisposed
  .field private initonly class Moq.AutoMock.AutoMocker '<Mocker>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method private hidebysig specialname instance class Moq.AutoMock.AutoMocker 
          get_Mocker() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Moq.AutoMock.AutoMocker Moq.AutoMock.AutoMockerDisposable::'<Mocker>k__BackingField'
    IL_0006:  ret
  } // end of method AutoMockerDisposable::get_Mocker

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Moq.AutoMock.AutoMocker mocker) cil managed
  {
    // Code size       29 (0x1d)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  dup
    IL_0009:  brtrue.s   IL_0017

    IL_000b:  pop
    IL_000c:  ldstr      "mocker"
    IL_0011:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_0016:  throw

    IL_0017:  stfld      class Moq.AutoMock.AutoMocker Moq.AutoMock.AutoMockerDisposable::'<Mocker>k__BackingField'
    IL_001c:  ret
  } // end of method AutoMockerDisposable::.ctor

  .method private hidebysig instance void 
          Dispose(bool disposing) cil managed
  {
    // Code size       85 (0x55)
    .maxstack  2
    .locals init (class [netstandard]System.Collections.Generic.IEnumerator`1<class [netstandard]System.IDisposable> V_0,
             class [netstandard]System.IDisposable V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Moq.AutoMock.AutoMockerDisposable::_isDisposed
    IL_0006:  brtrue.s   IL_0054

    IL_0008:  ldarg.0
    IL_0009:  ldc.i4.1
    IL_000a:  stfld      bool Moq.AutoMock.AutoMockerDisposable::_isDisposed
    IL_000f:  ldarg.1
    IL_0010:  brfalse.s  IL_0054

    IL_0012:  ldarg.0
    IL_0013:  call       instance class Moq.AutoMock.AutoMocker Moq.AutoMock.AutoMockerDisposable::get_Mocker()
    IL_0018:  callvirt   instance class [netstandard]System.Collections.Generic.IReadOnlyDictionary`2<class [netstandard]System.Type,object> Moq.AutoMock.AutoMocker::get_ResolvedObjects()
    IL_001d:  callvirt   instance class [netstandard]System.Collections.Generic.IEnumerable`1<!1> class [netstandard]System.Collections.Generic.IReadOnlyDictionary`2<class [netstandard]System.Type,object>::get_Values()
    IL_0022:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::OfType<class [netstandard]System.IDisposable>(class [netstandard]System.Collections.IEnumerable)
    IL_0027:  callvirt   instance class [netstandard]System.Collections.Generic.IEnumerator`1<!0> class [netstandard]System.Collections.Generic.IEnumerable`1<class [netstandard]System.IDisposable>::GetEnumerator()
    IL_002c:  stloc.0
    .try
    {
      IL_002d:  br.s       IL_0040

      IL_002f:  ldloc.0
      IL_0030:  callvirt   instance !0 class [netstandard]System.Collections.Generic.IEnumerator`1<class [netstandard]System.IDisposable>::get_Current()
      IL_0035:  stloc.1
      IL_0036:  ldloc.1
      IL_0037:  ldarg.0
      IL_0038:  beq.s      IL_0040

      IL_003a:  ldloc.1
      IL_003b:  callvirt   instance void [netstandard]System.IDisposable::Dispose()
      IL_0040:  ldloc.0
      IL_0041:  callvirt   instance bool [netstandard]System.Collections.IEnumerator::MoveNext()
      IL_0046:  brtrue.s   IL_002f

      IL_0048:  leave.s    IL_0054

    }  // end .try
    finally
    {
      IL_004a:  ldloc.0
      IL_004b:  brfalse.s  IL_0053

      IL_004d:  ldloc.0
      IL_004e:  callvirt   instance void [netstandard]System.IDisposable::Dispose()
      IL_0053:  endfinally
    }  // end handler
    IL_0054:  ret
  } // end of method AutoMockerDisposable::Dispose

  .method public hidebysig newslot virtual final 
          instance void  Dispose() cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  call       instance void Moq.AutoMock.AutoMockerDisposable::Dispose(bool)
    IL_0007:  ldarg.0
    IL_0008:  call       void [netstandard]System.GC::SuppressFinalize(object)
    IL_000d:  ret
  } // end of method AutoMockerDisposable::Dispose

  .property instance class Moq.AutoMock.AutoMocker
          Mocker()
  {
    .get instance class Moq.AutoMock.AutoMocker Moq.AutoMock.AutoMockerDisposable::get_Mocker()
  } // end of property AutoMockerDisposable::Mocker
} // end of class Moq.AutoMock.AutoMockerDisposable

.class interface private abstract auto ansi Moq.AutoMock.IInstance
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
  .method public hidebysig newslot specialname abstract virtual 
          instance object  get_Value() cil managed
  {
  } // end of method IInstance::get_Value

  .method public hidebysig newslot specialname abstract virtual 
          instance bool  get_IsMock() cil managed
  {
  } // end of method IInstance::get_IsMock

  .property instance object Value()
  {
    .get instance object Moq.AutoMock.IInstance::get_Value()
  } // end of property IInstance::Value
  .property instance bool IsMock()
  {
    .get instance bool Moq.AutoMock.IInstance::get_IsMock()
  } // end of property IInstance::IsMock
} // end of class Moq.AutoMock.IInstance

.class private auto ansi sealed beforefieldinit Moq.AutoMock.MockArrayInstance
       extends [netstandard]System.Object
       implements Moq.AutoMock.IInstance
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .interfaceimpl type Moq.AutoMock.IInstance
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .class auto ansi serializable sealed nested private beforefieldinit '<>c'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public static initonly class Moq.AutoMock.MockArrayInstance/'<>c' '<>9'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,bool> '<>9__6_0'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method private hidebysig specialname rtspecialname static 
            void  .cctor() cil managed
    {
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  newobj     instance void Moq.AutoMock.MockArrayInstance/'<>c'::.ctor()
      IL_0005:  stsfld     class Moq.AutoMock.MockArrayInstance/'<>c' Moq.AutoMock.MockArrayInstance/'<>c'::'<>9'
      IL_000a:  ret
    } // end of method '<>c'::.cctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c'::.ctor

    .method assembly hidebysig instance bool 
            '<get_IsMock>b__6_0'(class Moq.AutoMock.IInstance m) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance bool Moq.AutoMock.IInstance::get_IsMock()
      IL_0006:  ret
    } // end of method '<>c'::'<get_IsMock>b__6_0'

  } // end of class '<>c'

  .field private initonly class [netstandard]System.Type _type
  .field private initonly class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance> _mocks
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [netstandard]System.Type 'type') cil managed
  {
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance>::.ctor()
    IL_0006:  stfld      class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance> Moq.AutoMock.MockArrayInstance::_mocks
    IL_000b:  ldarg.0
    IL_000c:  call       instance void [netstandard]System.Object::.ctor()
    IL_0011:  ldarg.0
    IL_0012:  ldarg.1
    IL_0013:  stfld      class [netstandard]System.Type Moq.AutoMock.MockArrayInstance::_type
    IL_0018:  ret
  } // end of method MockArrayInstance::.ctor

  .method public hidebysig newslot specialname virtual final 
          instance object  get_Value() cil managed
  {
    // Code size       91 (0x5b)
    .maxstack  5
    .locals init (int32 V_0,
             class [netstandard]System.Array V_1,
             valuetype [netstandard]System.Collections.Generic.List`1/Enumerator<class Moq.AutoMock.IInstance> V_2,
             class Moq.AutoMock.IInstance V_3)
    IL_0000:  ldc.i4.0
    IL_0001:  stloc.0
    IL_0002:  ldarg.0
    IL_0003:  ldfld      class [netstandard]System.Type Moq.AutoMock.MockArrayInstance::_type
    IL_0008:  ldarg.0
    IL_0009:  ldfld      class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance> Moq.AutoMock.MockArrayInstance::_mocks
    IL_000e:  callvirt   instance int32 class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance>::get_Count()
    IL_0013:  call       class [netstandard]System.Array [netstandard]System.Array::CreateInstance(class [netstandard]System.Type,
                                                                                                   int32)
    IL_0018:  stloc.1
    IL_0019:  ldarg.0
    IL_001a:  ldfld      class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance> Moq.AutoMock.MockArrayInstance::_mocks
    IL_001f:  callvirt   instance valuetype [netstandard]System.Collections.Generic.List`1/Enumerator<!0> class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance>::GetEnumerator()
    IL_0024:  stloc.2
    .try
    {
      IL_0025:  br.s       IL_0040

      IL_0027:  ldloca.s   V_2
      IL_0029:  call       instance !0 valuetype [netstandard]System.Collections.Generic.List`1/Enumerator<class Moq.AutoMock.IInstance>::get_Current()
      IL_002e:  stloc.3
      IL_002f:  ldloc.1
      IL_0030:  ldloc.3
      IL_0031:  callvirt   instance object Moq.AutoMock.IInstance::get_Value()
      IL_0036:  ldloc.0
      IL_0037:  dup
      IL_0038:  ldc.i4.1
      IL_0039:  add
      IL_003a:  stloc.0
      IL_003b:  callvirt   instance void [netstandard]System.Array::SetValue(object,
                                                                             int32)
      IL_0040:  ldloca.s   V_2
      IL_0042:  call       instance bool valuetype [netstandard]System.Collections.Generic.List`1/Enumerator<class Moq.AutoMock.IInstance>::MoveNext()
      IL_0047:  brtrue.s   IL_0027

      IL_0049:  leave.s    IL_0059

    }  // end .try
    finally
    {
      IL_004b:  ldloca.s   V_2
      IL_004d:  constrained. valuetype [netstandard]System.Collections.Generic.List`1/Enumerator<class Moq.AutoMock.IInstance>
      IL_0053:  callvirt   instance void [netstandard]System.IDisposable::Dispose()
      IL_0058:  endfinally
    }  // end handler
    IL_0059:  ldloc.1
    IL_005a:  ret
  } // end of method MockArrayInstance::get_Value

  .method public hidebysig newslot specialname virtual final 
          instance bool  get_IsMock() cil managed
  {
    // Code size       43 (0x2b)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance> Moq.AutoMock.MockArrayInstance::_mocks
    IL_0006:  ldsfld     class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,bool> Moq.AutoMock.MockArrayInstance/'<>c'::'<>9__6_0'
    IL_000b:  dup
    IL_000c:  brtrue.s   IL_0025

    IL_000e:  pop
    IL_000f:  ldsfld     class Moq.AutoMock.MockArrayInstance/'<>c' Moq.AutoMock.MockArrayInstance/'<>c'::'<>9'
    IL_0014:  ldftn      instance bool Moq.AutoMock.MockArrayInstance/'<>c'::'<get_IsMock>b__6_0'(class Moq.AutoMock.IInstance)
    IL_001a:  newobj     instance void class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,bool>::.ctor(object,
                                                                                                                  native int)
    IL_001f:  dup
    IL_0020:  stsfld     class [netstandard]System.Func`2<class Moq.AutoMock.IInstance,bool> Moq.AutoMock.MockArrayInstance/'<>c'::'<>9__6_0'
    IL_0025:  call       bool [netstandard]System.Linq.Enumerable::Any<class Moq.AutoMock.IInstance>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                     class [netstandard]System.Func`2<!!0,bool>)
    IL_002a:  ret
  } // end of method MockArrayInstance::get_IsMock

  .method public hidebysig instance void 
          Add(class Moq.AutoMock.IInstance 'instance') cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance> Moq.AutoMock.MockArrayInstance::_mocks
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void class [netstandard]System.Collections.Generic.List`1<class Moq.AutoMock.IInstance>::Add(!0)
    IL_000c:  ret
  } // end of method MockArrayInstance::Add

  .property instance object Value()
  {
    .get instance object Moq.AutoMock.MockArrayInstance::get_Value()
  } // end of property MockArrayInstance::Value
  .property instance bool IsMock()
  {
    .get instance bool Moq.AutoMock.MockArrayInstance::get_IsMock()
  } // end of property MockArrayInstance::IsMock
} // end of class Moq.AutoMock.MockArrayInstance

.class public abstract auto ansi sealed beforefieldinit Moq.AutoMock.MockExtensions
       extends [netstandard]System.Object
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
  .class auto ansi serializable sealed nested private beforefieldinit '<>c'
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public static initonly class Moq.AutoMock.MockExtensions/'<>c' '<>9'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method private hidebysig specialname rtspecialname static 
            void  .cctor() cil managed
    {
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  newobj     instance void Moq.AutoMock.MockExtensions/'<>c'::.ctor()
      IL_0005:  stsfld     class Moq.AutoMock.MockExtensions/'<>c' Moq.AutoMock.MockExtensions/'<>c'::'<>9'
      IL_000a:  ret
    } // end of method '<>c'::.cctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c'::.ctor

    .method assembly hidebysig instance class [netstandard]System.Reflection.MethodInfo 
            '<.cctor>b__7_0'() cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
      // Code size       21 (0x15)
      .maxstack  8
      IL_0000:  ldtoken    [Moq]Moq.Mock
      IL_0005:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
      IL_000a:  ldstr      "As"
      IL_000f:  call       instance class [netstandard]System.Reflection.MethodInfo [netstandard]System.Type::GetMethod(string)
      IL_0014:  ret
    } // end of method '<>c'::'<.cctor>b__7_0'

  } // end of class '<>c'

  .class auto ansi serializable sealed nested private beforefieldinit '<>c__0`1'<class T>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public static initonly class Moq.AutoMock.MockExtensions/'<>c__0`1'<!T> '<>9'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> '<>9__0_0'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method private hidebysig specialname rtspecialname static 
            void  .cctor() cil managed
    {
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  newobj     instance void class Moq.AutoMock.MockExtensions/'<>c__0`1'<!T>::.ctor()
      IL_0005:  stsfld     class Moq.AutoMock.MockExtensions/'<>c__0`1'<!0> class Moq.AutoMock.MockExtensions/'<>c__0`1'<!T>::'<>9'
      IL_000a:  ret
    } // end of method '<>c__0`1'::.cctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__0`1'::.ctor

    .method assembly hidebysig instance bool 
            '<SetupWithAny>b__0_0'(class [netstandard]System.Reflection.MethodInfo x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       37 (0x25)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance string [netstandard]System.Reflection.MemberInfo::get_Name()
      IL_0006:  ldstr      "Setup"
      IL_000b:  call       bool [netstandard]System.String::op_Equality(string,
                                                                        string)
      IL_0010:  brfalse.s  IL_0023

      IL_0012:  ldarg.1
      IL_0013:  callvirt   instance class [netstandard]System.Type [netstandard]System.Reflection.MethodInfo::get_ReturnType()
      IL_0018:  callvirt   instance class [netstandard]System.Type[] [netstandard]System.Type::GetGenericArguments()
      IL_001d:  ldlen
      IL_001e:  conv.i4
      IL_001f:  ldc.i4.1
      IL_0020:  ceq
      IL_0022:  ret

      IL_0023:  ldc.i4.0
      IL_0024:  ret
    } // end of method '<>c__0`1'::'<SetupWithAny>b__0_0'

  } // end of class '<>c__0`1'

  .class auto ansi serializable sealed nested private beforefieldinit '<>c__1`2'<class T,TResult>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public static initonly class Moq.AutoMock.MockExtensions/'<>c__1`2'<!T,!TResult> '<>9'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public static class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> '<>9__1_0'
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method private hidebysig specialname rtspecialname static 
            void  .cctor() cil managed
    {
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  newobj     instance void class Moq.AutoMock.MockExtensions/'<>c__1`2'<!T,!TResult>::.ctor()
      IL_0005:  stsfld     class Moq.AutoMock.MockExtensions/'<>c__1`2'<!0,!1> class Moq.AutoMock.MockExtensions/'<>c__1`2'<!T,!TResult>::'<>9'
      IL_000a:  ret
    } // end of method '<>c__1`2'::.cctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__1`2'::.ctor

    .method assembly hidebysig instance bool 
            '<SetupWithAny>b__1_0'(class [netstandard]System.Reflection.MethodInfo x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       37 (0x25)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance string [netstandard]System.Reflection.MemberInfo::get_Name()
      IL_0006:  ldstr      "Setup"
      IL_000b:  call       bool [netstandard]System.String::op_Equality(string,
                                                                        string)
      IL_0010:  brfalse.s  IL_0023

      IL_0012:  ldarg.1
      IL_0013:  callvirt   instance class [netstandard]System.Type [netstandard]System.Reflection.MethodInfo::get_ReturnType()
      IL_0018:  callvirt   instance class [netstandard]System.Type[] [netstandard]System.Type::GetGenericArguments()
      IL_001d:  ldlen
      IL_001e:  conv.i4
      IL_001f:  ldc.i4.2
      IL_0020:  ceq
      IL_0022:  ret

      IL_0023:  ldc.i4.0
      IL_0024:  ret
    } // end of method '<>c__1`2'::'<SetupWithAny>b__1_0'

  } // end of class '<>c__1`2'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass2_0`1'<T>
         extends [netstandard]System.Object
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public string methodName
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .field public class [netstandard]System.Reflection.MethodInfo isAnyMethod
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [netstandard]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass2_0`1'::.ctor

    .method assembly hidebysig instance bool 
            '<GetExpression>b__0'(class [netstandard]System.Reflection.MethodInfo x) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       19 (0x13)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance string [netstandard]System.Reflection.MemberInfo::get_Name()
      IL_0006:  ldarg.0
      IL_0007:  ldfld      string class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!T>::methodName
      IL_000c:  ldc.i4.4
      IL_000d:  call       bool [netstandard]System.String::Equals(string,
                                                                   string,
                                                                   valuetype [netstandard]System.StringComparison)
      IL_0012:  ret
    } // end of method '<>c__DisplayClass2_0`1'::'<GetExpression>b__0'

    .method assembly hidebysig instance class [netstandard]System.Linq.Expressions.MethodCallExpression 
            '<GetExpression>b__1'(class [netstandard]System.Reflection.ParameterInfo parameter) cil managed
    {
      .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
      // Code size       35 (0x23)
      .maxstack  5
      .locals init (class [netstandard]System.Reflection.MethodInfo V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [netstandard]System.Reflection.MethodInfo class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!T>::isAnyMethod
      IL_0006:  ldc.i4.1
      IL_0007:  newarr     [netstandard]System.Type
      IL_000c:  dup
      IL_000d:  ldc.i4.0
      IL_000e:  ldarg.1
      IL_000f:  callvirt   instance class [netstandard]System.Type [netstandard]System.Reflection.ParameterInfo::get_ParameterType()
      IL_0014:  stelem.ref
      IL_0015:  callvirt   instance class [netstandard]System.Reflection.MethodInfo [netstandard]System.Reflection.MethodInfo::MakeGenericMethod(class [netstandard]System.Type[])
      IL_001a:  stloc.0
      IL_001b:  ldnull
      IL_001c:  ldloc.0
      IL_001d:  call       class [netstandard]System.Linq.Expressions.MethodCallExpression [netstandard]System.Linq.Expressions.Expression::Call(class [netstandard]System.Linq.Expressions.Expression,
                                                                                                                                                 class [netstandard]System.Reflection.MethodInfo)
      IL_0022:  ret
    } // end of method '<>c__DisplayClass2_0`1'::'<GetExpression>b__1'

  } // end of class '<>c__DisplayClass2_0`1'

  .field private static initonly class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo> '<AsMethod>k__BackingField'
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 01 02 00 00 ) 
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig static class [Moq]Moq.Language.Flow.ISetup`1<!!T> 
          SetupWithAny<class T>(class [Moq]Moq.Mock`1<!!T> mock,
                                string methodName) cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       104 (0x68)
    .maxstack  6
    .locals init (class [netstandard]System.Linq.Expressions.LambdaExpression V_0)
    IL_0000:  ldarg.0
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "mock"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.1
    IL_000f:  brtrue.s   IL_001c

    IL_0011:  ldstr      "methodName"
    IL_0016:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001b:  throw

    IL_001c:  ldarg.1
    IL_001d:  call       class [netstandard]System.Linq.Expressions.LambdaExpression Moq.AutoMock.MockExtensions::GetExpression<!!0>(string)
    IL_0022:  stloc.0
    IL_0023:  ldarg.0
    IL_0024:  callvirt   instance class [netstandard]System.Type [netstandard]System.Object::GetType()
    IL_0029:  callvirt   instance class [netstandard]System.Reflection.MethodInfo[] [netstandard]System.Type::GetMethods()
    IL_002e:  ldsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> class Moq.AutoMock.MockExtensions/'<>c__0`1'<!!T>::'<>9__0_0'
    IL_0033:  dup
    IL_0034:  brtrue.s   IL_004d

    IL_0036:  pop
    IL_0037:  ldsfld     class Moq.AutoMock.MockExtensions/'<>c__0`1'<!0> class Moq.AutoMock.MockExtensions/'<>c__0`1'<!!T>::'<>9'
    IL_003c:  ldftn      instance bool class Moq.AutoMock.MockExtensions/'<>c__0`1'<!!T>::'<SetupWithAny>b__0_0'(class [netstandard]System.Reflection.MethodInfo)
    IL_0042:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool>::.ctor(object,
                                                                                                                                     native int)
    IL_0047:  dup
    IL_0048:  stsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> class Moq.AutoMock.MockExtensions/'<>c__0`1'<!!T>::'<>9__0_0'
    IL_004d:  call       !!0 [netstandard]System.Linq.Enumerable::Single<class [netstandard]System.Reflection.MethodInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                          class [netstandard]System.Func`2<!!0,bool>)
    IL_0052:  ldarg.0
    IL_0053:  ldc.i4.1
    IL_0054:  newarr     [netstandard]System.Object
    IL_0059:  dup
    IL_005a:  ldc.i4.0
    IL_005b:  ldloc.0
    IL_005c:  stelem.ref
    IL_005d:  callvirt   instance object [netstandard]System.Reflection.MethodBase::Invoke(object,
                                                                                           object[])
    IL_0062:  castclass  class [Moq]Moq.Language.Flow.ISetup`1<!!T>
    IL_0067:  ret
  } // end of method MockExtensions::SetupWithAny

  .method public hidebysig static class [Moq]Moq.Language.Flow.ISetup`2<!!T,!!TResult> 
          SetupWithAny<class T,TResult>(class [Moq]Moq.Mock`1<!!T> mock,
                                        string methodName) cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
    .param type TResult 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       128 (0x80)
    .maxstack  6
    .locals init (class [netstandard]System.Linq.Expressions.LambdaExpression V_0)
    IL_0000:  ldarg.0
    IL_0001:  brtrue.s   IL_000e

    IL_0003:  ldstr      "mock"
    IL_0008:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_000d:  throw

    IL_000e:  ldarg.1
    IL_000f:  brtrue.s   IL_001c

    IL_0011:  ldstr      "methodName"
    IL_0016:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_001b:  throw

    IL_001c:  ldarg.1
    IL_001d:  call       class [netstandard]System.Linq.Expressions.LambdaExpression Moq.AutoMock.MockExtensions::GetExpression<!!0>(string)
    IL_0022:  stloc.0
    IL_0023:  ldarg.0
    IL_0024:  callvirt   instance class [netstandard]System.Type [netstandard]System.Object::GetType()
    IL_0029:  callvirt   instance class [netstandard]System.Reflection.MethodInfo[] [netstandard]System.Type::GetMethods()
    IL_002e:  ldsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> class Moq.AutoMock.MockExtensions/'<>c__1`2'<!!T,!!TResult>::'<>9__1_0'
    IL_0033:  dup
    IL_0034:  brtrue.s   IL_004d

    IL_0036:  pop
    IL_0037:  ldsfld     class Moq.AutoMock.MockExtensions/'<>c__1`2'<!0,!1> class Moq.AutoMock.MockExtensions/'<>c__1`2'<!!T,!!TResult>::'<>9'
    IL_003c:  ldftn      instance bool class Moq.AutoMock.MockExtensions/'<>c__1`2'<!!T,!!TResult>::'<SetupWithAny>b__1_0'(class [netstandard]System.Reflection.MethodInfo)
    IL_0042:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool>::.ctor(object,
                                                                                                                                     native int)
    IL_0047:  dup
    IL_0048:  stsfld     class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool> class Moq.AutoMock.MockExtensions/'<>c__1`2'<!!T,!!TResult>::'<>9__1_0'
    IL_004d:  call       !!0 [netstandard]System.Linq.Enumerable::Single<class [netstandard]System.Reflection.MethodInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                          class [netstandard]System.Func`2<!!0,bool>)
    IL_0052:  ldc.i4.1
    IL_0053:  newarr     [netstandard]System.Type
    IL_0058:  dup
    IL_0059:  ldc.i4.0
    IL_005a:  ldtoken    !!TResult
    IL_005f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0064:  stelem.ref
    IL_0065:  callvirt   instance class [netstandard]System.Reflection.MethodInfo [netstandard]System.Reflection.MethodInfo::MakeGenericMethod(class [netstandard]System.Type[])
    IL_006a:  ldarg.0
    IL_006b:  ldc.i4.1
    IL_006c:  newarr     [netstandard]System.Object
    IL_0071:  dup
    IL_0072:  ldc.i4.0
    IL_0073:  ldloc.0
    IL_0074:  stelem.ref
    IL_0075:  callvirt   instance object [netstandard]System.Reflection.MethodBase::Invoke(object,
                                                                                           object[])
    IL_007a:  castclass  class [Moq]Moq.Language.Flow.ISetup`2<!!T,!!TResult>
    IL_007f:  ret
  } // end of method MockExtensions::SetupWithAny

  .method private hidebysig static class [netstandard]System.Linq.Expressions.LambdaExpression 
          GetExpression<T>(string methodName) cil managed
  {
    .param type T 
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       243 (0xf3)
    .maxstack  6
    .locals init (class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!!T> V_0,
             class [netstandard]System.Reflection.MethodInfo[] V_1,
             class [netstandard]System.Reflection.MethodInfo V_2,
             class [netstandard]System.Type V_3,
             class [netstandard]System.Linq.Expressions.MethodCallExpression[] V_4,
             class [netstandard]System.Linq.Expressions.ParameterExpression V_5,
             class [netstandard]System.Reflection.MethodInfo V_6,
             int32 V_7,
             class [netstandard]System.Linq.Expressions.Expression[] V_8)
    IL_0000:  newobj     instance void class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!!T>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldarg.0
    IL_0008:  stfld      string class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!!T>::methodName
    IL_000d:  ldtoken    !!T
    IL_0012:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0017:  call       instance class [netstandard]System.Reflection.MethodInfo[] [netstandard]System.Type::GetMethods()
    IL_001c:  ldloc.0
    IL_001d:  ldftn      instance bool class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!!T>::'<GetExpression>b__0'(class [netstandard]System.Reflection.MethodInfo)
    IL_0023:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.MethodInfo,bool>::.ctor(object,
                                                                                                                                     native int)
    IL_0028:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::Where<class [netstandard]System.Reflection.MethodInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                      class [netstandard]System.Func`2<!!0,bool>)
    IL_002d:  call       !!0[] [netstandard]System.Linq.Enumerable::ToArray<class [netstandard]System.Reflection.MethodInfo>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0032:  stloc.1
    IL_0033:  ldloc.1
    IL_0034:  ldlen
    IL_0035:  conv.i4
    IL_0036:  stloc.s    V_7
    IL_0038:  ldloc.s    V_7
    IL_003a:  brfalse.s  IL_0043

    IL_003c:  ldloc.s    V_7
    IL_003e:  ldc.i4.1
    IL_003f:  beq.s      IL_005e

    IL_0041:  br.s       IL_0065

    IL_0043:  ldtoken    !!T
    IL_0048:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_004d:  callvirt   instance string [netstandard]System.Reflection.MemberInfo::get_Name()
    IL_0052:  ldloc.0
    IL_0053:  ldfld      string class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!!T>::methodName
    IL_0058:  newobj     instance void [netstandard]System.MissingMethodException::.ctor(string,
                                                                                         string)
    IL_005d:  throw

    IL_005e:  ldloc.1
    IL_005f:  ldc.i4.0
    IL_0060:  ldelem.ref
    IL_0061:  stloc.s    V_6
    IL_0063:  br.s       IL_0080

    IL_0065:  ldstr      "Cannot create a Setup on method '"
    IL_006a:  ldloc.0
    IL_006b:  ldfld      string class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!!T>::methodName
    IL_0070:  ldstr      "'. SetupWithAny does not support methods with mult"
    + "iple overloads."
    IL_0075:  call       string [netstandard]System.String::Concat(string,
                                                                   string,
                                                                   string)
    IL_007a:  newobj     instance void [netstandard]System.Reflection.AmbiguousMatchException::.ctor(string)
    IL_007f:  throw

    IL_0080:  ldloc.s    V_6
    IL_0082:  stloc.2
    IL_0083:  ldtoken    [Moq]Moq.It
    IL_0088:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_008d:  stloc.3
    IL_008e:  ldloc.0
    IL_008f:  ldloc.3
    IL_0090:  ldstr      "IsAny"
    IL_0095:  callvirt   instance class [netstandard]System.Reflection.MethodInfo [netstandard]System.Type::GetMethod(string)
    IL_009a:  stfld      class [netstandard]System.Reflection.MethodInfo class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!!T>::isAnyMethod
    IL_009f:  ldloc.2
    IL_00a0:  callvirt   instance class [netstandard]System.Reflection.ParameterInfo[] [netstandard]System.Reflection.MethodBase::GetParameters()
    IL_00a5:  ldloc.0
    IL_00a6:  ldftn      instance class [netstandard]System.Linq.Expressions.MethodCallExpression class Moq.AutoMock.MockExtensions/'<>c__DisplayClass2_0`1'<!!T>::'<GetExpression>b__1'(class [netstandard]System.Reflection.ParameterInfo)
    IL_00ac:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Reflection.ParameterInfo,class [netstandard]System.Linq.Expressions.MethodCallExpression>::.ctor(object,
                                                                                                                                                                                                   native int)
    IL_00b1:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!1> [netstandard]System.Linq.Enumerable::Select<class [netstandard]System.Reflection.ParameterInfo,class [netstandard]System.Linq.Expressions.MethodCallExpression>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                                                                                          class [netstandard]System.Func`2<!!0,!!1>)
    IL_00b6:  call       !!0[] [netstandard]System.Linq.Enumerable::ToArray<class [netstandard]System.Linq.Expressions.MethodCallExpression>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_00bb:  stloc.s    V_4
    IL_00bd:  ldtoken    !!T
    IL_00c2:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_00c7:  ldstr      "x"
    IL_00cc:  call       class [netstandard]System.Linq.Expressions.ParameterExpression [netstandard]System.Linq.Expressions.Expression::Parameter(class [netstandard]System.Type,
                                                                                                                                                   string)
    IL_00d1:  stloc.s    V_5
    IL_00d3:  ldloc.s    V_5
    IL_00d5:  ldloc.2
    IL_00d6:  ldloc.s    V_4
    IL_00d8:  stloc.s    V_8
    IL_00da:  ldloc.s    V_8
    IL_00dc:  call       class [netstandard]System.Linq.Expressions.MethodCallExpression [netstandard]System.Linq.Expressions.Expression::Call(class [netstandard]System.Linq.Expressions.Expression,
                                                                                                                                               class [netstandard]System.Reflection.MethodInfo,
                                                                                                                                               class [netstandard]System.Linq.Expressions.Expression[])
    IL_00e1:  ldc.i4.0
    IL_00e2:  ldc.i4.1
    IL_00e3:  newarr     [netstandard]System.Linq.Expressions.ParameterExpression
    IL_00e8:  dup
    IL_00e9:  ldc.i4.0
    IL_00ea:  ldloc.s    V_5
    IL_00ec:  stelem.ref
    IL_00ed:  call       class [netstandard]System.Linq.Expressions.LambdaExpression [netstandard]System.Linq.Expressions.Expression::Lambda(class [netstandard]System.Linq.Expressions.Expression,
                                                                                                                                             bool,
                                                                                                                                             class [netstandard]System.Linq.Expressions.ParameterExpression[])
    IL_00f2:  ret
  } // end of method MockExtensions::GetExpression

  .method private hidebysig specialname static 
          class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo> 
          get_AsMethod() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .param [0]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 01 02 00 00 ) 
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldsfld     class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo> Moq.AutoMock.MockExtensions::'<AsMethod>k__BackingField'
    IL_0005:  ret
  } // end of method MockExtensions::get_AsMethod

  .method assembly hidebysig static class [Moq]Moq.Mock 
          As(class [Moq]Moq.Mock mock,
             class [netstandard]System.Type interfaceType) cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       83 (0x53)
    .maxstack  5
    .locals init (class [netstandard]System.Reflection.MethodInfo V_0)
    IL_0000:  call       class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo> Moq.AutoMock.MockExtensions::get_AsMethod()
    IL_0005:  callvirt   instance !0 class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo>::get_Value()
    IL_000a:  stloc.0
    IL_000b:  ldloc.0
    IL_000c:  brfalse.s  IL_002f

    IL_000e:  ldloc.0
    IL_000f:  ldc.i4.1
    IL_0010:  newarr     [netstandard]System.Type
    IL_0015:  dup
    IL_0016:  ldc.i4.0
    IL_0017:  ldarg.1
    IL_0018:  stelem.ref
    IL_0019:  callvirt   instance class [netstandard]System.Reflection.MethodInfo [netstandard]System.Reflection.MethodInfo::MakeGenericMethod(class [netstandard]System.Type[])
    IL_001e:  ldarg.0
    IL_001f:  call       !!0[] [netstandard]System.Array::Empty<object>()
    IL_0024:  callvirt   instance object [netstandard]System.Reflection.MethodBase::Invoke(object,
                                                                                           object[])
    IL_0029:  castclass  [Moq]Moq.Mock
    IL_002e:  ret

    IL_002f:  ldstr      "Could not find "
    IL_0034:  ldtoken    [Moq]Moq.Mock
    IL_0039:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_003e:  callvirt   instance string [netstandard]System.Type::get_FullName()
    IL_0043:  ldstr      ".As method"
    IL_0048:  call       string [netstandard]System.String::Concat(string,
                                                                   string,
                                                                   string)
    IL_004d:  newobj     instance void [netstandard]System.InvalidOperationException::.ctor(string)
    IL_0052:  throw
  } // end of method MockExtensions::As

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       27 (0x1b)
    .maxstack  8
    IL_0000:  ldsfld     class Moq.AutoMock.MockExtensions/'<>c' Moq.AutoMock.MockExtensions/'<>c'::'<>9'
    IL_0005:  ldftn      instance class [netstandard]System.Reflection.MethodInfo Moq.AutoMock.MockExtensions/'<>c'::'<.cctor>b__7_0'()
    IL_000b:  newobj     instance void class [netstandard]System.Func`1<class [netstandard]System.Reflection.MethodInfo>::.ctor(object,
                                                                                                                                native int)
    IL_0010:  newobj     instance void class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo>::.ctor(class [netstandard]System.Func`1<!0>)
    IL_0015:  stsfld     class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo> Moq.AutoMock.MockExtensions::'<AsMethod>k__BackingField'
    IL_001a:  ret
  } // end of method MockExtensions::.cctor

  .property class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo>
          AsMethod()
  {
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8[]) = ( 01 00 02 00 00 00 01 02 00 00 ) 
    .get class [netstandard]System.Lazy`1<class [netstandard]System.Reflection.MethodInfo> Moq.AutoMock.MockExtensions::get_AsMethod()
  } // end of property MockExtensions::AsMethod
} // end of class Moq.AutoMock.MockExtensions

.class private auto ansi sealed beforefieldinit Moq.AutoMock.MockInstance
       extends [netstandard]System.Object
       implements Moq.AutoMock.IInstance
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .interfaceimpl type Moq.AutoMock.IInstance
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .field private initonly class [Moq]Moq.Mock '<Mock>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [Moq]Moq.Mock 'value') cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [Moq]Moq.Mock Moq.AutoMock.MockInstance::'<Mock>k__BackingField'
    IL_000d:  ret
  } // end of method MockInstance::.ctor

  .method public hidebysig newslot specialname virtual final 
          instance object  get_Value() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .param [0]
    .custom instance void System.Diagnostics.CodeAnalysis.NotNullAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance class [Moq]Moq.Mock Moq.AutoMock.MockInstance::get_Mock()
    IL_0006:  callvirt   instance object [Moq]Moq.Mock::get_Object()
    IL_000b:  ret
  } // end of method MockInstance::get_Value

  .method public hidebysig specialname instance class [Moq]Moq.Mock 
          get_Mock() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [Moq]Moq.Mock Moq.AutoMock.MockInstance::'<Mock>k__BackingField'
    IL_0006:  ret
  } // end of method MockInstance::get_Mock

  .method public hidebysig newslot specialname virtual final 
          instance bool  get_IsMock() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.1
    IL_0001:  ret
  } // end of method MockInstance::get_IsMock

  .property instance object Value()
  {
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .get instance object Moq.AutoMock.MockInstance::get_Value()
  } // end of property MockInstance::Value
  .property instance class [Moq]Moq.Mock Mock()
  {
    .get instance class [Moq]Moq.Mock Moq.AutoMock.MockInstance::get_Mock()
  } // end of property MockInstance::Mock
  .property instance bool IsMock()
  {
    .get instance bool Moq.AutoMock.MockInstance::get_IsMock()
  } // end of property MockInstance::IsMock
} // end of class Moq.AutoMock.MockInstance

.class public auto ansi beforefieldinit Moq.AutoMock.ObjectGraphContext
       extends [netstandard]System.Object
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .field private initonly valuetype [netstandard]System.Reflection.BindingFlags '<BindingFlags>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private initonly class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> '<VisitedTypes>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(bool enablePrivate) cil managed
  {
    // Code size       30 (0x1e)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  call       valuetype [netstandard]System.Reflection.BindingFlags Moq.AutoMock.ObjectGraphContext::GetBindingFlags(bool)
    IL_000d:  stfld      valuetype [netstandard]System.Reflection.BindingFlags Moq.AutoMock.ObjectGraphContext::'<BindingFlags>k__BackingField'
    IL_0012:  ldarg.0
    IL_0013:  newobj     instance void class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type>::.ctor()
    IL_0018:  stfld      class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> Moq.AutoMock.ObjectGraphContext::'<VisitedTypes>k__BackingField'
    IL_001d:  ret
  } // end of method ObjectGraphContext::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Moq.AutoMock.ObjectGraphContext context) cil managed
  {
    // Code size       50 (0x32)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ldarg.1
    IL_0007:  brtrue.s   IL_0014

    IL_0009:  ldstr      "context"
    IL_000e:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_0013:  throw

    IL_0014:  ldarg.0
    IL_0015:  ldarg.1
    IL_0016:  callvirt   instance valuetype [netstandard]System.Reflection.BindingFlags Moq.AutoMock.ObjectGraphContext::get_BindingFlags()
    IL_001b:  stfld      valuetype [netstandard]System.Reflection.BindingFlags Moq.AutoMock.ObjectGraphContext::'<BindingFlags>k__BackingField'
    IL_0020:  ldarg.0
    IL_0021:  ldarg.1
    IL_0022:  callvirt   instance class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> Moq.AutoMock.ObjectGraphContext::get_VisitedTypes()
    IL_0027:  newobj     instance void class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type>::.ctor(class [netstandard]System.Collections.Generic.IEnumerable`1<!0>)
    IL_002c:  stfld      class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> Moq.AutoMock.ObjectGraphContext::'<VisitedTypes>k__BackingField'
    IL_0031:  ret
  } // end of method ObjectGraphContext::.ctor

  .method public hidebysig specialname instance valuetype [netstandard]System.Reflection.BindingFlags 
          get_BindingFlags() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype [netstandard]System.Reflection.BindingFlags Moq.AutoMock.ObjectGraphContext::'<BindingFlags>k__BackingField'
    IL_0006:  ret
  } // end of method ObjectGraphContext::get_BindingFlags

  .method public hidebysig specialname instance class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> 
          get_VisitedTypes() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> Moq.AutoMock.ObjectGraphContext::'<VisitedTypes>k__BackingField'
    IL_0006:  ret
  } // end of method ObjectGraphContext::get_VisitedTypes

  .method private hidebysig static valuetype [netstandard]System.Reflection.BindingFlags 
          GetBindingFlags(bool enablePrivate) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  2
    .locals init (valuetype [netstandard]System.Reflection.BindingFlags V_0)
    IL_0000:  ldc.i4.s   20
    IL_0002:  stloc.0
    IL_0003:  ldarg.0
    IL_0004:  brfalse.s  IL_000b

    IL_0006:  ldloc.0
    IL_0007:  ldc.i4.s   32
    IL_0009:  or
    IL_000a:  stloc.0
    IL_000b:  ldloc.0
    IL_000c:  ret
  } // end of method ObjectGraphContext::GetBindingFlags

  .property instance valuetype [netstandard]System.Reflection.BindingFlags
          BindingFlags()
  {
    .get instance valuetype [netstandard]System.Reflection.BindingFlags Moq.AutoMock.ObjectGraphContext::get_BindingFlags()
  } // end of property ObjectGraphContext::BindingFlags
  .property instance class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type>
          VisitedTypes()
  {
    .get instance class [netstandard]System.Collections.Generic.HashSet`1<class [netstandard]System.Type> Moq.AutoMock.ObjectGraphContext::get_VisitedTypes()
  } // end of property ObjectGraphContext::VisitedTypes
} // end of class Moq.AutoMock.ObjectGraphContext

.class private auto ansi sealed beforefieldinit Moq.AutoMock.RealInstance
       extends [netstandard]System.Object
       implements Moq.AutoMock.IInstance
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .interfaceimpl type Moq.AutoMock.IInstance
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .field private initonly object '<Value>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(object 'value') cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      object Moq.AutoMock.RealInstance::'<Value>k__BackingField'
    IL_000d:  ret
  } // end of method RealInstance::.ctor

  .method public hidebysig newslot specialname virtual final 
          instance object  get_Value() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      object Moq.AutoMock.RealInstance::'<Value>k__BackingField'
    IL_0006:  ret
  } // end of method RealInstance::get_Value

  .method public hidebysig newslot specialname virtual final 
          instance bool  get_IsMock() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.0
    IL_0001:  ret
  } // end of method RealInstance::get_IsMock

  .property instance object Value()
  {
    .get instance object Moq.AutoMock.RealInstance::get_Value()
  } // end of property RealInstance::Value
  .property instance bool IsMock()
  {
    .get instance bool Moq.AutoMock.RealInstance::get_IsMock()
  } // end of property RealInstance::IsMock
} // end of class Moq.AutoMock.RealInstance

.class private auto ansi beforefieldinit Moq.AutoMock.Resources.Strings
       extends [netstandard]System.Object
{
  .custom instance void [netstandard]System.CodeDom.Compiler.GeneratedCodeAttribute::.ctor(string,
                                                                                           string) = ( 01 00 32 4D 69 63 72 6F 73 6F 66 74 2E 42 75 69   // ..2Microsoft.Bui
                                                                                                       6C 64 2E 54 61 73 6B 73 2E 53 74 72 6F 6E 67 6C   // ld.Tasks.Strongl
                                                                                                       79 54 79 70 65 64 52 65 73 6F 75 72 63 65 42 75   // yTypedResourceBu
                                                                                                       69 6C 64 65 72 08 31 35 2E 31 2E 30 2E 30 00 00 ) // ilder.15.1.0.0..
  .custom instance void [netstandard]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class [netstandard]System.Resources.ResourceManager resourceMan
  .field private static class [netstandard]System.Globalization.CultureInfo resourceCulture
  .method assembly hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ret
  } // end of method Strings::.ctor

  .method assembly hidebysig specialname static 
          class [netstandard]System.Resources.ResourceManager 
          get_ResourceManager() cil managed
  {
    // Code size       43 (0x2b)
    .maxstack  8
    IL_0000:  ldsfld     class [netstandard]System.Resources.ResourceManager Moq.AutoMock.Resources.Strings::resourceMan
    IL_0005:  brtrue.s   IL_0025

    IL_0007:  ldstr      "Moq.AutoMock.Resources.Strings"
    IL_000c:  ldtoken    Moq.AutoMock.Resources.Strings
    IL_0011:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0016:  callvirt   instance class [netstandard]System.Reflection.Assembly [netstandard]System.Type::get_Assembly()
    IL_001b:  newobj     instance void [netstandard]System.Resources.ResourceManager::.ctor(string,
                                                                                            class [netstandard]System.Reflection.Assembly)
    IL_0020:  stsfld     class [netstandard]System.Resources.ResourceManager Moq.AutoMock.Resources.Strings::resourceMan
    IL_0025:  ldsfld     class [netstandard]System.Resources.ResourceManager Moq.AutoMock.Resources.Strings::resourceMan
    IL_002a:  ret
  } // end of method Strings::get_ResourceManager

  .method assembly hidebysig specialname static 
          class [netstandard]System.Globalization.CultureInfo 
          get_Culture() cil managed
  {
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldsfld     class [netstandard]System.Globalization.CultureInfo Moq.AutoMock.Resources.Strings::resourceCulture
    IL_0005:  ret
  } // end of method Strings::get_Culture

  .method assembly hidebysig specialname static 
          void  set_Culture(class [netstandard]System.Globalization.CultureInfo 'value') cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  stsfld     class [netstandard]System.Globalization.CultureInfo Moq.AutoMock.Resources.Strings::resourceCulture
    IL_0006:  ret
  } // end of method Strings::set_Culture

  .method assembly hidebysig specialname static 
          string  get_VerifyWithValueReturn() cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  call       class [netstandard]System.Resources.ResourceManager Moq.AutoMock.Resources.Strings::get_ResourceManager()
    IL_0005:  ldstr      "VerifyWithValueReturn"
    IL_000a:  ldsfld     class [netstandard]System.Globalization.CultureInfo Moq.AutoMock.Resources.Strings::resourceCulture
    IL_000f:  callvirt   instance string [netstandard]System.Resources.ResourceManager::GetString(string,
                                                                                                  class [netstandard]System.Globalization.CultureInfo)
    IL_0014:  ret
  } // end of method Strings::get_VerifyWithValueReturn

  .property class [netstandard]System.Resources.ResourceManager
          ResourceManager()
  {
    .custom instance void [netstandard]System.ComponentModel.EditorBrowsableAttribute::.ctor(valuetype [netstandard]System.ComponentModel.EditorBrowsableState) = ( 01 00 02 00 00 00 00 00 ) 
    .get class [netstandard]System.Resources.ResourceManager Moq.AutoMock.Resources.Strings::get_ResourceManager()
  } // end of property Strings::ResourceManager
  .property class [netstandard]System.Globalization.CultureInfo
          Culture()
  {
    .custom instance void [netstandard]System.ComponentModel.EditorBrowsableAttribute::.ctor(valuetype [netstandard]System.ComponentModel.EditorBrowsableState) = ( 01 00 02 00 00 00 00 00 ) 
    .get class [netstandard]System.Globalization.CultureInfo Moq.AutoMock.Resources.Strings::get_Culture()
    .set void Moq.AutoMock.Resources.Strings::set_Culture(class [netstandard]System.Globalization.CultureInfo)
  } // end of property Strings::Culture
  .property string VerifyWithValueReturn()
  {
    .get string Moq.AutoMock.Resources.Strings::get_VerifyWithValueReturn()
  } // end of property Strings::VerifyWithValueReturn
} // end of class Moq.AutoMock.Resources.Strings

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.ArrayResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       118 (0x76)
    .maxstack  4
    .locals init (class [netstandard]System.Type V_0,
             class Moq.AutoMock.MockArrayInstance V_1,
             class Moq.AutoMock.IInstance V_2)
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_0006:  callvirt   instance bool [netstandard]System.Type::get_IsArray()
    IL_000b:  brfalse.s  IL_0075

    IL_000d:  ldarg.1
    IL_000e:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_0013:  ldtoken    [netstandard]System.String
    IL_0018:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_001d:  call       bool [netstandard]System.Type::op_Inequality(class [netstandard]System.Type,
                                                                      class [netstandard]System.Type)
    IL_0022:  brfalse.s  IL_0075

    IL_0024:  ldarg.1
    IL_0025:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_002a:  callvirt   instance class [netstandard]System.Type [netstandard]System.Type::GetElementType()
    IL_002f:  dup
    IL_0030:  brtrue.s   IL_0049

    IL_0032:  pop
    IL_0033:  ldstr      "Could not determine element type for '{0}'"
    IL_0038:  ldarg.1
    IL_0039:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_003e:  call       string [netstandard]System.String::Format(string,
                                                                   object)
    IL_0043:  newobj     instance void [netstandard]System.InvalidOperationException::.ctor(string)
    IL_0048:  throw

    IL_0049:  stloc.0
    IL_004a:  ldloc.0
    IL_004b:  newobj     instance void Moq.AutoMock.MockArrayInstance::.ctor(class [netstandard]System.Type)
    IL_0050:  stloc.1
    IL_0051:  ldarg.1
    IL_0052:  callvirt   instance class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::get_AutoMocker()
    IL_0057:  ldloc.0
    IL_0058:  ldarg.1
    IL_0059:  callvirt   instance class Moq.AutoMock.ObjectGraphContext Moq.AutoMock.Resolvers.MockResolutionContext::get_ObjectGraphContext()
    IL_005e:  ldloca.s   V_2
    IL_0060:  callvirt   instance bool Moq.AutoMock.AutoMocker::TryGet(class [netstandard]System.Type,
                                                                       class Moq.AutoMock.ObjectGraphContext,
                                                                       class Moq.AutoMock.IInstance&)
    IL_0065:  brfalse.s  IL_006e

    IL_0067:  ldloc.1
    IL_0068:  ldloc.2
    IL_0069:  callvirt   instance void Moq.AutoMock.MockArrayInstance::Add(class Moq.AutoMock.IInstance)
    IL_006e:  ldarg.1
    IL_006f:  ldloc.1
    IL_0070:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0075:  ret
  } // end of method ArrayResolver::Resolve

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ret
  } // end of method ArrayResolver::.ctor

} // end of class Moq.AutoMock.Resolvers.ArrayResolver

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.AutoMockerDisposableResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       41 (0x29)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_0006:  ldtoken    Moq.AutoMock.IAutoMockerDisposable
    IL_000b:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0010:  callvirt   instance bool [netstandard]System.Type::IsAssignableFrom(class [netstandard]System.Type)
    IL_0015:  brfalse.s  IL_0028

    IL_0017:  ldarg.1
    IL_0018:  ldarg.1
    IL_0019:  callvirt   instance class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::get_AutoMocker()
    IL_001e:  newobj     instance void Moq.AutoMock.AutoMockerDisposable::.ctor(class Moq.AutoMock.AutoMocker)
    IL_0023:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0028:  ret
  } // end of method AutoMockerDisposableResolver::Resolve

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ret
  } // end of method AutoMockerDisposableResolver::.ctor

} // end of class Moq.AutoMock.Resolvers.AutoMockerDisposableResolver

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.CacheResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .interfaceimpl type Moq.AutoMock.Resolvers.IMockResolver
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .field private initonly class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> '<TypeMap>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method assembly hidebysig specialname 
          instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> 
          get_TypeMap() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.Resolvers.CacheResolver::'<TypeMap>k__BackingField'
    IL_0006:  ret
  } // end of method CacheResolver::get_TypeMap

  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    // Code size       29 (0x1d)
    .maxstack  3
    .locals init (class Moq.AutoMock.IInstance V_0)
    IL_0000:  ldarg.0
    IL_0001:  call       instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.Resolvers.CacheResolver::get_TypeMap()
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_000c:  ldloca.s   V_0
    IL_000e:  callvirt   instance bool class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::TryGetValue(!0,
                                                                                                                                                                       !1&)
    IL_0013:  brfalse.s  IL_001c

    IL_0015:  ldarg.1
    IL_0016:  ldloc.0
    IL_0017:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_001c:  ret
  } // end of method CacheResolver::Resolve

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>::.ctor()
    IL_0006:  stfld      class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.Resolvers.CacheResolver::'<TypeMap>k__BackingField'
    IL_000b:  ldarg.0
    IL_000c:  call       instance void [netstandard]System.Object::.ctor()
    IL_0011:  ret
  } // end of method CacheResolver::.ctor

  .property instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance>
          TypeMap()
  {
    .get instance class [NonBlocking]NonBlocking.ConcurrentDictionary`2<class [netstandard]System.Type,class Moq.AutoMock.IInstance> Moq.AutoMock.Resolvers.CacheResolver::get_TypeMap()
  } // end of property CacheResolver::TypeMap
} // end of class Moq.AutoMock.Resolvers.CacheResolver

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.CancellationTokenResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       105 (0x69)
    .maxstack  3
    .locals init (object V_0,
             class [netstandard]System.Threading.CancellationTokenSource V_1)
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_0006:  ldtoken    [netstandard]System.Threading.CancellationToken
    IL_000b:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0010:  call       bool [netstandard]System.Type::op_Equality(class [netstandard]System.Type,
                                                                    class [netstandard]System.Type)
    IL_0015:  brfalse.s  IL_0068

    IL_0017:  ldarg.1
    IL_0018:  callvirt   instance class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::get_AutoMocker()
    IL_001d:  callvirt   instance class [netstandard]System.Collections.Generic.IReadOnlyDictionary`2<class [netstandard]System.Type,object> Moq.AutoMock.AutoMocker::get_ResolvedObjects()
    IL_0022:  dup
    IL_0023:  brtrue.s   IL_0029

    IL_0025:  pop
    IL_0026:  ldc.i4.0
    IL_0027:  br.s       IL_003a

    IL_0029:  ldtoken    [netstandard]System.Threading.CancellationTokenSource
    IL_002e:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0033:  ldloca.s   V_0
    IL_0035:  callvirt   instance bool class [netstandard]System.Collections.Generic.IReadOnlyDictionary`2<class [netstandard]System.Type,object>::TryGetValue(!0,
                                                                                                                                                               !1&)
    IL_003a:  brfalse.s  IL_0058

    IL_003c:  ldloc.0
    IL_003d:  isinst     [netstandard]System.Threading.CancellationTokenSource
    IL_0042:  stloc.1
    IL_0043:  ldloc.1
    IL_0044:  brfalse.s  IL_0058

    IL_0046:  ldarg.1
    IL_0047:  ldloc.1
    IL_0048:  callvirt   instance valuetype [netstandard]System.Threading.CancellationToken [netstandard]System.Threading.CancellationTokenSource::get_Token()
    IL_004d:  box        [netstandard]System.Threading.CancellationToken
    IL_0052:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0057:  ret

    IL_0058:  ldarg.1
    IL_0059:  call       valuetype [netstandard]System.Threading.CancellationToken [netstandard]System.Threading.CancellationToken::get_None()
    IL_005e:  box        [netstandard]System.Threading.CancellationToken
    IL_0063:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0068:  ret
  } // end of method CancellationTokenResolver::Resolve

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ret
  } // end of method CancellationTokenResolver::.ctor

} // end of class Moq.AutoMock.Resolvers.CancellationTokenResolver

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.EnumerableResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       97 (0x61)
    .maxstack  4
    .locals init (class Moq.AutoMock.AutoMocker V_0,
             class [netstandard]System.Type V_1,
             class [netstandard]System.Type V_2,
             class [netstandard]System.Array V_3,
             class Moq.AutoMock.AutoMocker V_4,
             class [netstandard]System.Type V_5,
             object V_6)
    IL_0000:  ldarg.1
    IL_0001:  ldloca.s   V_4
    IL_0003:  ldloca.s   V_5
    IL_0005:  ldloca.s   V_6
    IL_0007:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::Deconstruct(class Moq.AutoMock.AutoMocker&,
                                                                                                 class [netstandard]System.Type&,
                                                                                                 object&)
    IL_000c:  ldloc.s    V_4
    IL_000e:  stloc.0
    IL_000f:  ldloc.s    V_5
    IL_0011:  stloc.1
    IL_0012:  ldloc.1
    IL_0013:  call       class [netstandard]System.Reflection.TypeInfo [netstandard]System.Reflection.IntrospectionExtensions::GetTypeInfo(class [netstandard]System.Type)
    IL_0018:  callvirt   instance bool [netstandard]System.Type::get_IsGenericType()
    IL_001d:  brfalse.s  IL_0036

    IL_001f:  ldloc.1
    IL_0020:  callvirt   instance class [netstandard]System.Type [netstandard]System.Type::GetGenericTypeDefinition()
    IL_0025:  ldtoken    [netstandard]System.Collections.Generic.IEnumerable`1
    IL_002a:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002f:  call       bool [netstandard]System.Type::op_Inequality(class [netstandard]System.Type,
                                                                      class [netstandard]System.Type)
    IL_0034:  brfalse.s  IL_0037

    IL_0036:  ret

    IL_0037:  ldloc.1
    IL_0038:  callvirt   instance class [netstandard]System.Type[] [netstandard]System.Type::GetGenericArguments()
    IL_003d:  call       !!0 [netstandard]System.Linq.Enumerable::Single<class [netstandard]System.Type>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0042:  stloc.2
    IL_0043:  ldloc.2
    IL_0044:  ldc.i4.1
    IL_0045:  call       class [netstandard]System.Array [netstandard]System.Array::CreateInstance(class [netstandard]System.Type,
                                                                                                   int32)
    IL_004a:  stloc.3
    IL_004b:  ldloc.3
    IL_004c:  ldloc.0
    IL_004d:  ldloc.2
    IL_004e:  callvirt   instance object Moq.AutoMock.AutoMocker::Get(class [netstandard]System.Type)
    IL_0053:  ldc.i4.0
    IL_0054:  callvirt   instance void [netstandard]System.Array::SetValue(object,
                                                                           int32)
    IL_0059:  ldarg.1
    IL_005a:  ldloc.3
    IL_005b:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0060:  ret
  } // end of method EnumerableResolver::Resolve

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ret
  } // end of method EnumerableResolver::.ctor

} // end of class Moq.AutoMock.Resolvers.EnumerableResolver

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.FuncResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       33 (0x21)
    .maxstack  4
    .locals init (class [netstandard]System.Type V_0,
             class [netstandard]System.Delegate V_1,
             class Moq.AutoMock.AutoMocker V_2,
             class [netstandard]System.Type V_3,
             object V_4)
    IL_0000:  ldarg.1
    IL_0001:  ldloca.s   V_2
    IL_0003:  ldloca.s   V_3
    IL_0005:  ldloca.s   V_4
    IL_0007:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::Deconstruct(class Moq.AutoMock.AutoMocker&,
                                                                                                 class [netstandard]System.Type&,
                                                                                                 object&)
    IL_000c:  ldloc.2
    IL_000d:  ldloc.3
    IL_000e:  stloc.0
    IL_000f:  ldloc.0
    IL_0010:  ldloca.s   V_1
    IL_0012:  call       bool Moq.AutoMock.Extensions.TypeExtensions::TryCompileGetter(class Moq.AutoMock.AutoMocker,
                                                                                       class [netstandard]System.Type,
                                                                                       class [netstandard]System.Delegate&)
    IL_0017:  brfalse.s  IL_0020

    IL_0019:  ldarg.1
    IL_001a:  ldloc.1
    IL_001b:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0020:  ret
  } // end of method FuncResolver::Resolve

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ret
  } // end of method FuncResolver::.ctor

} // end of class Moq.AutoMock.Resolvers.FuncResolver

.class interface public abstract auto ansi Moq.AutoMock.Resolvers.IMockResolver
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .method public hidebysig newslot abstract virtual 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
  } // end of method IMockResolver::Resolve

} // end of class Moq.AutoMock.Resolvers.IMockResolver

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.LazyResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       153 (0x99)
    .maxstack  6
    .locals init (class Moq.AutoMock.AutoMocker V_0,
             class [netstandard]System.Type V_1,
             class [netstandard]System.Type V_2,
             class [netstandard]System.Delegate V_3,
             class Moq.AutoMock.AutoMocker V_4,
             class [netstandard]System.Type V_5,
             object V_6,
             class [netstandard]System.Type V_7)
    IL_0000:  ldarg.1
    IL_0001:  ldloca.s   V_4
    IL_0003:  ldloca.s   V_5
    IL_0005:  ldloca.s   V_6
    IL_0007:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::Deconstruct(class Moq.AutoMock.AutoMocker&,
                                                                                                 class [netstandard]System.Type&,
                                                                                                 object&)
    IL_000c:  ldloc.s    V_4
    IL_000e:  stloc.0
    IL_000f:  ldloc.s    V_5
    IL_0011:  stloc.1
    IL_0012:  ldloc.1
    IL_0013:  call       class [netstandard]System.Reflection.TypeInfo [netstandard]System.Reflection.IntrospectionExtensions::GetTypeInfo(class [netstandard]System.Type)
    IL_0018:  callvirt   instance bool [netstandard]System.Type::get_IsGenericType()
    IL_001d:  brfalse.s  IL_0036

    IL_001f:  ldloc.1
    IL_0020:  callvirt   instance class [netstandard]System.Type [netstandard]System.Type::GetGenericTypeDefinition()
    IL_0025:  ldtoken    [netstandard]System.Lazy`1
    IL_002a:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_002f:  call       bool [netstandard]System.Type::op_Inequality(class [netstandard]System.Type,
                                                                      class [netstandard]System.Type)
    IL_0034:  brfalse.s  IL_0037

    IL_0036:  ret

    IL_0037:  ldloc.1
    IL_0038:  callvirt   instance class [netstandard]System.Type[] [netstandard]System.Type::GetGenericArguments()
    IL_003d:  call       !!0 [netstandard]System.Linq.Enumerable::Single<class [netstandard]System.Type>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0042:  stloc.2
    IL_0043:  ldloc.0
    IL_0044:  ldtoken    [netstandard]System.Func`1
    IL_0049:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_004e:  ldc.i4.1
    IL_004f:  newarr     [netstandard]System.Type
    IL_0054:  dup
    IL_0055:  ldc.i4.0
    IL_0056:  ldloc.2
    IL_0057:  stelem.ref
    IL_0058:  callvirt   instance class [netstandard]System.Type [netstandard]System.Type::MakeGenericType(class [netstandard]System.Type[])
    IL_005d:  ldloca.s   V_3
    IL_005f:  call       bool Moq.AutoMock.Extensions.TypeExtensions::TryCompileGetter(class Moq.AutoMock.AutoMocker,
                                                                                       class [netstandard]System.Type,
                                                                                       class [netstandard]System.Delegate&)
    IL_0064:  brfalse.s  IL_0098

    IL_0066:  ldtoken    [netstandard]System.Lazy`1
    IL_006b:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0070:  ldc.i4.1
    IL_0071:  newarr     [netstandard]System.Type
    IL_0076:  dup
    IL_0077:  ldc.i4.0
    IL_0078:  ldloc.2
    IL_0079:  stelem.ref
    IL_007a:  callvirt   instance class [netstandard]System.Type [netstandard]System.Type::MakeGenericType(class [netstandard]System.Type[])
    IL_007f:  stloc.s    V_7
    IL_0081:  ldarg.1
    IL_0082:  ldloc.s    V_7
    IL_0084:  ldc.i4.1
    IL_0085:  newarr     [netstandard]System.Object
    IL_008a:  dup
    IL_008b:  ldc.i4.0
    IL_008c:  ldloc.3
    IL_008d:  stelem.ref
    IL_008e:  call       object [netstandard]System.Activator::CreateInstance(class [netstandard]System.Type,
                                                                              object[])
    IL_0093:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0098:  ret
  } // end of method LazyResolver::Resolve

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ret
  } // end of method LazyResolver::.ctor

} // end of class Moq.AutoMock.Resolvers.LazyResolver

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.MockResolutionContext
       extends [netstandard]System.Object
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .field private object _value
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
  .field private initonly class Moq.AutoMock.AutoMocker '<AutoMocker>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private initonly class [netstandard]System.Type '<RequestType>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private initonly class Moq.AutoMock.ObjectGraphContext '<ObjectGraphContext>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private bool '<ValueProvided>k__BackingField'
  .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Moq.AutoMock.AutoMocker autoMocker,
                               class [netstandard]System.Type requestType,
                               class Moq.AutoMock.ObjectGraphContext objectGraphContext) cil managed
  {
    // Code size       73 (0x49)
    .maxstack  3
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  dup
    IL_0009:  brtrue.s   IL_0017

    IL_000b:  pop
    IL_000c:  ldstr      "autoMocker"
    IL_0011:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_0016:  throw

    IL_0017:  stfld      class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::'<AutoMocker>k__BackingField'
    IL_001c:  ldarg.0
    IL_001d:  ldarg.2
    IL_001e:  dup
    IL_001f:  brtrue.s   IL_002d

    IL_0021:  pop
    IL_0022:  ldstr      "requestType"
    IL_0027:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_002c:  throw

    IL_002d:  stfld      class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::'<RequestType>k__BackingField'
    IL_0032:  ldarg.0
    IL_0033:  ldarg.3
    IL_0034:  dup
    IL_0035:  brtrue.s   IL_0043

    IL_0037:  pop
    IL_0038:  ldstr      "objectGraphContext"
    IL_003d:  newobj     instance void [netstandard]System.ArgumentNullException::.ctor(string)
    IL_0042:  throw

    IL_0043:  stfld      class Moq.AutoMock.ObjectGraphContext Moq.AutoMock.Resolvers.MockResolutionContext::'<ObjectGraphContext>k__BackingField'
    IL_0048:  ret
  } // end of method MockResolutionContext::.ctor

  .method public hidebysig specialname instance class Moq.AutoMock.AutoMocker 
          get_AutoMocker() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::'<AutoMocker>k__BackingField'
    IL_0006:  ret
  } // end of method MockResolutionContext::get_AutoMocker

  .method public hidebysig specialname instance class [netstandard]System.Type 
          get_RequestType() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::'<RequestType>k__BackingField'
    IL_0006:  ret
  } // end of method MockResolutionContext::get_RequestType

  .method public hidebysig specialname instance object 
          get_Value() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      object Moq.AutoMock.Resolvers.MockResolutionContext::_value
    IL_0006:  ret
  } // end of method MockResolutionContext::get_Value

  .method public hidebysig specialname instance void 
          set_Value(object 'value') cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      object Moq.AutoMock.Resolvers.MockResolutionContext::_value
    IL_0007:  ldarg.0
    IL_0008:  ldc.i4.1
    IL_0009:  call       instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_ValueProvided(bool)
    IL_000e:  ret
  } // end of method MockResolutionContext::set_Value

  .method public hidebysig specialname instance class Moq.AutoMock.ObjectGraphContext 
          get_ObjectGraphContext() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Moq.AutoMock.ObjectGraphContext Moq.AutoMock.Resolvers.MockResolutionContext::'<ObjectGraphContext>k__BackingField'
    IL_0006:  ret
  } // end of method MockResolutionContext::get_ObjectGraphContext

  .method assembly hidebysig specialname 
          instance bool  get_ValueProvided() cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Moq.AutoMock.Resolvers.MockResolutionContext::'<ValueProvided>k__BackingField'
    IL_0006:  ret
  } // end of method MockResolutionContext::get_ValueProvided

  .method private hidebysig specialname instance void 
          set_ValueProvided(bool 'value') cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      bool Moq.AutoMock.Resolvers.MockResolutionContext::'<ValueProvided>k__BackingField'
    IL_0007:  ret
  } // end of method MockResolutionContext::set_ValueProvided

  .method public hidebysig instance void 
          Deconstruct([out] class Moq.AutoMock.AutoMocker& autoMocker,
                      [out] class [netstandard]System.Type& 'type',
                      [out] object& 'value') cil managed
  {
    .param [3]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  ldarg.0
    IL_0002:  call       instance class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::get_AutoMocker()
    IL_0007:  stind.ref
    IL_0008:  ldarg.2
    IL_0009:  ldarg.0
    IL_000a:  call       instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_000f:  stind.ref
    IL_0010:  ldarg.3
    IL_0011:  ldarg.0
    IL_0012:  call       instance object Moq.AutoMock.Resolvers.MockResolutionContext::get_Value()
    IL_0017:  stind.ref
    IL_0018:  ret
  } // end of method MockResolutionContext::Deconstruct

  .property instance class Moq.AutoMock.AutoMocker
          AutoMocker()
  {
    .get instance class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::get_AutoMocker()
  } // end of property MockResolutionContext::AutoMocker
  .property instance class [netstandard]System.Type
          RequestType()
  {
    .get instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
  } // end of property MockResolutionContext::RequestType
  .property instance object Value()
  {
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .get instance object Moq.AutoMock.Resolvers.MockResolutionContext::get_Value()
    .set instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
  } // end of property MockResolutionContext::Value
  .property instance class Moq.AutoMock.ObjectGraphContext
          ObjectGraphContext()
  {
    .get instance class Moq.AutoMock.ObjectGraphContext Moq.AutoMock.Resolvers.MockResolutionContext::get_ObjectGraphContext()
  } // end of property MockResolutionContext::ObjectGraphContext
  .property instance bool ValueProvided()
  {
    .get instance bool Moq.AutoMock.Resolvers.MockResolutionContext::get_ValueProvided()
    .set instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_ValueProvided(bool)
  } // end of property MockResolutionContext::ValueProvided
} // end of class Moq.AutoMock.Resolvers.MockResolutionContext

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.MockResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .field private initonly valuetype [Moq]Moq.MockBehavior _mockBehavior
  .field private initonly valuetype [Moq]Moq.DefaultValue _defaultValue
  .field private initonly class [Moq]Moq.DefaultValueProvider _defaultValueProvider
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
  .field private initonly bool _callBase
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype [Moq]Moq.MockBehavior mockBehavior,
                               valuetype [Moq]Moq.DefaultValue defaultValue,
                               bool callBase) cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldnull
    IL_0004:  ldarg.3
    IL_0005:  call       instance void Moq.AutoMock.Resolvers.MockResolver::.ctor(valuetype [Moq]Moq.MockBehavior,
                                                                                  valuetype [Moq]Moq.DefaultValue,
                                                                                  class [Moq]Moq.DefaultValueProvider,
                                                                                  bool)
    IL_000a:  ret
  } // end of method MockResolver::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype [Moq]Moq.MockBehavior mockBehavior,
                               valuetype [Moq]Moq.DefaultValue defaultValue,
                               class [Moq]Moq.DefaultValueProvider defaultValueProvider,
                               bool callBase) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    // Code size       36 (0x24)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      valuetype [Moq]Moq.MockBehavior Moq.AutoMock.Resolvers.MockResolver::_mockBehavior
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      valuetype [Moq]Moq.DefaultValue Moq.AutoMock.Resolvers.MockResolver::_defaultValue
    IL_0014:  ldarg.0
    IL_0015:  ldarg.3
    IL_0016:  stfld      class [Moq]Moq.DefaultValueProvider Moq.AutoMock.Resolvers.MockResolver::_defaultValueProvider
    IL_001b:  ldarg.0
    IL_001c:  ldarg.s    callBase
    IL_001e:  stfld      bool Moq.AutoMock.Resolvers.MockResolver::_callBase
    IL_0023:  ret
  } // end of method MockResolver::.ctor

  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       83 (0x53)
    .maxstack  7
    .locals init (class [Moq]Moq.Mock V_0)
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_0006:  ldtoken    [netstandard]System.String
    IL_000b:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0010:  call       bool [netstandard]System.Type::op_Equality(class [netstandard]System.Type,
                                                                    class [netstandard]System.Type)
    IL_0015:  brfalse.s  IL_0018

    IL_0017:  ret

    IL_0018:  ldarg.1
    IL_0019:  callvirt   instance class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::get_AutoMocker()
    IL_001e:  ldarg.1
    IL_001f:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_0024:  ldarg.0
    IL_0025:  ldfld      valuetype [Moq]Moq.MockBehavior Moq.AutoMock.Resolvers.MockResolver::_mockBehavior
    IL_002a:  ldarg.0
    IL_002b:  ldfld      valuetype [Moq]Moq.DefaultValue Moq.AutoMock.Resolvers.MockResolver::_defaultValue
    IL_0030:  ldarg.0
    IL_0031:  ldfld      class [Moq]Moq.DefaultValueProvider Moq.AutoMock.Resolvers.MockResolver::_defaultValueProvider
    IL_0036:  ldarg.0
    IL_0037:  ldfld      bool Moq.AutoMock.Resolvers.MockResolver::_callBase
    IL_003c:  ldarg.1
    IL_003d:  callvirt   instance class Moq.AutoMock.ObjectGraphContext Moq.AutoMock.Resolvers.MockResolutionContext::get_ObjectGraphContext()
    IL_0042:  callvirt   instance class [Moq]Moq.Mock Moq.AutoMock.AutoMocker::CreateMock(class [netstandard]System.Type,
                                                                                          valuetype [Moq]Moq.MockBehavior,
                                                                                          valuetype [Moq]Moq.DefaultValue,
                                                                                          class [Moq]Moq.DefaultValueProvider,
                                                                                          bool,
                                                                                          class Moq.AutoMock.ObjectGraphContext)
    IL_0047:  stloc.0
    IL_0048:  ldloc.0
    IL_0049:  brfalse.s  IL_0052

    IL_004b:  ldarg.1
    IL_004c:  ldloc.0
    IL_004d:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0052:  ret
  } // end of method MockResolver::Resolve

} // end of class Moq.AutoMock.Resolvers.MockResolver

.class public auto ansi beforefieldinit Moq.AutoMock.Resolvers.SelfResolver
       extends [netstandard]System.Object
       implements Moq.AutoMock.Resolvers.IMockResolver
{
  .method public hidebysig newslot virtual final 
          instance void  Resolve(class Moq.AutoMock.Resolvers.MockResolutionContext context) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
    // Code size       36 (0x24)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance class [netstandard]System.Type Moq.AutoMock.Resolvers.MockResolutionContext::get_RequestType()
    IL_0006:  ldtoken    Moq.AutoMock.AutoMocker
    IL_000b:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0010:  call       bool [netstandard]System.Type::op_Equality(class [netstandard]System.Type,
                                                                    class [netstandard]System.Type)
    IL_0015:  brfalse.s  IL_0023

    IL_0017:  ldarg.1
    IL_0018:  ldarg.1
    IL_0019:  callvirt   instance class Moq.AutoMock.AutoMocker Moq.AutoMock.Resolvers.MockResolutionContext::get_AutoMocker()
    IL_001e:  callvirt   instance void Moq.AutoMock.Resolvers.MockResolutionContext::set_Value(object)
    IL_0023:  ret
  } // end of method SelfResolver::Resolve

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [netstandard]System.Object::.ctor()
    IL_0006:  ret
  } // end of method SelfResolver::.ctor

} // end of class Moq.AutoMock.Resolvers.SelfResolver

.class private abstract auto ansi sealed beforefieldinit Moq.AutoMock.Extensions.TypeExtensions
       extends [netstandard]System.Object
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig static bool  TryCompileGetter(class Moq.AutoMock.AutoMocker autoMocker,
                                                         class [netstandard]System.Type funcType,
                                                         [out] class [netstandard]System.Delegate& delegate) cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
    .param [3]
    .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 02 00 00 ) 
    .custom instance void System.Diagnostics.CodeAnalysis.NotNullWhenAttribute::.ctor(bool) = ( 01 00 01 00 00 ) 
    // Code size       246 (0xf6)
    .maxstack  8
    .locals init (class [netstandard]System.Reflection.TypeInfo V_0,
             class [netstandard]System.Type V_1,
             class [netstandard]System.Type[] V_2,
             class [netstandard]System.Collections.Generic.IEnumerable`1<class [netstandard]System.Linq.Expressions.ParameterExpression> V_3,
             class [netstandard]System.Type V_4,
             class [netstandard]System.Linq.Expressions.Expression V_5,
             class [netstandard]System.Reflection.TypeInfo V_6)
    IL_0000:  ldarg.2
    IL_0001:  ldnull
    IL_0002:  stind.ref
    IL_0003:  ldarg.1
    IL_0004:  call       class [netstandard]System.Reflection.TypeInfo [netstandard]System.Reflection.IntrospectionExtensions::GetTypeInfo(class [netstandard]System.Type)
    IL_0009:  stloc.0
    IL_000a:  ldtoken    [netstandard]System.Delegate
    IL_000f:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0014:  ldarg.1
    IL_0015:  callvirt   instance bool [netstandard]System.Type::IsAssignableFrom(class [netstandard]System.Type)
    IL_001a:  brfalse.s  IL_0052

    IL_001c:  ldloc.0
    IL_001d:  callvirt   instance bool [netstandard]System.Type::get_IsGenericType()
    IL_0022:  brfalse.s  IL_0052

    IL_0024:  ldarg.1
    IL_0025:  callvirt   instance class [netstandard]System.Type [netstandard]System.Type::GetGenericTypeDefinition()
    IL_002a:  stloc.1
    IL_002b:  ldloc.1
    IL_002c:  brfalse.s  IL_0052

    IL_002e:  ldloc.1
    IL_002f:  callvirt   instance string [netstandard]System.Type::get_Namespace()
    IL_0034:  ldstr      "System"
    IL_0039:  call       bool [netstandard]System.String::op_Inequality(string,
                                                                        string)
    IL_003e:  brtrue.s   IL_0052

    IL_0040:  ldloc.1
    IL_0041:  callvirt   instance string [netstandard]System.Reflection.MemberInfo::get_Name()
    IL_0046:  ldstr      "^Func\\b"
    IL_004b:  call       bool [netstandard]System.Text.RegularExpressions.Regex::IsMatch(string,
                                                                                         string)
    IL_0050:  brtrue.s   IL_0054

    IL_0052:  ldc.i4.0
    IL_0053:  ret

    IL_0054:  ldarg.1
    IL_0055:  callvirt   instance class [netstandard]System.Type[] [netstandard]System.Type::GetGenericArguments()
    IL_005a:  stloc.2
    IL_005b:  ldloc.2
    IL_005c:  ldloc.2
    IL_005d:  ldlen
    IL_005e:  conv.i4
    IL_005f:  ldc.i4.1
    IL_0060:  sub
    IL_0061:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!0> [netstandard]System.Linq.Enumerable::Take<class [netstandard]System.Type>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                    int32)
    IL_0066:  ldnull
    IL_0067:  ldftn      class [netstandard]System.Linq.Expressions.ParameterExpression [netstandard]System.Linq.Expressions.Expression::Parameter(class [netstandard]System.Type)
    IL_006d:  newobj     instance void class [netstandard]System.Func`2<class [netstandard]System.Type,class [netstandard]System.Linq.Expressions.ParameterExpression>::.ctor(object,
                                                                                                                                                                              native int)
    IL_0072:  call       class [netstandard]System.Collections.Generic.IEnumerable`1<!!1> [netstandard]System.Linq.Enumerable::Select<class [netstandard]System.Type,class [netstandard]System.Linq.Expressions.ParameterExpression>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                                                                     class [netstandard]System.Func`2<!!0,!!1>)
    IL_0077:  stloc.3
    IL_0078:  ldloc.2
    IL_0079:  call       !!0 [netstandard]System.Linq.Enumerable::Last<class [netstandard]System.Type>(class [netstandard]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_007e:  stloc.s    V_4
    IL_0080:  ldarg.0
    IL_0081:  call       class [netstandard]System.Linq.Expressions.ConstantExpression [netstandard]System.Linq.Expressions.Expression::Constant(object)
    IL_0086:  ldstr      "Get"
    IL_008b:  ldnull
    IL_008c:  ldc.i4.1
    IL_008d:  newarr     [netstandard]System.Linq.Expressions.Expression
    IL_0092:  dup
    IL_0093:  ldc.i4.0
    IL_0094:  ldloc.s    V_4
    IL_0096:  ldtoken    [netstandard]System.Type
    IL_009b:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_00a0:  call       class [netstandard]System.Linq.Expressions.ConstantExpression [netstandard]System.Linq.Expressions.Expression::Constant(object,
                                                                                                                                                 class [netstandard]System.Type)
    IL_00a5:  stelem.ref
    IL_00a6:  call       class [netstandard]System.Linq.Expressions.MethodCallExpression [netstandard]System.Linq.Expressions.Expression::Call(class [netstandard]System.Linq.Expressions.Expression,
                                                                                                                                               string,
                                                                                                                                               class [netstandard]System.Type[],
                                                                                                                                               class [netstandard]System.Linq.Expressions.Expression[])
    IL_00ab:  stloc.s    V_5
    IL_00ad:  ldloc.s    V_4
    IL_00af:  call       class [netstandard]System.Reflection.TypeInfo [netstandard]System.Reflection.IntrospectionExtensions::GetTypeInfo(class [netstandard]System.Type)
    IL_00b4:  stloc.s    V_6
    IL_00b6:  ldloc.s    V_6
    IL_00b8:  callvirt   instance bool [netstandard]System.Type::get_IsValueType()
    IL_00bd:  brtrue.s   IL_00c8

    IL_00bf:  ldloc.s    V_6
    IL_00c1:  callvirt   instance bool [netstandard]System.Type::get_IsPrimitive()
    IL_00c6:  brfalse.s  IL_00d5

    IL_00c8:  ldloc.s    V_5
    IL_00ca:  ldloc.s    V_4
    IL_00cc:  call       class [netstandard]System.Linq.Expressions.UnaryExpression [netstandard]System.Linq.Expressions.Expression::Unbox(class [netstandard]System.Linq.Expressions.Expression,
                                                                                                                                           class [netstandard]System.Type)
    IL_00d1:  stloc.s    V_5
    IL_00d3:  br.s       IL_00e0

    IL_00d5:  ldloc.s    V_5
    IL_00d7:  ldloc.s    V_4
    IL_00d9:  call       class [netstandard]System.Linq.Expressions.UnaryExpression [netstandard]System.Linq.Expressions.Expression::TypeAs(class [netstandard]System.Linq.Expressions.Expression,
                                                                                                                                            class [netstandard]System.Type)
    IL_00de:  stloc.s    V_5
    IL_00e0:  ldarg.2
    IL_00e1:  ldarg.1
    IL_00e2:  ldloc.s    V_5
    IL_00e4:  ldloc.3
    IL_00e5:  call       class [netstandard]System.Linq.Expressions.LambdaExpression [netstandard]System.Linq.Expressions.Expression::Lambda(class [netstandard]System.Type,
                                                                                                                                             class [netstandard]System.Linq.Expressions.Expression,
                                                                                                                                             class [netstandard]System.Collections.Generic.IEnumerable`1<class [netstandard]System.Linq.Expressions.ParameterExpression>)
    IL_00ea:  callvirt   instance class [netstandard]System.Delegate [netstandard]System.Linq.Expressions.LambdaExpression::Compile()
    IL_00ef:  stind.ref
    IL_00f0:  ldarg.2
    IL_00f1:  ldind.ref
    IL_00f2:  ldnull
    IL_00f3:  cgt.un
    IL_00f5:  ret
  } // end of method TypeExtensions::TryCompileGetter

  .method public hidebysig static bool  IsDelegateType(class [netstandard]System.Type 'type') cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       22 (0x16)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance class [netstandard]System.Type [netstandard]System.Type::get_BaseType()
    IL_0006:  ldtoken    [netstandard]System.MulticastDelegate
    IL_000b:  call       class [netstandard]System.Type [netstandard]System.Type::GetTypeFromHandle(valuetype [netstandard]System.RuntimeTypeHandle)
    IL_0010:  call       bool [netstandard]System.Type::op_Equality(class [netstandard]System.Type,
                                                                    class [netstandard]System.Type)
    IL_0015:  ret
  } // end of method TypeExtensions::IsDelegateType

  .method public hidebysig static bool  IsMockable(class [netstandard]System.Type 'type') cil managed
  {
    .custom instance void [netstandard]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance bool [netstandard]System.Type::get_IsSealed()
    IL_0006:  brtrue.s   IL_0010

    IL_0008:  ldarg.0
    IL_0009:  callvirt   instance bool [netstandard]System.Type::get_IsArray()
    IL_000e:  brfalse.s  IL_0017

    IL_0010:  ldarg.0
    IL_0011:  call       bool Moq.AutoMock.Extensions.TypeExtensions::IsDelegateType(class [netstandard]System.Type)
    IL_0016:  ret

    IL_0017:  ldc.i4.1
    IL_0018:  ret
  } // end of method TypeExtensions::IsMockable

} // end of class Moq.AutoMock.Extensions.TypeExtensions


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file Moq.AutoMock.res
