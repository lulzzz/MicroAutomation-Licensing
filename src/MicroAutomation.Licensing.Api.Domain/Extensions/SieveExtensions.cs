#region Using

using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace MicroAutomation.Licensing.Api.Domain.Extensions;

public static class SieveExtensions
{
    /// <summary>
    /// Apply sieve formatting to queryable request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sieveProcessor"></param>
    /// <param name="query"></param>
    /// <param name="sieveModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<PagedResult<T>> GetPagedAsync<T>(this ISieveProcessor sieveProcessor, IQueryable<T> query, SieveModel sieveModel = null, CancellationToken cancellationToken = default) where T : class
    {
        var result = new PagedResult<T>();

        var (pagedQuery, page, pageSize, rowCount, pageCount) = await GetPagedResultAsync(sieveProcessor, query, sieveModel, cancellationToken);

        result.CurrentPage = page;
        result.PageSize = pageSize;
        result.RowCount = rowCount;
        result.PageCount = pageCount;

        result.Results = await pagedQuery.ToListAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Internal method to retrieve data from datastore with sieve formatting.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sieveProcessor"></param>
    /// <param name="query"></param>
    /// <param name="sieveModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<(IQueryable<T> pagedQuery, int page, int pageSize, int rowCount, int pageCount)> GetPagedResultAsync<T>(ISieveProcessor sieveProcessor, IQueryable<T> query, SieveModel sieveModel = null, CancellationToken cancellationToken = default) where T : class
    {
        var page = sieveModel?.Page ?? 1;
        var pageSize = sieveModel?.PageSize ?? 50;

        if (sieveModel != null)
        {
            // apply pagination in a later step
            query = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
        }

        var rowCount = await query.CountAsync(cancellationToken);

        var pageCount = (int)Math.Ceiling((double)rowCount / pageSize);

        var skip = (page - 1) * pageSize;
        var pagedQuery = query.Skip(skip).Take(pageSize);

        return (pagedQuery, page, pageSize, rowCount, pageCount);
    }
}

/// <summary>
/// Sieve paged result model.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedResult<T> where T : class
{
    public List<T> Results { get; set; }
    public int CurrentPage { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public long RowCount { get; set; }

    public PagedResult()
    {
        Results = new List<T>();
    }
}