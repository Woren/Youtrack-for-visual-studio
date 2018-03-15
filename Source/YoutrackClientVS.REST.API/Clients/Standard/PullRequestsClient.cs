﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParseDiff;
using RestSharp;
using YouTrack.REST.API.Helpers;
using YouTrack.REST.API.Interfaces;
using YouTrack.REST.API.Mappings;
using YouTrack.REST.API.Models.Standard;
using YouTrack.REST.API.QueryBuilders;
using YouTrack.REST.API.Wrappers;

namespace YouTrack.REST.API.Clients.Standard
{
    public class PullRequestsClient : ApiClient, IPullRequestsClient
    {
        private readonly IYouTrackRestClient _internalRestClient;
        private readonly IYouTrackRestClient _webClient;
        private readonly IYouTrackRestClient _versionOneClient;

        public PullRequestsClient(
            IYouTrackRestClient restClient,
            IYouTrackRestClient internalRestClient,
            IYouTrackRestClient webClient,
            IYouTrackRestClient versionOneClient,
            Connection connection) : base(restClient, connection)
        {
            _internalRestClient = internalRestClient;
            _webClient = webClient;
            _versionOneClient = versionOneClient;
        }

        public async Task<IEnumerable<UserShort>> GetAuthors(string repositoryName, string ownerName)
        {
            var url = ApiUrls.PullRequestsAuthors(ownerName, repositoryName);
            return await _internalRestClient.GetAllPages<UserShort>(url, 100);
        }

        public async Task<IteratorBasedPage<PullRequest>> GetPullRequestsPage(string repositoryName, string ownerName, int page, int limit = 50,
            IPullRequestQueryBuilder builder = null)
        {
            var url = ApiUrls.PullRequests(ownerName, repositoryName);
            var request = new YouTrackRestRequest(url, Method.GET);

            if (builder != null)
                foreach (var param in builder.GetQueryParameters())
                    request.AddQueryParameter(param.Key, param.Value);

            request.AddQueryParameter("pagelen", limit.ToString()).AddQueryParameter("page", page.ToString());

            var response = await RestClient.ExecuteTaskAsync<IteratorBasedPage<PullRequest>>(request);
            return response.Data;
        }

        public async Task<IEnumerable<PullRequest>> GetPullRequests(string repositoryName, string ownerName, int limit = 50, IPullRequestQueryBuilder queryBuilder = null)
        {
            var url = ApiUrls.PullRequests(ownerName, repositoryName);

            var queryString = new QueryString();

            if (queryBuilder != null)
                foreach (var param in queryBuilder.GetQueryParameters())
                    queryString.Add(param.Key, param.Value);

            return await RestClient.GetAllPages<PullRequest>(url, limit, queryString);
        }

        public async Task<IEnumerable<FileDiff>> GetPullRequestDiff(string repositoryName, string owner, long id)
        {
            var url = ApiUrls.PullRequestDiff(owner, repositoryName, id);
            var request = new YouTrackRestRequest(url, Method.GET);
            var response = await RestClient.ExecuteTaskAsync(request);
            return DiffFileParser.Parse(response.Content);
        }

        public async Task<Participant> ApprovePullRequest(string repositoryName, string ownerName, long id)
        {
            var url = ApiUrls.PullRequestApprove(ownerName, repositoryName, id);
            var request = new YouTrackRestRequest(url, Method.POST);
            var response = await RestClient.ExecuteTaskAsync<Participant>(request);
            return response.Data;
        }

        public async Task DisapprovePullRequest(string repositoryName, string ownerName, long id)
        {
            var url = ApiUrls.PullRequestApprove(ownerName, repositoryName, id);
            var request = new YouTrackRestRequest(url, Method.DELETE);
            await RestClient.ExecuteTaskAsync(request);
        }

        public async Task DeclinePullRequest(string repositoryName, string ownerName, long id, string version)
        {
            var url = ApiUrls.PullRequestDecline(ownerName, repositoryName, id);
            var request = new YouTrackRestRequest(url, Method.POST);
            await RestClient.ExecuteTaskAsync(request);
        }

        public async Task MergePullRequest(string repositoryName, string ownerName, MergeRequest mergeRequest)
        {
            var url = ApiUrls.PullRequestMerge(ownerName, repositoryName, mergeRequest.Id);
            var request = new YouTrackRestRequest(url, Method.POST);
            request.AddParameter("application/json; charset=utf-8", request.JsonSerializer.Serialize(mergeRequest), ParameterType.RequestBody);
            await RestClient.ExecuteTaskAsync(request);
        }

        public async Task<IEnumerable<UserShort>> GetDefaultReviewers(string repositoryName, string ownerName)
        {
            var url = ApiUrls.DefaultReviewers(ownerName, repositoryName);
            return await RestClient.GetAllPages<UserShort>(url, 100);
        }

        public IPullRequestQueryBuilder GetPullRequestQueryBuilder()
        {
            return new PullRequestQueryBuilder();
        }

        public async Task<IEnumerable<Commit>> GetPullRequestCommits(string repositoryName, string ownerName, long id)
        {
            var url = ApiUrls.PullRequestCommits(ownerName, repositoryName, id);
            var commits = await RestClient.GetAllPages<Commit>(url);
            foreach (var commit in commits)
            {
                commit.CommitHref = $"{Connection.MainUrl}{ownerName}/{repositoryName}/commits/{commit.Hash}";
            }
            return commits;
        }

        public async Task<IEnumerable<Comment>> GetPullRequestComments(string repositoryName, string ownerName, long id)
        {
            var url = ApiUrls.PullRequestCommentsV1(ownerName, repositoryName, id);
            var request = new YouTrackRestRequest(url, Method.GET);
            return (await _versionOneClient.ExecuteTaskAsync<List<CommentV1>>(request)).Data.MapTo<List<Comment>>();
        }

        public async Task<Comment> AddPullRequestComment(string repositoryName, string ownerName, long id, string content, long? lineFrom = null, long? lineTo = null, string fileName = null, long? parentId = null)
        {
            var url = ApiUrls.PullRequestCommentsV1(ownerName, repositoryName, id);
            var request = new YouTrackRestRequest(url, Method.POST);

            var body = new CommentV1()
            {
                Content = content,
                LineFrom = parentId != null ? null : lineFrom,
                LineTo = parentId != null ? null : lineTo,
                FileName = fileName,
                ParentId = parentId
            };

            if (body.LineFrom != null)//we can't set both
                body.LineTo = null;

            request.AddParameter("application/json; charset=utf-8", request.JsonSerializer.Serialize(body), ParameterType.RequestBody);

            var response = await _versionOneClient.ExecuteTaskAsync<CommentV1>(request);
            return response.Data.MapTo<Comment>();
        }

        public async Task DeletePullRequestComment(string repositoryName, string ownerName, long pullRequestId, long id, long version)
        {
            var url = ApiUrls.PullRequestCommentV1(ownerName, repositoryName, pullRequestId, id);
            var request = new YouTrackRestRequest(url, Method.DELETE);
            await _versionOneClient.ExecuteTaskAsync(request);
        }

        public async Task<Comment> EditPullRequestComment(string repositoryName, string ownerName, long pullRequestId, long id, string content, long commentVersion)
        {
            var url = ApiUrls.PullRequestCommentV1(ownerName, repositoryName, pullRequestId, id);
            var request = new YouTrackRestRequest(url, Method.PUT);

            var body = new CommentV1()
            {
                Content = content
            };

            request.AddParameter("application/json; charset=utf-8", request.JsonSerializer.Serialize(body), ParameterType.RequestBody);

            var response = await _versionOneClient.ExecuteTaskAsync<CommentV1>(request);
            return response.Data.MapTo<Comment>();
        }

        public async Task<PullRequest> GetPullRequest(string repositoryName, string owner, long id)
        {
            var url = ApiUrls.PullRequest(owner, repositoryName, id);
            var request = new YouTrackRestRequest(url, Method.GET);
            var response = await RestClient.ExecuteTaskAsync<PullRequest>(request);
            return response.Data;
        }

        public async Task<PullRequest> GetPullRequestForBranches(string repositoryName, string ownerName, string sourceBranch, string destBranch)
        {
            var url = ApiUrls.PullRequests(ownerName, repositoryName);

            var queryBuilderString = new QueryBuilder()
                .Add("source.branch.name", sourceBranch)
                .Add("destination.branch.name", destBranch)
                .State(PullRequestOptions.OPEN)
                .Build();

            var query = new QueryString()
            {
                {"q", queryBuilderString},
            };
            var response = await RestClient.GetAllPages<PullRequest>(url, query: query, limit: 20);
            var pq = response.SingleOrDefault();
            if (pq == null)
                return null;

            return await GetPullRequest(repositoryName, ownerName, pq.Id); // we need reviewers and participants
        }

        public async Task<IEnumerable<FileDiff>> GetCommitsDiff(string repoName, string owner, string fromCommit, string toCommit)
        {
            if (fromCommit == toCommit) //otherwise it produces diff against its parent
                return Enumerable.Empty<FileDiff>();

            var url = ApiUrls.CommitsDiff(owner, repoName, fromCommit, toCommit);
            var request = new YouTrackRestRequest(url, Method.GET);
            var response = await RestClient.ExecuteTaskAsync(request);
            return DiffFileParser.Parse(response.Content);
        }

        public async Task<string> GetFileContent(string repoName, string owner, string hash, string path)
        {
            var url = ApiUrls.DownloadFile(owner, repoName, hash, path);
            var request = new YouTrackRestRequest(url, Method.GET);
            var response = await _versionOneClient.ExecuteTaskAsync(request);
            return response.Content;
        }

        public async Task CreatePullRequest(PullRequest pullRequest, string repositoryName, string owner)
        {
            //pullRequest.Author = new User()
            //{
            //    Username = Connection.Credentials.Login
            //};

            var url = ApiUrls.PullRequests(owner, repositoryName);
            var request = new YouTrackRestRequest(url, Method.POST);
            request.AddParameter("application/json; charset=utf-8", request.JsonSerializer.Serialize(pullRequest), ParameterType.RequestBody);
            var response = await RestClient.ExecuteTaskAsync<PullRequest>(request);
        }


        public async Task UpdatePullRequest(PullRequest pullRequest, string repoName, string owner)
        {
            //pullRequest.Author = new User()
            //{
            //    Username = Connection.Credentials.Login
            //};

            var url = ApiUrls.PullRequest(owner, repoName, pullRequest.Id);
            var request = new YouTrackRestRequest(url, Method.PUT);
            request.AddParameter("application/json; charset=utf-8", request.JsonSerializer.Serialize(pullRequest), ParameterType.RequestBody);
            var response = await RestClient.ExecuteTaskAsync<PullRequest>(request);
        }


        public async Task<IEnumerable<UserShort>> GetRepositoryUsers(string repositoryName, string ownerName, string filter)
        {
            var url = ApiUrls.Mentions(ownerName, repositoryName);
            var query = new QueryString()
            {
                {"term",filter }
            };

            var request = new YouTrackRestRequest(url, Method.GET);

            foreach (var par in query)
                request.AddQueryParameter(par.Key, par.Value);

            var response = await _webClient.ExecuteTaskAsync<List<UserShort>>(request);
            return response.Data;
        }
    }
}