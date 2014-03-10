//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using NUnit.Framework;

using Sce.Atf.Applications.NetworkTargetServices;

namespace UnitTests.Atf
{
    [TestFixture]
    class TestTcpIpTargetInfo
    {
        [Test]
        public void TestIPv4EndPoint()
        {
            var newTarget = new TcpIpTargetInfo();
            newTarget.IPEndPoint = TcpIpTargetInfo.TryParseIPEndPoint(@"192.168.0.1:12345");
            Assert.IsNotNull(newTarget.IPEndPoint);
            Assert.AreEqual(newTarget.IPEndPoint.Port, 12345);

            var fromEndpointString = new TcpIpTargetInfo();
            fromEndpointString.Endpoint = @"192.168.0.1:12345";
            Assert.AreEqual(newTarget.IPEndPoint, fromEndpointString.IPEndPoint);
        }

        [Test]
        public void TestIPv6EndPoint()
        {
            var newTarget = new TcpIpTargetInfo();
            newTarget.IPEndPoint = TcpIpTargetInfo.TryParseIPEndPoint(@"2001:740:8deb:0::1:12345");
            Assert.IsNotNull(newTarget.IPEndPoint);
            Assert.AreEqual(newTarget.IPEndPoint.Port, 12345);
        }

        [Test]
        public void TestFixedPort_FullyQualifiedEndpoint()
        {
            var newTarget = new TcpIpTargetInfo();
            newTarget.FixedPort = 3001;
            newTarget.IPEndPoint = TcpIpTargetInfo.TryParseEndPointUsingPort(@"192.168.0.1:12345", newTarget.FixedPort);
            Assert.IsNotNull(newTarget.IPEndPoint);
            Assert.AreEqual(newTarget.IPEndPoint.Port, newTarget.FixedPort);
        }

        [Test]
        public void TestFixedPort_IPAddressOnlyEndpoint()
        {
            var newTarget = new TcpIpTargetInfo();
            newTarget.FixedPort = 3001;
            newTarget.IPEndPoint = TcpIpTargetInfo.TryParseEndPointUsingPort(@"192.168.0.1", newTarget.FixedPort);
            Assert.IsNotNull(newTarget.IPEndPoint);
            Assert.AreEqual(newTarget.IPEndPoint.Port, newTarget.FixedPort);
        }
    }
}
