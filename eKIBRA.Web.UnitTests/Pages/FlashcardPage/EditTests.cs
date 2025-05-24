using System.Security.Claims;
using System.Text.Json;
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

public sealed class EditTests : IDisposable
{
    private readonly Mock<ILogger<EditModel>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly ApplicationDbContext _context;
    private readonly EditModel _pageModel;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationUser _anotherUser;

    public EditTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        _mockLogger = new Mock<ILogger<EditModel>>();

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
        _pageModel = new EditModel(
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
        // Create test decks
        var testDecks = new List<Deck>
        {
            new()
            {
                Id = "deck-1",
                UserId = _testUser.Id,
                Title = "Mathematics Deck",
                Description = "Math questions and answers"
            },
            new()
            {
                Id = "deck-2",
                UserId = _testUser.Id,
                Title = "Science Deck",
                Description = "Science questions and answers"
            },
            new()
            {
                Id = "deck-3",
                UserId = _testUser.Id,
                Title = "History Deck",
                Description = "Historical questions and answers"
            },
            new()
            {
                Id = "another-user-deck",
                UserId = _anotherUser.Id,
                Title = "Private Deck",
                Description = "Should not appear in search"
            },
            new()
            {
                Id = "existing-title-deck",
                UserId = _testUser.Id,
                Title = "Existing Deck Title",
                Description = "For testing duplicate scenarios"
            }
        };

        _context.Decks.AddRange(testDecks);

        // Create test flashcards
        var testFlashcards = new List<Flashcard>
        {
            new()
            {
                Id = "test-flashcard-id",
                UserId = _testUser.Id,
                DeckId = "deck-1",
                Question = "What is 2 + 2?",
                Answer = "4",
                Created = DateTime.UtcNow.AddDays(-5),
                Modified = DateTime.UtcNow.AddDays(-2),
                ModifierUserId = _testUser.Id,
                Incorrects = []
            },
            new()
            {
                Id = "another-flashcard-id",
                UserId = _anotherUser.Id,
                DeckId = "another-user-deck",
                Question = "Private Question",
                Answer = "Private Answer",
                Created = DateTime.UtcNow.AddDays(-3),
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifierUserId = _anotherUser.Id,
                Incorrects = []
            },
            new()
            {
                Id = "existing-question-flashcard",
                UserId = _testUser.Id,
                DeckId = "deck-2",
                Question = "Existing Question",
                Answer = "Existing Answer",
                Created = DateTime.UtcNow.AddDays(-4),
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifierUserId = _testUser.Id,
                Incorrects = []
            },
            new()
            {
                Id = "flashcard-with-partial-incorrects",
                UserId = _testUser.Id,
                DeckId = "deck-1",
                Question = "Question with fewer incorrects",
                Answer = "Answer",
                Incorrects = [],
                Created = DateTime.UtcNow.AddDays(-6),
                Modified = DateTime.UtcNow.AddDays(-3),
                ModifierUserId = _testUser.Id
            }
        };

        _context.Flashcards.AddRange(testFlashcards);
        _context.SaveChanges();
    }

    [Fact]
    public async Task OnGetSearchDeckAsync_WhenUserNotSignedIn_RedirectsToLogin()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);

        // Act
        var result = await _pageModel.OnGetSearchDeckAsync("Math");

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Account/Login", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetSearchDeckAsync_WhenUserNotFound_ReturnsPageWithErrorMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(null as ApplicationUser);

        // Act
        var result = await _pageModel.OnGetSearchDeckAsync("Math");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Your account was not found", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnGetSearchDeckAsync_WhenValidSearch_ReturnsMatchingDecks()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnGetSearchDeckAsync("Math");

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var jsonString = Assert.IsType<string>(jsonResult.Value);

        var searchResults = JsonSerializer.Deserialize<List<object>>(jsonString);
        Assert.NotNull(searchResults);
        Assert.Single(searchResults); // Should find "Mathematics Deck"
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
    public async Task OnGetAsync_WhenValidRequest_PopulatesInputWithFlashcardData()
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
        Assert.Equal("deck-1", _pageModel.Input.DeckId);
        Assert.Equal("Mathematics Deck", _pageModel.Input.DeckTitle);
        Assert.Equal("What is 2 + 2?", _pageModel.Input.Question);
    }

    [Fact]
    public async Task OnPostAsync_WhenModelStateInvalid_ReturnsPageWithErrorMessage()
    {
        // Arrange
        _pageModel.Input = new EditViewModel
        {
            Id = "test-flashcard-id",
            DeckId = "deck-1",
            Question = "",
            Anwser = "Valid Answer",
            DeckTitle = "Deck Title"
        };
        _pageModel.ModelState.AddModelError("Question", "Question is required");

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Invalid Flashcard. Check the required fields or try entering new values.",
            _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserNotSignedIn_RedirectsToLogin()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);
        _pageModel.Input = new EditViewModel
        {
            Id = "test-flashcard-id",
            DeckId = "deck-1",
            Question = "Updated Question",
            Anwser = "Updated Answer",
            DeckTitle = "Deck Title"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

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

        _pageModel.Input = new EditViewModel
        {
            Id = "test-flashcard-id",
            DeckId = "deck-1",
            Question = "Updated Question",
            Anwser = "Updated Answer",
            DeckTitle = "Deck Title"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

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
        _pageModel.Input = new EditViewModel
        {
            Id = "nonexistent-flashcard-id",
            DeckId = "deck-1",
            Question = "Updated Question",
            Anwser = "Updated Answer",
            DeckTitle = "Deck Title"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenValidRequest_UpdatesFlashcardSuccessfully()
    {
        // Arrange
        var originalModified = _context.Flashcards.First(f => f.Id == "test-flashcard-id").Modified;

        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        _pageModel.Input = new EditViewModel
        {
            Id = "test-flashcard-id",
            DeckId = "deck-1",
            Question = "Updated: What is 3 + 3?", // Changed Question
            Anwser = "6", // Changed Answer
            DeckTitle = "Deck Title",
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Flashcard updated", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Success), _pageModel.StatusMessage);

        // Verify database changes
        var updatedFlashcard = await _context.Flashcards.FindAsync("test-flashcard-id");
        Assert.NotNull(updatedFlashcard);
        Assert.Equal("Updated: What is 3 + 3?", updatedFlashcard.Question);
        Assert.Equal("6", updatedFlashcard.Answer);
        Assert.Equal(_testUser.Id, updatedFlashcard.ModifierUserId);
        Assert.True(updatedFlashcard.Modified > originalModified);
        Assert.Equal(updatedFlashcard.Modified, _pageModel.Input.Modified);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserTriesToEditAnotherUsersFlashcard_ReturnsWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        _pageModel.Input = new EditViewModel
        {
            Id = "another-flashcard-id", // This belongs to another user
            DeckId = "deck-1",
            Question = "Hacked Question",
            Anwser = "Hacked Answer",
            DeckTitle = "Deck Title"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);

        // Verify the other user's flashcard was not modified
        var otherUserFlashcard = await _context.Flashcards.FindAsync("another-flashcard-id");
        Assert.NotNull(otherUserFlashcard);
        Assert.Equal("Private Question", otherUserFlashcard.Question);
        Assert.Equal("Private Answer", otherUserFlashcard.Answer);
    }

    [Theory]
    [InlineData("", "Valid answer", "deck-1")]
    [InlineData("Valid question", "", "deck-1")]
    [InlineData("Valid question", "Valid answer", "")]
    public async Task OnPostAsync_WithInvalidInput_ReturnsPageWithError(string question, string answer, string deckId)
    {
        // Arrange
        _pageModel.Input = new EditViewModel
        {
            Id = "test-flashcard-id",
            Question = question,
            Anwser = answer,
            DeckId = deckId,
            DeckTitle = "Deck Title"
        };

        // Add appropriate model errors
        if (string.IsNullOrEmpty(question))
            _pageModel.ModelState.AddModelError("Question", "Question is required");
        if (string.IsNullOrEmpty(answer))
            _pageModel.ModelState.AddModelError("Answer", "Answer is required");
        if (string.IsNullOrEmpty(deckId))
            _pageModel.ModelState.AddModelError("DeckId", "Deck is required");

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Invalid Flashcard", _pageModel.StatusMessage);
        Assert.False(_pageModel.ModelState.IsValid);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}