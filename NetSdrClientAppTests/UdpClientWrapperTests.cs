using NUnit.Framework;
using NetSdrClientApp.Networking;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class UdpClientWrapperTests
    {
        private int _testPort = 54322;

        [Test]
        public async Task StartListening_And_Exit_ExecutesFullLifecycle()
        {
            // Arrange
            var clientWrapper = new UdpClientWrapper(_testPort);

            // Act
            
            var listeningTask = clientWrapper.StartListeningAsync();

            clientWrapper.Exit();
            await listeningTask;

            // Assert
            Assert.Pass("StartListeningAsync and Exit completed without exceptions.");
        }

        [Test]
        public async Task StartListening_And_Stop_StopsListening()
        {
            // Arrange
            var clientWrapper = new UdpClientWrapper(_testPort);

            // Act
            var listeningTask = clientWrapper.StartListeningAsync();

            clientWrapper.StopListening();
            await listeningTask;

            // Assert
            Assert.Pass("StartListeningAsync and StopListening completed without exceptions.");
        }

        [Test]
        public void Exit_WhenNotStarted_DoesNotThrow()
        {
            // Arrange
            var clientWrapper = new UdpClientWrapper(_testPort);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                clientWrapper.Exit();
            });
        }

        [Test]
        public void StopListening_WhenNotStarted_DoesNotThrow()
        {
            // Arrange
            var clientWrapper = new UdpClientWrapper(_testPort);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                clientWrapper.StopListening();
            });
        }

        [Test]
        public async Task MessageReceived_Event_FiresOnMessage()
        {
            // Arrange
            var clientWrapper = new UdpClientWrapper(_testPort);
            var tcs = new TaskCompletionSource<byte[]>();

            clientWrapper.MessageReceived += (sender, e) =>
            {
                tcs.TrySetResult(e);
            };

            // Act
            var listeningTask = clientWrapper.StartListeningAsync();
            using (var sendingClient = new UdpClient())
            {
                var testMessage = Encoding.UTF8.GetBytes("test");
                await sendingClient.SendAsync(testMessage, testMessage.Length, new IPEndPoint(IPAddress.Loopback, _testPort));
            }

            var receivedMessageTask = tcs.Task;
            var completedTask = await Task.WhenAny(receivedMessageTask, Task.Delay(1000));

            clientWrapper.Exit();
            await listeningTask;

            // Assert
            Assert.That(completedTask, Is.EqualTo(receivedMessageTask), "Повідомлення не було отримано протягом 1 сек.");
            var receivedMessage = await receivedMessageTask;
            Assert.That(receivedMessage, Is.EqualTo(Encoding.UTF8.GetBytes("test")));
        }

        [Test]
        public void GetHashCode_ReturnsConsistentValue()
        {
            // Arrange
            var clientWrapper1 = new UdpClientWrapper(_testPort);
            var clientWrapper2 = new UdpClientWrapper(_testPort);

            // Act
            var hash1 = clientWrapper1.GetHashCode();
            var hash2 = clientWrapper2.GetHashCode();

            // Assert
            Assert.That(hash1, Is.EqualTo(hash2));
        }

        [Test]
        public void GetHashCode_ReturnsDifferentValueForDifferentPort()
        {
            // Arrange
            var clientWrapper1 = new UdpClientWrapper(_testPort);
            var clientWrapper2 = new UdpClientWrapper(_testPort + 1);

            // Act
            var hash1 = clientWrapper1.GetHashCode();
            var hash2 = clientWrapper2.GetHashCode();

            // Assert
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }
    }
}