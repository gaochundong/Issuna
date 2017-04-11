using System;
using System.Linq;
using Xunit;

namespace Issuna.Core.Tests
{
    public class MongoIdTests
    {
        [Fact]
        public void TestByteArrayConstructor()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var mongoId = new MongoId(bytes);
            Assert.Equal(0x01020304, mongoId.Timestamp);
            Assert.Equal(0x050607, mongoId.Machine);
            Assert.Equal(0x0809, mongoId.Pid);
            Assert.Equal(0x0a0b0c, mongoId.Increment);
            Assert.Equal(0x050607, mongoId.Machine);
            Assert.Equal(0x0809, mongoId.Pid);
            Assert.Equal(0x0a0b0c, mongoId.Increment);
            Assert.Equal(MongoId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304), mongoId.CreationTime);
            Assert.Equal("0102030405060708090a0b0c", mongoId.ToString());
            Assert.True(bytes.SequenceEqual(mongoId.ToByteArray()));
        }

        [Fact]
        public void TestIntIntShortIntConstructor()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var mongoId = new MongoId(0x01020304, 0x050607, 0x0809, 0x0a0b0c);
            Assert.Equal(0x01020304, mongoId.Timestamp);
            Assert.Equal(0x050607, mongoId.Machine);
            Assert.Equal(0x0809, mongoId.Pid);
            Assert.Equal(0x0a0b0c, mongoId.Increment);
            Assert.Equal(0x050607, mongoId.Machine);
            Assert.Equal(0x0809, mongoId.Pid);
            Assert.Equal(0x0a0b0c, mongoId.Increment);
            Assert.Equal(MongoId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304), mongoId.CreationTime);
            Assert.Equal("0102030405060708090a0b0c", mongoId.ToString());
            Assert.True(bytes.SequenceEqual(mongoId.ToByteArray()));
        }

        [Fact]
        public void TestIntIntShortIntConstructorWithInvalidIncrement()
        {
            var mongoId = new MongoId(0, 0, 0, 0x00ffffff);
            Assert.Equal(0x00ffffff, mongoId.Increment);
            Assert.Throws<ArgumentOutOfRangeException>(() => new MongoId(0, 0, 0, 0x01000000));
        }

        [Fact]
        public void TestIntIntShortIntConstructorWithInvalidMachine()
        {
            var mongoId = new MongoId(0, 0x00ffffff, 0, 0);
            Assert.Equal(0x00ffffff, mongoId.Machine);
            Assert.Throws<ArgumentOutOfRangeException>(() => new MongoId(0, 0x01000000, 0, 0));
        }

        [Fact]
        public void TestPackWithInvalidIncrement()
        {
            var mongoId = new MongoId(MongoId.Pack(0, 0, 0, 0x00ffffff));
            Assert.Equal(0x00ffffff, mongoId.Increment);
            Assert.Throws<ArgumentOutOfRangeException>(() => new MongoId(MongoId.Pack(0, 0, 0, 0x01000000)));
        }

        [Fact]
        public void TestPackWithInvalidMachine()
        {
            var mongoId = new MongoId(MongoId.Pack(0, 0x00ffffff, 0, 0));
            Assert.Equal(0x00ffffff, mongoId.Machine);
            Assert.Throws<ArgumentOutOfRangeException>(() => new MongoId(MongoId.Pack(0, 0x01000000, 0, 0)));
        }

        [Fact]
        public void TestDateTimeConstructor()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var timestamp = MongoId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304);
            var mongoId = new MongoId(timestamp, 0x050607, 0x0809, 0x0a0b0c);
            Assert.Equal(0x01020304, mongoId.Timestamp);
            Assert.Equal(0x050607, mongoId.Machine);
            Assert.Equal(0x0809, mongoId.Pid);
            Assert.Equal(0x0a0b0c, mongoId.Increment);
            Assert.Equal(0x050607, mongoId.Machine);
            Assert.Equal(0x0809, mongoId.Pid);
            Assert.Equal(0x0a0b0c, mongoId.Increment);
            Assert.Equal(MongoId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304), mongoId.CreationTime);
            Assert.Equal("0102030405060708090a0b0c", mongoId.ToString());
            Assert.True(bytes.SequenceEqual(mongoId.ToByteArray()));
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void TestDateTimeConstructorAtEdgeOfRange(int secondsSinceEpoch)
        {
            var timestamp = MongoId.ObjectIdTimer.UnixEpoch.AddSeconds(secondsSinceEpoch);
            var mongoId = new MongoId(timestamp, 0, 0, 0);
            Assert.Equal(timestamp, mongoId.CreationTime);
        }

        [Theory]
        [InlineData((long)int.MinValue - 1)]
        [InlineData((long)int.MaxValue + 1)]
        public void TestDateTimeConstructorArgumentOutOfRangeException(long secondsSinceEpoch)
        {
            var timestamp = MongoId.ObjectIdTimer.UnixEpoch.AddSeconds(secondsSinceEpoch);
            Assert.Throws<ArgumentOutOfRangeException>(() => new MongoId(timestamp, 0, 0, 0));
        }

        [Fact]
        public void TestStringConstructor()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var mongoId = new MongoId("0102030405060708090a0b0c");
            Assert.Equal(0x01020304, mongoId.Timestamp);
            Assert.Equal(0x050607, mongoId.Machine);
            Assert.Equal(0x0809, mongoId.Pid);
            Assert.Equal(0x0a0b0c, mongoId.Increment);
            Assert.Equal(0x050607, mongoId.Machine);
            Assert.Equal(0x0809, mongoId.Pid);
            Assert.Equal(0x0a0b0c, mongoId.Increment);
            Assert.Equal(MongoId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304), mongoId.CreationTime);
            Assert.Equal("0102030405060708090a0b0c", mongoId.ToString());
            Assert.True(bytes.SequenceEqual(mongoId.ToByteArray()));
        }

        [Fact]
        public void TestGenerateNewId()
        {
            // compare against two timestamps in case seconds since epoch changes in middle of test
            var timestamp1 = (int)Math.Floor((DateTime.UtcNow - MongoId.ObjectIdTimer.UnixEpoch).TotalSeconds);
            var mongoId = MongoId.GenerateNewId();
            var timestamp2 = (int)Math.Floor((DateTime.UtcNow - MongoId.ObjectIdTimer.UnixEpoch).TotalSeconds);
            Assert.True(mongoId.Timestamp == timestamp1 || mongoId.Timestamp == timestamp2);
            Assert.True(mongoId.Machine != 0);
            Assert.True(mongoId.Pid != 0);
        }

        [Fact]
        public void TestGenerateNewIdWithDateTime()
        {
            var timestamp = new DateTime(2011, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var mongoId = MongoId.GenerateNewId(timestamp);
            Assert.True(mongoId.CreationTime == timestamp);
            Assert.True(mongoId.Machine != 0);
            Assert.True(mongoId.Pid != 0);
        }

        [Fact]
        public void TestGenerateNewIdWithTimestamp()
        {
            var timestamp = 0x01020304;
            var mongoId = MongoId.GenerateNewId(timestamp);
            Assert.True(mongoId.Timestamp == timestamp);
            Assert.True(mongoId.Machine != 0);
            Assert.True(mongoId.Pid != 0);
        }

        [Fact]
        public void TestIComparable()
        {
            var mongoId1 = MongoId.GenerateNewId();
            var mongoId2 = MongoId.GenerateNewId();
            Assert.Equal(0, mongoId1.CompareTo(mongoId1));
            Assert.Equal(-1, mongoId1.CompareTo(mongoId2));
            Assert.Equal(1, mongoId2.CompareTo(mongoId1));
            Assert.Equal(0, mongoId2.CompareTo(mongoId2));
        }

        [Fact]
        public void TestCompareEqualGeneratedIds()
        {
            var mongoId1 = MongoId.GenerateNewId();
            var mongoId2 = mongoId1;
            Assert.False(mongoId1 < mongoId2);
            Assert.True(mongoId1 <= mongoId2);
            Assert.False(mongoId1 != mongoId2);
            Assert.True(mongoId1 == mongoId2);
            Assert.False(mongoId1 > mongoId2);
            Assert.True(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareSmallerTimestamp()
        {
            var mongoId1 = new MongoId("0102030405060708090a0b0c");
            var mongoId2 = new MongoId("0102030505060708090a0b0c");
            Assert.True(mongoId1 < mongoId2);
            Assert.True(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.False(mongoId1 > mongoId2);
            Assert.False(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareSmallerMachine()
        {
            var mongoId1 = new MongoId("0102030405060708090a0b0c");
            var mongoId2 = new MongoId("0102030405060808090a0b0c");
            Assert.True(mongoId1 < mongoId2);
            Assert.True(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.False(mongoId1 > mongoId2);
            Assert.False(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareSmallerPid()
        {
            var mongoId1 = new MongoId("0102030405060708090a0b0c");
            var mongoId2 = new MongoId("01020304050607080a0a0b0c");
            Assert.True(mongoId1 < mongoId2);
            Assert.True(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.False(mongoId1 > mongoId2);
            Assert.False(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareSmallerIncrement()
        {
            var mongoId1 = new MongoId("0102030405060708090a0b0c");
            var mongoId2 = new MongoId("0102030405060708090a0b0d");
            Assert.True(mongoId1 < mongoId2);
            Assert.True(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.False(mongoId1 > mongoId2);
            Assert.False(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareSmallerGeneratedId()
        {
            var mongoId1 = MongoId.GenerateNewId();
            var mongoId2 = MongoId.GenerateNewId();
            Assert.True(mongoId1 < mongoId2);
            Assert.True(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.False(mongoId1 > mongoId2);
            Assert.False(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareLargerTimestamp()
        {
            var mongoId1 = new MongoId("0102030405060708090a0b0c");
            var mongoId2 = new MongoId("0102030305060708090a0b0c");
            Assert.False(mongoId1 < mongoId2);
            Assert.False(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.True(mongoId1 > mongoId2);
            Assert.True(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareLargerMachine()
        {
            var mongoId1 = new MongoId("0102030405060808090a0b0c");
            var mongoId2 = new MongoId("0102030405060708090a0b0c");
            Assert.False(mongoId1 < mongoId2);
            Assert.False(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.True(mongoId1 > mongoId2);
            Assert.True(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareLargerPid()
        {
            var mongoId1 = new MongoId("01020304050607080a0a0b0c");
            var mongoId2 = new MongoId("0102030405060708090a0b0c");
            Assert.False(mongoId1 < mongoId2);
            Assert.False(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.True(mongoId1 > mongoId2);
            Assert.True(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareLargerIncrement()
        {
            var mongoId1 = new MongoId("0102030405060708090a0b0d");
            var mongoId2 = new MongoId("0102030405060708090a0b0c");
            Assert.False(mongoId1 < mongoId2);
            Assert.False(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.True(mongoId1 > mongoId2);
            Assert.True(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestCompareLargerGeneratedId()
        {
            var mongoId2 = MongoId.GenerateNewId(); // generate before mongoId2
            var mongoId1 = MongoId.GenerateNewId();
            Assert.False(mongoId1 < mongoId2);
            Assert.False(mongoId1 <= mongoId2);
            Assert.True(mongoId1 != mongoId2);
            Assert.False(mongoId1 == mongoId2);
            Assert.True(mongoId1 > mongoId2);
            Assert.True(mongoId1 >= mongoId2);
        }

        [Fact]
        public void TestParse()
        {
            var mongoId1 = MongoId.Parse("0102030405060708090a0b0c"); // lower case
            var mongoId2 = MongoId.Parse("0102030405060708090A0B0C"); // upper case
            Assert.True(mongoId1.ToByteArray().SequenceEqual(mongoId2.ToByteArray()));
            Assert.True(mongoId1.ToString() == "0102030405060708090a0b0c"); // ToString returns lower case
            Assert.True(mongoId1.ToString() == mongoId2.ToString());
            Assert.Throws<FormatException>(() => MongoId.Parse("102030405060708090a0b0c")); // too short
            Assert.Throws<FormatException>(() => MongoId.Parse("x102030405060708090a0b0c")); // invalid character
            Assert.Throws<FormatException>(() => MongoId.Parse("00102030405060708090a0b0c")); // too long
        }

        [Fact]
        public void TestTryParse()
        {
            MongoId mongoId1, mongoId2;
            Assert.True(MongoId.TryParse("0102030405060708090a0b0c", out mongoId1)); // lower case
            Assert.True(MongoId.TryParse("0102030405060708090A0B0C", out mongoId2)); // upper case
            Assert.True(mongoId1.ToByteArray().SequenceEqual(mongoId2.ToByteArray()));
            Assert.True(mongoId1.ToString() == "0102030405060708090a0b0c"); // ToString returns lower case
            Assert.True(mongoId1.ToString() == mongoId2.ToString());
            Assert.False(MongoId.TryParse("102030405060708090a0b0c", out mongoId1)); // too short
            Assert.False(MongoId.TryParse("x102030405060708090a0b0c", out mongoId1)); // invalid character
            Assert.False(MongoId.TryParse("00102030405060708090a0b0c", out mongoId1)); // too long
            Assert.False(MongoId.TryParse(null, out mongoId1)); // should return false not throw ArgumentNullException
        }

        [Fact]
        public void TestConvertObjectIdToObjectId()
        {
            var oid = MongoId.GenerateNewId();

            var oidConverted = Convert.ChangeType(oid, typeof(MongoId));

            Assert.Equal(oid, oidConverted);
        }
    }
}
