// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SearchFxApi
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var analysisService = new ApiPortService("portability.dot.net", new ProductInformation("MyAPIQueryProgram"));

            Console.Write("Enter API you want to search for:  ");
            var api = Console.ReadLine();

            var matchingApis = FindMatchingApis(analysisService, api).Result;

            Console.WriteLine("Enter the number of the API you want to get more information about.");

            for (int i = 0; i < matchingApis.Count; i++)
            {
                Console.WriteLine("[" + i + "] " + matchingApis[i].FullName);
            }

            Console.Write("Enter api number:  ");
            var index = int.Parse(Console.ReadLine(), CultureInfo.CurrentCulture);

            var apiToSearchFor = matchingApis[index];
            var apiInformation = GetApi(analysisService, apiToSearchFor.DocId).Result;

            Console.WriteLine("These are the platforms this API is supported on: ");

            foreach (var platform in apiInformation.Supported.Select(x => x.FullName))
            {
                Console.WriteLine(platform);
            }

            Console.Write("Enter any key to quit...");
            Console.ReadLine();
        }

        private static async Task<IReadOnlyList<ApiDefinition>> FindMatchingApis(IApiPortService service, string api)
        {
            var response = await service.SearchFxApiAsync(api, top: 20);
            return response.Response.ToList();
        }

        private static async Task<ApiInformation> GetApi(IApiPortService service, string apiDocId)
        {
            var response = await service.GetApiInformationAsync(apiDocId);
            return response.Response;
        }
    }
}
