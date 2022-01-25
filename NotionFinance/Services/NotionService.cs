using System.Diagnostics;
using Notion.Client;
using NotionFinance.Data;
using NotionFinance.Exceptions;
using NotionFinance.Models.Tables;
using Serilog;
using Polly;
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

        if (res?.Results == null || res.Results.Count <= 0) return new List<Database>();

        var tempList = res.Results.Cast<Database>()
            .Where(database => database != null)
            .DistinctBy(x => x.Id)
            .Where(database => _databases.All(x => x.Id != database.Id)).ToList();
        _databases.AddRange(tempList);

        return _databases;
    }

    public async Task<Database> GetDatabaseByIdAsync(string databaseId)
    {
        var database = await _client.Databases.RetrieveAsync(databaseId);
        if (database == null) throw new NotionDatabaseNotFoundException();
        return database;
    }

    public async Task<Database> GetDatabaseByNameAsync(string databaseName)
    {
        await GetDatabasesAsync();
        var database = _databases.Find(x => x.Title[0].PlainText == databaseName);
        if (database == null) throw new NotionDatabaseNotFoundException($"{databaseName} not found");
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
            if (page != null && !_pages.Any(x => x.Id == page.Id)) _pages.Add(page);
        }

        return _pages;
    }

    public async Task<Page> GetPageByIdAsync(string pageId)
    {
        await GetPagesAsync();
        var page = _pages.Find(x => x.Id == pageId);
        if (page == null) throw new NotionPageNotFoundException();
        return page;
    }

    public async Task<Page?> GetPageByNameAsync(string name)
    {
        await GetPagesAsync();
        foreach (var page in _pages)
        {
            try
            {
                var namePropertyValue = page.Properties["title"];
                var n = namePropertyValue == null
                    ? null
                    : (namePropertyValue as TitlePropertyValue)!.Title[0].PlainText;
                if (n == null) throw new NotionPageNotFoundException();
                if (n.Contains(name)) return page;
            }
            catch (KeyNotFoundException e)
            {
                // Ignored
            }
        }

        throw new NotionPageNotFoundException();
    }

    public async Task<IEnumerable<Page>> GetPagesByDatabaseAsync(string databaseId)
    {
        await GetPagesAsync();
        var pagesOfDatabase = _pages.Where(page =>
                page.Parent is {Type: ParentType.DatabaseId} &&
                (page.Parent as DatabaseParent)!.DatabaseId == databaseId)
            .ToList();
        return pagesOfDatabase;
    }

    public async Task<Page> UpdatePageAsync(Page page, PagesUpdateParameters pagesUpdateParameters)
    {
        return await Policy
            .Handle<NotionApiException>()
            .WaitAndRetryAsync(10, i => TimeSpan.FromMilliseconds(500))
            .ExecuteAsync(async () => await _client.Pages.UpdateAsync(page.Id, pagesUpdateParameters));
    }

    public async Task UpdateDatabasesAndPagesAsync()
    {
        await GetDatabasesAsync();
        await GetPagesAsync();
    }

    public async Task<MasterTable> GetMasterTable()
    {
        return await Policy
            .Handle<NotionDatabaseNotFoundException>()
            .WaitAndRetryAsync(10, (x) => TimeSpan.FromMilliseconds(500))
            .ExecuteAsync(async () =>
            {
                await GetDatabasesAsync();
                var masterDb = _databases.Find(database => database.Title[0].PlainText == "Master Database");
                if (masterDb == null) throw new NotionDatabaseNotFoundException();
                return await MasterTable.Create(masterDb);
            });
    }

    public async Task<Database> CreateMasterTable()
    {
        try
        {
            var page = await GetPageByNameAsync("Notion Finance Tracker Home");
            if (page == null) throw new NotionPageNotFoundException();

            var database = await _client.Databases.CreateAsync(new DatabasesCreateParameters()
            {
                Parent = new ParentPageInput() {PageId = page.Id},
                Title = new List<RichTextBaseInput>()
                    {new RichTextTextInput() {Text = new Text {Content = "Master Database", Link = null}}},
                Properties = new Dictionary<string, IPropertySchema>()
                {
                    {"Name", new TitlePropertySchema() {Title = new Dictionary<string, object>()}},
                    {
                        "Type", new SelectPropertySchema()
                        {
                            Select = new OptionWrapper<SelectOptionSchema>()
                            {
                                Options = new List<SelectOptionSchema>()
                                {
                                    new SelectOptionSchema() {Color = Color.Orange, Name = "Token"},
                                    new SelectOptionSchema() {Color = Color.Blue, Name = "Bond ETF"},
                                    new SelectOptionSchema() {Color = Color.Purple, Name = "Individual Stock"},
                                    new SelectOptionSchema() {Color = Color.Gray, Name = "ETF"},
                                    new SelectOptionSchema() {Color = Color.Pink, Name = "FX"}
                                }
                            }
                        }
                    },
                    {
                        "Current Price",
                        new NumberPropertySchema() {Number = new Number() {Format = NumberFormat.Dollar}}
                    },
                    {"Ticker", new RichTextPropertySchema() {RichText = new Dictionary<string, object>()}}
                }
            });
            _databases.Add(database);
            return database;
        }
        catch (Exception e)
        {
            Log.Information(e, "Some error on CreateMasterTable");
            throw new NotionDatabaseNotFoundException(e.Message);
        }
    }
}