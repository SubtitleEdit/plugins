using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ColorToDialog.Logic;

namespace UnitTests
{
    [TestClass]
    public class ColorToDialogUnitTest
    {
        [TestMethod]
        public void TestDialog1()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ffff00\">That was really delicious.</font>" + Environment.NewLine +
                                   "I know.", "- ");
            Assert.AreEqual("- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestDialog2()
        {
            var result = DashAdder.GetFixedText("That's it!" + Environment.NewLine +
                                                "<font color=\"#ffff00\">..sped to victory.</font>", "- ");
            Assert.AreEqual("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }

        [TestMethod]
        public void TestDialog3()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ff0000\">That's it!</font>" + Environment.NewLine +
                                                "<font color=\"#ffff00\">..sped to victory.</font>", "- ");
            Assert.AreEqual("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }

        [TestMethod]
        public void TestDialogThreeLines1()
        {
            var result = DashAdder.GetFixedText("<font color=\"#00ffff\">Got a sinking feeling.</font>" + Environment.NewLine +
                                                "<font color=\"#00ff00\">\"Evacuate from</font>" + Environment.NewLine +
                                                "<font color=\"#00ff00\">an underwater chopper.\"</font> ", "- ");
            Assert.AreEqual("- Got a sinking feeling." + Environment.NewLine + "- \"Evacuate from" + Environment.NewLine + "an underwater chopper.\"", result);
        }

        [TestMethod]
        public void TestDialogThreeLines2()
        {
            var result = DashAdder.GetFixedText("<font color=\"#00ffff\">Got a sinking feeling</font>" + Environment.NewLine +
                                                "<font color=\"#00ffff\">about all of this.</font>" + Environment.NewLine +
                                                "<font color=\"#00ff00\">Don't worry.</font> ", "- ");
            Assert.AreEqual("- Got a sinking feeling" + Environment.NewLine + "about all of this." + Environment.NewLine + "- Don't worry.", result);
        }

        [TestMethod]
        public void TestNoChange1()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ffff00\">That's it!</font>" + Environment.NewLine +
                                                "..sped to victory.", "- ");
            Assert.AreEqual("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }

        [TestMethod]
        public void TestNoChange2()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ffff00\">- That was really delicious.</font>" + Environment.NewLine +
                                                "- I know.", "- ");
            Assert.AreEqual("- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestNoChange3()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ffff00\">That's it!</font>" + Environment.NewLine +
                                                "<font color=\"#ffff00\">..sped to victory.</font>", "- ");
            Assert.AreEqual("That's it!" + Environment.NewLine + "..sped to victory.", result);
        }
    }
}
