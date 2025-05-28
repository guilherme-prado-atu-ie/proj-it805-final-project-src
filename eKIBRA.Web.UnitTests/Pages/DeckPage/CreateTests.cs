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
using System.Security.Claims;

namespace eKIBRA.Web.UnitTests.Pages.DeckPage;

public sealed class CreateTests : IDisposable
{
    private readonly Mock<ILogger<CreateModel>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly ApplicationDbContext _context;
    private readonly CreateModel _pageModel;

    public CreateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        _mockLogger = new Mock<ILogger<CreateModel>>();

        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null!, null!, null!, null!);

        // Create page model
        _pageModel = new CreateModel(
            _mockLogger.Object,
            _context,
            _mockUserManager.Object,
            _mockSignInManager.Object);

        // Setup PageContext for the model
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext
        {
            HttpContext = httpContext
        };
        _pageModel.PageContext = pageContext;
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
    public async Task OnPostAsync_WhenModelStateInvalid_ReturnsPageWithErrorMessage()
    {
        // Arrange
        _pageModel.Input = new CreateViewModel
        {
            Title = "Valid Title",
            Description = "Valid Description"
        };

        // Manually add model state errors to simulate validation failures
        _pageModel.ModelState.AddModelError("Title", "Title is required");
        _pageModel.ModelState.AddModelError("Description", "Description must be at least 10 characters");

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Invalid Deck. Check the required fields or try entering new values.", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);

        // Verify that no database operations were performed
        var deckCount = await _context.Decks.CountAsync();
        Assert.Equal(0, deckCount);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserNotSignedIn_RedirectsToLogin()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);
        _pageModel.Input = new CreateViewModel { Title = "Test Deck", Description = "Test Description" };

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
        _pageModel.Input = new CreateViewModel { Title = "Test Deck", Description = "Test Description" };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Your account was not found", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageModel.StatusMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenValidInput_CreatesDeckAndReturnsSuccessMessage()
    {
        // Arrange
        var testUser = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            IsDeleted = false
        };

        _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(testUser);

        _pageModel.Input = new CreateViewModel
        {
            Title = "Test Deck",
            Description = "Test Description"
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Contains("Deck created", _pageModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Success), _pageModel.StatusMessage);

        // Verify deck was added to context
        var createdDeck = await _context.Decks.FirstOrDefaultAsync();
        Assert.NotNull(createdDeck);
        Assert.Equal("Test Deck", createdDeck.Title);
        Assert.Equal("Test Description", createdDeck.Description);
        Assert.Equal(testUser.Id, createdDeck.UserId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}