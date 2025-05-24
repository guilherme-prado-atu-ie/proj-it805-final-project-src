using System.Security.Claims;
using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.FlashcardPage;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace eKIBRA.Web.UnitTests.Pages.FlashcardPage;

public sealed class DeleteTests : IDisposable
{
    private readonly Mock<ILogger<DeleteModel>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly ApplicationDbContext _context;
    private readonly DeleteModel _pageModel;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationUser _anotherUser;

    public DeleteTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        _mockLogger = new Mock<ILogger<DeleteModel>>();

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
        _pageModel = new DeleteModel(
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
        // Create test decks first
        var testDeck = new Deck
        {
            Id = "test-deck-id",
            UserId = _testUser.Id,
            Title = "Test Deck",
            Description = "Deck for flashcard testing"
        };

        var anotherUserDeck = new Deck
        {
            Id = "another-deck-id",
            UserId = _anotherUser.Id,
            Title = "Another User's Deck",
            Description = "Should not be accessible"
        };

        _context.Decks.AddRange(testDeck, anotherUserDeck);

        // Create test flashcards
        var testFlashcard = new Flashcard
        {
            Id = "test-flashcard-id",
            UserId = _testUser.Id,
            DeckId = "test-deck-id",
            Question = "What is the capital of France?",
            Answer = "Paris",
            Incorrects = new List<string> { "London", "Berlin", "Madrid" },
            Created = DateTime.UtcNow.AddDays(-5),
            Modified = DateTime.UtcNow.AddDays(-2)
        };

        var anotherUserFlashcard = new Flashcard
        {
            Id = "another-flashcard-id",
            UserId = _anotherUser.Id,
            DeckId = "another-deck-id",
            Question = "Private Question",
            Answer = "Private Answer",
            Created = DateTime.UtcNow.AddDays(-3),
            Modified = DateTime.UtcNow.AddDays(-1),
            Incorrects = []
        };

        var flashcardWithComplexData = new Flashcard
        {
            Id = "complex-flashcard-id",
            UserId = _testUser.Id,
            DeckId = "test-deck-id",
            Question = "Complex question with special characters!@#$%",
            Answer = "Complex answer with unicode: αβγδε",
            Incorrects = ["Wrong 1", "Wrong 2"],
            Created = DateTime.UtcNow.AddDays(-7),
            Modified = DateTime.UtcNow.AddDays(-3)
        };

        _context.Flashcards.AddRange(testFlashcard, anotherUserFlashcard, flashcardWithComplexData);
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
    }

    [Fact]
    public async Task OnGetAsync_WhenFlashcardNotFound_ReturnsPageWithWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnGetAsync("nonexistent-flashcard-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenFlashcardBelongsToAnotherUser_ReturnsPageWithWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnGetAsync("another-flashcard-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidRequest_ReturnsPageWithFlashcardData()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnGetAsync("test-flashcard-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(string.Empty, _pageModel.StatusMessage);
        Assert.NotNull(_pageModel.Input);
        Assert.Equal("test-flashcard-id", _pageModel.Input.Id);
        Assert.Equal("What is the capital of France?", _pageModel.Input.Question);
        Assert.Equal("Paris", _pageModel.Input.Answer);
        Assert.Equal(_testUser.Id, _pageModel.Input.UserId);
        Assert.Equal("test-deck-id", _pageModel.Input.DeckId);
    }

    [Fact]
    public async Task OnPostAsync_WhenIdIsNull_ReturnsPageWithWarningMessage()
    {
        // Act
        var result = await _pageModel.OnPostAsync(null);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Required parameter [id] is missing", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserNotSignedIn_RedirectsToLogin()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);

        // Act
        var result = await _pageModel.OnPostAsync("test-flashcard-id");

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Account/Login", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserNotFound_ReturnsPageWithErrorMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(null as ApplicationUser);

        // Act
        var result = await _pageModel.OnPostAsync("test-flashcard-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Your account was not found", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenFlashcardNotFound_ReturnsPageWithWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnPostAsync("nonexistent-flashcard-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenValidRequest_SoftDeletesFlashcardSuccessfully()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnPostAsync("test-flashcard-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record was removed successfully", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Success), _pageModel.StatusMessage);
        Assert.Null(_pageModel.Input);

        // Verify soft deletion in database
        var deletedFlashcard = await _context.Flashcards.FindAsync("test-flashcard-id");
        Assert.NotNull(deletedFlashcard);
        Assert.True(deletedFlashcard.IsDeleted);
        Assert.StartsWith("Deleted ", deletedFlashcard.Question);
        Assert.Contains("test-flashcard-id", deletedFlashcard.Question);
    }

    [Fact]
    public async Task OnPostAsync_WhenFlashcardBelongsToAnotherUser_ReturnsPageWithWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnPostAsync("another-flashcard-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);

        // Verify the other user's flashcard was not modified
        var otherUserFlashcard = await _context.Flashcards.FindAsync("another-flashcard-id");
        Assert.NotNull(otherUserFlashcard);
        Assert.False(otherUserFlashcard.IsDeleted);
        Assert.Equal("Private Question", otherUserFlashcard.Question);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}