//******************************************************************************************************************************************************************************************//
// Copyright (c) 2011 George Mamaladze                                                                                                                                                      //
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


#if NETFX_CORE
using Windows.UI;
#elif SILVERLIGHT
using System.Windows.Media;
#else
using System.Drawing;

#endif


namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render
{
    /// <summary>
    /// Port from Lea Vero's Contrast ratio javascript library.
    /// Formula that's on W3C
    /// http://www.w3.org/TR/2008/REC-WCAG20-20081211/#relativeluminancedef
    /// http://www.w3.org/TR/2008/REC-WCAG20-20081211/#contrast-ratiodef
    /// Even though contrast ratio is used mainly for human eye. But decoder is very similar. Highest ratio for black and white is easiest to identify and decode.
    /// Where two same color is impossible, closest color is similar as well. 
    /// Decoder normally turn colored color to White Gray Black colors.
    /// Another important to note is proper decoder won't treat lighter color as 0, they will use pattern to match. Thus inverse color is possible.
    /// Pattern 1011101 << "w b w w w b w" is exactly same as "b w b b b w b"
    /// </summary>
    public static class ColorContrast
    {
#if NETFX_CORE || SILVERLIGHT
        private static double Luminance(Color color)
        {
#else
        private static double Luminance(GColor color)
        {
#endif

            double r, g, b;
            r = Colorspace(color.R);
            g = Colorspace(color.G);
            b = Colorspace(color.B);

            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        private static double Colorspace(byte rgb)
        {
            double result = Convert.ToDouble(rgb) / Convert.ToDouble(byte.MaxValue);
            result = result < 0.03928 ? result / 12.92 : Math.Pow(((result + 0.055) / 1.055), 2.4);
            return result;
        }

#if NETFX_CORE || SILVERLIGHT
        private static Color InverseColor(Color color)
        {
            Color result = Color.FromArgb(color.A, ConvertByte(byte.MaxValue - color.R), ConvertByte(byte.MaxValue - color.G), ConvertByte(byte.MaxValue - color.B));
            return result;
        }
#else
        private static GColor InverseColor(GColor color)
        {
            Color result = Color.FromArgb(color.A, byte.MaxValue - color.R, byte.MaxValue - color.G, byte.MaxValue - color.B);
            return new FormColor(result);
        }
#endif

#if NETFX_CORE || SILVERLIGHT
        private static Color OverlayOn(this Color color, Color backGround)
        {
#else
        private static GColor OverlayOn(this GColor color, GColor backGround)
        {
#endif
            byte r, g, b, a;
            double alpha, bAlpha;
            if (color.A == 255)
#if NETFX_CORE || SILVERLIGHT
                return Color.FromArgb(color.A, color.R, color.G, color.B);
#else
                return new FormColor(Color.FromArgb(color.A, color.R, color.G, color.B));
#endif
            alpha = Convert.ToDouble(color.A) / Convert.ToDouble(byte.MaxValue);
            bAlpha = Convert.ToDouble(backGround.A) / Convert.ToDouble(byte.MaxValue);
            r = ConvertByte(Convert.ToDouble(color.R) * alpha + Convert.ToDouble(backGround.R) * bAlpha * (1 - alpha));
            g = ConvertByte(Convert.ToDouble(color.G) * alpha + Convert.ToDouble(backGround.G) * bAlpha * (1 - alpha));
            b = ConvertByte(Convert.ToDouble(color.B) * alpha + Convert.ToDouble(backGround.B) * bAlpha * (1 - alpha));

            a = backGround.A == byte.MaxValue ? byte.MaxValue : ConvertByte((alpha + bAlpha * (1 - alpha)) * Convert.ToDouble(byte.MaxValue));
#if NETFX_CORE || SILVERLIGHT
            return Color.FromArgb(a, r, g, b);
#else
            return new FormColor(Color.FromArgb(a, r, g, b));
#endif
        }

        private static byte ConvertByte(double doubleVal)
        {
            byte result = 0;
            if (doubleVal >= 255)
                return 255;
            if (doubleVal <= 0)
                return 0;
            result = Convert.ToByte(doubleVal);
            return result;
        }


        /// <summary>
        /// Calculate background and fronntcolor's contrast ratio.
        /// To assist dark module and light module's choose. 
        /// Higher ratio means easier to decode by decoder.
        /// Black and White will have highest ratio = 21.
        /// </summary>
        /// <param name="backGround">light module color</param>
        /// <param name="frontColor">dark module color</param>
        /// <returns>Contrast object.</returns>
#if NETFX_CORE || SILVERLIGHT
        public static Contrast GetContrast(Color backGround, Color frontColor)
        {
            Color back = Color.FromArgb(backGround.A, backGround.R, backGround.G, backGround.B);
            Color front = Color.FromArgb(frontColor.A, frontColor.R, frontColor.G, frontColor.B);
#else
        public static Contrast GetContrast(GColor backGround, GColor frontColor)
        {
            GColor back = new FormColor(Color.FromArgb(backGround.A, backGround.R, backGround.G, backGround.B));
            GColor front = new FormColor(Color.FromArgb(frontColor.A, frontColor.R, frontColor.G, frontColor.B));
#endif

            if (back.A == 255)
            {
                if (front.A < 255)
                    front = OverlayOn(front, back);
                double l1 = Luminance(back) + 0.05;
                double l2 = Luminance(front) + 0.05;
                double ratio = l2 > l1 ? l2 / l1 : l1 / l2;

                ratio = Math.Round(ratio, 1);
                return new Contrast()
                    {
                        Ratio = ratio,
                        Error = 0,
                        Min = ratio,
                        Max = ratio,
                        Closet = null,
                        Farthest = null
                    };
            }

            double onBlack = GetContrast(OverlayOn(back, Black), front).Ratio;
            double onWhite = GetContrast(OverlayOn(back, White), front).Ratio;

            double max = Math.Max(onBlack, onWhite);
#if NETFX_CORE || SILVERLIGHT
            Color closest = Closest(backGround, frontColor);
#else
            GColor closest = Closest(backGround, frontColor);
#endif
            double min = GetContrast(OverlayOn(back, closest), front).Ratio;

            return new Contrast()
                {
                    Ratio = Math.Round((max + min) / 2, 2),
                    Error = Math.Round((max - min) / 2, 2),
                    Min = min,
                    Max = max,
                    Closet = closest,
                    Farthest = onWhite == max ? White : Black
                };
        }

#if NETFX_CORE || SILVERLIGHT
        private static Color Closest(Color backGround, Color frontColor)
        {
#else
        private static GColor Closest(GColor backGround, GColor frontColor)
        {
#endif
            double alpha = Convert.ToDouble(backGround.A) / Convert.ToDouble(byte.MaxValue);
#if NETFX_CORE || SILVERLIGHT
            return Color.FromArgb(byte.MaxValue,
                ConvertByte((Convert.ToDouble(frontColor.R) - Convert.ToDouble(backGround.R) * alpha) / (1 - alpha)),
                ConvertByte((Convert.ToDouble(frontColor.G) - Convert.ToDouble(backGround.G) * alpha) / (1 - alpha)),
                ConvertByte((Convert.ToDouble(frontColor.B) - Convert.ToDouble(backGround.B) * alpha) / (1 - alpha)));
#else
            return new FormColor(Color.FromArgb(byte.MaxValue,
                ConvertByte((Convert.ToDouble(frontColor.R) - Convert.ToDouble(backGround.R) * alpha) / (1 - alpha)),
                ConvertByte((Convert.ToDouble(frontColor.G) - Convert.ToDouble(backGround.G) * alpha) / (1 - alpha)),
                ConvertByte((Convert.ToDouble(frontColor.B) - Convert.ToDouble(backGround.B) * alpha) / (1 - alpha))));
#endif

        }


#if NETFX_CORE || SILVERLIGHT
        private static Color Black
        {
            get
            {
                return Colors.Black;
            }
        }

        private static Color White
        {
            get
            {
                return Colors.White;
            }
        }
#else
        private static GColor Black
        {
            get
            {
                return new FormColor(Color.Black);
            }
        }

        private static GColor White
        {
            get
            {
                return new FormColor(Color.White);
            }
        }
#endif




    }

    public class Contrast
    {
        public double Ratio { get; set; }
        public double Error { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
#if NETFX_CORE || SILVERLIGHT
        public Color? Closet { get; set; }
        public Color? Farthest { get; set; }
#else
        public GColor Closet { get; set; }
        public GColor Farthest { get; set; }
#endif

    }
}
