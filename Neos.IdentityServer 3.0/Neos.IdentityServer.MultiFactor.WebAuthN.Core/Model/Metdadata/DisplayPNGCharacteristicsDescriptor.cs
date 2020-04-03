//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// The DisplayPNGCharacteristicsDescriptor describes a PNG image characteristics as defined in the PNG [PNG] spec for IHDR (image header) and PLTE (palette table)
    /// </summary>
    /// <remarks>
    /// <see href="https://fidoalliance.org/specs/fido-v2.0-rd-20180702/fido-metadata-statement-v2.0-rd-20180702.html#displaypngcharacteristicsdescriptor-dictionary"/>
    /// </remarks>
    public class DisplayPNGCharacteristicsDescriptor
    {
        /// <summary>
        /// Gets or sets the image width.
        /// </summary>
        [JsonProperty("width", Required = Required.Always)]
        public ulong Width { get; set; }
        /// <summary>
        /// Gets or sets the image height.
        /// </summary>
        [JsonProperty("height", Required = Required.Always)]
        public ulong Height { get; set; }
        /// <summary>
        /// Gets or sets the bit depth - bits per sample or per palette index.
        /// </summary>
        [JsonProperty("bitDepth", Required = Required.Always)]
        public byte BitDepth { get; set; }
        /// <summary>
        /// Gets or sets the color type defines the PNG image type.
        /// </summary>
        [JsonProperty("colorType", Required = Required.Always)]
        public byte ColorType { get; set; }
        /// <summary>
        /// Gets or sets the compression method used to compress the image data.
        /// </summary>
        [JsonProperty("compression", Required = Required.Always)]
        public byte Compression { get; set; }
        /// <summary>
        /// Gets or sets the filter method is the preprocessing method applied to the image data before compression.
        /// </summary>
        [JsonProperty("filter", Required = Required.Always)]
        public byte Filter { get; set; }
        /// <summary>
        /// Gets or sets the interlace method is the transmission order of the image data.
        /// </summary>
        [JsonProperty("interlace", Required = Required.Always)]
        public byte Interlace { get; set; }
        /// <summary>
        /// Gets or sets the palette (1 to 256 palette entries).
        /// </summary>
        [JsonProperty("plte")]
        public RgbPaletteEntry[] Plte { get; set; }
    }
}
