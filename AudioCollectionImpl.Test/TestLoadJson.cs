using System.Linq.Expressions;
using Xunit.Abstractions;
using TestBasis;

namespace AudioCollectionImpl.Test {
    public class TestLoadJson {

        private ITestOutputHelper output;
        public TestLoadJson(ITestOutputHelper outp) { output = outp; }


        [Fact]
        public async Task Test1() {
            var th = new TestHelper(output);
            var repos = new JsonMediaRepository2(th.CreateMockedLoggerFactory());

            await repos.LoadAllAsync("./data");
            Assert.Equal(3, repos.GetCategories().Count);
            var cat1 = repos.GetCategories().First();
            Assert.Equal(4, cat1.Entries.Count);
        }
    }
}