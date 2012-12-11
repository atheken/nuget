using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace NuGet.Test.Utility
{
    public static class XDocumentAssert
    {
        public static void Equal(string expected, string actual)
        {
            var e = XDocument.Parse(expected).ToString();
            var a = XDocument.Parse(actual).ToString();
            Assert.Equal(e, a);
        }

    }

    public static class XElementAssert
    {
        public static void Equal(string expected, string actual)
        {
            var e = XElement.Parse(expected).ToString();
            var a = XElement.Parse(actual).ToString();
            Assert.Equal(e, a);
        }
    }
}
