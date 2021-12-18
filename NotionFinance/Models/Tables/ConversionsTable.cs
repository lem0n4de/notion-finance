using Notion.Client;
using NotionFinance.Models.Pages;
using Serilog;

namespace NotionFinance.Models.Tables;

public class ConversionsTable
{
    private List<ConversionsTablePage> _pages;

    public List<ConversionsTablePage> TokenPages =>
        _pages.Where(page =>
        {
            try
            {
                return page.Type == AssetType.Token;
            }
            catch (Exception e)
            {
                return false;
            }
        }).ToList();

    public List<ConversionsTablePage> FxPages =>
        _pages.Where(page =>
        {
            try
            {
                return page.Type == AssetType.Fx;
            }
            catch (Exception e)
            {
                return false;
            }
        }).ToList();

    private Database _database { get; }

    public ConversionsTable(Database database)
    {
        _database = database;
    }

    public static async Task<ConversionsTable> Create(Database database)
    {
        var conversionsTable = new ConversionsTable(database);
        return conversionsTable;
    }

    public static async Task<ConversionsTable> Create(Database database, IEnumerable<Page> pages)
    {
        var conversionsTable = new ConversionsTable(database);
        conversionsTable._pages = new List<ConversionsTablePage>();
        conversionsTable._pages.AddRange(pages.ToList().ConvertAll(ConversionsTablePage.CreateFromNotionPage));
        return conversionsTable;
    }
}