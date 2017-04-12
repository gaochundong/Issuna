using Xunit;

namespace Issuna.Core.Tests
{
    public class CatkinIdTests
    {
        [Fact]
        public void TestLongConstructor()
        {
            var catkinId = new CatkinId(36671638107855309);
            Assert.Equal(0, catkinId.Reserved);
            Assert.Equal(8743199851, catkinId.Timestamp);
            Assert.Equal(0, catkinId.Region);
            Assert.Equal(0, catkinId.Machine);
            Assert.Equal(6605, catkinId.Sequence);
            Assert.Equal(CatkinId.CatkinIdTimer.CatkinIdEpoch.AddMilliseconds(8743199851), catkinId.CreationTime);
            Assert.Equal("36671638107855309", catkinId.ToString());
        }

        [Fact]
        public void TestFullSizeDefaultArgumentsConstructor()
        {
            var catkinId = new CatkinId(0, 219902320000, 5, 31, 1023);
            Assert.Equal(0, catkinId.Reserved);
            Assert.Equal(219902320000, catkinId.Timestamp);
            Assert.Equal(5, catkinId.Region);
            Assert.Equal(31, catkinId.Machine);
            Assert.Equal(1023, catkinId.Sequence);
            Assert.Equal(CatkinId.CatkinIdTimer.CatkinIdEpoch.AddMilliseconds(219902320000), catkinId.CreationTime);
            Assert.Equal("922337180386845695", catkinId.ToString());
        }

        [Fact]
        public void TestFullSizeCustomizedArgumentsConstructor()
        {
            var catkinId = new CatkinId(1, 8679772108, 5, 31, 1023);
            Assert.Equal(1, catkinId.Reserved);
            Assert.Equal(8679772108, catkinId.Timestamp);
            Assert.Equal(5, catkinId.Region);
            Assert.Equal(31, catkinId.Machine);
            Assert.Equal(1023, catkinId.Sequence);
            Assert.Equal(CatkinId.CatkinIdTimer.CatkinIdEpoch.AddMilliseconds(8679772108), catkinId.CreationTime);
            Assert.Equal("-9186966433981537281", catkinId.ToString());
        }

        [Fact]
        public void TestParse()
        {
            var catkinId1 = CatkinId.Parse("36671638107855309");
            Assert.True(catkinId1.ToLong().Equals(36671638107855309));
            Assert.True(catkinId1.ToString() == "36671638107855309");

            var catkinId2 = CatkinId.Parse("-9186966433981537281");
            Assert.True(catkinId2.ToLong().Equals(-9186966433981537281));
            Assert.True(catkinId2.ToString() == "-9186966433981537281");
        }
    }
}
