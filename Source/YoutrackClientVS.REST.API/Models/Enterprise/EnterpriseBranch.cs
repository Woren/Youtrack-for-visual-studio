﻿using Newtonsoft.Json;

namespace YouTrack.REST.API.Models.Enterprise
{
    public class EnterpriseBranch
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "displayId")]
        public string DisplayId { get; set; }

        [JsonProperty(PropertyName = "latestCommit")]
        public string LatestCommitId { get; set; }

        [JsonProperty(PropertyName = "isDefault")]
        public bool IsDefault { get; set; }
    }
}