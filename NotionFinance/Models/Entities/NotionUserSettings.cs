using System.ComponentModel.DataAnnotations;

namespace NotionFinance.Models;

public class NotionUserSettings
{
    public long NotionUserSettingsId { get; set; }
    public string? NotionId { get; set; }
    public string? AuthorizedWorkspaceId { get; set; }
    public string? NotionAccessToken { get; set; }
    public string? MasterDatabaseName { get; set; }
    public bool MasterDatabaseExists { get; set; }
    public string? HomePageName { get; set; }
    public DateTime MasterDatabaseCreationTime { get; set; }
    public int MasterDatabaseFetchingFailedCount { get; set; } = 0;
}