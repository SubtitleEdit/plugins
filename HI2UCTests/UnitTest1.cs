using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HI2UCTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void MyTestMethod()
        {
            const string expected = "(<font color=\"#808080\">NATIVE HAWAIIAN CHANTING</font>)";
            var hiStyler = new Nikse.SubtitleEdit.PluginLogic.HearingImpaired();
            string output = hiStyler.ChangeMoodsToUppercase("(<font color=\"#808080\">native hawaiian chanting</font>)");
            Assert.AreEqual(expected, output);
        }
    }
}
