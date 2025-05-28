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

public sealed class CreateTests : IDisposable
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly ApplicationDbContext _context;
    private readonly CreateModel _pageModel;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationUser _anotherUser;

    public CreateTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        var mockLogger = new Mock<ILogger<CreateModel>>();

        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null!, null!, null!, null!);

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
        _pageModel = new CreateModel(
            mockLogger.Object,
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
        var testDecks = new List<Deck>
        {
            new Deck
            {
                Id = "deck-1",
                UserId = _testUser.Id,
                Title = "Mathematics Deck",
                Description = "Math questions and answers"
            },
            new Deck
            {
                Id = "deck-2",
                UserId = _testUser.Id,
                Title = "Science Deck",
                Description = "Science questions and answers"
            },
            new Deck
            {
                Id = "deck-3",
                UserId = _testUser.Id,
                Title = "History Deck",
                Description = "Historical questions and answers"
            },
            new Deck
            {
                Id = "another-user-deck",
                UserId = _anotherUser.Id,
                Title = "Private Deck",
                Description = "Should not appear in search"
            }
        };

        var existingFlashcard = new Flashcard
        {
            Id = "existing-flashcard",
            UserId = _testUser.Id,
            DeckId = "deck-1",
            Question = "Existing Question",
            Answer = "Existing Answer",
            Incorrects = []
        };

        _context.Decks.AddRange(testDecks);
        _context.Flashcards.Add(existingFlashcard);
        _context.SaveChanges();
    }

    [Fact]
    public void OnGet_WhenUserNotSignedIn_RedirectsToLogin()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);

        // Act
        var result = _pageModel.OnGet();

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Account/Login", redirectResult.PageName);
        Assert.Equal("Identity", redirectResult.RouteValues?["area"]);
    }

    [Fact]
    public void OnGet_WhenUserSignedIn_ReturnsPageResult()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);

        // Act
        var result = _pageModel.OnGet();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(string.Empty, _pageModel.StatusMessage);
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

        // Deserialize to verify content
        var searchResults = JsonSerializer.Deserialize<List<object>>(jsonString);
        Assert.NotNull(searchResults);
        Assert.Single(searchResults); // Should find "Mathematics Deck"
    }

    [Fact]
    public async Task OnGetSearchDeckAsync_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnGetSearchDeckAsync("NonExistent");

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var jsonString = Assert.IsType<string>(jsonResult.Value);

        var searchResults = JsonSerializer.Deserialize<List<object>>(jsonString);
        Assert.NotNull(searchResults);
        Assert.Empty(searchResults);
    }

    [Fact]
    public async Task OnPostAsync_WhenModelStateInvalid_ReturnsPageWithErrorMessage()
    {
        // Arrange
        _pageModel.Input = new CreateViewModel
        {
            DeckId = "deck-1",
            Question = "",
            Anwser = "",
            DeckTitle = "Mathematics Deck"
        };
        _pageModel.ModelState.AddModelError("Question", "Question is required");

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Invalid Deck. Check the required fields or try entering new values.",
            _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserNotSignedIn_RedirectsToLogin()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);
        _pageModel.Input = new CreateViewModel
        {
            DeckId = "deck-1",
            Question = "Test Question",
            Anwser = "Test Answer",
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

        _pageModel.Input = new CreateViewModel
        {
            DeckId = "deck-1",
            Question = "Test Question",
            Anwser = "Test Answer",
            DeckTitle = ""
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Your account was not found", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenValidInput_CreatesFlashcardSuccessfully()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        _pageModel.Input = new CreateViewModel
        {
            DeckId = "deck-1",
            Question = "What is 2 + 2?",
            Anwser = "4",
            DeckTitle = "Mathematics Deck"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Flashcard created", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Success), _pageModel.StatusMessage);

        // Verify flashcard was created in database
        var createdFlashcard = await _context.Flashcards
            .FirstOrDefaultAsync(f => f.Question == "What is 2 + 2?");

        Assert.NotNull(createdFlashcard);
        Assert.Equal("4", createdFlashcard.Answer);
        Assert.Equal("deck-1", createdFlashcard.DeckId);
        Assert.Equal(_testUser.Id, createdFlashcard.UserId);
    }

    [Theory]
    [InlineData("", "Valid Answer")]
    [InlineData("Valid Question", "")]
    [InlineData("", "")]
    public async Task OnPostAsync_WithInvalidInput_ReturnsPageWithError(string question, string answer)
    {
        // Arrange
        _pageModel.Input = new CreateViewModel
        {
            DeckId = "deck-1",
            Question = question,
            Anwser = answer,
            DeckTitle = "Mathematics Deck"
        };

        // Add appropriate model errors
        if (string.IsNullOrEmpty(question))
            _pageModel.ModelState.AddModelError("Question", "Question is required");
        if (string.IsNullOrEmpty(answer))
            _pageModel.ModelState.AddModelError("Anwser", "Answer is required");

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Invalid Deck", _pageModel.StatusMessage);
        Assert.False(_pageModel.ModelState.IsValid);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}