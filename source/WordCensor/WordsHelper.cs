using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class WordsHelper
    {
        public static IEnumerable<string> GetWords()
        {
            var words = @"anal
anus
arse
ass
ballsack
balls
bastard
bitch
biatch
bloody
blowjob
blow job
bollock
bollok
boner
boob
bugger
bum
butt
buttplug
clitoris
cock
coon
crap
cunt
damn
dick
dildo
dyke
fag
feck
fellate
fellatio
felching
fuck
fucking
fucked
f u c k
fudgepacker
fudge packer
flange
Goddamn
God damn
hell
homo
jerk
jizz
knobend
knob end
labia
lmao
lmfao
muff
nigger
nigga
omg
penis
piss
poop
prick
pube
pussy
queer
scrotum
sex
shit
s hit
sh1t
slut
smegma
spunk
tit
tosser
turd
twat
vagina
wank
whore
wtf";

            // TODO: Handle words like: fucking* (ing)
            return words.SplitToLines();

            // #*@!#!!
            // fu**
            // d%ick
        }

        public static string ColorWordRed(string word)
        {
            Color c = Color.Red;
            string colorString = string.Format("#{0:x2}{1:x2}{2:x2}", c.R, c.G, c.B);
            return $"<font color=\"{colorString}\">{word}</font>";
        }

    }
}
