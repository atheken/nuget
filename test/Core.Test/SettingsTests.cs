﻿using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using Xunit;
using NuGet.Test.Mocks;
using Xunit.Extensions;
using NuGet.Test.Utility;

namespace NuGet.Test
{
    public class SettingsTests
    {
        [Fact]
        public void CallingCtroWithNullFileSystemWithThrowException()
        {
            // Act & Assert
            ExceptionAssert.Throws<ArgumentNullException>(() => new Settings(null));
        }

        [Fact]
        public void WillGetConfigurationFromSpecifiedPath()
        {
            // Arrange 
            const string configFile = "NuGet.Config";
            var mockFileSystem = new Mock<IFileSystem>();
            string config = @"
<configuration>
    <SectionName>
        <add key='key1' value='value1' />
        <add Key='key2' Value='value2' />
    </SectionName>
</configuration>";
            mockFileSystem.Setup(m => m.OpenFile(configFile)).Returns(config.AsStream());
            mockFileSystem.Setup(m => m.FileExists(configFile)).Returns(true);

            // Act
            new Settings(mockFileSystem.Object);

            // Assert 
            mockFileSystem.Verify(x => x.OpenFile(configFile), Times.Once(), "File was not read");
        }

        [Fact]
        public void CallingGetValuesWithNullSectionWillThrowException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.GetValues(null));
        }

        [Fact]
        public void CallingGetValueWithNullSectionWillThrowException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.GetValue(null, "SomeKey"));
        }

        [Fact]
        public void CallingGetValueWithNullKeyWillThrowException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.GetValue("SomeSection", null));
        }

        [Fact]
        public void CallingCtorWithMalformedConfigThrowsException()
        {
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<configuration><sectionName></configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);

            // Act & Assert
            ExceptionAssert.Throws<System.Xml.XmlException>(() => new Settings(mockFileSystem));

        }

        [Fact]
        public void UserSetting_CallingGetValuesWithNonExistantSectionReturnsEmpty()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            
            string config = @"<configuration></configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result = settings.GetValues("DoesNotExisit");

            // Assert 
            Assert.Empty(result);
        }

        [Fact]
        public void CallingGetValuesWithSectionWithInvalidAddItemsThrows()
        {
            // Arrange
            var config = @"
<configuration>
    <SectionName>
        <add Key='key2' Value='value2' />
    </SectionName>
</configuration>";
            var nugetConfigPath = "NuGet.Config";
            var mockFileSystem = new MockFileSystem(@"x:\test");
            mockFileSystem.AddFile(nugetConfigPath, config.AsStream());
            Settings settings = new Settings(mockFileSystem);

            // Act and Assert
            ExceptionAssert.Throws<InvalidDataException>(() => settings.GetValues("SectionName"), @"Unable to parse config file 'x:\test\NuGet.Config'.");
        }

        [Fact]
        public void GetValuesThrowsIfSettingsIsMissingKeys()
        {
            var config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
<packageSources>
<add key="""" value=""C:\Temp\Nuget"" />
</packageSources>
<activePackageSource>
<add key=""test2"" value=""C:\Temp\Nuget"" />
</activePackageSource>
</configuration>";
            var nugetConfigPath = "NuGet.Config";
            var mockFileSystem = new MockFileSystem(@"x:\test");
            mockFileSystem.AddFile(nugetConfigPath, config.AsStream());
            Settings settings = new Settings(mockFileSystem);

            // Act and Assert
            ExceptionAssert.Throws<InvalidDataException>(() => settings.GetValues("packageSources"), @"Unable to parse config file 'x:\test\NuGet.Config'.");
        }

        [Fact]
        public void CallingGetValuesWithoutSectionReturnsEmptyList()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"
<configuration>
    <SectionName>
        <add key='key1' value='value1' />
        <add key='key2' value='value2' />
    </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result = settings.GetValues("NotTheSectionName");

            // Arrange 
            Assert.Empty(result);
        }

        [Fact]
        public void CallingGetValueWithoutSectionReturnsNull()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"
<configuration>
    <SectionName>
        <add key='key1' value='value1' />
        <add key='key2' value='value2' />
    </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result = settings.GetValue("NotTheSectionName", "key1");

            // Arrange 
            Assert.Null(result);
        }

        [Fact]
        public void CallingGetValueWithSectionButNoValidKeyReturnsNull()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"
<configuration>
    <SectionName>
        <add key='key1' value='value1' />
        <add key='key2' value='value2' />
    </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result = settings.GetValue("SectionName", "key3");

            // Assert 
            Assert.Null(result);
        }

        [Fact]
        public void CallingGetValuesWithSectionReturnsDictionary()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"
<configuration>
    <SectionName>
        <add key='key1' value='value1' />
        <add key='key2' value='value2' />
    </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result = settings.GetValues("SectionName");

            // Assert 
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void CallingGetValueWithSectionAndKeyReturnsValue()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"
<configuration>
    <SectionName>
        <add key='key1' value='value1' />
    </SectionName>
    <SectionNameTwo>
        <add key='key2' value='value2' />
    </SectionNameTwo>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result1 = settings.GetValue("SectionName", "key1");
            var result2 = settings.GetValue("SectionNameTwo", "key2");

            // Assert 
            Assert.Equal("value1", result1);
            Assert.Equal("value2", result2);
        }

        [Fact]
        public void CallingSetValueWithEmptySectionNameThrowsException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.SetValue("", "SomeKey", "SomeValue"));
        }

        [Fact]
        public void CallingSetValueWithEmptyKeyThrowsException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.SetValue("SomeKey", "", "SomeValue"));
        }

        [Fact]
        public void CallingSetValueWillAddSectionIfItDoesNotExist()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetValue("NewSectionName", "key", "value");

            // Assert
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
  <NewSectionName>
    <add key=""key"" value=""value"" />
  </NewSectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetValueWillAddToSectionIfItExist()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetValue("SectionName", "keyTwo", "valueTwo");

            // Assert
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
    <add key=""keyTwo"" value=""valueTwo"" />
  </SectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetValueWillOverrideValueIfKeyExists()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetValue("SectionName", "key", "NewValue");

            // Assert
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""NewValue"" />
  </SectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetValuesWithEmptySectionThrowsException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var values = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("key", "value") };
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.SetValues("", values));
        }

        [Fact]
        public void CallingSetValuesWithNullValuesThrowsException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentNullException>(() => settings.SetValues("Section", null));
        }

        [Fact]
        public void CallingSetValuesWithEmptyKeyThrowsException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var values = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("", "value") };
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.SetValues("Section", values));
        }

        [Fact]
        public void CallingSetValuseWillAddSectionIfItDoesNotExist()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            var values = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("key", "value") };
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetValues("NewSectionName", values);

            // Assert
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
  <NewSectionName>
    <add key=""key"" value=""value"" />
  </NewSectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetValuesWillAddToSectionIfItExist()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            var values = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("keyTwo", "valueTwo") };
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetValues("SectionName", values);

            // Assert
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
    <add key=""keyTwo"" value=""valueTwo"" />
  </SectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetValuesWillOverrideValueIfKeyExists()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            var values = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("key", "NewValue") };
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetValues("SectionName", values);

            // Assert
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""NewValue"" />
  </SectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetValuesWilladdValuesInOrder()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""Value"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            var values = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("key1", "Value1"), 
                                                                    new KeyValuePair<string, string>("key2", "Value2") };
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetValues("SectionName", values);

            // Assert
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""Value"" />
    <add key=""key1"" value=""Value1"" />
    <add key=""key2"" value=""Value2"" />
  </SectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetNestedValuesAddsItemsInNestedElement()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            var values = new [] { new KeyValuePair<string, string>("key1", "Value1"), 
                                  new KeyValuePair<string, string>("key2", "Value2") };
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetNestedValues("SectionName", "MyKey", values);

            // Assert
            XDocumentAssert.Equal(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <MyKey>
      <add key=""key1"" value=""Value1"" />
      <add key=""key2"" value=""Value2"" />
    </MyKey>
  </SectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetNestedValuesPreservesOtherKeys()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <MyKey>
      <add key=""key1"" value=""Value1"" />
      <add key=""key2"" value=""Value2"" />
    </MyKey>
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            var values = new[] { new KeyValuePair<string, string>("key3", "Value3"), 
                                  new KeyValuePair<string, string>("key4", "Value4") };
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetNestedValues("SectionName", "MyKey2", values);

            // Assert
            XDocumentAssert.Equal(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <MyKey>
      <add key=""key1"" value=""Value1"" />
      <add key=""key2"" value=""Value2"" />
    </MyKey>
    <MyKey2>
      <add key=""key3"" value=""Value3"" />
      <add key=""key4"" value=""Value4"" />
    </MyKey2>
  </SectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingSetNestedAppendsValuesToExistingKeys()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <MyKey>
      <add key=""key1"" value=""Value1"" />
      <add key=""key2"" value=""Value2"" />
    </MyKey>
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            var values = new[] { new KeyValuePair<string, string>("key3", "Value3"), 
                                  new KeyValuePair<string, string>("key4", "Value4") };
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetNestedValues("SectionName", "MyKey", values);

            // Assert
            XDocumentAssert.Equal(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <MyKey>
      <add key=""key1"" value=""Value1"" />
      <add key=""key2"" value=""Value2"" />
      <add key=""key3"" value=""Value3"" />
      <add key=""key4"" value=""Value4"" />
    </MyKey>
  </SectionName>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingDeleteValueWithEmptyKeyThrowsException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.DeleteValue("SomeSection", ""));
        }

        [Fact]
        public void CallingDeleteValueWithEmptySectionThrowsException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.DeleteValue("", "SomeKey"));
        }

        [Fact]
        public void CallingDeleteValueWhenSectionNameDoesntExistReturnsFalse()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value="""" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);
            
            // Act & Assert
            Assert.False(settings.DeleteValue("SectionDoesNotExists", "SomeKey"));
        }

        [Fact]
        public void CallingDeleteValueWhenKeyDoesntExistThrowsException()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value="""" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act & Assert
            Assert.False(settings.DeleteValue("SectionName", "KeyDoesNotExist"));
        }

        [Fact]
        public void CallingDeleteValueWithValidSectionAndKeyDeletesTheEntryAndReturnsTrue()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""DeleteMe"" value=""value"" />
    <add key=""keyNotToDelete"" value=""value"" />
  </SectionName>
  <SectionName2>
    <add key=""key"" value=""value"" />
  </SectionName2>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act & Assert
            Assert.True(settings.DeleteValue("SectionName", "DeleteMe"));
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""keyNotToDelete"" value=""value"" />
  </SectionName>
  <SectionName2>
    <add key=""key"" value=""value"" />
  </SectionName2>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }

        [Fact]
        public void CallingDeleteSectionWithEmptySectionThrowsException()
        {
            // Arrange 
            var mockFileSystem = new MockFileSystem();
            var settings = new Settings(mockFileSystem);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() => settings.DeleteSection(""));
        }

        [Fact]
        public void CallingDeleteSectionWhenSectionNameDoesntExistReturnsFalse()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value="""" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act & Assert
            Assert.False(settings.DeleteSection("SectionDoesNotExists"));
        }

        [Fact]
        public void CallingDeleteSectionWithValidSectionDeletesTheSectionAndReturnsTrue()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""DeleteMe"" value=""value"" />
    <add key=""keyNotToDelete"" value=""value"" />
  </SectionName>
  <SectionName2>
    <add key=""key"" value=""value"" />
  </SectionName2>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act & Assert
            Assert.True(settings.DeleteSection("SectionName"));
            XDocumentAssert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName2>
    <add key=""key"" value=""value"" />
  </SectionName2>
</configuration>", mockFileSystem.ReadAllText(nugetConfigPath));
        }


        /* Extension Methods for Settings Class */
        [Fact]
        public void UserSettingsExtentions_SetEncryptedValue()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value=""value"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            settings.SetEncryptedValue("SectionName", "key", "NewValue");

            // Assert
            var content = mockFileSystem.ReadAllText(nugetConfigPath);
            Assert.False(content.Contains("NewValue"));
        }

        [Fact]
        public void UserSettingsExtentions_GetEncryptedValue()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            Settings settings = new Settings(mockFileSystem);
            settings.SetEncryptedValue("SectionName", "key", "value");

            // Act
            var result = settings.GetDecryptedValue("SectionName", "key");

            // Assert
            Assert.Equal("value", result);
        }

        [Fact]
        public void UserSettingsExtentions_GetDecryptedValueWithEmptyValueReturnsEmptyString()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value="""" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result = settings.GetDecryptedValue("SectionName", "key");

            // Assert
            Assert.Equal(String.Empty, result);
        }

        [Fact]
        public void UserSettingsExtentions_GetDecryptedValueWithNoKeyReturnsNull()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key"" value="""" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result = settings.GetDecryptedValue("SectionName", "NoKeyByThatName");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetConfigSettingReadsValueFromConfigSection()
        {
            // Arrange
            var settings = new Mock<ISettings>(MockBehavior.Strict);
            settings.Setup(s => s.GetValue("config", "foo"))
                    .Returns("bar")
                    .Verifiable();

            // Act
            var result = settings.Object.GetConfigValue("foo");

            // Assert
            Assert.Equal("bar", result);
            settings.Verify();
        }

        [Fact]
        public void GetConfigSettingReadsEncryptedValueFromConfigSection()
        {
            // Arrange
            var settings = new Mock<ISettings>(MockBehavior.Strict);
            var value = EncryptionUtility.EncryptString("hello world");
            settings.Setup(s => s.GetValue("config", "foo"))
                    .Returns(value)
                    .Verifiable();

            // Act
            var result = settings.Object.GetConfigValue("foo", decrypt: true);

            // Assert
            Assert.Equal("hello world", result);
            settings.Verify();
        }

        [Fact]
        public void SetConfigSettingWritesValueToConfigSection()
        {
            // Arrange
            var settings = new Mock<ISettings>(MockBehavior.Strict);
            settings.Setup(s => s.SetValue("config", "foo", "bar"))
                    .Verifiable();

            // Act
            settings.Object.SetConfigValue("foo", "bar");

            // Assert
            settings.Verify();
        }

        [Fact]
        public void SetConfigSettingWritesEncryptedValueToConfigSection()
        {
            // Arrange
            string value = null;
            var settings = new Mock<ISettings>(MockBehavior.Strict);
            settings.Setup(s => s.SetValue("config", "foo", It.IsAny<string>()))
                    .Callback<string, string, string>((_, __, v) => value = v)
                    .Verifiable();

            // Act
            settings.Object.SetConfigValue("foo", "bar", encrypt: true);

            // Assert
            settings.Verify();
            Assert.Equal("bar", EncryptionUtility.DecryptString(value));
        }

        [Fact]
        public void DeleteConfigSettingDeletesValueFromConfigSection()
        {
            // Arrange
            var settings = new Mock<ISettings>(MockBehavior.Strict);
            settings.Setup(s => s.DeleteValue("config", "foo"))
                    .Returns(true)
                    .Verifiable();

            // Act
            bool result = settings.Object.DeleteConfigValue("foo");

            // Assert
            Assert.True(result);
            settings.Verify();
        }

        [Fact]
        public void GetValueIgnoresClearedValues()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""foo"" />
    <clear />
    <add key=""key2"" value=""bar"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result1 = settings.GetValue("SectionName", "Key1");
            var result2 = settings.GetValue("SectionName", "Key2");

            // Assert
            Assert.Null(result1);
            Assert.Equal("bar", result2);
        }

        private void AssertEqualCollections(IList<KeyValuePair<string, string>> actual, string[] expected)
        {
            Assert.Equal(actual.Count, expected.Length/2);
            for (int i=0;i<actual.Count;++i)
            {
                Assert.Equal(expected[2 * i], actual[i].Key);
                Assert.Equal(expected[2 * i + 1], actual[i].Value);
            }
        }

        [Fact]
        public void GetValuesIgnoresClearedValues()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var nugetConfigPath = "NuGet.Config";
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""value1"" />
    <add key=""key2"" value=""value2"" />
    <clear />
    <add key=""key3"" value=""value3"" />
    <add key=""key4"" value=""value4"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(nugetConfigPath, config);
            Settings settings = new Settings(mockFileSystem);

            // Act
            var result = settings.GetValues("SectionName");
            

            // Assert
            AssertEqualCollections(result, new [] { "key3", "value3", "key4", "value4"});            
        }



        [Fact]
        public void GetValuesMultipleConfFilesAdditive()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem(@"C:\mockfilesystem\dir1\dir2");
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key3"" value=""value3"" />
    <add key=""key4"" value=""value4"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile("NuGet.Config", config);
            config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""value1"" />
    <add key=""key2"" value=""value2"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(@"C:\mockfilesystem\dir1\NuGet.Config", config);

            var settings = Settings.LoadDefaultSettings(mockFileSystem);

            // Act
            var result = settings.GetValues("SectionName");

            // Assert
            AssertEqualCollections(result, new[] {"key1", "value1", "key2", "value2" , "key3", "value3", "key4", "value4" });
        }

        [Fact]
        public void GetValuesMultipleConfFilesClear()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem(@"C:\mockfilesystem\dir1\dir2");
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <clear /> <!-- i.e. ignore values from prior conf files -->
    <add key=""key3"" value=""value3"" />
    <add key=""key4"" value=""value4"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile("NuGet.Config", config);
            config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""value1"" />
    <add key=""key2"" value=""value2"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(@"C:\mockfilesystem\dir1\NuGet.Config", config);

            var settings = Settings.LoadDefaultSettings(mockFileSystem);

            // Act
            var result = settings.GetValues("SectionName");

            // Assert
            AssertEqualCollections(result, new[] { "key3", "value3", "key4", "value4" });
        }

        [Fact]
        public void GetSingleValuesMultipleConfFiles()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem(@"C:\mockfilesystem\dir1\dir2");
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key3"" value=""value3"" />
    <add key=""key4"" value=""value4"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile("NuGet.Config", config);
            config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""value1"" />
    <add key=""key2"" value=""value2"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(@"C:\mockfilesystem\dir1\NuGet.Config", config);

            var settings = Settings.LoadDefaultSettings(mockFileSystem);

            // Assert
            Assert.Equal("value4", settings.GetValue("SectionName", "key4"));
            Assert.Equal("value3", settings.GetValue("SectionName", "key3"));
            Assert.Equal("value2", settings.GetValue("SectionName", "key2"));
            Assert.Equal("value1", settings.GetValue("SectionName", "key1"));
        }

        [Fact]
        public void GetSingleValuesMultipleConfFilesWithDupes()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem(@"C:\mockfilesystem\dir1\dir2");
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""LastOneWins1"" />
    <add key=""key2"" value=""LastOneWins2"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(@"NuGet.Config", config);
            config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""value1"" />
    <add key=""key2"" value=""value2"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(@"C:\mockfilesystem\dir1\NuGet.Config", config);

            var settings = Settings.LoadDefaultSettings(mockFileSystem);

            // Assert
            Assert.Equal("LastOneWins2", settings.GetValue("SectionName", "key2"));
            Assert.Equal("LastOneWins1", settings.GetValue("SectionName", "key1"));
        }

        [Fact]
        public void GetSingleValuesMultipleConfFilesClear()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem(@"C:\mockfilesystem\dir1\dir2");
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <clear /> <!-- i.e. ignore values from prior conf files -->
    <add key=""key2"" value=""value2"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile("NuGet.Config", config);
            config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""value1"" />    
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(@"C:\mockfilesystem\dir1\NuGet.Config", config);

            var settings = Settings.LoadDefaultSettings(mockFileSystem);

            // Assert
            Assert.Equal("value2", settings.GetValue("SectionName", "key2"));
            Assert.Equal(null, settings.GetValue("SectionName", "key1"));
        }

        [Fact]
        public void GetValueReturnsPathRelativeToConfigWhenPathIsNotRooted()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem(@"x:\mock-directory\");
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""path-key"" value=""foo\bar"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile("nuget.config", config);
            var settings = new Settings(mockFileSystem, "nuget.config");

            // Act
            string result = settings.GetValue("SectionName", "path-key", isPath: true);

            // Assert
            Assert.Equal(@"x:\mock-directory\foo\bar", result);
        }

        [Fact]
        public void GetValuesWithUserSpecifiedDefaultConfigFile()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem(@"C:\mockfilesystem\dir1\dir2");
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key3"" value=""value3"" />
    <add key=""key4"" value=""value4"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile(@"C:\mockfilesystem\dir1\NuGet.Config", config);

            config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""key1"" value=""value1"" />
    <add key=""key2"" value=""value2"" />
  </SectionName>
</configuration>";
            mockFileSystem.AddFile("UserDefinedConfigFile.confg", config);

            var settings = Settings.LoadDefaultSettings(
                mockFileSystem, 
                "UserDefinedConfigFile.confg");

            // Act
            var result = settings.GetValues("SectionName");

            // Assert
            AssertEqualCollections(result, new[] { "key1", "value1", "key2", "value2", "key3", "value3", "key4", "value4" });
        }

        [Theory]
        [InlineData(@"z:\foo")]
        [InlineData(@"x:\foo\bar\qux")]
        public void GetValueReturnsPathWhenPathIsRooted(string value)
        {
            // Arrange
            var mockFileSystem = new MockFileSystem(@"x:\mock-directory\");
            string config = String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""path-key"" value=""{0}"" />
  </SectionName>
</configuration>", value);
            mockFileSystem.AddFile("nuget.config", config);
            var settings = new Settings(mockFileSystem, "nuget.config");

            // Act
            string result = settings.GetValue("SectionName", "path-key", isPath: true);

            // Assert
            XDocumentAssert.Equal(value, result);
        }

        [Fact]
        public void GetValueResolvesRelativePaths()
        {
            // Arrange
            var mockFileSystem = new Mock<MockFileSystem>(@"x:\mock-directory\") { CallBase = true };
            mockFileSystem.Setup(f => f.GetFullPath(@"x:\mock-directory\..\Blah")).Returns(@"x:\mock-directory\Qux\Blah").Verifiable();
            string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <SectionName>
    <add key=""path-key"" value=""..\Blah"" />
  </SectionName>
</configuration>";
            mockFileSystem.Object.AddFile("nuget.config", config);
            var settings = new Settings(mockFileSystem.Object, "nuget.config");

            // Act
            string result = settings.GetValue("SectionName", "path-key", isPath: true);

            // Assert
            mockFileSystem.Verify();
            Assert.Equal(@"x:\mock-directory\Qux\Blah", result);
        }
    }
}
