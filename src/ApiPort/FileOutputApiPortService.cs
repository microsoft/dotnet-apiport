using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Fx.Portability.ObjectModel;
using System.IO;
using System.Diagnostics;

namespace ApiPort
{
    internal class FileOutputApiPortService : IApiPortService
    {
        public Task<ServiceResponse<AnalyzeResponse>> GetAnalysisAsync(string submissionId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<byte[]>> GetAnalysisAsync(string submissionId, string format)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<ApiInformation>> GetApiInformationAsync(string docId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<IEnumerable<ResultFormatInformation>>> GetResultFormatsAsync()
        {
            var format = new ResultFormatInformation { DisplayName = "Excel", FileExtension = ".xlsx" };
            var response = new ServiceResponse<IEnumerable<ResultFormatInformation>>(new[] { format });

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = default(int?), int? top = default(int?), UsageDataFilter? filter = default(UsageDataFilter?), IEnumerable<string> targets = null)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = default(int?))
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a)
        {
            WriteOutput(a);

            return Task.FromResult(new ServiceResponse<AnalyzeResponse>(new AnalyzeResponse()));
        }

        public Task<ServiceResponse<byte[]>> SendAnalysisAsync(AnalyzeRequest a, string format)
        {
            WriteOutput(a);

            return Task.FromResult(new ServiceResponse<byte[]>(new byte[] { }));
        }

        private void WriteOutput(AnalyzeRequest a)
        {
            var tmp = $"{Path.GetTempFileName()}.json";

            using (var ms = new MemoryStream(a.Serialize()))
            using (var fs = File.OpenWrite(tmp))
            {
                ms.CopyTo(fs);
            }

            Process.Start(tmp);
        }
    }
}
