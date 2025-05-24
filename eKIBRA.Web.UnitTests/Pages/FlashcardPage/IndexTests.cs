using System.Security.Claims;
using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.FlashcardPage;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace eKIBRA.Web.UnitTests.Pages.FlashcardPage;

public sealed class IndexTests : IDisposable
{
    private readonly Mock<ILogger<IndexModel>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IndexModel _pageModel;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationUser _anotherUser;

    public IndexTests()
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

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var baseDate = DateTime.UtcNow.AddDays(-30);

        // Create test decks first
        var testDecks = new List<Deck>
        {
            new()
            {
                Id = "deck-1",
                UserId = _testUser.Id,
                Title = "Deck 1",
                Description = "Math deck"
            },
            new()
            {
                Id = "deck-2",
                UserId = _testUser.Id,
                Title = "Deck 2",
                Description = "Science deck"
            },
            new()
            {
                Id = "deck-3",
                UserId = _testUser.Id,
                Title = "Deck 3",
                Description = "History deck"
            },
            new()
            {
                Id = "another-user-deck",
                UserId = _anotherUser.Id,
                Title = "Private Deck",
                Description = "Should not be visible"
            }
        };

        _context.Decks.AddRange(testDecks);

        // Create test flashcards
        var testFlashcards = new List<Flashcard>
        {
            new()
            {
                Id = "flashcard-1",
                UserId = _testUser.Id,
                DeckId = "deck-1",
                Question = "Question 1",
                Answer = "Answer 1",
                Created = baseDate.AddDays(1),
                Modified = baseDate.AddDays(6),
                Incorrects = []
            },
            new()
            {
                Id = "flashcard-2",
                UserId = _testUser.Id,
                DeckId = "deck-2",
                Question = "Question 2",
                Answer = "Answer 2",
                Created = baseDate.AddDays(2),
                Modified = baseDate.AddDays(5),
                Incorrects = []
            },
            new()
            {
                Id = "flashcard-3",
                UserId = _testUser.Id,
                DeckId = "deck-3",
                Question = "Question 3",
                Answer = "Answer 3",
                Created = baseDate.AddDays(3),
                Modified = baseDate.AddDays(4),
                Incorrects = []
            },
            new()
            {
                Id = "flashcard-4",
                UserId = _testUser.Id,
                DeckId = "deck-4",
                Question = "Question 4",
                Answer = "Answer 4",
                Created = baseDate.AddDays(4),
                Modified = baseDate.AddDays(3),
                Incorrects = []
            },
            new()
            {
                Id = "flashcard-5",
                UserId = _testUser.Id,
                DeckId = "deck-5",
                Question = "Question 5",
                Answer = "Answer 5",
                Created = baseDate.AddDays(5),
                Modified = baseDate.AddDays(2),
                Incorrects = []
            },
            new()
            {
                Id = "flashcard-6",
                UserId = _testUser.Id,
                DeckId = "deck-6",
                Question = "Question 6",
                Answer = "Answer 6",
                Created = baseDate.AddDays(6),
                Modified = baseDate.AddDays(1),
                Incorrects = []
            },
            // Another user's flashcard (should not appear in results)
            new()
            {
                Id = "other-flashcard",
                UserId = _anotherUser.Id,
                DeckId = "another-user-deck",
                Question = "Private question",
                Answer = "Private answer",
                Created = baseDate.AddDays(7),
                Modified = baseDate.AddDays(7),
                Incorrects = []
            }
        };

        _context.Flashcards.AddRange(testFlashcards);
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
    public async Task OnGetAsync_WhenValidUser_ReturnsUserFlashcardsOnly()
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
        Assert.Equal(1, _pageModel.Data.EntityList.TotalPages); // Only test user's flashcards
        Assert.All(_pageModel.Data.EntityList, flashcard => Assert.Equal(_testUser.Id, flashcard.UserId));
    }

    [Theory]
    [InlineData("title_desc", new[] { "Deck 3", "Deck 2" })]
    [InlineData("title", new[] { "Deck 1", "Deck 2", "Deck 3" })]
    [InlineData("question_desc", new[] { "Question 3", "Question 2" })]
    [InlineData("question", new[] { "Question 1", "Question 2", "Question 3" })]
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
        string[] actualResults;
        if (sortBy.Contains("title"))
        {
            actualResults = _pageModel.Data.EntityList.Take(expectedOrder.Length)
                .Select(f => f.LinkedDeck!.Title).ToArray();
        }
        else
        {
            actualResults = _pageModel.Data.EntityList.Take(expectedOrder.Length)
                .Select(f => f.Question).ToArray();
        }

        Assert.Equal(expectedOrder, actualResults);
    }


    public void Dispose()
    {
        _context.Dispose();
    }
}