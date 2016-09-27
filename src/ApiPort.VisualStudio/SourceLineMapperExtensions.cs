using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace ApiPortVS
{
    internal static class SourceLineMapperExtensions
    {
        public static ErrorTask GetErrorWindowTask(this ISourceMappedItem item, IVsHierarchy hierarchy)
        {
            try
            {
                var task = new ErrorTask
                {
                    CanDelete = true,
                    Column = item.Column,
                    Document = item.Path,
                    ErrorCategory = TaskErrorCategory.Message,
                    Line = item.Line - 1, // Lines are indexed from 1
                    Text = GetErrorTaskMessage(GetName(item.Item), item.Item.RecommendedChanges, item.UnsupportedTargets),
                    HierarchyItem = hierarchy
                };

                task.Navigate += ApiPortVS.SourceMapping.CodeDocumentNavigator.Navigate;

                return task;
            }
            catch
            {
                return null;
            }
        }

        private static string GetName(MissingInfo item)
        {
            var memberInfo = item as MissingMemberInfo;
            var typeInfo = item as MissingTypeInfo;

            if (memberInfo != null)
            {
                return memberInfo.MemberName;
            }
            else if (typeInfo != null)
            {
                return typeInfo.TypeName;
            }
            else
            {
                Debug.Assert(true, "Unknown MissingInfo type");
                return string.Empty;
            }
        }

        private static string GetErrorTaskMessage(string name, string recommendations, IEnumerable<FrameworkName> unsupportedPlatforms)
        {
            var sb = new StringBuilder();

            var colonIndex = name.IndexOf(':') + 1;
            var typeName = name.Substring(colonIndex);

            sb.AppendLine(typeName);
            sb.Append(LocalizedStrings.NotSupportedOn);
            sb.Append(string.Join("; ", unsupportedPlatforms));

            if (!string.IsNullOrEmpty(recommendations))
            {
                sb.AppendLine();
                sb.Append(LocalizedStrings.Suggestion);
                sb.Append(recommendations);
            }

            return sb.ToString();
        }
    }
}
