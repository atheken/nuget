using NuGet.Test.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;
using Xunit.Extensions;

namespace NuGet.Test
{
    public class VersionUtilityTest
    {
        [Theory]
        [InlineData("boo\\foo.dll", "foo.dll")]
        [InlineData("far\\sub\\sub2\\foo.dll", "sub\\sub2\\foo.dll")]
        [InlineData("netum\\who\\bar.dll", "who\\bar.dll")]
        public void ParseFrameworkFolderNameStrictReturnsUnsupportedFxIfParsingFails(string path, string expectedEffectivePath)
        {
            path = PathFixUtility.FixPath(path);
            expectedEffectivePath = PathFixUtility.FixPath(expectedEffectivePath);

            // Act
            string effectivePath;
            var frameworkName = VersionUtility.ParseFrameworkFolderName(
                path, strictParsing: true, effectivePath: out effectivePath);

            // Assert
            Assert.Equal(VersionUtility.UnsupportedFrameworkName, frameworkName);
            Assert.Equal(expectedEffectivePath, effectivePath);
        }

        [Theory]
        [InlineData("lib\\net40\\foo.dll", "4.0", ".NETFramework", "foo.dll")]
        [InlineData("lib\\net40\\sub\\foo.dll", "4.0", ".NETFramework", "sub\\foo.dll")]
        [InlineData("lib\\foo.dll", null, null, "foo.dll")]
        [InlineData("content\\sl35\\javascript\\jQuery.js", "3.5", "Silverlight", "javascript\\jQuery.js")]
        [InlineData("content\\netmf\\CSS\\jQuery.css", "0.0", ".NETMicroFramework", "CSS\\jQuery.css")]
        [InlineData("tools\\winrt45\\install.ps1", "4.5", ".NETCore", "install.ps1")]
        [InlineData("tools\\winrt10\\uninstall.ps1", "1.0", ".NETCore", "uninstall.ps1")]
        [InlineData("tools\\winkt10\\uninstall.ps1", "0.0", "Unsupported", "uninstall.ps1")]
        [InlineData("tools\\init.ps1", null, null, "init.ps1")]
        [InlineData("random\\foo.txt", null, null, "random\\foo.txt")]
        public void TestParseFrameworkFolderNameFromFilePath(
            string filePath, string expectedVersion, string expectedIdentifier, string expectedEffectivePath)
        {
            filePath = PathFixUtility.FixPath(filePath);
            expectedEffectivePath = PathFixUtility.FixPath(expectedEffectivePath);

            // Act
            string effectivePath;
            var frameworkName = VersionUtility.ParseFrameworkNameFromFilePath(filePath, out effectivePath);

            // Assert
            if (expectedVersion == null)
            {
                Assert.Null(frameworkName);
            }
            else
            {
                Assert.NotNull(frameworkName);
                Assert.Equal(expectedIdentifier, frameworkName.Identifier);
                Assert.Equal(expectedVersion, frameworkName.Version.ToString());
            }

            Assert.Equal(expectedEffectivePath, effectivePath);
        }

        [Theory]
        [InlineData("net40\\foo.dll", "4.0", ".NETFramework", "foo.dll")]
        [InlineData("netmu40\\sub\\foo.dll", "0.0", "Unsupported", "sub\\foo.dll")]
        [InlineData("foo.dll", null, null, "foo.dll")]
        [InlineData("sl35\\javascript\\jQuery.js", "3.5", "Silverlight", "javascript\\jQuery.js")]
        [InlineData("netmf\\CSS\\jQuery.css", "0.0", ".NETMicroFramework", "CSS\\jQuery.css")]
        [InlineData("CSS\\jQuery.css", "0.0", "Unsupported", "jQuery.css")]
        [InlineData("winrt45\\install.ps1", "4.5", ".NETCore", "install.ps1")]
        [InlineData("winrt10\\uninstall.ps1", "1.0", ".NETCore", "uninstall.ps1")]
        [InlineData("winkt10\\uninstall.ps1", "0.0", "Unsupported", "uninstall.ps1")]
        [InlineData("init.ps1", null, null, "init.ps1")]
        [InlineData("random\\foo.txt", "0.0", "Unsupported", "foo.txt")]
        public void TestParseFrameworkFolderNameWithStrickParsing(
            string filePath, string expectedVersion, string expectedIdentifier, string expectedEffectivePath)
        {
            filePath = PathFixUtility.FixPath(filePath);
            expectedEffectivePath = PathFixUtility.FixPath(expectedEffectivePath);


            // Act
            string effectivePath;
            var frameworkName = VersionUtility.ParseFrameworkFolderName(filePath, strictParsing: true, effectivePath: out effectivePath);

            // Assert
            if (expectedVersion == null)
            {
                Assert.Null(frameworkName);
            }
            else
            {
                Assert.NotNull(frameworkName);
                Assert.Equal(expectedIdentifier, frameworkName.Identifier);
                Assert.Equal(expectedVersion, frameworkName.Version.ToString());
            }

            Assert.Equal(expectedEffectivePath, effectivePath);
        }

        [Theory]
        [InlineData("net40\\foo.dll", "4.0", ".NETFramework", "foo.dll")]
        [InlineData("net40\\sub\\foo.dll", "4.0", ".NETFramework", "sub\\foo.dll")]
        [InlineData("foo.dll", null, null, "foo.dll")]
        [InlineData("sl35\\javascript\\jQuery.js", "3.5", "Silverlight", "javascript\\jQuery.js")]
        [InlineData("netmf\\CSS\\jQuery.css", "0.0", ".NETMicroFramework", "CSS\\jQuery.css")]
        [InlineData("netmf\\CSS\\jQuery.css", "0.0", ".NETMicroFramework", "CSS\\jQuery.css")]
        [InlineData("winrt45\\install.ps1", "4.5", ".NETCore", "install.ps1")]
        [InlineData("winrt10\\uninstall.ps1", "1.0", ".NETCore", "uninstall.ps1")]
        [InlineData("winrt10\\uninstall.ps1", "1.0", ".NETCore", "uninstall.ps1")]
        [InlineData("init.ps1", null, null, "init.ps1")]
        [InlineData("random\\foo.txt", null, null, "random\\foo.txt")]
        public void TestParseFrameworkFolderNameWithNonStrickParsing(
            string filePath, string expectedVersion, string expectedIdentifier, string expectedEffectivePath)
        {
            filePath = PathFixUtility.FixPath(filePath);
            expectedEffectivePath = PathFixUtility.FixPath(expectedEffectivePath);

            // Act
            string effectivePath;
            var frameworkName = VersionUtility.ParseFrameworkFolderName(filePath, strictParsing: false, effectivePath: out effectivePath);

            // Assert
            if (expectedVersion == null)
            {
                Assert.Null(frameworkName);
            }
            else
            {
                Assert.NotNull(frameworkName);
                Assert.Equal(expectedIdentifier, frameworkName.Identifier);
                Assert.Equal(expectedVersion, frameworkName.Version.ToString());
            }

            Assert.Equal(expectedEffectivePath, effectivePath);
        }

        [Theory]
        [InlineData("content\\-\\wow\\cool.txt", "-\\wow\\cool.txt")]
        [InlineData("content\\-world\\x.dll", "-world\\x.dll")]
        public void ParseFrameworkNameFromFilePathDoesNotThrowIfPathHasADash(string path, string expectedPath)
        {
            path = PathFixUtility.FixPath(path);
            expectedPath = PathFixUtility.FixPath(expectedPath);

            // Act
            string effectivePath;
            var framework = VersionUtility.ParseFrameworkNameFromFilePath(path, out effectivePath);
            
            // Assert
            Assert.Null(framework);
            Assert.Equal(expectedPath, effectivePath);
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedNetFrameworkNames()
        {
            // Arrange
            var knownNameFormats = new[] { ".net", ".netframework", "net", "netframework" };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(fmt => VersionUtility.ParseFrameworkName(fmt));

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal(".NETFramework", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedPortableNetFrameworkNames()
        {
            // Arrange
            var knownNameFormats = new[] { ".netportable-sl3", "netportable-net4", "portable-netcore45" };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(fmt => VersionUtility.ParseFrameworkName(fmt));

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal(".NETPortable", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedWindowsPhoneNames()
        {
            // Arrange
            var knownNameFormats = new[] { "windowsphone", "wp" };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(fmt => VersionUtility.ParseFrameworkName(fmt));

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal("WindowsPhone", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedWinRTFrameworkNames()
        {
            // Arrange
            var knownNameFormats = new[] { "winrt", ".NETCore", "NetCore" };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(fmt => VersionUtility.ParseFrameworkName(fmt));

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal(".NETCore", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedWindowsFrameworkNames()
        {
            // Arrange
            var knownNameFormats = new[] { "Windows", "win" };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(fmt => VersionUtility.ParseFrameworkName(fmt));

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal("Windows", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedNetMicroFrameworkNames()
        {
            // Arrange
            var knownNameFormats = new[] { "netmf4.1", ".NETMicroFramework4.1" };
            Version version41 = new Version("4.1");

            // Act
            var frameworkNames = knownNameFormats.Select(fmt => VersionUtility.ParseFrameworkName(fmt));

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal(".NETMicroFramework", frameworkName.Identifier);
                Assert.Equal(version41, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedSilverlightNames()
        {
            // Arrange
            var knownNameFormats = new[] { "sl", "SL", "SilVerLight", "Silverlight", "Silverlight " };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(VersionUtility.ParseFrameworkName);

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal("Silverlight", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedMonoAndroidNames()
        {
            // Arrange
            var knownNameFormats = new[] { "MonoAndroid", "monoandroid", "MONOANDROID " };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(VersionUtility.ParseFrameworkName);

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal("MonoAndroid", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedMonoTouchNames()
        {
            // Arrange
            var knownNameFormats = new[] { "MonoTouch", "monotouch", "monoTOUCH  " };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(VersionUtility.ParseFrameworkName);

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal("MonoTouch", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameNormalizesSupportedMonoMacNames()
        {
            // Arrange
            var knownNameFormats = new[] { "MonoMac", "monomac", "mONOmAC " };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(VersionUtility.ParseFrameworkName);

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal("MonoMac", frameworkName.Identifier);
                Assert.Equal(defaultVersion, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameReturnsUnsupportedFrameworkNameIfUnrecognized()
        {
            // Act
            var frameworkName1 = VersionUtility.ParseFrameworkName("NETCF20");
            var frameworkName2 = VersionUtility.ParseFrameworkName("NET40ClientProfile");
            var frameworkName3 = VersionUtility.ParseFrameworkName("NET40Foo");

            // Assert
            Assert.Equal("Unsupported", frameworkName1.Identifier);
            Assert.Equal("Unsupported", frameworkName2.Identifier);
            Assert.Equal("Unsupported", frameworkName3.Identifier);
        }

        [Fact]
        public void ParseFrameworkNameUsesNetFrameworkIfOnlyVersionSpecified()
        {
            // Arrange
            Version version20 = new Version("2.0");

            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("20");

            // Assert
            Assert.Equal(".NETFramework", frameworkName.Identifier);
            Assert.Equal(version20, frameworkName.Version);
        }

        [Fact]
        public void ParseFrameworkNameVersionFormats()
        {
            // Arrange
            var versionFormats = new[] { "4.0", "40", "4" };
            Version version40 = new Version("4.0");

            // Act
            var frameworkNames = versionFormats.Select(VersionUtility.ParseFrameworkName);

            // Assert
            foreach (var frameworkName in frameworkNames)
            {
                Assert.Equal(".NETFramework", frameworkName.Identifier);
                Assert.Equal(version40, frameworkName.Version);
            }
        }

        [Fact]
        public void ParseFrameworkNameVersionIntegerLongerThan4CharsTrimsExccess()
        {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("NET41235");

            // Assert
            Assert.Equal(".NETFramework", frameworkName.Identifier);
            Assert.Equal(new Version("4.1.2.3"), frameworkName.Version);
        }

        [Fact]
        public void ParseFrameworkNameInvalidVersionFormatUsesDefaultVersion()
        {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("NET4.1.4.5.5");

            // Assert
            Assert.Equal("Unsupported", frameworkName.Identifier);
        }

        [Fact]
        public void ParseFrameworkNameWithProfile()
        {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("net40-client");

            // Assert
            Assert.Equal(".NETFramework", frameworkName.Identifier);
            Assert.Equal(new Version("4.0"), frameworkName.Version);
            Assert.Equal("Client", frameworkName.Profile);
        }

        [Fact]
        public void ParseFrameworkNameWithUnknownProfileUsesProfileAsIs()
        {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("net40-other");

            // Assert
            Assert.Equal(".NETFramework", frameworkName.Identifier);
            Assert.Equal(new Version("4.0"), frameworkName.Version);
            Assert.Equal("other", frameworkName.Profile);
        }

        [Fact]
        public void ParseFrameworkNameWithFullProfileNoamlizesToEmptyProfile()
        {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("net40-full");

            // Assert
            Assert.Equal(".NETFramework", frameworkName.Identifier);
            Assert.Equal(new Version("4.0"), frameworkName.Version);
            Assert.Equal(String.Empty, frameworkName.Profile);
        }

        [Fact]
        public void ParseFrameworkNameWithWPProfileGetNormalizedToWindowsPhone()
        {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("sl4-wp");

            // Assert
            Assert.Equal("Silverlight", frameworkName.Identifier);
            Assert.Equal(new Version("4.0"), frameworkName.Version);
            Assert.Equal("WindowsPhone", frameworkName.Profile);
        }

        [Fact]
        public void ParseFrameworkNameWithCFProfileGetNormalizedToCompactFramework()
        {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("net20-cf");

            // Assert
            Assert.Equal(".NETFramework", frameworkName.Identifier);
            Assert.Equal(new Version("2.0"), frameworkName.Version);
            Assert.Equal("CompactFramework", frameworkName.Profile);
        }

        [Fact]
        public void ParseFrameworkNameWithEmptyProfile()
        {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("sl4-");

            // Assert
            Assert.Equal("Silverlight", frameworkName.Identifier);
            Assert.Equal(new Version("4.0"), frameworkName.Version);
            Assert.Equal(String.Empty, frameworkName.Profile);
        }

        [Fact]
        public void ParseFrameworkNameWithInvalidFrameworkNameThrows()
        {
            // Act
            ExceptionAssert.ThrowsArgumentException(() => VersionUtility.ParseFrameworkName("-"), "frameworkName", "Framework name is missing.");
            ExceptionAssert.ThrowsArgumentException(() => VersionUtility.ParseFrameworkName("-client"), "frameworkName", "Framework name is missing.");
            ExceptionAssert.ThrowsArgumentException(() => VersionUtility.ParseFrameworkName(""), "frameworkName", "Framework name is missing.");
            ExceptionAssert.ThrowsArgumentException(() => VersionUtility.ParseFrameworkName("---"), "frameworkName", "Invalid framework name format. Expected {framework}{version}-{profile}.");
        }

        [Fact]
        public void ParseFrameworkFolderName()
        {
            // foo.dll
            // sub\foo.dll -> Unsupported since we can't tell if this was meant to be a framework name or not
            // {FrameworkName}{Version}\foo.dll
            // {FrameworkName}{Version}\sub1\foo.dll
            // {FrameworkName}{Version}\sub1\sub2\foo.dll
            var f1 = VersionUtility.ParseFrameworkFolderName(PathFixUtility.FixPath(@"foo.dll"));
            var f2 = VersionUtility.ParseFrameworkFolderName(PathFixUtility.FixPath(@"sub\foo.dll"));
            var f3 = VersionUtility.ParseFrameworkFolderName(PathFixUtility.FixPath(@"SL4\foo.dll"));
            var f4 = VersionUtility.ParseFrameworkFolderName(PathFixUtility.FixPath(@"SL3\sub1\foo.dll"));
            var f5 = VersionUtility.ParseFrameworkFolderName(PathFixUtility.FixPath(@"SL20\sub1\sub2\foo.dll"));
            var f6 = VersionUtility.ParseFrameworkFolderName(PathFixUtility.FixPath(@"net\foo.dll"));
            var f7 = VersionUtility.ParseFrameworkFolderName(PathFixUtility.FixPath(@"winrt45\foo.dll"));

            Assert.Null(f1);
            Assert.Equal("Unsupported", f2.Identifier);
            Assert.Equal("Silverlight", f3.Identifier);
            Assert.Equal(new Version("4.0"), f3.Version);
            Assert.Equal("Silverlight", f4.Identifier);
            Assert.Equal(new Version("3.0"), f4.Version);
            Assert.Equal("Silverlight", f5.Identifier);
            Assert.Equal(new Version("2.0"), f5.Version);
            Assert.Equal(".NETFramework", f6.Identifier);
            Assert.Equal(new Version(), f6.Version);
            Assert.Equal(".NETCore", f7.Identifier);
            Assert.Equal(new Version("4.5"), f7.Version);
        }

        [Fact]
        public void GetFrameworkStringFromFrameworkName()
        {
            // Arrange
            var net40 = new FrameworkName(".NETFramework", new Version(4, 0));
            var net40Client = new FrameworkName(".NETFramework", new Version(4, 0), "Client");
            var sl3 = new FrameworkName("Silverlight", new Version(3, 0));
            var sl4 = new FrameworkName("Silverlight", new Version(4, 0));
            var wp7 = new FrameworkName("Silverlight", new Version(4, 0), "WindowsPhone");
            var wp7Mango = new FrameworkName("Silverlight", new Version(4, 0), "WindowsPhone71");
            var netMicro41 = new FrameworkName(".NETMicroFramework", new Version(4, 1));
            var winrt = new FrameworkName(".NETCore", new Version(4, 5));

            // Act
            string net40Value = VersionUtility.GetFrameworkString(net40);
            string net40ClientValue = VersionUtility.GetFrameworkString(net40Client);
            string sl3Value = VersionUtility.GetFrameworkString(sl3);
            string sl4Value = VersionUtility.GetFrameworkString(sl4);
            string wp7Value = VersionUtility.GetFrameworkString(wp7);
            string wp7MangoValue = VersionUtility.GetFrameworkString(wp7Mango);
            string netMicro41Value = VersionUtility.GetFrameworkString(netMicro41);
            string winrtValue = VersionUtility.GetFrameworkString(winrt);

            // Assert
            Assert.Equal(".NETFramework4.0", net40Value);
            Assert.Equal(".NETFramework4.0-Client", net40ClientValue);
            Assert.Equal("Silverlight3.0", sl3Value);
            Assert.Equal("Silverlight4.0", sl4Value);
            Assert.Equal("Silverlight4.0-WindowsPhone", wp7Value);
            Assert.Equal("Silverlight4.0-WindowsPhone71", wp7MangoValue);
            Assert.Equal(".NETMicroFramework4.1", netMicro41Value);
            Assert.Equal(".NETCore4.5", winrtValue);
        }

        [Fact]
        public void ParseVersionSpecWithNullThrows()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgNull(() => VersionUtility.ParseVersionSpec(null), "value");
        }

        [Fact]
        public void ParseVersionSpecSimpleVersionNoBrackets()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("1.2");

            // Assert
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.True(versionInfo.IsMinInclusive);
            Assert.Equal(null, versionInfo.MaxVersion);
            Assert.False(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecSimpleVersionNoBracketsExtraSpaces()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("  1  .   2  ");

            // Assert
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.True(versionInfo.IsMinInclusive);
            Assert.Equal(null, versionInfo.MaxVersion);
            Assert.False(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecMaxOnlyInclusive()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("(,1.2]");

            // Assert
            Assert.Equal(null, versionInfo.MinVersion);
            Assert.False(versionInfo.IsMinInclusive);
            Assert.Equal("1.2", versionInfo.MaxVersion.ToString());
            Assert.True(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecMaxOnlyExclusive()
        {
            var versionInfo = VersionUtility.ParseVersionSpec("(,1.2)");
            Assert.Equal(null, versionInfo.MinVersion);
            Assert.False(versionInfo.IsMinInclusive);
            Assert.Equal("1.2", versionInfo.MaxVersion.ToString());
            Assert.False(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecExactVersion()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("[1.2]");

            // Assert
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.True(versionInfo.IsMinInclusive);
            Assert.Equal("1.2", versionInfo.MaxVersion.ToString());
            Assert.True(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecMinOnlyExclusive()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("(1.2,)");

            // Assert
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.False(versionInfo.IsMinInclusive);
            Assert.Equal(null, versionInfo.MaxVersion);
            Assert.False(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecRangeExclusiveExclusive()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("(1.2,2.3)");

            // Assert
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.False(versionInfo.IsMinInclusive);
            Assert.Equal("2.3", versionInfo.MaxVersion.ToString());
            Assert.False(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecRangeExclusiveInclusive()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("(1.2,2.3]");

            // Assert
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.False(versionInfo.IsMinInclusive);
            Assert.Equal("2.3", versionInfo.MaxVersion.ToString());
            Assert.True(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecRangeInclusiveExclusive()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("[1.2,2.3)");
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.True(versionInfo.IsMinInclusive);
            Assert.Equal("2.3", versionInfo.MaxVersion.ToString());
            Assert.False(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecRangeInclusiveInclusive()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("[1.2,2.3]");

            // Assert
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.True(versionInfo.IsMinInclusive);
            Assert.Equal("2.3", versionInfo.MaxVersion.ToString());
            Assert.True(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecRangeInclusiveInclusiveExtraSpaces()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("   [  1 .2   , 2  .3   ]  ");

            // Assert
            Assert.Equal("1.2", versionInfo.MinVersion.ToString());
            Assert.True(versionInfo.IsMinInclusive);
            Assert.Equal("2.3", versionInfo.MaxVersion.ToString());
            Assert.True(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void NormalizeVersionFillsInZerosForUnsetVersionParts()
        {
            // Act
            Version version = VersionUtility.NormalizeVersion(new Version("1.5"));

            // Assert
            Assert.Equal(new Version("1.5.0.0"), version);
        }

        [Fact]
        public void ParseVersionSpecRangeIntegerRanges()
        {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("   [1, 2]  ");

            // Assert
            Assert.Equal("1.0", versionInfo.MinVersion.ToString());
            Assert.True(versionInfo.IsMinInclusive);
            Assert.Equal("2.0", versionInfo.MaxVersion.ToString());
            Assert.True(versionInfo.IsMaxInclusive);
        }

        [Fact]
        public void ParseVersionSpecRangeNegativeIntegerRanges()
        {
            // Act
            IVersionSpec versionInfo;
            bool parsed = VersionUtility.TryParseVersionSpec("   [-1, 2]  ", out versionInfo);

            Assert.False(parsed);
            Assert.Null(versionInfo);
        }

        public static IEnumerable<object[]> TrimVersionData
        {
            get
            {
                yield return new object[] { new Version(1, 2, 3, 0), new Version(1, 2, 3) };
                yield return new object[] { new Version("1.2.3.0"), new Version("1.2.3") };
                yield return new object[] { new Version(1, 2, 0, 0), new Version(1, 2) };
                yield return new object[] { new Version("1.2.0.0"), new Version("1.2") };
                yield return new object[] { new Version(1, 2, 0, 5), new Version(1, 2, 0, 5) };

            }
        }

        [Theory]
        [PropertyData("TrimVersionData")]
        public void TrimVersionTrimsRevisionIfZero(Version version, Version expected)
        {
            // Act
            var result = VersionUtility.TrimVersion(version);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetAllPossibleVersionsTwoDigits()
        {
            // Arrange
            var expectedVersions = new[] { 
                new SemanticVersion("1.1"), 
                new SemanticVersion("1.1.0"),
                new SemanticVersion("1.1.0.0")
            };

            // Act
            var versions = VersionUtility.GetPossibleVersions(new SemanticVersion("1.1")).ToList();

            // Assert
            Assert.Equal(expectedVersions, versions);
        }

        [Fact]
        public void GetAllPossibleVersionsThreeDigits()
        {
            // Arrange
            var expectedVersions = new[] { 
                new SemanticVersion("1.0"), 
                new SemanticVersion("1.0.0"),
                new SemanticVersion("1.0.0.0"),
            };

            // Act
            var versions = VersionUtility.GetPossibleVersions(new SemanticVersion("1.0.0")).ToList();

            // Assert
            Assert.Equal(expectedVersions, versions);
        }

        [Fact]
        public void GetAllPossibleVersionsFourDigits()
        {
            // Arrange
            var expectedVersions = new[] { 
                new SemanticVersion("1.0"), 
                new SemanticVersion("1.0.0"),
                new SemanticVersion("1.0.0.0"),
            };
            var expectedVersionStrings = new[] {
                "1.0", 
                "1.0.0",
                "1.0.0.0"
            };

            // Act
            var versions = VersionUtility.GetPossibleVersions(new SemanticVersion("1.0.0.0")).ToList();

            // Assert
            Assert.Equal(expectedVersions, versions);
            Assert.Equal(expectedVersionStrings, versions.Select(v => v.ToString()));
        }

        [Fact]
        public void GetAllPossibleVersionsThreeDigitsWithZeroBetween()
        {
            // Arrange
            var expectedVersions = new[] { 
                new SemanticVersion("1.0.1"), 
                new SemanticVersion("1.0.1.0")
            };
            var expectedVersionStrings = new[] 
            {
                "1.0.1",
                "1.0.1.0",
            };

            // Act
            var versions = VersionUtility.GetPossibleVersions(new SemanticVersion("1.0.1")).ToList();

            // Assert
            Assert.Equal(expectedVersions, versions);
            Assert.Equal(expectedVersionStrings, versions.Select(v => v.ToString()));
        }

        [Fact]
        public void GetAllPossibleVersionsFourDigitsWithTrailingZeros()
        {
            // Arrange
            var expectedVersions = new[] { 
                new SemanticVersion("1.1"),
                new SemanticVersion("1.1.0"),
                new SemanticVersion("1.1.0.0"),
            };
            var expectedVersionStrings = new[] 
            {
                "1.1",
                "1.1.0",
                "1.1.0.0",
            };

            // Act
            var versions = VersionUtility.GetPossibleVersions(new SemanticVersion("1.1.0.0")).ToList();

            // Assert
            Assert.Equal(expectedVersions, versions);
            Assert.Equal(expectedVersionStrings, versions.Select(v => v.ToString()));
        }

        [Fact]
        public void GetSafeVersions()
        {
            // Act
            IVersionSpec versionSpec1 = VersionUtility.GetSafeRange(new SemanticVersion("1.3"));
            IVersionSpec versionSpec2 = VersionUtility.GetSafeRange(new SemanticVersion("0.9"));
            IVersionSpec versionSpec3 = VersionUtility.GetSafeRange(new SemanticVersion("2.9.45.6"));

            // Assert
            AssertSafeVersion(versionSpec1, new SemanticVersion("1.3"), new SemanticVersion("1.4"));
            AssertSafeVersion(versionSpec2, new SemanticVersion("0.9"), new SemanticVersion("0.10"));
            AssertSafeVersion(versionSpec3, new SemanticVersion("2.9.45.6"), new SemanticVersion("2.10"));
        }

        private void AssertSafeVersion(IVersionSpec versionSpec, SemanticVersion minVer, SemanticVersion maxVer)
        {
            Assert.True(versionSpec.IsMinInclusive);
            Assert.False(versionSpec.IsMaxInclusive);
            Assert.Equal(versionSpec.MinVersion, minVer);
            Assert.Equal(versionSpec.MaxVersion, maxVer);
        }

        [Fact]
        public void TrimVersionThrowsIfVersionNull()
        {
            ExceptionAssert.ThrowsArgNull(() => VersionUtility.TrimVersion(null), "version");
        }

        [Fact]
        public void IsCompatibleReturnsFalseForSlAndWindowsPhoneFrameworks()
        {
            // Arrange
            FrameworkName sl3 = VersionUtility.ParseFrameworkName("sl3");
            FrameworkName wp7 = VersionUtility.ParseFrameworkName("sl3-wp");

            // Act
            bool wp7CompatibleWithSl = VersionUtility.IsCompatible(sl3, wp7);
            bool slCompatibleWithWp7 = VersionUtility.IsCompatible(wp7, sl3);

            // Assert
            Assert.False(slCompatibleWithWp7);
            Assert.False(wp7CompatibleWithSl);
        }

        [Fact]
        public void IsCompatibleWindowsPhoneVersions()
        {
            // Arrange
            FrameworkName wp7 = VersionUtility.ParseFrameworkName("sl3-wp");
            FrameworkName wp7Mango = VersionUtility.ParseFrameworkName("sl4-wp71");
            FrameworkName wp8 = new FrameworkName("WindowsPhone, Version=v8.0");

            // Act
            bool wp7MangoCompatibleWithwp7 = VersionUtility.IsCompatible(wp7, wp7Mango);
            bool wp7CompatibleWithwp7Mango = VersionUtility.IsCompatible(wp7Mango, wp7);

            bool wp7CompatibleWithwp8 = VersionUtility.IsCompatible(wp8, wp7);
            bool wp7MangoCompatibleWithwp8 = VersionUtility.IsCompatible(wp8, wp7Mango);

            bool wp8CompatibleWithwp7 = VersionUtility.IsCompatible(wp7, wp8);
            bool wp8CompatbielWithwp7Mango = VersionUtility.IsCompatible(wp7Mango, wp8);

            // Assert
            Assert.False(wp7MangoCompatibleWithwp7);
            Assert.True(wp7CompatibleWithwp7Mango);

            Assert.True(wp7CompatibleWithwp8);
            Assert.True(wp7MangoCompatibleWithwp8);

            Assert.False(wp8CompatibleWithwp7);
            Assert.False(wp8CompatbielWithwp7Mango);
        }

        [Theory]
        [InlineData("wp")]
        [InlineData("wp7")]
        [InlineData("wp70")]
        [InlineData("windowsphone")]
        [InlineData("windowsphone7")]
        [InlineData("windowsphone70")]
        [InlineData("sl3-wp")]
        public void WindowsPhone7IdentifierCompatibleWithAllWPProjects(string wp7Identifier)
        {
            // Arrange
            var wp7Package = VersionUtility.ParseFrameworkName(wp7Identifier);

            var wp7Project = new FrameworkName("Silverlight, Version=v3.0, Profile=WindowsPhone");
            var mangoProject = new FrameworkName("Silverlight, Version=v4.0, Profile=WindowsPhone71");
            var apolloProject = new FrameworkName("WindowsPhone, Version=v8.0");

            // Act & Assert
            Assert.True(VersionUtility.IsCompatible(wp7Project, wp7Package));
            Assert.True(VersionUtility.IsCompatible(mangoProject, wp7Package));
            Assert.True(VersionUtility.IsCompatible(apolloProject, wp7Package));
        }

        [Theory]
        [InlineData("wp71")]
        [InlineData("windowsphone71")]
        [InlineData("sl4-wp71")]
        public void WindowsPhoneMangoIdentifierCompatibleWithAllWPProjects(string mangoIdentifier)
        {
            // Arrange
            var mangoPackage = VersionUtility.ParseFrameworkName(mangoIdentifier);

            var wp7Project = new FrameworkName("Silverlight, Version=v3.0, Profile=WindowsPhone");
            var mangoProject = new FrameworkName("Silverlight, Version=v4.0, Profile=WindowsPhone71");
            var apolloProject = new FrameworkName("WindowsPhone, Version=v8.0");

            // Act & Assert
            Assert.False(VersionUtility.IsCompatible(wp7Project, mangoPackage));
            Assert.True(VersionUtility.IsCompatible(mangoProject, mangoPackage));
            Assert.True(VersionUtility.IsCompatible(apolloProject, mangoPackage));
        }

        [Theory]
        [InlineData("wp8")]
        [InlineData("wp80")]
        [InlineData("windowsphone8")]
        [InlineData("windowsphone80")]
        public void WindowsPhoneApolloIdentifierCompatibleWithAllWPProjects(string apolloIdentifier)
        {
            // Arrange
            var apolloPackage = VersionUtility.ParseFrameworkName(apolloIdentifier);

            var wp7Project = new FrameworkName("Silverlight, Version=v3.0, Profile=WindowsPhone");
            var mangoProject = new FrameworkName("Silverlight, Version=v4.0, Profile=WindowsPhone71");
            var apolloProject = new FrameworkName("WindowsPhone, Version=v8.0");

            // Act & Assert
            Assert.False(VersionUtility.IsCompatible(wp7Project, apolloPackage));
            Assert.False(VersionUtility.IsCompatible(mangoProject, apolloPackage));
            Assert.True(VersionUtility.IsCompatible(apolloProject, apolloPackage));
        }

        [Theory]
        [InlineData("windows")]
        [InlineData("windows8")]
        [InlineData("win")]
        [InlineData("win8")]
        public void WindowsIdentifierCompatibleWithWindowsStoreAppProjects(string identifier)
        {
            // Arrange
            var packageFramework = VersionUtility.ParseFrameworkName(identifier);

            var projectFramework = new FrameworkName(".NETCore, Version=4.5");

            // Act && Assert
            Assert.True(VersionUtility.IsCompatible(projectFramework, packageFramework));
        }

        [Theory]
        [InlineData("windows9")]
        [InlineData("win9")]
        [InlineData("win10")]
        [InlineData("windows81")]
        [InlineData("windows45")]
        [InlineData("windows1")]
        public void WindowsIdentifierWithUnsupportedVersionNotCompatibleWithWindowsStoreAppProjects(string identifier)
        {
            // Arrange
            var packageFramework = VersionUtility.ParseFrameworkName(identifier);

            var projectFramework = new FrameworkName(".NETCore, Version=4.5");

            // Act && Assert
            Assert.False(VersionUtility.IsCompatible(projectFramework, packageFramework));
        }

        [Fact]
        public void NetFrameworkCompatibiilityIsCompatibleReturns()
        {
            // Arrange
            FrameworkName net40 = VersionUtility.ParseFrameworkName("net40");
            FrameworkName net40Client = VersionUtility.ParseFrameworkName("net40-client");

            // Act
            bool netClientCompatibleWithNet = VersionUtility.IsCompatible(net40, net40Client);
            bool netCompatibleWithClient = VersionUtility.IsCompatible(net40Client, net40);

            // Assert
            Assert.True(netClientCompatibleWithNet);
            Assert.True(netCompatibleWithClient);
        }

        [Fact]
        public void LowerFrameworkVersionsAreNotCompatibleWithHigherFrameworkVersionsWithSameFrameworkName()
        {
            // Arrange
            FrameworkName net40 = VersionUtility.ParseFrameworkName("net40");
            FrameworkName net20 = VersionUtility.ParseFrameworkName("net20");

            // Act
            bool net40CompatibleWithNet20 = VersionUtility.IsCompatible(net20, net40);
            bool net20CompatibleWithNet40 = VersionUtility.IsCompatible(net40, net20);

            // Assert
            Assert.False(net40CompatibleWithNet20);
            Assert.True(net20CompatibleWithNet40);
        }

        [Fact]
        public void IsCompatibleReturnsTrueIfSupportedFrameworkListIsEmpty()
        {
            // Arrange
            FrameworkName net40Client = VersionUtility.ParseFrameworkName("net40-client");

            // Act
            var result = VersionUtility.IsCompatible(net40Client, Enumerable.Empty<FrameworkName>());

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParseVersionThrowsIfExclusiveMinAndMaxVersionSpecContainsNoValues()
        {
            // Arrange
            var versionString = "(,)";

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => VersionUtility.ParseVersionSpec(versionString));
            Assert.Equal("'(,)' is not a valid version string.", exception.Message);
        }

        [Fact]
        public void ParseVersionThrowsIfInclusiveMinAndMaxVersionSpecContainsNoValues()
        {
            // Arrange
            var versionString = "[,]";

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => VersionUtility.ParseVersionSpec(versionString));
            Assert.Equal("'[,]' is not a valid version string.", exception.Message);
        }

        [Fact]
        public void ParseVersionThrowsIfInclusiveMinAndExclusiveMaxVersionSpecContainsNoValues()
        {
            // Arrange
            var versionString = "[,)";

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => VersionUtility.ParseVersionSpec(versionString));
            Assert.Equal("'[,)' is not a valid version string.", exception.Message);
        }

        [Fact]
        public void ParseVersionThrowsIfExclusiveMinAndInclusiveMaxVersionSpecContainsNoValues()
        {
            // Arrange
            var versionString = "(,]";

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => VersionUtility.ParseVersionSpec(versionString));
            Assert.Equal("'(,]' is not a valid version string.", exception.Message);
        }

        [Fact]
        public void ParseVersionThrowsIfVersionSpecIsMissingVersionComponent()
        {
            // Arrange
            var versionString = "(,1.3..2]";

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => VersionUtility.ParseVersionSpec(versionString));
            Assert.Equal("'(,1.3..2]' is not a valid version string.", exception.Message);
        }

        [Fact]
        public void ParseVersionThrowsIfVersionSpecContainsMoreThen4VersionComponents()
        {
            // Arrange
            var versionString = "(1.2.3.4.5,1.2]";

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => VersionUtility.ParseVersionSpec(versionString));
            Assert.Equal("'(1.2.3.4.5,1.2]' is not a valid version string.", exception.Message);
        }

        [Theory]
        [PropertyData("VersionSpecData")]
        public void ParseVersionParsesTokensVersionsCorrectly(string versionString, VersionSpec versionSpec)
        {
            // Act
            var actual = VersionUtility.ParseVersionSpec(versionString);

            // Assert
            Assert.Equal(versionSpec.IsMinInclusive, actual.IsMinInclusive);
            Assert.Equal(versionSpec.IsMaxInclusive, actual.IsMaxInclusive);
            Assert.Equal(versionSpec.MinVersion, actual.MinVersion);
            Assert.Equal(versionSpec.MaxVersion, actual.MaxVersion);
        }

        public static IEnumerable<object[]> VersionSpecData
        {
            get
            {
                yield return new object[] { "(1.2.3.4, 3.2)", new VersionSpec { MinVersion = new SemanticVersion("1.2.3.4"), MaxVersion = new SemanticVersion("3.2"), IsMinInclusive = false, IsMaxInclusive = false } };
                yield return new object[] { "(1.2.3.4, 3.2]", new VersionSpec { MinVersion = new SemanticVersion("1.2.3.4"), MaxVersion = new SemanticVersion("3.2"), IsMinInclusive = false, IsMaxInclusive = true } };
                yield return new object[] { "[1.2, 3.2.5)", new VersionSpec { MinVersion = new SemanticVersion("1.2"), MaxVersion = new SemanticVersion("3.2.5"), IsMinInclusive = true, IsMaxInclusive = false } };
                yield return new object[] { "[2.3.7, 3.2.4.5]", new VersionSpec { MinVersion = new SemanticVersion("2.3.7"), MaxVersion = new SemanticVersion("3.2.4.5"), IsMinInclusive = true, IsMaxInclusive = true } };
                yield return new object[] { "(, 3.2.4.5]", new VersionSpec { MinVersion = null, MaxVersion = new SemanticVersion("3.2.4.5"), IsMinInclusive = false, IsMaxInclusive = true } };
                yield return new object[] { "(1.6, ]", new VersionSpec { MinVersion = new SemanticVersion("1.6"), MaxVersion = null, IsMinInclusive = false, IsMaxInclusive = true } };
                yield return new object[] { "(1.6)", new VersionSpec { MinVersion = new SemanticVersion("1.6"), MaxVersion = new SemanticVersion("1.6"), IsMinInclusive = false, IsMaxInclusive = false } };
                yield return new object[] { "[2.7]", new VersionSpec { MinVersion = new SemanticVersion("2.7"), MaxVersion = new SemanticVersion("2.7"), IsMinInclusive = true, IsMaxInclusive = true } };
            }
        }

        [Fact]
        public void ParsePortableFrameworkNameThrowsIfProfileIsEmpty()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentException(
                () => VersionUtility.ParseFrameworkName("portable45"),
                "profilePart",
                "Portable target framework must not have an empty profile part.");
        }

        [Fact]
        public void ParsePortableFrameworkNameThrowsIfProfileContainsASpace()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentException(
                () => VersionUtility.ParseFrameworkName("portable45-sl4 net45"),
                "profilePart",
                "The profile part of a portable target framework must not contain empty space.");
        }

        [Fact]
        public void ParsePortableFrameworkNameThrowsIfProfileContainsEmptyComponent()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentException(
                () => VersionUtility.ParseFrameworkName("portable45-sl4++net45"),
                "profilePart",
                "The profile part of a portable target framework must not contain empty component.");
        }

        [Fact]
        public void ParsePortableFrameworkNameThrowsIfProfileContainsPortableFramework()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentException(
                () => VersionUtility.ParseFrameworkName("portable-net45+portable"),
                "profilePart",
                "The profile part of a portable target framework must not contain a portable framework component.");
        }

        [Fact]
        public void TestGetShortNameForPortableFramework()
        {
            // Arrange
            NetPortableProfileTable.Profiles = BuildProfileCollection();

            var framework = new FrameworkName(".NETPortable, Version=4.0, Profile=Profile1");

            // Act-1
            string shortName = VersionUtility.GetShortFrameworkName(framework);

            // Assert-2
            Assert.Equal("portable-net45+sl40+wp71", shortName);

            // Arrange
            var framework2 = new FrameworkName(".NETPortable, Version=4.0, Profile=Profile2");

            // Act-2
            string shortName2 = VersionUtility.GetShortFrameworkName(framework2);

            // Assert-2
            Assert.Equal("portable-win+sl30+wp71", shortName2);

            // Arrange
            var framework3 = new FrameworkName(".NETPortable, Version=4.0, Profile=Profile4");

            // Act-3
            string shortName3 = VersionUtility.GetShortFrameworkName(framework3);

            // Assert-4
            Assert.Equal("portable-sl20+wp", shortName3);
        }

        [Fact]
        public void GetShortNameDoesNotIncludeVersionIfVersionIs00()
        {
            // Act
            string shortName = VersionUtility.GetShortFrameworkName(new FrameworkName("Silverlight, Version=v0.0"));

            // Assert
            Assert.Equal("sl", shortName);
        }

        [Fact]
        public void GetShortNameForNetCore45ReturnsWindows()
        {
            // Act
            string shortName = VersionUtility.GetShortFrameworkName(new FrameworkName(".NETCore, Version=v4.5"));

            // Assert
            Assert.Equal("win", shortName);
        }

        [Fact]
        public void GetShortNameForWindowsPhoneReturnsWP()
        {
            // Act
            string shortName = VersionUtility.GetShortFrameworkName(new FrameworkName("Silverlight, Version=v3.0, Profile=WindowsPhone"));

            // Assert
            Assert.Equal("wp", shortName);
        }

        [Fact]
        public void GetShortNameForMangoReturnsWP71()
        {
            // Act
            string shortName = VersionUtility.GetShortFrameworkName(new FrameworkName("Silverlight, Version=v4.0, Profile=WindowsPhone71"));

            // Assert
            Assert.Equal("wp71", shortName);
        }

        [Fact]
        public void IsCompatibleReturnsTrueForPortableFrameworkAndNormalFramework()
        {
            // Arrange
            var portableFramework = VersionUtility.ParseFrameworkName("portable-netcore45+sl4");
            var normalFramework = VersionUtility.ParseFrameworkName("silverlight45");

            // Act
            bool isCompatible = VersionUtility.IsCompatible(normalFramework, portableFramework);

            // Assert
            Assert.True(isCompatible);
        }

        [Fact]
        public void IsCompatibleReturnsFalseForPortableFrameworkAndNormalFramework()
        {
            // Arrange
            var portableFramework = VersionUtility.ParseFrameworkName("portable-netcore45+sl4");
            var normalFramework = VersionUtility.ParseFrameworkName("silverlight3");

            // Act
            bool isCompatible = VersionUtility.IsCompatible(normalFramework, portableFramework);

            // Assert
            Assert.False(isCompatible);
        }

        [Fact]
        public void IsCompatibleReturnsFalseForPortableFrameworkAndNormalFramework2()
        {
            // Arrange
            var portableFramework = VersionUtility.ParseFrameworkName("portable-netcore45+sl4");
            var normalFramework = VersionUtility.ParseFrameworkName("wp7");

            // Act
            bool isCompatible = VersionUtility.IsCompatible(normalFramework, portableFramework);

            // Assert
            Assert.False(isCompatible);
        }

        [Theory]
        [InlineData("silverlight4")]
        [InlineData("silverlight3")]
        [InlineData("silverlight5")]
        [InlineData("netcore45")]
        [InlineData("netcore5")]
        public void IsCompatibleReturnsFalseForNormalFrameworkAndPortableFramework(string frameworkValue)
        {
            // Arrange
            var portableFramework = VersionUtility.ParseFrameworkName("portable-netcore45+sl4");
            var normalFramework = VersionUtility.ParseFrameworkName(frameworkValue);

            // Act
            bool isCompatible = VersionUtility.IsCompatible(portableFramework, normalFramework);

            // Assert
            Assert.False(isCompatible);
        }

        [Theory]
        [InlineData("portable-netcore45+sl4+wp", "portable-netcore45+sl4")]
        [InlineData("portable-netcore45+sl4+wp", "portable-netcore5+wp7")]
        [InlineData("portable-netcore45+sl4+wp+net", "portable-wp7")]
        public void IsCompatibleReturnsTrueForPortableFrameworkAndPortableFramework(string packageFramework, string projectFramework)
        {
            // Arrange
            var packagePortableFramework = VersionUtility.ParseFrameworkName(packageFramework);
            var projectPortableFramework = VersionUtility.ParseFrameworkName(projectFramework);

            // Act
            bool isCompatible = VersionUtility.IsCompatible(projectPortableFramework, packagePortableFramework);

            // Assert
            Assert.True(isCompatible);
        }

        [Theory]
        [InlineData("portable-netcore45+sl4+wp", "portable-netcore4+sl4")]
        [InlineData("portable-netcore45+sl4+wp", "portable-netcore5+wp7+net")]
        [InlineData("portable-netcore45+sl4+wp+net", "portable-wp7+netcore4")]
        [InlineData("portable-netcore45+sl4", "portable-net4+wp7")]
        public void IsCompatibleReturnsFalseForPortableFrameworkAndPortableFramework(string packageFramework, string projectFramework)
        {
            // Arrange
            var packagePortableFramework = VersionUtility.ParseFrameworkName(packageFramework);
            var projectPortableFramework = VersionUtility.ParseFrameworkName(projectFramework);

            // Act
            bool isCompatible = VersionUtility.IsCompatible(projectPortableFramework, packagePortableFramework);

            // Assert
            Assert.False(isCompatible);
        }

        [Theory]
        [InlineData("portable-net45+sl5+wp71", "portable-net45+sl5+wp71", -3)]
        [InlineData("portable-net45+sl5+wp71", "portable-net45+sl5+wp71+win8", -4)]
        [InlineData("portable-net45+sl5+wp71", "portable-net45+sl4+wp71+win8", -54)]
        [InlineData("portable-net45+sl5+wp71", "portable-net4+sl4+wp71+win8", -104)]
        [InlineData("portable-net45+sl5+wp71", "portable-net4+sl4+wp7+win8", -154)]
        [InlineData("portable-win8+wp8", "portable-win8+wp7", -52)]
        [InlineData("portable-win8+wp8", "portable-win8+wp7+silverlight4", -53)]
        public void TestGetCompatibilityBetweenPortableLibraryAndPortableLibrary(string frameworkName, string targetFrameworkName, int expectedScore)
        {
            // Arrange
            var framework = VersionUtility.ParseFrameworkName(frameworkName);
            var targetFramework = VersionUtility.ParseFrameworkName(targetFrameworkName);

            // Act
            int score = VersionUtility.GetCompatibilityBetweenPortableLibraryAndPortableLibrary(framework, targetFramework);

            // Assert
            Assert.Equal(expectedScore, score);
        }

        private NetPortableProfileCollection BuildProfileCollection()
        {
            var profileCollection = new NetPortableProfileCollection();
            var profile1 = new NetPortableProfile(
               "Profile1",
               new[] { 
                           new FrameworkName(".NETFramework, Version=4.5"), 
                           new FrameworkName("Silverlight, Version=4.0"), 
                           new FrameworkName("WindowsPhone, Version=7.1"), 
                      });

            var profile2 = new NetPortableProfile(
               "Profile2",
               new[] { 
                           new FrameworkName(".NetCore, Version=4.5"), 
                           new FrameworkName("Silverlight, Version=3.0"), 
                           new FrameworkName("WindowsPhone, Version=7.1"), 
                      });

            var profile3 = new NetPortableProfile(
               "Profile3",
               new[] { 
                           new FrameworkName(".NetCore, Version=4.5"), 
                           new FrameworkName(".NETFramework, Version=2.0"), 
                      });

            var profile4 = new NetPortableProfile(
               "Profile4",
               new[] { 
                           new FrameworkName("Silverlight, Version=2.0"), 
                           new FrameworkName("Silverlight, Version=3.0, Profile=WindowsPhone"), 
                      });

            profileCollection.Add(profile1);
            profileCollection.Add(profile2);
            profileCollection.Add(profile3);
            profileCollection.Add(profile4);

            return profileCollection;
        }
    }
}