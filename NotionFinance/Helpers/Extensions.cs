using Notion.Client;

namespace NotionFinance.Helpers;

public static class Extensions
{
    public static async Task ArchivePage(this INotionClient notionClient, Page page)
    {
        await notionClient.Pages.UpdateAsync(page.Id, new PagesUpdateParameters()
        {
            Archived = true
        });
    }

    public static async Task ArchivePages(this INotionClient notionClient, IEnumerable<Page> pages)
    {
        foreach (var page in pages)
        {
            await notionClient.ArchivePage(page);
        }
    }
}