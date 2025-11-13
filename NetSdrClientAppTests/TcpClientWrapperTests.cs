using NUnit.Framework;
using NetSdrClientApp.Networking;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System;

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class TcpClientWrapperTests
    {
        private TcpListener _testListener;
        private string _testHost = "127.0.0.1";

        [TearDown]
        public void TearDown()
        {
            _testListener?.Stop();
        }

    
        private int StartServerAndGetPort()
        {
            _testListener = new TcpListener(IPAddress.Loopback, 0);
            _testListener.Start();
            return ((IPEndPoint)_testListener.LocalEndpoint).Port;
        }

        [Test]
        public async Task Connect_And_Disconnect_ExecutesFullLifecycle()
        {
            // Arrange
            int port = StartServerAndGetPort();

            var acceptTask = _testListener.AcceptTcpClientAsync();
            var clientWrapper = new TcpClientWrapper(_testHost, port);

            // Act (Connect)
            clientWrapper.Connect();
            TcpClient serverSideClient = await acceptTask;

            Assert.That(serverSideClient.Connected, Is.True, "Сервер не прийняв клієнта");
            Assert.That(clientWrapper.Connected, Is.True, "Wrapper не вважає себе підключеним");

            // Act (Disconnect)
            clientWrapper.Disconnect();

            // Assert
            Assert.That(clientWrapper.Connected, Is.False, "Wrapper не відключився");

            // Cleanup (local variables)
            serverSideClient.Close();
        }

        [Test]
        public void Disconnect_WhenNotConnected_DoesNotThrow()
        {

            var clientWrapper = new TcpClientWrapper(_testHost, 55555);

            Assert.That(clientWrapper.Connected, Is.False);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                clientWrapper.Disconnect();
            });
        }

        [Test]
        public async Task SendMessageAsync_String_SendsCorrectBytes()
        {
            // Arrange
            int port = StartServerAndGetPort(); 

            var clientWrapper = new TcpClientWrapper(_testHost, port);
            string messageToSend = "Hello SonarCloud";
            byte[] expectedBytes = Encoding.UTF8.GetBytes(messageToSend);

            // Act
            clientWrapper.Connect();

            var serverClientTask = _testListener.AcceptTcpClientAsync();
            await clientWrapper.SendMessageAsync(messageToSend);
            var serverClient = await serverClientTask;

            // Assert
            var buffer = new byte[1024];
            var stream = serverClient.GetStream();
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(), CancellationToken.None);

            var receivedBytes = buffer.Take(bytesRead).ToArray();

            Assert.That(receivedBytes, Is.EqualTo(expectedBytes), "UTF8 байти не співпадають");

            serverClient.Close();
        }
    }
}