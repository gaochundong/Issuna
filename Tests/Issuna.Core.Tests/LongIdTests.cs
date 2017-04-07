using Xunit;

namespace Issuna.Core.Tests
{
    public class LongIdTests
    {
        [Fact]
        public void TestLongConstructor()
        {
            var longId = new LongId(8742531905370);
            Assert.Equal(0, longId.Reserved);
            Assert.Equal(0, longId.Region);
            Assert.Equal(0, longId.Machine);
            Assert.Equal(0, longId.Precision);
            Assert.Equal(8337528, longId.Timestamp);
            Assert.Equal(145242, longId.Sequence);
            Assert.Equal(LongId.LongIdTimer.LongIdEpoch.AddSeconds(8337528), longId.CreationTime);
            Assert.Equal("8742531905370", longId.ToString());
        }

        [Fact]
        public void TestFullSizeDefaultArgumentsConstructor()
        {
            var longId = new LongId(0, 0, 0, 0, 8337528, 145242);
            Assert.Equal(0, longId.Reserved);
            Assert.Equal(0, longId.Region);
            Assert.Equal(0, longId.Machine);
            Assert.Equal(0, longId.Precision);
            Assert.Equal(8337528, longId.Timestamp);
            Assert.Equal(145242, longId.Sequence);
            Assert.Equal(LongId.LongIdTimer.LongIdEpoch.AddSeconds(8337528), longId.CreationTime);
            Assert.Equal("8742531905370", longId.ToString());
        }

        [Fact]
        public void TestFullSizeCustomizedArgumentsConstructor()
        {
            var longId = new LongId(1, 2, 3, 1, 8337528000, 145);
            Assert.Equal(1, longId.Reserved);
            Assert.Equal(2, longId.Region);
            Assert.Equal(3, longId.Machine);
            Assert.Equal(1, longId.Precision);
            Assert.Equal(8337528000, longId.Timestamp);
            Assert.Equal(145, longId.Sequence);
            Assert.Equal(LongId.LongIdTimer.LongIdEpoch.AddMilliseconds(8337528000), longId.CreationTime);
            Assert.Equal("-4603796181450817391", longId.ToString());
        }

        [Fact]
        public void TestParse()
        {
            var longId1 = LongId.Parse("8742531905370");
            Assert.True(longId1.ToLong().Equals(8742531905370));
            Assert.True(longId1.ToString() == "8742531905370");

            var longId2 = LongId.Parse("-4603796181450817391");
            Assert.True(longId2.ToLong().Equals(-4603796181450817391));
            Assert.True(longId2.ToString() == "-4603796181450817391");
        }
    }
}
