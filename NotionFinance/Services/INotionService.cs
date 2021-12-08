using Notion.Client;
using NotionFinance.Models.Tables;
using User = NotionFinance.Models.User;

namespace NotionFinance.Services;

public interface INotionService
{
    public Task<IEnumerable<User>> GetUsersAsync();
    public Task<User> GetUserByIdAsync(string notionId);
    public Task<IEnumerable<Database>> GetDatabasesAsync();
    public Task<Database> GetDatabaseByIdAsync(string databaseId);
    public Task<Database> GetDatabaseByNameAsync(string databaseName);
    public Task<IEnumerable<Page>> GetPagesAsync();
    public Task<Page> GetPagesByIdAsync(string pageId);
    public Task<IEnumerable<Page>> GetPagesByDatabaseAsync(string databaseId);
    public Task UpdatePageAsync(Page page, PagesUpdateParameters pagesUpdateParameters);
    public Task UpdateDatabasesAndPagesAsync();

    public Task<MasterTable> GetMasterTable();
}