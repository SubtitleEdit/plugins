using System;
using Nikse.SubtitleEdit.PluginLogic.Common;
using Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;
using Xunit;

namespace Ivandrofly.Test
{
    public class UppercaseConverterStrategyTest
    {
        [Fact]
        public void ExecuteTest()
        {
            var sut = new UppercaseConverterStrategy(new ChunkReader());
            var input = "<font color=\"#0000ff\">Foobar</font>: Mmm.";
            var sentence = sut.Execute(input);
            Assert.NotEqual("<font color=\"#0000ff\">FOOBAR</font>: Mmm.", sentence);
        }
        
        [Fact]
        public void MoodsNoParenthesesTest()
        {
            var sut = new UppercaseConverterStrategy(new ChunkReader());
            Assert.Equal("CREATURE GROWLING", sut.Execute("creature growling"));
        }
        
        [Fact]
        public void MoodsTest()
        {
            var sut = new UppercaseConverterStrategy(new ChunkReader());
            
            var expected = "- <font color=\"#800000\">(CREATURE)</font>" + Environment.NewLine +
                        "- <font color=\"#800000\">(OBJECTS CRASHING AND)</font>";

            var inputTwo = "- <font color=\"#800000\">(CREATURE GROWLING)</font>" + Environment.NewLine +
                            "- <font color=\"#800000\">(OBJECTS CRASHING AND CLATTERING)</font>";
            
            var input = "- <font color=\"#800000\">(CREATURE)</font>" + Environment.NewLine +
                        "- <font color=\"#800000\">(objects crashing and)</font>";
            
            Assert.Equal(expected, sut.Execute(input));
            Assert.Equal(inputTwo, sut.Execute(inputTwo));
        }
        
    }
}