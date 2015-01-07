using Newtonsoft.Json;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class TargetInfo
    {
        public FrameworkName DisplayName { get; set; }

        public bool IsReleased { get; set; }

        [JsonIgnore]
        public string AreaPath { get; set; }

        public override string ToString()
        {
            return DisplayName.ToString();
        }
    }
}
