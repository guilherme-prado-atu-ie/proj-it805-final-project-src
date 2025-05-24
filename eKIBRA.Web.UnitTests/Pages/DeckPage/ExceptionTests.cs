using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.DeckPage;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace eKIBRA.Web.UnitTests.Pages.DeckPage;

public sealed class ExceptionTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreateModel _pageCreateModel;
    private readonly EditModel _pageEditModel;
    private readonly DeleteModel _pageDeleteModel;

    public ExceptionTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        var mockEditModelLogger = new Mock<ILogger<EditModel>>();
        var mockCreateModelLogger = new Mock<ILogger<CreateModel>>();

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        var mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

        // Create page model
        _pageCreateModel = new CreateModel(
            mockCreateModelLogger.Object,
            _context,
            mockUserManager.Object,
            mockSignInManager.Object);

        _pageEditModel = new EditModel(
            mockEditModelLogger.Object,
            _context,
            mockUserManager.Object,
            mockSignInManager.Object);

        // Setup PageContext
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext { HttpContext = httpContext };

        _pageEditModel.PageContext = pageContext;
        _pageCreateModel.PageContext = pageContext;
    }


    [Fact]
    public void HandleCreateException_WithGenericException_ReturnsErrorMessage()
    {
        // Arrange
        var genericException = new Exception("Generic error");

        _pageEditModel.Input = new EditViewModel
        {
            Id = "test-deck-id",
            Title = "Test Title"
        };

        _pageCreateModel.Input = new CreateViewModel
        {
            Title = "Test Title",
            Description = "Test Description"
        };

        // Act
        var editResult = _pageEditModel.HandleCreateException(genericException);
        var createResult = _pageCreateModel.HandleCreateException(genericException);

        // Assert
        Assert.IsType<PageResult>(editResult);
        Assert.Contains("Fail to update the existing Deck", _pageEditModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageEditModel.StatusMessage);

        Assert.IsType<PageResult>(createResult);
        Assert.Contains("Fail to create a new Deck", _pageCreateModel.StatusMessage);
        Assert.Contains(nameof(MessageType.Error), _pageCreateModel.StatusMessage);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}