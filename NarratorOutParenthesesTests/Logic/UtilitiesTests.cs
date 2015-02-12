using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.PluginLogic;
using Xunit;
namespace Nikse.SubtitleEdit.PluginLogic.Tests
{
    public class UtilitiesTests
    {
        [Fact()]
        public void FixIfInListTest()
        {
            var newName = Utilities.FixIfInList("(john)");
            Assert.Equal<string>("john", newName);
        }
    }
}
