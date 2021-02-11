using System;
using Amazon.CloudWatch.EMF.Serializer;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Newtonsoft.Json;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Serializer
{
    public class UnixMillisecondDateTimeConverterTests
    {
        private readonly IFixture _fixture;
        public UnixMillisecondDateTimeConverterTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }
        
        [Fact]
        public void WriteJson_Throws_When_ObjectIsNotDateTime()
        {
            // Arrange
            var serializer = _fixture.Create<UnixMillisecondDateTimeConverter>();
            var obj = string.Empty;

            //Act
            Action act = () => serializer.WriteJson(null, obj, null);

            //Assert
            Assert.Throws<JsonSerializationException>(act);
        }
        
        [Fact]
        public void WriteJson_Throws_When_DateTimeIsNotValid()
        {
            // Arrange
            var serializer = _fixture.Create<UnixMillisecondDateTimeConverter>();
            var obj = new DateTime(1969, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            //Act
            Action act = () => serializer.WriteJson(null, obj, null);

            //Assert
            Assert.Throws<JsonSerializationException>(act);
        }
        
        [Fact]
        public void WriteJson_Succeeds()
        {
            // Arrange
            var serializer = _fixture.Create<UnixMillisecondDateTimeConverter>();
            var writer = _fixture.Create<JsonWriter>();
            var jsonSerializer = _fixture.Create<JsonSerializer>();
            var obj = DateTime.Now;

            //Act
            serializer.WriteJson(writer, obj, jsonSerializer);

            //Assert
        }
    }
}