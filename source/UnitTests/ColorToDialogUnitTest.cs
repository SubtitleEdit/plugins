using System;
using ColorToDialog.Logic;
using Xunit;

namespace UnitTests
{
    public class ColorToDialogUnitTest
    {
        [Fact]
        public void TestDialog1()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ffff00\">That was really delicious.</font>" + Environment.NewLine +
                                                "I know.", "- ");
            Assert.Equal("- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [Fact]
        public void TestDialog2()
        {
            var result = DashAdder.GetFixedText("That's it!" + Environment.NewLine +
                                                "<font color=\"#ffff00\">..sped to victory.</font>", "- ");
            Assert.Equal("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }

        [Fact]
        public void TestDialog3()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ff0000\">That's it!</font>" + Environment.NewLine +
                                                "<font color=\"#ffff00\">..sped to victory.</font>", "- ");
            Assert.Equal("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }

        [Fact]
        public void TestDialogThreeLines1()
        {
            var result = DashAdder.GetFixedText("<font color=\"#00ffff\">Got a sinking feeling.</font>" + Environment.NewLine +
                                                "<font color=\"#00ff00\">\"Evacuate from</font>" + Environment.NewLine +
                                                "<font color=\"#00ff00\">an underwater chopper.\"</font> ", "- ");
            Assert.Equal("- Got a sinking feeling." + Environment.NewLine + "- \"Evacuate from" + Environment.NewLine + "an underwater chopper.\"", result);
        }

        [Fact]
        public void TestDialogThreeLines2()
        {
            var result = DashAdder.GetFixedText("<font color=\"#00ffff\">Got a sinking feeling</font>" + Environment.NewLine +
                                                "<font color=\"#00ffff\">about all of this.</font>" + Environment.NewLine +
                                                "<font color=\"#00ff00\">Don't worry.</font> ", "- ");
            Assert.Equal("- Got a sinking feeling" + Environment.NewLine + "about all of this." + Environment.NewLine + "- Don't worry.", result);
        }

        [Fact]
        public void TestNoChange1()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ffff00\">That's it!</font>" + Environment.NewLine +
                                                "..sped to victory.", "- ");
            Assert.Equal("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }

        [Fact]
        public void TestNoChange2()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ffff00\">- That was really delicious.</font>" + Environment.NewLine +
                                                "- I know.", "- ");
            Assert.Equal("- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [Fact]
        public void TestNoChange3()
        {
            var result = DashAdder.GetFixedText("<font color=\"#ffff00\">That's it!</font>" + Environment.NewLine +
                                                "<font color=\"#ffff00\">..sped to victory.</font>", "- ");
            Assert.Equal("That's it!" + Environment.NewLine + "..sped to victory.", result);
        }
    }
}