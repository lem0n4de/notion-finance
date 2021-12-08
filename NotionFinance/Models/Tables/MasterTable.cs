using Microsoft.AspNetCore.Mvc;
using Notion.Client;
using NotionFinance.Models.Pages;
using Serilog;

namespace NotionFinance.Models.Tables;

public class MasterTable
{
    private ILogger<MasterTable> _logger;
    private List<MasterTablePage> Pages;

    public List<MasterTablePage> TokenPages =>
        Pages.Where(page =>
        {
            try
            {
                return page.Type == MasterTablePage.MasterTablePageType.Token;
            }
            catch (Exception e)
            {
                // ignored
                return false;
            }
        }).ToList();

    public List<MasterTablePage> ForexPages =>
        Pages.Where(page =>
        {
            try
            {
                return page.Type == MasterTablePage.MasterTablePageType.Fx;
            }
            catch (Exception e)
            {
                return false;
            }
        }).ToList();

    private Database Database { get; }

    private MasterTable(Database database)
    {
        Database = database;
    }

    public static async Task<MasterTable> Create(Database database)
    {
        var masterTable = new MasterTable(database);
        return masterTable;
    }

    public static async Task<MasterTable> Create(Database database, IEnumerable<Page> pages)
    {
        var masterTable = new MasterTable(database);
        masterTable.Pages = new List<MasterTablePage>();
        masterTable.Pages.AddRange(pages.ToList().ConvertAll(MasterTablePage.CreateFromNotionPage));
        return masterTable;
    }
}