using Xunit;

namespace Issuna.Core.Tests
{
    public class JasmineIdTests
    {
        [Fact]
        public void TestLongConstructor()
        {
            var jasmineId = new JasmineId(8742531905370);
            Assert.Equal(0, jasmineId.Reserved);
            Assert.Equal(0, jasmineId.Region);
            Assert.Equal(0, jasmineId.Machine);
            Assert.Equal(0, jasmineId.Precision);
            Assert.Equal(8337528, jasmineId.Timestamp);
            Assert.Equal(145242, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddSeconds(8337528), jasmineId.CreationTime);
            Assert.Equal("8742531905370", jasmineId.ToString());
        }

        [Fact]
        public void TestFullSizeDefaultArgumentsConstructor()
        {
            var jasmineId = new JasmineId(0, 0, 0, 0, 8337528, 145242);
            Assert.Equal(0, jasmineId.Reserved);
            Assert.Equal(0, jasmineId.Region);
            Assert.Equal(0, jasmineId.Machine);
            Assert.Equal(0, jasmineId.Precision);
            Assert.Equal(8337528, jasmineId.Timestamp);
            Assert.Equal(145242, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddSeconds(8337528), jasmineId.CreationTime);
            Assert.Equal("8742531905370", jasmineId.ToString());
        }

        [Fact]
        public void TestFullSizeCustomizedArgumentsConstructor()
        {
            var jasmineId = new JasmineId(1, 2, 3, 1, 8337528000, 145);
            Assert.Equal(1, jasmineId.Reserved);
            Assert.Equal(2, jasmineId.Region);
            Assert.Equal(3, jasmineId.Machine);
            Assert.Equal(1, jasmineId.Precision);
            Assert.Equal(8337528000, jasmineId.Timestamp);
            Assert.Equal(145, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddMilliseconds(8337528000), jasmineId.CreationTime);
            Assert.Equal("-4603796181450817391", jasmineId.ToString());
        }

        [Fact]
        public void TestParse()
        {
            var jasmineId1 = JasmineId.Parse("8742531905370");
            Assert.True(jasmineId1.ToLong().Equals(8742531905370));
            Assert.True(jasmineId1.ToString() == "8742531905370");

            var jasmineId2 = JasmineId.Parse("-4603796181450817391");
            Assert.True(jasmineId2.ToLong().Equals(-4603796181450817391));
            Assert.True(jasmineId2.ToString() == "-4603796181450817391");
        }
    }
}
