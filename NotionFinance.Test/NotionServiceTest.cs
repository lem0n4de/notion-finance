using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Notion.Client;
using NotionFinance.Data;
using NotionFinance.Services;
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
}