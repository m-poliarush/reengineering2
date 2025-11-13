using NetArchTest.Rules;
using NetSdrClientApp; 
using System.Reflection; 

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class ArchitectureTests
    {
        private static readonly Assembly _appAssembly = typeof(NetSdrClient).Assembly;

        private static readonly Assembly _testAssembly = typeof(ArchitectureTests).Assembly;


        [Test]
        public void App_Not_Depend_On_EchoServer()
        {

            var result = Types.InAssembly(_appAssembly)
                .ShouldNot()
                .HaveDependencyOn("EchoServer")
                .GetResult();

            Assert.That(result.IsSuccessful, Is.True,
                "Клієнтський додаток не повинен мати прямої залежності від EchoServer (видаліть ProjectReference або using).");
        }

        [Test]
        public void Messages_Not_Depend_On_Networking()
        {
            var result = Types.InAssembly(_appAssembly)
                .That()
                .ResideInNamespace("NetSdrClientApp.Messages")
                .ShouldNot()
                .HaveDependencyOn("NetSdrClientApp.Networking")
                .GetResult();

            Assert.That(result.IsSuccessful, Is.True,
                "Шар Messages не повинен залежати від шару Networking.");
        }

       

        [Test]
        public void Tests_Not_Depend_On_Server()
        {


            var result = Types.InAssembly(_testAssembly)
                .ShouldNot()
                .HaveDependencyOn("EchoServer") 
                .GetResult();

            Assert.That(result.IsSuccessful, Is.True,
                "Проєкт тестів (NetSdrClientAppTests) не повинен посилатися на EchoServer.");
        }

        [Test]
        public void Test_Classes_Should_End_With_Tests_Suffix()
        {

            var result = Types.InAssembly(_testAssembly)
                .That()
                .ResideInNamespace("NetSdrClientAppTests")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Tests")
                .GetResult();

            Assert.That(result.IsSuccessful, Is.True,
                "Усі класи в проєкті NetSdrClientAppTests мають закінчуватися на 'Tests'.");
        }
    }
}