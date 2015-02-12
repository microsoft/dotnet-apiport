// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Microsoft.Fx.Portability.Analyzer
{
	public class MemberMetadataInfo
	{
		private string _name;
		public string nameSpace = null;
		private List<string> _names = new List<string>();
		public MemberMetadataInfo parentType = null;  //for memberRefs, the type is from the parent
		public AssemblyReferenceHandle assembly;   //assembly where it is defined
		public bool assemblySet = false;
		public ModuleReferenceHandle module;
		public bool moduleSet = false;
		public Kind kind = Kind.Type;
		public enum Kind
		{
			Type,
			Method,
			Field,
			Unknown
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (kind == Kind.Method || kind == Kind.Field)
			{
				_name = _name.Replace("<", "{").Replace(">", "}");
				//get the full name from the type
				sb.Append(parentType.ToString());
				sb.Append(".");

				if (kind == Kind.Method)
				{
					_name = _name.Replace(".", "#");	//expected output is #ctor instead of .ctor
				}
				sb.Append(_name);
				//To do: add method signature 
			}
			else
			{
				if (nameSpace != null)
				{
					sb.Append(nameSpace);
					sb.Append(".");
				}

				List<string> displayNames = new List<string>(_names);
				displayNames.Add(_name);


				for (int i = 0; i < displayNames.Count; i++)
				{
					if (i > 0)
						sb.Append(".");
					sb.Append(displayNames[i]);
				}
			}
			return sb.ToString();
		}
		public MemberMetadataInfo(string name)
		{
			_name = name;
		}

		public void Join(MemberMetadataInfo info2)
		{
			_names.AddRange(info2._names);
			_names.Add(info2._name);
			if (info2.nameSpace != null)
				nameSpace = info2.nameSpace;

			if (info2.assemblySet)
			{
				assembly = info2.assembly;
				assemblySet = true;
			}
		}
	}
}
