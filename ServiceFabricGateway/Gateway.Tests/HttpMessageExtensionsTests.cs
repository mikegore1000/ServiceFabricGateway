using System;
using System.Net;
using System.Net.Http;
using NUnit.Framework;

namespace Gateway.Tests
{
    [TestFixture]
    public abstract class HttpMessageExtensionsSpecification
    {
        protected HttpRequestMessage Expected { get; private set; }

        protected HttpRequestMessage Actual { get; private set; }

        [SetUp]
        public void Setup()
        {
            Expected = CreateExpected();

            Actual = Expected.Clone(new Uri("http://overridden"));
        }

        protected abstract HttpRequestMessage CreateExpected();

        [Test]
        public void then_the_method_is_cloned()
        {
            Assert.That(Actual.Method, Is.EqualTo(Expected.Method));
        }

        [Test]
        public void then_the_uri_is_overridden()
        {
            Assert.That(Actual.RequestUri, Is.EqualTo(new Uri("http://overridden")));
        }

        [Test]
        public void then_the_version_is_cloned()
        {
            Assert.That(Actual.Version, Is.EqualTo(Expected.Version));
        }

        [Test]
        public void then_the_headers_are_clone()
        {
            Assert.That(Actual.Headers, Is.EquivalentTo(Expected.Headers));
        }

        [Test]
        public void then_the_properties_are_cloned()
        {
            Assert.That(Actual.Properties, Is.EquivalentTo(Expected.Properties));
        }

        [Test]
        public void then_the_content_is_cloned()
        {
            Assert.That(Expected.Content, Is.EqualTo(Actual.Content));
        }
    }

    [TestFixture]
    public class GivenAPostRequest : HttpMessageExtensionsSpecification
    {
        protected override HttpRequestMessage CreateExpected()
        {
            var expected = new HttpRequestMessage(HttpMethod.Post, "http://original");
            expected.Version = HttpVersion.Version11;
            expected.Headers.Add("X-MyCustomHeader", "HeaderValue");
            expected.Properties.Add("MyTestProperty", "PropertyValue");
            expected.Content = new StringContent("TestContent");

            return expected;
        }
    }

    [TestFixture]
    public class GivenAGetRequest : HttpMessageExtensionsSpecification
    {
        protected override HttpRequestMessage CreateExpected()
        {
            var expected = new HttpRequestMessage(HttpMethod.Get, "http://original");
            expected.Version = HttpVersion.Version10;
            expected.Headers.Add("X-MyCustomHeader", "HeaderValue");
            expected.Properties.Add("MyTestProperty", "PropertyValue");

            return expected;
        }
    }
}
