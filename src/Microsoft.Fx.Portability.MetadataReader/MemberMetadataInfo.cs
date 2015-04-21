// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class MemberMetadataInfo
    {
        public MemberMetadataInfo(string name)
        {
            Name = name;
            Kind = MemberKind.Type;
            Names = new List<string>();
        }

        public List<string> Names { get; set; }

        public MethodSignature<MemberMetadataInfo> MethodSignature { get; set; }

        public ModuleReferenceHandle Module { get; set; }

        public List<MemberMetadataInfo> GenericTypeArgs { get; set; }

        public bool IsArrayType { get; set; }

        public string ArrayTypeInfo { get; set; }

        public bool IsTypeDef { get; set; }

        public bool IsPrimitiveType { get; set; }

        public MemberKind Kind { get; set; }

        public bool IsGenericInstance { get; set; }

        public bool IsEnclosedType { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public MemberMetadataInfo ParentType { get; set; }

        public AssemblyReference DefinedInAssembly { get; set; }

        public bool IsAssemblySet { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Kind == MemberKind.Method || Kind == MemberKind.Field)
            {
                //get the full name from the type
                sb.Append(ParentType.ToString());
                if (ParentType.IsArrayType)
                {
                    sb.Append(ParentType.ArrayTypeInfo);
                }
                sb.Append(".");

                string name = Name.Replace("<", "{").Replace(">", "}");
                if (Kind == MemberKind.Method)
                {
                    sb.Append(name.Replace(".", "#"));  // Expected output is '#ctor' instead of '.ctor'

                    if (MethodSignature.GenericParameterCount > 0)
                    {
                        sb.Append($"``{MethodSignature.GenericParameterCount}");
                    }
                }
                else
                {
                    sb.Append(name);
                }

                //add method signature 
                if (Kind == MemberKind.Method)
                {
                    if (MethodSignature.ParameterTypes.Count() > 0)
                    {
                        sb.Append("(");
                        MethodSignature.ParameterTypes[0].IsEnclosedType = true;
                        sb.Append(MethodSignature.ParameterTypes[0].ToString());

                        for (int i = 1; i < MethodSignature.ParameterTypes.Count(); i++)
                        {
                            sb.Append(",");
                            MethodSignature.ParameterTypes[i].IsEnclosedType = true;
                            sb.Append(MethodSignature.ParameterTypes[i].ToString());
                        }
                        sb.Append(")");
                    }
                }
            }
            else
            {
                if (Namespace != null)
                {
                    sb.Append(Namespace);
                    sb.Append(".");
                }

                List<string> displayNames = new List<string>(Names);
                displayNames.Add(Name);

                //add the type arguments for generic instances
                //example output: Hashtable{`0}.KeyValuePair
                //Go through all type names, if it is generic such as Hashtable`1 remove the '1 , look in the arguments list 
                //and put the list of arguments in between {}
                if (IsGenericInstance && GenericTypeArgs != null && IsEnclosedType)
                {
                    int index = 0;
                    for (int i = 0; i < displayNames.Count; i++)
                    {
                        int pos = displayNames[i].IndexOf('`');
                        if (pos > 0)
                        {
                            int numArgs;
                            bool success = Int32.TryParse(displayNames[i].Substring(pos + 1, 1), out numArgs);

                            if (success)
                            {
                                string substr1 = displayNames[i].Substring(0, pos);
                                string substr2 = "";
                                if (displayNames[i].Length > pos + 2)
                                    substr2 = displayNames[i].Substring(pos + 2);

                                displayNames[i] = substr1;
                                if (index + numArgs <= GenericTypeArgs.Count)
                                {
                                    List<MemberMetadataInfo> args = GenericTypeArgs.GetRange(index, numArgs);
                                    string argsList = "{" + String.Join(",", args) + "}";
                                    displayNames[i] += argsList;
                                }
                                else
                                {
                                    Console.WriteLine("error");
                                }
                                displayNames[i] += substr2;

                                //advance the index in the args list
                                index += numArgs;
                            }
                        }
                    }
                }


                for (int i = 0; i < displayNames.Count; i++)
                {
                    if (i > 0)
                        sb.Append(".");
                    sb.Append(displayNames[i]);
                }
            }
            return sb.ToString();
        }

        public void Join(MemberMetadataInfo info2)
        {
            Names.AddRange(info2.Names);
            Names.Add(info2.Name);
            if (info2.Namespace != null)
                Namespace = info2.Namespace;

            if (info2.IsAssemblySet)
            {
                DefinedInAssembly = info2.DefinedInAssembly;
                IsAssemblySet = true;
            }
        }

        public static MemberMetadataInfo GetFullName(TypeReference typeReference, MetadataReader reader)
        {
            TypeDecoder provider = new TypeDecoder(reader);
            return provider.GetFullName(typeReference);
        }

        public static MemberMetadataInfo GetMemberRefInfo(MemberReference memberReference, MetadataReader reader)
        {
            TypeDecoder provider = new TypeDecoder(reader);
            return provider.GetMemberRefInfo(memberReference);
        }
    }
}
