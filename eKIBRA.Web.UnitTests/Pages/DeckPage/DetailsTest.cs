using System.Security.Claims;
using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.DeckPage;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace eKIBRA.Web.UnitTests.Pages.DeckPage;

public sealed class DetailsTest : IDisposable
{
    private readonly Mock<ILogger<DetailsModel>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly ApplicationDbContext _context;
    private readonly DetailsModel _pageModel;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationUser _anotherUser;

    public DetailsTest()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        _mockLogger = new Mock<ILogger<DetailsModel>>();

        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

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
        _pageModel = new DetailsModel(
            _mockLogger.Object,
            _context,
            _mockUserManager.Object,
            _mockSignInManager.Object);

        // Setup PageContext
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext { HttpContext = httpContext };
        _pageModel.PageContext = pageContext;

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var testDeck = new Deck
        {
            Id = "test-deck-id",
            UserId = _testUser.Id,
            Title = "Test Deck Details",
            Description = "Detailed description for testing the details page",
            Created = DateTime.UtcNow.AddDays(-10),
            Modified = DateTime.UtcNow.AddDays(-2),
            ModifierUserId = _testUser.Id
        };

        var anotherUserDeck = new Deck
        {
            Id = "another-deck-id",
            UserId = _anotherUser.Id,
            Title = "Another User's Deck",
            Description = "Should not be accessible to test user",
            Created = DateTime.UtcNow.AddDays(-5),
            Modified = DateTime.UtcNow.AddDays(-1),
            ModifierUserId = _anotherUser.Id
        };

        var testDeckWithFlashcards = new Deck
        {
            Id = "deck-with-flashcards-id",
            UserId = _testUser.Id,
            Title = "Deck With Flashcards",
            Description = "Test deck containing flashcards",
            Created = DateTime.UtcNow.AddDays(-7),
            Modified = DateTime.UtcNow.AddDays(-3),
            ModifierUserId = _testUser.Id,
            Flashcards =
            [
                new Flashcard
                {
                    Id = "flashcard-1",
                    Question = "What is the capital of France?",
                    Answer = "Paris",
                    DeckId = "deck-with-flashcards-id",
                    UserId = _testUser.Id,
                    Incorrects = []
                },

                new Flashcard
                {
                    Id = "flashcard-2",
                    Question = "What is 2 + 2?",
                    Answer = "4",
                    DeckId = "deck-with-flashcards-id",
                    UserId = _testUser.Id,
                    Incorrects = []
                }
            ]
        };

        _context.Decks.AddRange(testDeck, anotherUserDeck, testDeckWithFlashcards);
        _context.SaveChanges();
    }

    [Fact]
    public async Task OnGetAsync_WhenIdIsNull_ReturnsPageWithWarningMessage()
    {
        // Act
        var result = await _pageModel.OnGetAsync(null);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Required parameter [id] is missing", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
        Assert.Null(_pageModel.Input); // Input should remain null when id is missing
    }

    [Fact]
    public async Task OnGetAsync_WhenUserNotSignedIn_RedirectsToLogin()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);

        // Act
        var result = await _pageModel.OnGetAsync("test-deck-id");

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Account/Login", redirectResult.PageName);
        Assert.Equal("Identity", redirectResult.RouteValues?["area"]);
        Assert.Null(_pageModel.Input); // Input should remain null when not authenticated
    }

    [Fact]
    public async Task OnGetAsync_WhenUserNotFound_ReturnsPageWithErrorMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(null as ApplicationUser);

        // Act
        var result = await _pageModel.OnGetAsync("test-deck-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Your account was not found", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
        Assert.Null(_pageModel.Input); // Input should remain null when user not found
    }

    [Fact]
    public async Task OnGetAsync_WhenDeckNotFound_ReturnsPageWithWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnGetAsync("nonexistent-deck-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
        Assert.Null(_pageModel.Input); // Input should remain null when deck not found
    }

    [Fact]
    public async Task OnGetAsync_WhenValidDeck_ReturnsPageWithDeckData()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnGetAsync("test-deck-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(string.Empty, _pageModel.StatusMessage);
        Assert.NotNull(_pageModel.Input);
        Assert.Equal("test-deck-id", _pageModel.Input.Id);
        Assert.Equal("Test Deck Details", _pageModel.Input.Title);
        Assert.Equal("Detailed description for testing the details page", _pageModel.Input.Description);
        Assert.Equal(_testUser.Id, _pageModel.Input.UserId);
        Assert.True(_pageModel.Input.Created < DateTime.UtcNow);
        Assert.True(_pageModel.Input.Modified < DateTime.UtcNow);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}