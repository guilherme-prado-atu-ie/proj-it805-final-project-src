using System.Security.Claims;
using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.DeckPage;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace eKIBRA.Web.UnitTests.Pages.DeckPage;

public sealed class IndexTest : IDisposable
{
    private readonly Mock<ILogger<IndexModel>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IndexModel _pageModel;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationUser _anotherUser;


    public IndexTest()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        _mockLogger = new Mock<ILogger<IndexModel>>();

        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

        // Create real configuration with test values
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "PageSize", "5" } // Set page size for testing
            }!)
            .Build();

        // Create test users
        _testUser = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            IsDeleted = false
        };

        _anotherUser = new ApplicationUser
        {
            Id = "another-user-id",
            UserName = "anotheruser@example.com",
            Email = "anotheruser@example.com",
            IsDeleted = false
        };

        // Create page model
        _pageModel = new IndexModel(
            _mockLogger.Object,
            _context,
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _configuration);

        // Setup PageContext
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext { HttpContext = httpContext };
        _pageModel.PageContext = pageContext;

        SeedTestData();
    }

    private void SeedTestData()
    {
        var baseDate = DateTime.UtcNow.AddDays(-30);

        var testDecks = new List<Deck>
        {
            new()
            {
                Id = "deck-1",
                UserId = _testUser.Id,
                Title = "Deck 1",
                Description = "English related deck",
                Created = baseDate.AddDays(1),
                Modified = baseDate.AddDays(6)
            },
            new()
            {
                Id = "deck-2",
                UserId = _testUser.Id,
                Title = "Deck 2",
                Description = "Azure related deck",
                Created = baseDate.AddDays(2),
                Modified = baseDate.AddDays(5)
            },
            new()
            {
                Id = "deck-3",
                UserId = _testUser.Id,
                Title = "Deck 3",
                Description = "AWS related deck",
                Created = baseDate.AddDays(3),
                Modified = baseDate.AddDays(4)
            },
            new()
            {
                Id = "deck-4",
                UserId = _testUser.Id,
                Title = "Deck 4",
                Description = "Programming related deck",
                Created = baseDate.AddDays(4),
                Modified = baseDate.AddDays(3)
            },
            new()
            {
                Id = "deck-5",
                UserId = _testUser.Id,
                Title = "Deck 5",
                Description = "Science related deck",
                Created = baseDate.AddDays(5),
                Modified = baseDate.AddDays(2)
            },
            new()
            {
                Id = "deck-6",
                UserId = _testUser.Id,
                Title = "Deck 6",
                Description = "Mathematics related deck",
                Created = baseDate.AddDays(6),
                Modified = baseDate.AddDays(1)
            },
            // Another user's decks (should not appear in results)
            new()
            {
                Id = "other-deck-1",
                UserId = _anotherUser.Id,
                Title = "Other User Deck",
                Description = "Should not be visible",
                Created = baseDate.AddDays(7),
                Modified = baseDate.AddDays(7)
            }
        };

        _context.Decks.AddRange(testDecks);
        _context.SaveChanges();
    }

    [Fact]
    public async Task OnGetAsync_WhenUserNotSignedIn_RedirectsAndSetsEmptyPaginatedList()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);

        // Act
        await _pageModel.OnGetAsync(null, null, null, null, null, null);

        // Assert
        Assert.NotNull(_pageModel.Data.EntityList);
        Assert.Empty(_pageModel.Data.EntityList);
        Assert.Equal(0, _pageModel.Data.EntityList.PageIndex);
        Assert.Equal(0, _pageModel.Data.EntityList.TotalPages);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserNotFound_SetsErrorMessageAndEmptyList()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(null as ApplicationUser);

        // Act
        await _pageModel.OnGetAsync(null, null, null, null, null, null);

        // Assert
        Assert.Contains("Your account was not found", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
        Assert.NotNull(_pageModel.Data.EntityList);
        Assert.Empty(_pageModel.Data.EntityList);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidUser_ReturnsUserDecksOnly()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        await _pageModel.OnGetAsync(null, null, null, null, null, null);

        // Assert
        Assert.Equal(string.Empty, _pageModel.StatusMessage);
        Assert.NotNull(_pageModel.Data.EntityList);
        Assert.Equal(2, _pageModel.Data.EntityList.TotalPages); // Only test user's decks
        Assert.All(_pageModel.Data.EntityList, deck => Assert.Equal(_testUser.Id, deck.UserId));
    }

    [Theory]
    [InlineData("title_desc", new[] { "Deck 6", "Deck 5", "Deck 4", "Deck 3", "Deck 2" })]
    [InlineData("title", new[] { "Deck 1", "Deck 2", "Deck 3", "Deck 4", "Deck 5" })]
    [InlineData("created_desc", new[] { "Deck 6", "Deck 5", "Deck 4", "Deck 3", "Deck 2" })]
    [InlineData("Created", new[] { "Deck 1", "Deck 2", "Deck 3", "Deck 4", "Deck 5" })]
    public async Task OnGetAsync_WithDifferentSorting_ReturnsCorrectOrder(string sortBy, string[] expectedOrder)
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        await _pageModel.OnGetAsync(sortBy, null, null, null, null, null);

        // Assert
        var actualTitles = _pageModel.Data.EntityList.Take(expectedOrder.Length).Select(d => d.Title).ToArray();
        Assert.Equal(expectedOrder, actualTitles);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}