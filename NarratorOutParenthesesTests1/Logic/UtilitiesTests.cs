using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.PluginLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Nikse.SubtitleEdit.PluginLogic.Tests
{
    [TestClass()]
    public class UtilitiesTests
    {
        [TestMethod()]
        public void FixIfInListTest()
        {
            Assert.IsTrue(Utilities.FixIfInList("(john)"));
        }
    }
}
