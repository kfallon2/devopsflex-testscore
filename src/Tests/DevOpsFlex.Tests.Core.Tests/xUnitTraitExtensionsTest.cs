﻿namespace DevOpsFlex.Tests.Core.Tests
{
    using Xunit;

    public class XunitTraitExtensionsTest
    {
        [Fact, IsUnit]
        public void Test_IsUnit() { }

        [Fact, IsIntegration]
        public void Test_IsIntegration() { }

        [Fact, IsFakes]
        public void Test_IsFakes() { }

        [Fact, IsCodeContract]
        public void Test_IsCodeContract() { }

        [Fact, IsRoslyn]
        public void Test_IsRoslyn() { }

        [Fact, IsDev]
        public void Test_IsIsDev() { }

        [Fact, IsProfilerCpu]
        public void Test_IsProfilerCpu() { }

        [Fact, IsProfilerMemory]
        public void Test_IsProfilerMemory() { }
    }
}
