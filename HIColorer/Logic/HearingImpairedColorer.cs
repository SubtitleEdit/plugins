using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    public class HearingImpairedColorer
    {
        private readonly Color _moodColor;
        private readonly Color _narratorColor;

        public HearingImpairedColorer(Color moodColor, Color narratorCollor)
        {
            _moodColor = moodColor;
            _narratorColor = narratorCollor;
        }
    }
}
