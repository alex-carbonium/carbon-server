using System;
using System.Collections.Generic;
using Carbon.Framework.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Test.Unit.Framework
{
    [TestClass]
    public class UriExtensionsTest
    {
        [TestMethod]
        public void AppendQueryWhenNoQuery()
        {
            var uri = new Uri("/path/", UriKind.RelativeOrAbsolute);
            var q = new Dictionary<string, string>();
            q.Add("a", "1");
            
            var result = uri.AppendQuery(q);

            Assert.AreEqual("/path/?a=1", result.ToString());
        }
        [TestMethod]
        public void AppendQueryWhenNoQueryButSeparatorExists()
        {
            var uri = new Uri("/path?", UriKind.RelativeOrAbsolute);
            var q = new Dictionary<string, string>();
            q.Add("a", "1");

            var result = uri.AppendQuery(q);

            Assert.AreEqual("/path?a=1", result.ToString());
        }
        [TestMethod]
        public void AppendQueryWhenQueryExists()
        {
            var uri = new Uri("/path?a=1", UriKind.RelativeOrAbsolute);
            var q = new Dictionary<string, string>();
            q.Add("b", "2");

            var result = uri.AppendQuery(q);

            Assert.AreEqual("/path?a=1&b=2", result.ToString());
        }
        [TestMethod]
        public void AppendQueryMultipleValues()
        {
            var uri = new Uri("/path?", UriKind.RelativeOrAbsolute);
            var q = new Dictionary<string, string>();
            q.Add("a", "1");
            q.Add("b", "2");

            var result = uri.AppendQuery(q);

            Assert.AreEqual("/path?a=1&b=2", result.ToString());
        }

        [TestMethod]
        public void TirimQueryNoQuery()
        {
            var uri = new Uri("/path", UriKind.RelativeOrAbsolute);
            
            var result = uri.TrimQuery();

            Assert.AreEqual("/path", result.ToString());
        }
        [TestMethod]
        public void TirimQueryWhenSeparatorOnly()
        {
            var uri = new Uri("/path?", UriKind.RelativeOrAbsolute);

            var result = uri.TrimQuery();

            Assert.AreEqual("/path", result.ToString());
        }
        [TestMethod]
        public void TirimQueryWhenQueryExists()
        {
            var uri = new Uri("/path?a=1", UriKind.RelativeOrAbsolute);

            var result = uri.TrimQuery();

            Assert.AreEqual("/path", result.ToString());
        }
        [TestMethod]
        public void TirimQueryWithReplacement()
        {
            var uri = new Uri("http://server/path?a=1&b=2", UriKind.RelativeOrAbsolute);

            var result = uri.TrimQuery(new Dictionary<string, string>{{"a", "x"}});

            Assert.AreEqual("http://server/path?a=x&b=2", result.ToString());
        }
    }
}
