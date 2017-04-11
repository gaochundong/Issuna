using Xunit;

namespace Issuna.Core.Tests
{
    public class JasmineIdTests
    {
        [Fact]
        public void TestLongConstructor()
        {
            var jasmineId = new JasmineId(4611695116789851300);
            Assert.Equal(0, jasmineId.Reserved);
            Assert.Equal(4, jasmineId.Region);
            Assert.Equal(0, jasmineId.Machine);
            Assert.Equal(8676874, jasmineId.Timestamp);
            Assert.Equal(631972, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddSeconds(8676874), jasmineId.CreationTime);
            Assert.Equal("4611695116789851300", jasmineId.ToString());
        }

        [Fact]
        public void TestFullSizeDefaultArgumentsConstructor()
        {
            var jasmineId = new JasmineId(0, 4, 0, 8676874, 631972);
            Assert.Equal(0, jasmineId.Reserved);
            Assert.Equal(4, jasmineId.Region);
            Assert.Equal(0, jasmineId.Machine);
            Assert.Equal(8676874, jasmineId.Timestamp);
            Assert.Equal(631972, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddSeconds(8676874), jasmineId.CreationTime);
            Assert.Equal("4611695116789851300", jasmineId.ToString());
        }

        [Fact]
        public void TestFullSizeCustomizedArgumentsConstructor()
        {
            var jasmineId = new JasmineId(1, 5, 256, 8676875, 23456);
            Assert.Equal(1, jasmineId.Reserved);
            Assert.Equal(5, jasmineId.Region);
            Assert.Equal(256, jasmineId.Machine);
            Assert.Equal(8676875, jasmineId.Timestamp);
            Assert.Equal(23456, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddSeconds(8676875), jasmineId.CreationTime);
            Assert.Equal("-3170525039305925728", jasmineId.ToString());
        }

        [Fact]
        public void TestParse()
        {
            var jasmineId1 = JasmineId.Parse("4611695116789851300");
            Assert.True(jasmineId1.ToLong().Equals(4611695116789851300));
            Assert.True(jasmineId1.ToString() == "4611695116789851300");

            var jasmineId2 = JasmineId.Parse("-3170525039305925728");
            Assert.True(jasmineId2.ToLong().Equals(-3170525039305925728));
            Assert.True(jasmineId2.ToString() == "-3170525039305925728");
        }
    }
}
