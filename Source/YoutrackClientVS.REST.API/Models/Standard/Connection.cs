﻿using System;
using YouTrack.REST.API.Authentication;

namespace YouTrack.REST.API.Models.Standard
{
    public class Connection
    {

        public Connection(Uri mainUrl, Uri apiUrl, Credentials credentials)
        {
            MainUrl = mainUrl;
            ApiUrl = apiUrl;
            Credentials = credentials;
        }

        public Credentials Credentials { get; }

        public Uri ApiUrl { get; }

        public Uri MainUrl { get; }
    }
}