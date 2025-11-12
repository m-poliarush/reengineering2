using NUnit.Framework;
using NetSdrClientApp.Networking;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class TcpClientWrapperTests
    {
        private TcpListener _testListener;
        private int _testPort = 54321; 
        private string _testHost = "127.0.0.1";

        [TearDown]
        public void TearDown()
        {
            _testListener?.Stop();
        }

        [Test]
        [TearDown]
        public async Task Connect_And_Disconnect_ExecutesFullLifecycle()
        {
            // Arrange
            _testListener = new TcpListener(IPAddress.Loopback, _testPort);
            _testListener.Start();

            var acceptTask = _testListener.AcceptTcpClientAsync();
            var clientWrapper = new TcpClientWrapper(_testHost, _testPort);

            // Act (Connect)
            clientWrapper.Connect();
            TcpClient serverSideClient = await acceptTask;
            Assert.That(serverSideClient.Connected, Is.True, "Сервер не прийняв клієнта");

            Assert.That(clientWrapper.Connected, Is.True, "Wrapper не вважає себе підключеним");

            // Act (Disconnect)
            clientWrapper.Disconnect();

            // Assert
            Assert.That(clientWrapper.Connected, Is.False, "Wrapper не відключився");

            serverSideClient.Close();
            _testListener.Stop();
        }

        [Test]
        public void Disconnect_WhenNotConnected_DoesNotThrow()
        {
            // Arrange
            var clientWrapper = new TcpClientWrapper(_testHost, _testPort);

            Assert.That(clientWrapper.Connected, Is.False);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                clientWrapper.Disconnect();
            });
        }
    }
}