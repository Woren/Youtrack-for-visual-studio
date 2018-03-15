﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace YouTrackClientVS.Infrastructure.Extensions
{
    public static class CommonExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string GetPropertyName<T, TResult>(this Expression<Func<T, TResult>> expression)
        {
            var member = expression.Body as MemberExpression;
            return member?.Member?.Name;
        }

        public static void AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }
    }
}
