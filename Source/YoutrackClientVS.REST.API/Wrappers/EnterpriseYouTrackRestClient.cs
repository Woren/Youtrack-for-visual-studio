using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using YouTrack.REST.API.Interfaces;
using YouTrack.REST.API.Models.Enterprise;
using YouTrack.REST.API.Models.Standard;
using YouTrack.REST.API.QueryBuilders;
using YouTrack.REST.API.Serializers;

namespace YouTrack.REST.API.Wrappers
{
    public class EnterpriseYouTrackRestClient : YouTrackRestClientBase, IEnterpriseYouTrackRestClient
    {
        public EnterpriseYouTrackRestClient(Connection connection) : base(connection)
        {
        }

        public override async Task<IEnumerable<T>> GetAllPages<T>(string url, int limit = 50, QueryString query = null)
        {
            var result = new EnterpriseIteratorBasedPage<T>()
            {
                Values = new List<T>()
            };
            IRestResponse<EnterpriseIteratorBasedPage<T>> response;
            ulong pageNumber = 0;
            do
            {
                var request = new YouTrackRestRequest(url, Method.GET);
                request.AddQueryParameter("limit", limit.ToString()).AddQueryParameter("start", pageNumber.ToString());

                if (query != null)
                    foreach (var par in query)
                        request.AddQueryParameter(par.Key, par.Value);

                response = await this.ExecuteTaskAsync<EnterpriseIteratorBasedPage<T>>(request);

                if (response.Data?.Values == null)
                    break;

                result.Values.AddRange(response.Data.Values);

                pageNumber = response.Data.NextPageStart;

            } while (response.Data?.IsLastPage == false);

            return result.Values;
        }

        protected override string DeserializeError(IRestResponse response)
        {
            var serializer = new NewtonsoftJsonSerializer();
            var deserialized = serializer.Deserialize<EnterpriseErrorWrapper>(response.Content);
            return string.Join(",", deserialized.Errors.Select(x => x.Message));
        }
    }
}