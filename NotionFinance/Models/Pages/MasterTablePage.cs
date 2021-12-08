using Notion.Client;

namespace NotionFinance.Models.Pages;

public class MasterTablePage
{
    public string? NotionId { get; set; }
    public string? Name { get; set; }
    public MasterTablePageType? Type { get; set; }
    public string? Ticker { get; set; }
    public Date? EntryDate { get; set; }
    public float? InvestmentAmount { get; set; }
    public float? UnitPrice { get; set; }
    public float? CurrentPrice { get; set; }
    public MasterTablePageDirection? Direction { get; set; }
    public Page NotionPage { get; set; }

    public static MasterTablePage CreateFromNotionPage(Page page)
    {
        var tablePage = new MasterTablePage();
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
                "Token" => MasterTablePageType.Token,
                "FX" => MasterTablePageType.Fx,
                "IndividualStock" => MasterTablePageType.IndividualStock,
                "Bond ETF" => MasterTablePageType.BondEtf,
                "ETF" => MasterTablePageType.Etf,
                _ => null
            };

            // TICKER
            var tickerPropertyValue = page.Properties["Ticker"];
            tablePage.Ticker = tickerPropertyValue == null
                ? null
                : (tickerPropertyValue as RichTextPropertyValue)!.RichText[0].PlainText;

            // ENTRY DATE
            var entryDatePropertyValue = page.Properties["Entry Date"];
            tablePage.EntryDate = entryDatePropertyValue == null
                ? null
                : (entryDatePropertyValue as DatePropertyValue)!.Date;

            // INVESTMENT AMOUNT
            var investmentAmountPropertyValue = page.Properties["Investment Amount"];
            tablePage.InvestmentAmount = investmentAmountPropertyValue == null
                ? null
                : (float?) (investmentAmountPropertyValue as NumberPropertyValue)!.Number;

            // UNIT PRICE
            var unitPricePropertyValue = page.Properties["UNIT PRICE / Share (On Date of Purchase)"];
            tablePage.UnitPrice = unitPricePropertyValue == null
                ? null
                : (float?) (unitPricePropertyValue as NumberPropertyValue)!.Number;

            // CURRENT PRICE
            var currentPricePropertyValue = page.Properties["Current Price"];
            tablePage.CurrentPrice = currentPricePropertyValue == null
                ? null
                : (float?) (currentPricePropertyValue as NumberPropertyValue)!.Number;

            // DIRECTION
            var directionPropertyValue = page.Properties["Direction"];
            var directionString = directionPropertyValue == null
                ? null
                : (directionPropertyValue as SelectPropertyValue)!.Select.Name;
            tablePage.Direction = directionString switch
            {
                "Long" => MasterTablePageDirection.Long,
                "Short" => MasterTablePageDirection.Short,
                _ => null
            };
        }
        catch (Exception e)
        {
            // Ignored
        }

        return tablePage;
    }

    public enum MasterTablePageType
    {
        Token,
        Fx,
        Etf,
        BondEtf,
        IndividualStock
    }

    public enum MasterTablePageDirection
    {
        Long,
        Short
    }
}