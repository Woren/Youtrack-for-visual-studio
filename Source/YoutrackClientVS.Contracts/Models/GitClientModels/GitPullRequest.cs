﻿using System;
using System.Collections.Generic;

namespace YouTrackClientVS.Contracts.Models.GitClientModels
{
    public class GitPullRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public GitBranch SourceBranch { get; set; }
        public GitBranch DestinationBranch { get; set; }
        public GitPullRequestStatus Status { get; set; }
        public long Id { get; set; }
        public GitUser Author { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool? CloseSourceBranch { get; set; }
        public string Url { get; set; }
        public Dictionary<GitUser, bool> Reviewers { get; set; }
        public string Link { get; set; }
        public int CommentsCount { get; set; }
        public string Version { get; set; }

        public GitPullRequest(string title, string description, GitBranch sourceBranch, GitBranch destinationBranch) : 
            this(title, description, sourceBranch, destinationBranch, null)
        {
        }

        public GitPullRequest(string title, string description, GitBranch sourceBranch, GitBranch destinationBranch, Dictionary<GitUser, bool> reviewers)
        {
            Title = title;
            Description = description;
            SourceBranch = sourceBranch;
            DestinationBranch = destinationBranch;
            Reviewers = reviewers;
        }
    }
}