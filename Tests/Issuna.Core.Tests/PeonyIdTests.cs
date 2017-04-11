using Xunit;

namespace Issuna.Core.Tests
{
    public class PeonyIdTests
    {
        [Fact]
        public void TestLongConstructor()
        {
            var peonyId = new PeonyId(72811205743345696);
            Assert.Equal(0, peonyId.Reserved);
            Assert.Equal(8679772108, peonyId.Timestamp);
            Assert.Equal(0, peonyId.Region);
            Assert.Equal(0, peonyId.Machine);
            Assert.Equal(32, peonyId.Sequence);
            Assert.Equal(PeonyId.PeonyIdTimer.PeonyIdEpoch.AddMilliseconds(8679772108), peonyId.CreationTime);
            Assert.Equal("72811205743345696", peonyId.ToString());
        }

        [Fact]
        public void TestFullSizeDefaultArgumentsConstructor()
        {
            var peonyId = new PeonyId(0, 8679772108, 0, 0, 32);
            Assert.Equal(0, peonyId.Reserved);
            Assert.Equal(8679772108, peonyId.Timestamp);
            Assert.Equal(0, peonyId.Region);
            Assert.Equal(0, peonyId.Machine);
            Assert.Equal(32, peonyId.Sequence);
            Assert.Equal(PeonyId.PeonyIdTimer.PeonyIdEpoch.AddMilliseconds(8679772108), peonyId.CreationTime);
            Assert.Equal("72811205743345696", peonyId.ToString());
        }

        [Fact]
        public void TestFullSizeCustomizedArgumentsConstructor()
        {
            var peonyId = new PeonyId(1, 8679772108, 5, 256, 1023);
            Assert.Equal(1, peonyId.Reserved);
            Assert.Equal(5, peonyId.Region);
            Assert.Equal(256, peonyId.Machine);
            Assert.Equal(8679772108, peonyId.Timestamp);
            Assert.Equal(1023, peonyId.Sequence);
            Assert.Equal(PeonyId.PeonyIdTimer.PeonyIdEpoch.AddMilliseconds(8679772108), peonyId.CreationTime);
            Assert.Equal("-9150560831105924097", peonyId.ToString());
        }

        [Fact]
        public void TestParse()
        {
            var peonyId1 = PeonyId.Parse("72811205743345696");
            Assert.True(peonyId1.ToLong().Equals(72811205743345696));
            Assert.True(peonyId1.ToString() == "72811205743345696");

            var peonyId2 = PeonyId.Parse("-9150560831105924097");
            Assert.True(peonyId2.ToLong().Equals(-9150560831105924097));
            Assert.True(peonyId2.ToString() == "-9150560831105924097");
        }
    }
}
