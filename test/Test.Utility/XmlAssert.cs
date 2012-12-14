using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace NuGet.Test.Utility
{
	public static class XmlAssert
	{
		private static Regex _isXmlDocument = new Regex("^\\s*\\<\\?", RegexOptions.Compiled | RegexOptions.Singleline);

		/// <summary>
		/// Will compare the two XML strings, due to differences in
		/// the way that Mono and .Net implement XElement, XElement.ToString() 
		/// produces different strings, though both are "correct,"
		/// the hardcoded XML in some tests cause spurious failures, this fixes them.
		/// </summary>
		/// <param name="expected">The expected XML Document, or XML Element.</param>
		/// <param name="actual">The actual XML produced.</param>
		public static void Equal(string expected, string actual)
		{
			if (_isXmlDocument.IsMatch(expected))
			{
				var e = XDocument.Parse(expected).ToString();
				var a = XDocument.Parse(actual).ToString();
				Assert.Equal(e, a);
			}
			else
			{
				var e = XElement.Parse(expected).ToString();
				var a = XElement.Parse(actual).ToString();
				Assert.Equal(e, a);
			}
		}
	}
}
