﻿using System.Globalization;
using AutoMapper;
using YouTrack.REST.API.Helpers;
using YouTrack.REST.API.Models.Enterprise;
using YouTrack.REST.API.Models.Standard;

namespace YouTrack.REST.API.Mappings.Converters
{
    public class EnterpriseCommitTypeConverter : ITypeConverter<EnterpriseCommit, Commit>
    {
        public Commit Convert(EnterpriseCommit source, Commit destination, ResolutionContext context)
        {
            var commit = new Commit()
            {
                Author = new Author()
                {
                    User = source.Author.MapTo<UserShort>(),
                },
                Hash = source.Id,
                Date = source.Date.FromUnixTimeStamp().ToString(CultureInfo.InvariantCulture),
                Message = source.Message
            };
            return commit;
        }
    }
}