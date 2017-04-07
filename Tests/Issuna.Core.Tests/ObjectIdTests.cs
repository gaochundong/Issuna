using System;
using System.Linq;
using Xunit;

namespace Issuna.Core.Tests
{
    public class ObjectIdTests
    {
        [Fact]
        public void TestByteArrayConstructor()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var objectId = new ObjectId(bytes);
            Assert.Equal(0x01020304, objectId.Timestamp);
            Assert.Equal(0x050607, objectId.Machine);
            Assert.Equal(0x0809, objectId.Pid);
            Assert.Equal(0x0a0b0c, objectId.Increment);
            Assert.Equal(0x050607, objectId.Machine);
            Assert.Equal(0x0809, objectId.Pid);
            Assert.Equal(0x0a0b0c, objectId.Increment);
            Assert.Equal(ObjectId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304), objectId.CreationTime);
            Assert.Equal("0102030405060708090a0b0c", objectId.ToString());
            Assert.True(bytes.SequenceEqual(objectId.ToByteArray()));
        }

        [Fact]
        public void TestIntIntShortIntConstructor()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var objectId = new ObjectId(0x01020304, 0x050607, 0x0809, 0x0a0b0c);
            Assert.Equal(0x01020304, objectId.Timestamp);
            Assert.Equal(0x050607, objectId.Machine);
            Assert.Equal(0x0809, objectId.Pid);
            Assert.Equal(0x0a0b0c, objectId.Increment);
            Assert.Equal(0x050607, objectId.Machine);
            Assert.Equal(0x0809, objectId.Pid);
            Assert.Equal(0x0a0b0c, objectId.Increment);
            Assert.Equal(ObjectId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304), objectId.CreationTime);
            Assert.Equal("0102030405060708090a0b0c", objectId.ToString());
            Assert.True(bytes.SequenceEqual(objectId.ToByteArray()));
        }

        [Fact]
        public void TestIntIntShortIntConstructorWithInvalidIncrement()
        {
            var objectId = new ObjectId(0, 0, 0, 0x00ffffff);
            Assert.Equal(0x00ffffff, objectId.Increment);
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectId(0, 0, 0, 0x01000000));
        }

        [Fact]
        public void TestIntIntShortIntConstructorWithInvalidMachine()
        {
            var objectId = new ObjectId(0, 0x00ffffff, 0, 0);
            Assert.Equal(0x00ffffff, objectId.Machine);
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectId(0, 0x01000000, 0, 0));
        }

        [Fact]
        public void TestPackWithInvalidIncrement()
        {
            var objectId = new ObjectId(ObjectId.Pack(0, 0, 0, 0x00ffffff));
            Assert.Equal(0x00ffffff, objectId.Increment);
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectId(ObjectId.Pack(0, 0, 0, 0x01000000)));
        }

        [Fact]
        public void TestPackWithInvalidMachine()
        {
            var objectId = new ObjectId(ObjectId.Pack(0, 0x00ffffff, 0, 0));
            Assert.Equal(0x00ffffff, objectId.Machine);
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectId(ObjectId.Pack(0, 0x01000000, 0, 0)));
        }

        [Fact]
        public void TestDateTimeConstructor()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var timestamp = ObjectId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304);
            var objectId = new ObjectId(timestamp, 0x050607, 0x0809, 0x0a0b0c);
            Assert.Equal(0x01020304, objectId.Timestamp);
            Assert.Equal(0x050607, objectId.Machine);
            Assert.Equal(0x0809, objectId.Pid);
            Assert.Equal(0x0a0b0c, objectId.Increment);
            Assert.Equal(0x050607, objectId.Machine);
            Assert.Equal(0x0809, objectId.Pid);
            Assert.Equal(0x0a0b0c, objectId.Increment);
            Assert.Equal(ObjectId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304), objectId.CreationTime);
            Assert.Equal("0102030405060708090a0b0c", objectId.ToString());
            Assert.True(bytes.SequenceEqual(objectId.ToByteArray()));
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void TestDateTimeConstructorAtEdgeOfRange(int secondsSinceEpoch)
        {
            var timestamp = ObjectId.ObjectIdTimer.UnixEpoch.AddSeconds(secondsSinceEpoch);
            var objectId = new ObjectId(timestamp, 0, 0, 0);
            Assert.Equal(timestamp, objectId.CreationTime);
        }

        [Theory]
        [InlineData((long)int.MinValue - 1)]
        [InlineData((long)int.MaxValue + 1)]
        public void TestDateTimeConstructorArgumentOutOfRangeException(long secondsSinceEpoch)
        {
            var timestamp = ObjectId.ObjectIdTimer.UnixEpoch.AddSeconds(secondsSinceEpoch);
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectId(timestamp, 0, 0, 0));
        }

        [Fact]
        public void TestStringConstructor()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var objectId = new ObjectId("0102030405060708090a0b0c");
            Assert.Equal(0x01020304, objectId.Timestamp);
            Assert.Equal(0x050607, objectId.Machine);
            Assert.Equal(0x0809, objectId.Pid);
            Assert.Equal(0x0a0b0c, objectId.Increment);
            Assert.Equal(0x050607, objectId.Machine);
            Assert.Equal(0x0809, objectId.Pid);
            Assert.Equal(0x0a0b0c, objectId.Increment);
            Assert.Equal(ObjectId.ObjectIdTimer.UnixEpoch.AddSeconds(0x01020304), objectId.CreationTime);
            Assert.Equal("0102030405060708090a0b0c", objectId.ToString());
            Assert.True(bytes.SequenceEqual(objectId.ToByteArray()));
        }

        [Fact]
        public void TestGenerateNewId()
        {
            // compare against two timestamps in case seconds since epoch changes in middle of test
            var timestamp1 = (int)Math.Floor((DateTime.UtcNow - ObjectId.ObjectIdTimer.UnixEpoch).TotalSeconds);
            var objectId = ObjectId.GenerateNewId();
            var timestamp2 = (int)Math.Floor((DateTime.UtcNow - ObjectId.ObjectIdTimer.UnixEpoch).TotalSeconds);
            Assert.True(objectId.Timestamp == timestamp1 || objectId.Timestamp == timestamp2);
            Assert.True(objectId.Machine != 0);
            Assert.True(objectId.Pid != 0);
        }

        [Fact]
        public void TestGenerateNewIdWithDateTime()
        {
            var timestamp = new DateTime(2011, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var objectId = ObjectId.GenerateNewId(timestamp);
            Assert.True(objectId.CreationTime == timestamp);
            Assert.True(objectId.Machine != 0);
            Assert.True(objectId.Pid != 0);
        }

        [Fact]
        public void TestGenerateNewIdWithTimestamp()
        {
            var timestamp = 0x01020304;
            var objectId = ObjectId.GenerateNewId(timestamp);
            Assert.True(objectId.Timestamp == timestamp);
            Assert.True(objectId.Machine != 0);
            Assert.True(objectId.Pid != 0);
        }

        [Fact]
        public void TestIComparable()
        {
            var objectId1 = ObjectId.GenerateNewId();
            var objectId2 = ObjectId.GenerateNewId();
            Assert.Equal(0, objectId1.CompareTo(objectId1));
            Assert.Equal(-1, objectId1.CompareTo(objectId2));
            Assert.Equal(1, objectId2.CompareTo(objectId1));
            Assert.Equal(0, objectId2.CompareTo(objectId2));
        }

        [Fact]
        public void TestCompareEqualGeneratedIds()
        {
            var objectId1 = ObjectId.GenerateNewId();
            var objectId2 = objectId1;
            Assert.False(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.False(objectId1 != objectId2);
            Assert.True(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerTimestamp()
        {
            var objectId1 = new ObjectId("0102030405060708090a0b0c");
            var objectId2 = new ObjectId("0102030505060708090a0b0c");
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerMachine()
        {
            var objectId1 = new ObjectId("0102030405060708090a0b0c");
            var objectId2 = new ObjectId("0102030405060808090a0b0c");
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerPid()
        {
            var objectId1 = new ObjectId("0102030405060708090a0b0c");
            var objectId2 = new ObjectId("01020304050607080a0a0b0c");
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerIncrement()
        {
            var objectId1 = new ObjectId("0102030405060708090a0b0c");
            var objectId2 = new ObjectId("0102030405060708090a0b0d");
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerGeneratedId()
        {
            var objectId1 = ObjectId.GenerateNewId();
            var objectId2 = ObjectId.GenerateNewId();
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerTimestamp()
        {
            var objectId1 = new ObjectId("0102030405060708090a0b0c");
            var objectId2 = new ObjectId("0102030305060708090a0b0c");
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerMachine()
        {
            var objectId1 = new ObjectId("0102030405060808090a0b0c");
            var objectId2 = new ObjectId("0102030405060708090a0b0c");
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerPid()
        {
            var objectId1 = new ObjectId("01020304050607080a0a0b0c");
            var objectId2 = new ObjectId("0102030405060708090a0b0c");
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerIncrement()
        {
            var objectId1 = new ObjectId("0102030405060708090a0b0d");
            var objectId2 = new ObjectId("0102030405060708090a0b0c");
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerGeneratedId()
        {
            var objectId2 = ObjectId.GenerateNewId(); // generate before objectId2
            var objectId1 = ObjectId.GenerateNewId();
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestParse()
        {
            var objectId1 = ObjectId.Parse("0102030405060708090a0b0c"); // lower case
            var objectId2 = ObjectId.Parse("0102030405060708090A0B0C"); // upper case
            Assert.True(objectId1.ToByteArray().SequenceEqual(objectId2.ToByteArray()));
            Assert.True(objectId1.ToString() == "0102030405060708090a0b0c"); // ToString returns lower case
            Assert.True(objectId1.ToString() == objectId2.ToString());
            Assert.Throws<FormatException>(() => ObjectId.Parse("102030405060708090a0b0c")); // too short
            Assert.Throws<FormatException>(() => ObjectId.Parse("x102030405060708090a0b0c")); // invalid character
            Assert.Throws<FormatException>(() => ObjectId.Parse("00102030405060708090a0b0c")); // too long
        }

        [Fact]
        public void TestTryParse()
        {
            ObjectId objectId1, objectId2;
            Assert.True(ObjectId.TryParse("0102030405060708090a0b0c", out objectId1)); // lower case
            Assert.True(ObjectId.TryParse("0102030405060708090A0B0C", out objectId2)); // upper case
            Assert.True(objectId1.ToByteArray().SequenceEqual(objectId2.ToByteArray()));
            Assert.True(objectId1.ToString() == "0102030405060708090a0b0c"); // ToString returns lower case
            Assert.True(objectId1.ToString() == objectId2.ToString());
            Assert.False(ObjectId.TryParse("102030405060708090a0b0c", out objectId1)); // too short
            Assert.False(ObjectId.TryParse("x102030405060708090a0b0c", out objectId1)); // invalid character
            Assert.False(ObjectId.TryParse("00102030405060708090a0b0c", out objectId1)); // too long
            Assert.False(ObjectId.TryParse(null, out objectId1)); // should return false not throw ArgumentNullException
        }

        [Fact]
        public void TestConvertObjectIdToObjectId()
        {
            var oid = ObjectId.GenerateNewId();

            var oidConverted = Convert.ChangeType(oid, typeof(ObjectId));

            Assert.Equal(oid, oidConverted);
        }
    }
}
