using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Notion.Client;
using NotionFinance.Data;
using NotionFinance.Exceptions;
using NotionFinance.Services;
using Polly.Utilities;
using Xunit;

namespace NotionFinance.Tests;

public class NotionServiceTest
{
    [Theory, AutoMoqData]
    public async Task GetDatabasesAsyncReturnAllAvailableDatabases(Mock<INotionClient> notionClientMock,
        Mock<UserDbContext> userDbContextMock, [NotNull] PaginatedList<Database> dbs)
    {
        // Arrange
        var returnVal = new PaginatedList<IObject>() {Results = new List<IObject>(dbs.Results)};
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>())).ReturnsAsync(returnVal);
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);

        // Act
        var databases = await notionService.GetDatabasesAsync();

        // Assert
        notionClientMock.Verify(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()), Times.Once);
        databases.Should().NotBeEmpty();
        databases.Should().NotContainNulls();
        databases.Should().BeEquivalentTo(dbs.Results);
    }

    [Theory, AutoMoqData]
    public async Task GetDatabasesAsyncReturnEmptyListIfNoDatabaseAvailable(Mock<INotionClient> notionClientMock,
        Mock<UserDbContext> userDbContextMock)
    {
        // Arrange
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>() {Results = new List<IObject>()});
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);

        // Act
        var databases = await notionService.GetDatabasesAsync();

        // Assert
        notionClientMock.Verify(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()), Times.Once);
        databases.Should().BeEmpty();
    }

    [Theory, AutoMoqData]
    public async Task GetDatabasesAsyncShouldNotContainAnyNullValues(Mock<INotionClient> notionClientMock,
        Mock<UserDbContext> userDbContextMock, Mock<Database> database1)
    {
        // Arrange
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
                {Results = new List<IObject>(new List<Database>() {database1.Object, null})});
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);

        // Act
        var databases = await notionService.GetDatabasesAsync();

        // Assert
        notionClientMock.Verify(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()), Times.Once);
        databases.Should().NotBeEmpty();
        databases.Should().HaveCount(1);
        databases.Should().NotContainNulls();
    }

    [Theory, AutoMoqData]
    public async Task GetDatabasesAsyncDoesNotReturnMultipleDatabasesWithTheSameId(
        Mock<UserDbContext> userDbContextMock, Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var database1 = new Database() {Id = Guid.NewGuid().ToString()};
        var database2 = new Database() {Id = Guid.NewGuid().ToString()};
        var database3 = new Database() {Id = database1.Id};
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Database>() {database1, database2, database3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);

        // Act
        var databases = await notionService.GetDatabasesAsync();

        // Assert
        notionClientMock.Verify(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()), Times.Once);
        databases.Should().NotBeEmpty();
        databases.Should().OnlyHaveUniqueItems(x => x.Id);
        databases.Should().HaveCount(2);
    }

    [Theory, AutoMoqData]
    public async Task GetDatabaseByIdAsyncThrowsIfDatabaseNotFound(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var database1 = new Database() {Id = Guid.NewGuid().ToString()};
        var database2 = new Database() {Id = Guid.NewGuid().ToString()};
        var database3 = new Database() {Id = Guid.NewGuid().ToString()};
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Database>() {database1, database2, database3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act & Assert
        await notionService.Awaiting(x => x.GetDatabaseByIdAsync(Guid.NewGuid().ToString())).Should()
            .ThrowAsync<NotionDatabaseNotFoundException>();
    }

    [Theory, AutoMoqData]
    public async Task GetDatabaseByIdAsyncReturnsCorrectDatabase(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var database1 = new Database() {Id = Guid.NewGuid().ToString()};
        var database2 = new Database() {Id = Guid.NewGuid().ToString()};
        var database3 = new Database() {Id = Guid.NewGuid().ToString()};
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Database>() {database1, database2, database3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act
        var database = await notionService.GetDatabaseByIdAsync(database1.Id);
        // Assert
        database.Should().NotBeNull();
        database.Should().BeEquivalentTo(database1);
    }

    [Theory, AutoMoqData]
    public async Task GetDatabaseByNameAsyncThrowsIfDatabaseNotFound(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var database1 = new Database()
        {
            Id = Guid.NewGuid().ToString(),
            Title = new List<RichTextBase>()
                {new RichTextText() {Text = new Text() {Content = "Database1", Link = null}}}
        };
        var database2 = new Database()
        {
            Id = Guid.NewGuid().ToString(),
            Title = new List<RichTextBase>()
                {new RichTextText() {Text = new Text() {Content = "Database2", Link = null}}}
        };
        var database3 = new Database()
        {
            Id = Guid.NewGuid().ToString(),
            Title = new List<RichTextBase>()
                {new RichTextText() {Text = new Text() {Content = "Database3", Link = null}}}
        };
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Database>() {database1, database2, database3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act & Assert
        await notionService.Awaiting(x => x.GetDatabaseByNameAsync("")).Should()
            .ThrowAsync<NotionDatabaseNotFoundException>();
    }

    [Theory, AutoMoqData]
    public async Task GetDatabaseByNameAsyncReturnsCorrectDatabase(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var database1 = new Database()
        {
            Id = Guid.NewGuid().ToString(),
            Title = new List<RichTextBase>()
                {new RichTextText() {Text = new Text() {Content = "Database1", Link = null}}}
        };
        var database2 = new Database()
        {
            Id = Guid.NewGuid().ToString(),
            Title = new List<RichTextBase>()
                {new RichTextText() {Text = new Text() {Content = "Database2", Link = null}}}
        };
        var database3 = new Database()
        {
            Id = Guid.NewGuid().ToString(),
            Title = new List<RichTextBase>()
                {new RichTextText() {Text = new Text() {Content = "Database3", Link = null}}}
        };
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Database>() {database1, database2, database3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act
        var database = await notionService.GetDatabaseByNameAsync(database1.Title[0].PlainText);
        // Assert
        database.Should().NotBeNull();
        database.Should().BeEquivalentTo(database1);
    }

    [Theory, AutoMoqData]
    public async Task GetPagesAsyncReturnAllAvailablePages(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock, PaginatedList<Page> pgs)
    {
        // Arrange
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>() {Results = pgs.Results.Cast<IObject>().ToList()});
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act
        var pages = await notionService.GetPagesAsync();
        // Assert
        notionClientMock.Verify(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()), Times.Once);
        pages.Should().NotBeEmpty();
        pages.Should().NotContainNulls();
        pages.Should().BeEquivalentTo(pgs.Results);
    }

    [Theory, AutoMoqData]
    public async Task GetPagesAsyncReturnEmptyListIfNoPageAvailable(Mock<INotionClient> notionClientMock,
        Mock<UserDbContext> userDbContextMock)
    {
        // Arrange
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>() {Results = new List<IObject>()});
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);

        // Act
        var pages = await notionService.GetPagesAsync();

        // Assert
        notionClientMock.Verify(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()), Times.Once);
        pages.Should().BeEmpty();
    }

    [Theory, AutoMoqData]
    public async Task GetPagesAsyncShouldNotContainAnyNullValues(Mock<INotionClient> notionClientMock,
        Mock<UserDbContext> userDbContextMock, Mock<Page> page1)
    {
        // Arrange
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
                {Results = new List<IObject>(new List<Page>() {page1.Object, null})});
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);

        // Act
        var pages = await notionService.GetPagesAsync();

        // Assert
        notionClientMock.Verify(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()), Times.Once);
        pages.Should().NotBeEmpty();
        pages.Should().HaveCount(1);
        pages.Should().NotContainNulls();
    }

    [Theory, AutoMoqData]
    public async Task GetPagesAsyncDoesNotReturnMultiplePagesWithTheSameId(
        Mock<UserDbContext> userDbContextMock, Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var page1 = new Page() {Id = Guid.NewGuid().ToString()};
        var page2 = new Page() {Id = Guid.NewGuid().ToString()};
        var page3 = new Page() {Id = page1.Id};
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Page>() {page1, page2, page3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);

        // Act
        var pages = await notionService.GetPagesAsync();

        // Assert
        notionClientMock.Verify(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()), Times.Once);
        pages.Should().NotBeEmpty();
        pages.Should().OnlyHaveUniqueItems(x => x.Id);
        pages.Should().HaveCount(2);
    }

    [Theory, AutoMoqData]
    public async Task GetPageByIdAsyncThrowsIfNoPageFound(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>()
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act & Assert
        await notionService.Awaiting(x => x.GetPageByIdAsync("some page id")).Should()
            .ThrowAsync<NotionPageNotFoundException>();
    }

    [Theory, AutoMoqData]
    public async Task GetPageByIdAsyncReturnsCorrectPage(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var page1 = new Page() {Id = Guid.NewGuid().ToString()};
        var page2 = new Page() {Id = Guid.NewGuid().ToString()};
        var page3 = new Page() {Id = Guid.NewGuid().ToString()};
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Page>() {page1, page2, page3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act
        var page = await notionService.GetPageByIdAsync(page2.Id);
        // Assert
        page.Should().NotBeNull();
        page.Should().BeEquivalentTo(page2);
    }

    [Theory, AutoMoqData]
    public async Task GetPageByNameThrowsIfNoPageAvailable(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>()
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act & Assert
        await notionService.Awaiting(x => x.GetPageByNameAsync("some page name")).Should()
            .ThrowAsync<NotionPageNotFoundException>();
    }

    [Theory, AutoMoqData]
    public async Task GetPageByNameReturnsCorrectPage(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var page1 = new Page()
        {
            Id = Guid.NewGuid().ToString(),
            Properties = new Dictionary<string, PropertyValue>()
            {
                {
                    "title",
                    new TitlePropertyValue()
                    {
                        Title = new List<RichTextBase>()
                        {
                            new RichTextText()
                                {PlainText = "title1", Text = new Text() {Content = "title1", Link = null}}
                        }
                    }
                }
            }
        };
        var page2 = new Page()
        {
            Id = Guid.NewGuid().ToString(),
            Properties = new Dictionary<string, PropertyValue>()
            {
                {
                    "title",
                    new TitlePropertyValue()
                    {
                        Title = new List<RichTextBase>()
                        {
                            new RichTextText()
                                {PlainText = "title2", Text = new Text() {Content = "title2", Link = null}}
                        }
                    }
                }
            }
        };
        var page3 = new Page()
        {
            Id = Guid.NewGuid().ToString(),
            Properties = new Dictionary<string, PropertyValue>()
            {
                {
                    "title",
                    new TitlePropertyValue()
                    {
                        Title = new List<RichTextBase>()
                        {
                            new RichTextText()
                                {PlainText = "title3", Text = new Text() {Content = "title3", Link = null}}
                        }
                    }
                }
            }
        };
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Page>() {page1, page2, page3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act
        var page = await notionService.GetPageByNameAsync("title3");
        // Assert
        page.Should().NotBeNull();
        page.Should().BeEquivalentTo(page3);
    }

    [Theory, AutoMoqData]
    public async Task GetPagesByDatabaseAsyncReturnsCorrectPages(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var database1 = new Database() {Id = Guid.NewGuid().ToString()};
        var database2 = new Database() {Id = Guid.NewGuid().ToString()};
        var page1 = new Page()
        {
            Id = Guid.NewGuid().ToString(),
            Parent = new DatabaseParent() {DatabaseId = database1.Id, Type = ParentType.DatabaseId}
        };
        var page2 = new Page()
        {
            Id = Guid.NewGuid().ToString(),
            Parent = new DatabaseParent() {DatabaseId = database1.Id, Type = ParentType.DatabaseId}
        };
        var page3 = new Page()
        {
            Id = Guid.NewGuid().ToString(),
            Parent = new DatabaseParent() {DatabaseId = database2.Id, Type = ParentType.DatabaseId}
        };
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Page>() {page1, page2, page3})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act
        var pages = await notionService.GetPagesByDatabaseAsync(database1.Id);
        // Assert
        pages.Should().NotBeNull();
        pages.Should().NotBeEmpty();
        pages.Should().HaveCount(2);
        pages.Should().Contain(page1);
        pages.Should().Contain(page2);
        pages.Should().NotContain(page3);
    }

    [Theory, AutoMoqData]
    public async Task GetPagesByDatabaseAsyncReturnsEmptyListIfNoPageAvailable(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var database1 = new Database() {Id = Guid.NewGuid().ToString()};
        var database2 = new Database() {Id = Guid.NewGuid().ToString()};
        var page1 = new Page()
        {
            Id = Guid.NewGuid().ToString(),
            Parent = new DatabaseParent() {DatabaseId = database1.Id, Type = ParentType.DatabaseId}
        };
        var page2 = new Page()
        {
            Id = Guid.NewGuid().ToString(),
            Parent = new DatabaseParent() {DatabaseId = database1.Id, Type = ParentType.DatabaseId}
        };
        notionClientMock.Setup(x => x.Search.SearchAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(new PaginatedList<IObject>()
            {
                Results = new List<IObject>(new List<Page>() {page1, page2})
            });
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act
        var pages = await notionService.GetPagesByDatabaseAsync(database2.Id);
        // Assert
        await notionService.Awaiting(x => x.GetPagesByDatabaseAsync(database2.Id)).Should().NotThrowAsync();
        pages.Should().NotBeNull();
        pages.Should().BeEmpty();
    }

    [Theory, AutoMoqData]
    public async Task UpdatePageAsyncShouldCallCorrectMethod(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var page = new Page() {Id = Guid.NewGuid().ToString(), IsArchived = true};
        var pageUpdateParameters = new PagesUpdateParameters() {Archived = false};
        notionClientMock.Setup(x =>
                x.Pages.UpdateAsync(It.Is<string>(y => y == page.Id), It.IsAny<PagesUpdateParameters>()))
            .ReturnsAsync(new Page() {Id = page.Id, IsArchived = pageUpdateParameters.Archived});
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act
        var p = await notionService.UpdatePageAsync(page, pageUpdateParameters);
        // Assert
        p.Should().NotBeNull();
        p.Should().NotBeEquivalentTo(page);
        p.Id.Should().Be(page.Id);
    }
    
    [Theory, AutoMoqData]
    public async Task UpdatePageAsyncShouldTryAtLeastFiveSecondsIfNotionThrewApiException(Mock<UserDbContext> userDbContextMock,
        Mock<INotionClient> notionClientMock)
    {
        // Arrange
        var page = new Page() {Id = Guid.NewGuid().ToString(), IsArchived = true};
        var pageUpdateParameters = new PagesUpdateParameters() {Archived = false};
        notionClientMock.Setup(x =>
                x.Pages.UpdateAsync(It.Is<string>(y => y == page.Id), It.IsAny<PagesUpdateParameters>()))
            .ThrowsAsync(new NotionApiException(HttpStatusCode.BadRequest, NotionAPIErrorCode.RateLimited, "Rate Limited"));
        var notionService = new NotionService(userDbContextMock.Object, notionClientMock.Object);
        // Act & Assert
        var time = DateTime.Now;
        await notionService.Awaiting(x => x.UpdatePageAsync(page, pageUpdateParameters)).Should()
            .ThrowAsync<NotionApiException>();
        time.Should().BeAtLeast(TimeSpan.FromSeconds(5)).Before(DateTime.Now);
    }
}