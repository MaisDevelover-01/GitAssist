namespace GitAssist.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public bool HasNext { get; set; }
    public int Page { get; set; }
}