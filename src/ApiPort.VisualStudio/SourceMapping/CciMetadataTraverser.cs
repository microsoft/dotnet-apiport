// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

namespace ApiPortVS.SourceMapping
{
    internal class CciMetadataTraverser : MetadataVisitor
    {
        public ICollection<ISourceMappedItem> FoundItems { get { return _foundItems; } }

        private readonly List<ISourceMappedItem> _foundItems = new List<ISourceMappedItem>();
        private readonly IDictionary<string, MissingMemberInfo> _interestingMethods;
        private readonly IDictionary<string, MissingTypeInfo> _interestingTypes;
        private readonly ReportingResult _analysis;
        private readonly PdbReader _pdbReader;

        public CciMetadataTraverser(ReportingResult analysis, PdbReader pdbReader)
        {
            _analysis = analysis;
            _pdbReader = pdbReader;

            var missingMembers = from type in analysis.GetMissingTypes()
                                 from member in type.MissingMembers
                                 select member;

            _interestingMethods = missingMembers
                .ToDictionary(m => m.MemberName, m => m);

            _interestingTypes = analysis.GetMissingTypes()
                .Where(type => type.IsMissing)
                .ToDictionary(t => t.DocId, t => t);
        }

        // TODO: Add support for fields, properties, type definitions and attributes as well
        // 
        // Example:
        //  [Serializable]
        //  public class Foo {}
        //
        // The 'Serializable' (Which is not supported on most platforms) is not found

        public override void Visit(IMethodBody method)
        {
            var calls = method.Operations
                              .Where(opcode => opcode.OperationCode == OperationCode.Call
                                            || opcode.OperationCode == OperationCode.Callvirt
                                            || opcode.OperationCode == OperationCode.Newobj);

            var foundInterestingCall = false;
            foreach (var call in calls)
            {
                var calledMethod = (IMethodReference)call.Value;
                var calledId = calledMethod.DocId();

                MissingMemberInfo memberInfo = null;
                if (!_interestingMethods.TryGetValue(calledId, out memberInfo))
                {
                    // Try again with unwrapped member
                    var unwrappedId = calledMethod.UnWrapMember().DocId();
                    _interestingMethods.TryGetValue(unwrappedId, out memberInfo);
                }

                if (memberInfo != null)
                {
                    foundInterestingCall = true;

                    var sourceItems = _pdbReader
                        .GetClosestPrimarySourceLocationsFor(call.Location)
                        .Select(l => GetSourceMappedItem(l, memberInfo));

                    _foundItems.AddRange(sourceItems);
                }
            }

            // Check signature for interesting return type to catch polymorphic returns
            // TODO: Does this condition need to be met? Should we check every return type?
            if (!foundInterestingCall)
            {
                CheckReturnType(method);
            }

            base.Visit(method);
        }

        private void CheckReturnType(IMethodBody method)
        {
            MissingTypeInfo typeInfo;
            var returnType = method.MethodDefinition.Type.DocId();

            if (!_interestingTypes.TryGetValue(returnType, out typeInfo))
            {
                return;
            }

            var sourceItems = _pdbReader
                .GetPrimarySourceLocationsFor(method.MethodDefinition.Locations)
                .Select(l => GetSourceMappedItem(l, typeInfo));

            _foundItems.AddRange(sourceItems);
        }

        private IEnumerable<FrameworkName> GetUnsupportedPlatformNames(IEnumerable<Version> targetStatus)
        {
            return _analysis.Targets.Zip(targetStatus, Tuple.Create)
                .Where(t => t.Item2 == null || t.Item2 > t.Item1.Version)
                .Select(t => t.Item1);
        }

        private IEnumerable<Version> GetTargetStatus(MissingInfo missingInfo)
        {
            var memberInfo = missingInfo as MissingMemberInfo;
            var typeInfo = missingInfo as MissingTypeInfo;

            if (memberInfo != null)
            {
                return memberInfo.TargetVersionStatus;
            }
            else if (typeInfo != null)
            {
                return typeInfo.TargetVersionStatus;
            }
            else
            {
                Debug.Assert(true, "Unknown MissingInfo type");
                return new List<Version>();
            }
        }

        private ISourceMappedItem GetSourceMappedItem(IPrimarySourceLocation location, MissingInfo memberInfo)
        {
            var unsupportedPlatformNames = GetUnsupportedPlatformNames(GetTargetStatus(memberInfo));

            return new CciSourceItem(location, memberInfo, unsupportedPlatformNames);
        }

        [DebuggerDisplay("{_item}")]
        private class CciSourceItem : ISourceMappedItem
        {
            private readonly IEnumerable<FrameworkName> _unsupportedPlatforms;
            private readonly MissingInfo _item;
            private readonly int _column;
            private readonly int _line;
            private readonly string _path;

            public CciSourceItem(IPrimarySourceLocation location, MissingInfo item, IEnumerable<FrameworkName> unsupportedPlatformNames)
            {
                _column = location.StartColumn;
                _line = location.StartLine;
                _path = location.SourceDocument.Location;
                _item = item;
                _unsupportedPlatforms = unsupportedPlatformNames;
            }

            MissingInfo ISourceMappedItem.Item { get { return _item; } }

            public IEnumerable<FrameworkName> UnsupportedTargets { get { return _unsupportedPlatforms; } }

            public MissingInfo Item { get { return _item; } }

            public int Column { get { return _column; } }

            public int Line { get { return _line; } }

            public string Path { get { return _path; } }
        }
    }
}
