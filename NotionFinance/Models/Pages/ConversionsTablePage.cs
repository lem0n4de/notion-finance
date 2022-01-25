using Notion.Client;
using Serilog;

namespace NotionFinance.Models.Pages;

public class ConversionsTablePage
{
    public string? NotionId { get; set; }
    public string? Name { get; set; }
    public AssetType? Type { get; set; }
    public string? Ticker { get; set; }
    public float? CurrentPricePerUnit { get; set; }
    public string? Unit { get; set; }
    public Page NotionPage { get; set; }

    public static ConversionsTablePage CreateFromNotionPage(Page page)
    {
        var tablePage = new ConversionsTablePage();
        tablePage.NotionPage = page;

        try
        {
            // ID
            tablePage.NotionId = page.Id;

            // NAME
            var namePropertyValue = page.Properties["Name"];
            tablePage.Name = namePropertyValue == null
                ? null
                : (namePropertyValue as TitlePropertyValue)!.Title[0].PlainText;

            // TYPE
            var selectPropertyValue = page.Properties["Type"];
            var typeString = selectPropertyValue == null
                ? null
                : (selectPropertyValue as SelectPropertyValue)!.Select.Name;

            tablePage.Type = typeString switch
            {
                "CRYPTO" => AssetType.Token,
                "FX" => AssetType.Fx,
                "COMMODITIES" => AssetType.Commodity,
                _ => null
            };

            // TICKER
            var tickerPropertyValue = page.Properties["Ticker"];
            tablePage.Ticker = tickerPropertyValue == null
                ? null
                : (tickerPropertyValue as RichTextPropertyValue)!.RichText[0].PlainText;

            // CURRENT PRICE
            var currentPricePropertyValue = page.Properties["Current Price"];
            tablePage.CurrentPricePerUnit = currentPricePropertyValue == null
                ? null
                : (float?) (currentPricePropertyValue as NumberPropertyValue)!.Number;

            // UNIT
            var unitPropertyValue = page.Properties["Unit"];
            tablePage.Unit = unitPropertyValue == null
                ? null
                : (unitPropertyValue as RichTextPropertyValue)!.RichText[0].PlainText;
        }
        catch (Exception e)
        {
            Log.Debug(e, "NotionPage -> Page conversion failed: {PageId}", page.Id);
        }

        return tablePage;
    }
}