using gbacs;
using NUnit.Framework;

namespace gbacsTests
{
    /// <summary>
    /// Runs various checks to determine the validity
    /// of the CPU emulator.
    /// </summary>
    public class CPUTests
    {
        private ARM7TDMi CPU;

        [SetUp]
        public void Setup()
        {
            CPU = new ARM7TDMi();
        }

        [Test]
        public void Test()
        {
        }
    }
}