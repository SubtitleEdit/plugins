﻿// Author: Adalberto L. Simeone (Taranto, Italy)
// E-Mail: avengerdragon@gmail.com
// Website: http://www.avengersutd.com/blog
//
// This source code is Intellectual property of the Author
// and is released under the Creative Commons Attribution
// NonCommercial License, available at:
// http://creativecommons.org/licenses/by-nc/3.0/
// You can alter and use this source code as you wish,
// provided that you do not use the results in commercial
// projects, without the express and written consent of
// the Author.

using System;
using System.Drawing;

namespace AssaDraw.ColorPicker
{
    public static class ColorHandler
    {
        // Handle conversions between RGB and HSV
        // (and Color types, as well).

        public static Argb HsvToRgb(int a, int h, int s, int v)
        {
            // H, S, and V must all be between 0 and 255.
            return HsvToRgb(new Hsv(a, h, s, v));
        }

        public static Color HsvToColor(Hsv hsv)
        {
            var argb = HsvToRgb(hsv);
            return Color.FromArgb(argb.Alpha, argb.Red, argb.Green, argb.Blue);
        }

        public static Color HsvToColor(int a, int h, int s, int v)
        {
            return HsvToColor(new Hsv(a, h, s, v));
        }

        public static Argb HsvToRgb(Hsv hsv)
        {
            // HSV contains values scaled as in the color wheel:
            // that is, all from 0 to 255.

            // for ( this code to work, HSV.Hue needs
            // to be scaled from 0 to 360 (it//s the angle of the selected
            // point within the circle). HSV.Saturation and HSV.value must be
            // scaled to be between 0 and 1.

            double r = 0;
            double g = 0;
            double b = 0;

            // Scale Hue to be between 0 and 360. Saturation
            // and value scale to be between 0 and 1.
            var h = ((double)hsv.Hue / 255 * 360) % 360;
            var s = (double)hsv.Saturation / 255;
            var v = (double)hsv.Value / 255;

            if (Math.Abs(s) < 0.01)
            {
                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                r = v;
                g = v;
                b = v;
            }
            else
            {
                // The color wheel consists of 6 sectors.
                // Figure out which sector you//re in.
                var sectorPos = h / 60;
                var sectorNumber = (int)(Math.Floor(sectorPos));

                // get the fractional part of the sector.
                // That is, how many degrees into the sector
                // are you?
                var fractionalSector = sectorPos - sectorNumber;

                // Calculate values for the three axes
                // of the color.
                var p = v * (1 - s);
                var q = v * (1 - (s * fractionalSector));
                var t = v * (1 - (s * (1 - fractionalSector)));

                // Assign the fractional colors to r, g, and b
                // based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;

                    case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }
            }
            // return an RGB structure, with values scaled
            // to be between 0 and 255.
            return new Argb(hsv.Alpha, (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public static Hsv RgbToHsv(Argb argb)
        {
            // In this function, R, G, and B values must be scaled
            // to be between 0 and 1.
            // HSV.Hue will be a value between 0 and 360, and
            // HSV.Saturation and value are between 0 and 1.
            // The code must scale these to be between 0 and 255 for
            // the purposes of this application.

            var r = (double)argb.Red / 255;
            var g = (double)argb.Green / 255;
            var b = (double)argb.Blue / 255;

            var min = Math.Min(Math.Min(r, g), b);
            var max = Math.Max(Math.Max(r, g), b);

            double h;
            double s;
            var v = max;

            var delta = max - min;
            if (Math.Abs(max) < 0.01 || Math.Abs(delta) < 0.01)
            {
                // R, G, and B must be 0, or all the same.
                // In this case, S is 0, and H is undefined.
                // Using H = 0 is as good as any...
                s = 0;
                h = 0;
            }
            else
            {
                s = delta / max;
                if (Math.Abs(r - max) < 0.01)
                {
                    // Between Yellow and Magenta
                    h = (g - b) / delta;
                }
                else if (Math.Abs(g - max) < 0.01)
                {
                    // Between Cyan and Yellow
                    h = 2 + (b - r) / delta;
                }
                else
                {
                    // Between Magenta and Cyan
                    h = 4 + (r - g) / delta;
                }
            }
            // Scale h to be between 0 and 360.
            // This may require adding 360, if the value
            // is negative.
            h *= 60;
            if (h < 0)
            {
                h += 360;
            }

            // Scale to the requirements of this
            // application. All values are between 0 and 255.
            return new Hsv(argb.Alpha, (int)(h / 360 * 255), (int)(s * 255), (int)(v * 255));
        }

        public struct Argb
        {
            // All values are between 0 and 255.
            public Argb(int a, int r, int g, int b)
                : this()
            {
                Alpha = Math.Max(Math.Min(a, byte.MaxValue), 0);
                Red = Math.Max(Math.Min(r, byte.MaxValue), 0);
                Green = Math.Max(Math.Min(g, byte.MaxValue), 0);
                Blue = Math.Max(Math.Min(b, byte.MaxValue), 0);
            }

            public int Alpha { get; set; }
            public int Red { get; set; }
            public int Green { get; set; }
            public int Blue { get; set; }

            public override string ToString()
            {
                return $"({Alpha}, {Red}, {Green} {Blue})";
            }
        }

        public struct Hsv
        {
            // All values are between 0 and 255.
            public Hsv(int a, int h, int s, int v)
                : this()
            {
                Alpha = Math.Max(Math.Min(a, byte.MaxValue), 0);
                Hue = Math.Max(Math.Min(h, byte.MaxValue), 0);
                Saturation = Math.Max(Math.Min(s, byte.MaxValue), 0);
                Value = Math.Max(Math.Min(v, byte.MaxValue), 0);
            }

            public int Alpha { get; set; }
            public int Hue { get; set; }
            public int Saturation { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return $"({Hue}, {Saturation}, {Value})";
            }
        }
    }
}
