﻿using Newtonsoft.Json;

namespace YouTrack.REST.API.Models.Enterprise
{
    public class EnterpriseParticipant
    {
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "approved")]
        public bool Approved { get; set; }

        [JsonProperty(PropertyName = "user")]
        public EnterpriseUser User { get; set; }
    }
}