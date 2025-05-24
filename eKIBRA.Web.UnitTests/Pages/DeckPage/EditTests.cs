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
        var testDeck = new Deck
        {
            Id = "test-deck-id",
            UserId = _testUser.Id,
            Title = "Original Test Deck",
            Description = "Original test deck description",
            Created = DateTime.UtcNow.AddDays(-5),
            Modified = DateTime.UtcNow.AddDays(-2),
            ModifierUserId = _testUser.Id
        };

        var anotherUserDeck = new Deck
        {
            Id = "another-deck-id",
            UserId = _anotherUser.Id,
            Title = "Another User's Deck",
            Description = "Should not be accessible",
            Created = DateTime.UtcNow.AddDays(-3),
            Modified = DateTime.UtcNow.AddDays(-1),
            ModifierUserId = _anotherUser.Id
        };

        var existingDeckForDuplicateTest = new Deck
        {
            Id = "existing-deck-id",
            UserId = _testUser.Id,
            Title = "Existing Deck Title",
            Description = "For testing duplicate title scenario",
            Created = DateTime.UtcNow.AddDays(-4),
            Modified = DateTime.UtcNow.AddDays(-1),
            ModifierUserId = _testUser.Id
        };

        _context.Decks.AddRange(testDeck, anotherUserDeck, existingDeckForDuplicateTest);
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
    }

    [Fact]
    public async Task OnGetAsync_WhenDeckBelongsToAnotherUser_ReturnsPageWithWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _pageModel.OnGetAsync("another-deck-id");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidRequest_PopulatesInputWithDeckData()
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
        Assert.Equal("Original Test Deck", _pageModel.Input.Title);
        Assert.Equal("Original test deck description", _pageModel.Input.Description);
        Assert.True(_pageModel.Input.Created < DateTime.UtcNow);
        Assert.True(_pageModel.Input.Modified < DateTime.UtcNow);
    }

    [Theory]
    [InlineData("", "Valid description")]
    [InlineData("Valid Title", "")]
    [InlineData("", "")]
    public async Task OnPostAsync_WhenModelStateInvalid_ReturnsPageWithErrorMessage(string title, string description)
    {
        // Arrange
        _pageModel.Input = new EditViewModel
        {
            Id = "test-deck-id",
            Title = title,
            Description = description
        };

        // Add appropriate model errors
        if (string.IsNullOrEmpty(title))
            _pageModel.ModelState.AddModelError("Title", "Title is required");
        if (string.IsNullOrEmpty(description))
            _pageModel.ModelState.AddModelError("Description", "Description is required");

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Invalid Deck", _pageModel.StatusMessage);
        Assert.False(_pageModel.ModelState.IsValid);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserNotSignedIn_RedirectsToLogin()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);
        _pageModel.Input = new EditViewModel
        {
            Id = "test-deck-id",
            Title = "Updated Title",
            Description = "Updated Description"
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
            Id = "test-deck-id",
            Title = "Updated Title",
            Description = "Updated Description"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Your account was not found", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenDeckNotFound_ReturnsPageWithWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);
        _pageModel.Input = new EditViewModel
        {
            Id = "nonexistent-deck-id",
            Title = "Updated Title",
            Description = "Updated Description"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenValidRequest_UpdatesDeckSuccessfully()
    {
        // Arrange
        var originalModified = _context.Decks.First(d => d.Id == "test-deck-id").Modified;

        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        _pageModel.Input = new EditViewModel
        {
            Id = "test-deck-id",
            Title = "Updated Test Deck",
            Description = "Updated test deck description",
            Created = DateTime.UtcNow.AddDays(-5), // Should not change
            Modified = originalModified // Should be updated
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Deck updated", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Success), _pageModel.StatusMessage);

        // Verify database changes
        var updatedDeck = await _context.Decks.FindAsync("test-deck-id");
        Assert.NotNull(updatedDeck);
        Assert.Equal("Updated Test Deck", updatedDeck.Title);
        Assert.Equal("Updated test deck description", updatedDeck.Description);
        Assert.Equal(_testUser.Id, updatedDeck.ModifierUserId);
        Assert.True(updatedDeck.Modified > originalModified);

        // Verify Input.Modified was updated with the new timestamp
        Assert.Equal(updatedDeck.Modified, _pageModel.Input.Modified);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserTriesToEditAnotherUsersDeck_ReturnsWarningMessage()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);

        _pageModel.Input = new EditViewModel
        {
            Id = "another-deck-id", // This belongs to another user
            Title = "Hacked Title",
            Description = "Hacked Description"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);

        // Verify the other user's deck was not modified
        var otherUserDeck = await _context.Decks.FindAsync("another-deck-id");
        Assert.NotNull(otherUserDeck);
        Assert.Equal("Another User's Deck", otherUserDeck.Title);
        Assert.Equal("Should not be accessible", otherUserDeck.Description);
    }


    public void Dispose()
    {
        _context.Dispose();
    }
}