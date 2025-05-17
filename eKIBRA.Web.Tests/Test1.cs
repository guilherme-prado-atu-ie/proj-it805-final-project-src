namespace eKIBRA.Web.Tests
{
    [TestClass]
    public sealed class Test1
    {
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
