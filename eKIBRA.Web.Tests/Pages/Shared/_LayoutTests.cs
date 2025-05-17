using eKIBRA.Web.Pages;
using eKIBRA.Web.Tests.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using eKIBRA.Web.Tests.Utilities;

namespace eKIBRA.Web.Tests
{

    [TestClass]
    public class BasicTests
    {
        private static CustomWebApplicationFactory<Program> _factory;

        [ClassInitialize]
        public static void AssemblyInitialize(TestContext _)
        {
            _factory = new CustomWebApplicationFactory<Program>();
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static void AssemblyCleanup(TestContext _)
        {
            _factory.Dispose();
        }

        [TestMethod]
        [DataRow("/Index")]
        public async Task Get_UnauthenticatedUser_NotRenderRestrictContent(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetStringAsync(url);

            // Assert
            Assert.DoesNotContain("/Account/Manager/Index", response);
        }

        [TestMethod]
        [DataRow("/")]
        [DataRow("/Index")]
        [DataRow("/Privacy")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.AreEqual("text/html; charset=utf-8",
                response.Content.Headers.ContentType?.ToString());
        }
    }



    [TestClass]
    public sealed class LayoutTests
    {
        [TestInitialize]
        public void TestInitialize()
        { 


        }

        [TestCleanup]
        public void TestCleanup()
        { 

        }
        
        /*        
        [TestMethod]
        public void OnPostAddMessageAsync_ReturnsAPageResult_WhenModelStateIsInvalid()
        {
                // Arrange
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContextTest>()
                    .UseSqlite();
                var mockAppDbContext = new Mock<ApplicationDbContextTest>(optionsBuilder.Options);
                var expectedMessages = ApplicationDbContextTest.GetMessagesAsync();
                mockAppDbContext.Setup(db => db.GetMessagesAsync()).Returns(Task.FromResult(expectedMessages));
                var httpContext = new DefaultHttpContext();
                var modelState = new ModelStateDictionary();
                var actionContext = new ActionContext(httpContext, new(), new PageActionDescriptor(), modelState);
                var modelMetadataProvider = new EmptyModelMetadataProvider();
                var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
                var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
                var pageContext = new PageContext(actionContext)
                {
                    ViewData = viewData
                };
                var pageModel = new IndexModel(mockAppDbContext.Object)
                {
                    PageContext = pageContext,
                    TempData = tempData,
                    Url = new UrlHelper(actionContext)
                };
                pageModel.ModelState.AddModelError("Message.Text", "The Text field is required.");

                // Act
                var result = await pageModel.OnPostAddMessageAsync();

                // Assert
                Assert.IsType<PageResult>(result);                    
        }
        */

        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            int expected = 5;

            // Act
            int actual = 2 + 3;

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
