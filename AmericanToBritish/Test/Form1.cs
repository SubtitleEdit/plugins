using Nikse.SubtitleEdit.PluginLogic;
using System.Windows.Forms;

namespace AmericanToBritishTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            IPlugin plugin = new AmericanToBritish();
            const string subtitle = @"1
00:00:24,275 --> 00:00:27,362
What color is that=

2
00:02:30,944 --> 00:02:33,279
Don't apologize.

3
00:02:44,833 --> 00:02:47,001
I favor the flavor!

4
00:02:47,126 --> 00:02:49,170
apologize
apologize";
            plugin.DoAction(null, subtitle, 25, "<br />", null, @"C:\Users\Nikse\Desktop\Trailers\Game Of Thrones Season Trailer.mp4", subtitle);
            Application.Exit();
        }
    }
}