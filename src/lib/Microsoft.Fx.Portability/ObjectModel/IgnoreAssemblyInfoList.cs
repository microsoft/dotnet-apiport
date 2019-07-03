// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class IgnoreAssemblyInfoList : IEnumerable<IgnoreAssemblyInfo>
    {
        private IEnumerable<IgnoreAssemblyInfo> _innerList = Enumerable.Empty<IgnoreAssemblyInfo>();

        public void LoadJsonFile(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                LoadJson(File.ReadAllText(jsonPath));
            }
        }

        public void LoadJson(string json)
        {
            Load(JsonConvert.DeserializeObject<IgnoreAssemblyInfo[]>(json));
        }

        /// <summary>
        /// Loads a collection of IgnoreAssemblyInfos such that each assembly identity is represented once, with a
        /// combined set of targets ignored representing the union of all ignored targets for each instance of that assembly
        /// identity in the input. If the IgnoreAssemblyInfoList already contains IgnoreAssemblyInfos, the new ones will
        /// be merged with the existing list.
        /// </summary>
        /// <param name="inputs">The initial set of IgnoreAssemblyInfos to remove duplicate assembly identities from.</param>
        public void Load(IEnumerable<IgnoreAssemblyInfo> inputs)
        {
            List<IgnoreAssemblyInfo> merged = new List<IgnoreAssemblyInfo>();
            foreach (string assemblyId in _innerList.Concat(inputs).Select(i => i.AssemblyIdentity).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var assemblyInputs = _innerList.Concat(inputs).Where(x => assemblyId.Equals(x.AssemblyIdentity, StringComparison.OrdinalIgnoreCase));
                if (assemblyInputs.Any(x => x.IgnoreForAllTargets))
                {
                    merged.Add(new IgnoreAssemblyInfo() { AssemblyIdentity = assemblyId });
                }
                else
                {
                    merged.Add(new IgnoreAssemblyInfo()
                    {
                        AssemblyIdentity = assemblyId,
                        TargetsIgnored = assemblyInputs.SelectMany(x => x.TargetsIgnored).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                    });
                }
            }

            _innerList = merged;
        }

        public IEnumerator<IgnoreAssemblyInfo> GetEnumerator()
        {
            return _innerList?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_innerList)?.GetEnumerator();
        }
    }
}
