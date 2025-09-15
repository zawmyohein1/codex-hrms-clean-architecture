using System.Collections.Generic;

namespace HRMS.Models.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();

    public int Total { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
