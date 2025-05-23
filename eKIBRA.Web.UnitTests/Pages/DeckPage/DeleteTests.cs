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

public sealed class DeleteTests
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
        
        // Create a test user
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

        // Setup PageContext for the model
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext
        {
            HttpContext = httpContext
        };
        _pageModel.PageContext = pageContext;

        SeedTestData();
    }
    
    private void SeedTestData()
    {
        
        var testDeck = new Deck
        {
            Id = "test-deck-id",
            UserId = _testUser.Id,
            User = _testUser,
            Created =  DateTime.UtcNow,
            Title = "Test Deck for Deletion",
            Description = "Test deck description",
            IsDeleted = false,
            Flashcards =
            [
                new Flashcard
                {
                    Id = "flashcard-1",
                    Question = "Test Question 1",
                    Answer = "Test Answer 1",
                    DeckId = "test-deck-id",
                    UserId = _testUser.Id,
                    Incorrects = [],
                    LinkedDeck = null,
                    IsDeleted = false
                },
                new Flashcard
                {
                    Id = "flashcard-2",
                    Question = "Test Question 2",
                    Answer = "Test Answer 2",
                    DeckId = "test-deck-id",
                    UserId = _testUser.Id,
                    Incorrects = [],
                    LinkedDeck = null,
                    IsDeleted = false
                }
            ]
        };

        var anotherUserDeck = new Deck
        {
            Id = "another-deck-id",
            UserId = "another-user-id",
            User = _anotherUser,
            Created = DateTime.UtcNow,
            Title = "Another User's Deck",
            Description = "Should not be accessible",
            IsDeleted = false
        };

        _context.Decks.AddRange(testDeck, anotherUserDeck);
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
    public async Task OnGetAsync_WhenValidRequest_ReturnsPageWithDeckData()
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
        Assert.Equal("Test Deck for Deletion", _pageModel.Input.Title);
        Assert.Equal(_testUser.Id, _pageModel.Input.UserId);
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
            var result = await _pageModel.OnPostAsync("test-deck-id");

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
            var result = await _pageModel.OnPostAsync("test-deck-id");

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

            // Act
            var result = await _pageModel.OnPostAsync("nonexistent-deck-id");

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
            Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);
        }

        // [Fact(Skip = "The post on the test is deleting the deck double check")]
        // public async Task OnPostAsync_WhenValidRequest_SoftDeletesDeckAndFlashcards()
        // {
        //     // Arrange
        //     _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
        //                      .Returns(true);
        //     _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        //                    .ReturnsAsync(_testUser);
        //
        //     // Act
        //     var result = await _pageModel.OnPostAsync("test-deck-id");
        //
        //     // Assert
        //     Assert.IsType<PageResult>(result);
        //     Assert.Contains("The record was removed successfully", _pageModel.StatusMessage);
        //     Assert.Contains(nameof(MessageType.Success), _pageModel.StatusMessage);
        //     Assert.Null(_pageModel.Input);
        //
        //     // Verify soft deletion in database
        //     var deletedDeck = await _context.Decks
        //         .Include(d => d.Flashcards)
        //         .FirstOrDefaultAsync(d => d.Id == "test-deck-id");
        //
        //     Assert.NotNull(deletedDeck);
        //     Assert.True(deletedDeck.IsDeleted);
        //     Assert.StartsWith("Deleted ", deletedDeck.Title);
        //     Assert.Contains("test-deck-id", deletedDeck.Title);
        //
        //     // Verify all flashcards are soft deleted
        //     Assert.All(deletedDeck.Flashcards, flashcard =>
        //     {
        //         Assert.True(flashcard.IsDeleted);
        //         Assert.StartsWith("Deleted ", flashcard.Question);
        //         Assert.Contains(flashcard.Id, flashcard.Question);
        //     });
        // }

        [Fact]
        public async Task OnPostAsync_WhenDeckBelongsToAnotherUser_ReturnsPageWithWarningMessage()
        {
            // Arrange
            _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
                             .Returns(true);
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(_testUser);

            // Act
            var result = await _pageModel.OnPostAsync("another-deck-id");

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.Contains("The record no longer exists", _pageModel.StatusMessage);
            Assert.Contains(nameof(MessageType.Warning), _pageModel.StatusMessage);

            // Verify the other user's deck was not modified
            var otherUserDeck = await _context.Decks.FindAsync("another-deck-id");
            Assert.NotNull(otherUserDeck);
            Assert.False(otherUserDeck.IsDeleted);
            Assert.Equal("Another User's Deck", otherUserDeck.Title);
        }

        [Fact]
        public async Task OnPostAsync_WhenDeckHasNoFlashcards_DeletesOnlyDeck()
        {
            // Arrange
            var deckWithoutFlashcards = new Deck
            {
                Id = "deck-no-flashcards",
                UserId = _testUser.Id,
                Title = "Deck Without Flashcards",
                Description = "Test deck with no flashcards"
            };
            _context.Decks.Add(deckWithoutFlashcards);
            await _context.SaveChangesAsync();

            _mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
                             .Returns(true);
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(_testUser);

            // Act
            var result = await _pageModel.OnPostAsync("deck-no-flashcards");

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.Contains("The record was removed successfully", _pageModel.StatusMessage);

            // Verify deck is soft deleted
            var deletedDeck = await _context.Decks.FindAsync("deck-no-flashcards");
            Assert.NotNull(deletedDeck);
            Assert.True(deletedDeck.IsDeleted);
            Assert.StartsWith("Deleted ", deletedDeck.Title);
        }
}