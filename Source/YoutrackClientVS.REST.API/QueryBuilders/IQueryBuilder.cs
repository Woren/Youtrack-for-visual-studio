﻿using System;
using YouTrack.REST.API.Models.Standard;

namespace YouTrack.REST.API.QueryBuilders
{
    public interface IQueryBuilder
    {//todo or can be supported
        IQueryBuilder UpdatedOn(DateTime date, Operators queryOperator);
        IQueryBuilder CreatedOn(DateTime date, Operators queryOperator);
        IQueryBuilder State(PullRequestOptions option);
        IQueryBuilder Add(string prop, string value);
        string Build();
    }
}