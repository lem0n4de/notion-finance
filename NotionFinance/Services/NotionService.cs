using Notion.Client;
using NotionFinance.Data;
using NotionFinance.Exceptions;
using User = NotionFinance.Models.User;

namespace NotionFinance.Services;

public class NotionService : INotionService
{
    private UserDbContext _context;
    private INotionClient _client;
    private List<Database> _databases;
    private List<Page> _pages;

    public NotionService(UserDbContext userDbContext, INotionClient notionClient)
    {
        _context = userDbContext;
        _client = notionClient;
        // Initialized for Any() check later on as they might be called earlier than these lists are populated
        _databases = new List<Database>();
        _pages = new List<Page>();
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUserByIdAsync(string notionId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Database>> GetDatabasesAsync()
    {
        var res = await _client.Search.SearchAsync(new SearchParameters()
        {
            Filter = new SearchFilter() {Value = SearchObjectType.Database}
        });

        var tempList = res.Results.Cast<Database>().Where(database => database != null).ToList();
        _databases = tempList;

        return _databases;
    }

    public async Task<Database> GetDatabaseByIdAsync(string databaseId)
    {
        if (!_databases.Any()) await GetDatabasesAsync();
        var database = _databases.Find(x => x.Id == databaseId);
        if (database == null) throw new NotionDatabaseNotFoundException();
        return database;
    }

    public async Task<Database> GetDatabaseByNameAsync(string databaseName)
    {
        if (!_databases.Any()) await GetDatabasesAsync();
        var database = _databases.Find(x => x.Title[0].PlainText == databaseName);
        if (database == null) throw new NotionDatabaseNotFoundException();
        return database;
    }

    public async Task<IEnumerable<Page>> GetPagesAsync()
    {
        var res = await _client.Search.SearchAsync(new SearchParameters
        {
            Filter = new SearchFilter() {Value = SearchObjectType.Page}
        });

        foreach (var page in res.Results.Cast<Page>())
        {
            if (page != null) _pages.Add(page);
        }

        return _pages;
    }

    public async Task<Page> GetPagesByIdAsync(string pageId)
    {
        if (!_pages.Any()) await GetPagesAsync();
        var page = _pages.Find(x => x.Id == pageId);
        if (page == null) throw new NotionPageNotFoundException();
        return page;
    }

    public async Task<IEnumerable<Page>> GetPagesByDatabaseAsync(string databaseId)
    {
        if (!_pages.Any()) await GetPagesAsync();
        var pagesOfDatabase = _pages.Where(page =>
                page.Parent is {Type: ParentType.DatabaseId} &&
                (page.Parent as DatabaseParent)!.DatabaseId == databaseId)
            .ToList();
        return pagesOfDatabase;
    }

    public async Task UpdatePageAsync(Page page, PagesUpdateParameters pagesUpdateParameters)
    {
        await _client.Pages.UpdateAsync(page.Id, pagesUpdateParameters);
    }

    public async Task UpdateDatabasesAndPagesAsync()
    {
        await GetDatabasesAsync();
        await GetPagesAsync();
    }
}