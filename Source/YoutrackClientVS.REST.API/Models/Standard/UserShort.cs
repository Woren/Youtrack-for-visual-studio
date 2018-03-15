﻿using Newtonsoft.Json;

namespace YouTrack.REST.API.Models.Standard
{
    public class UserShort
    {
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "display_name")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "links")]
        public Links Links { get; set; }

        public string Email { get; set; }
    }
}