//******************************************************************************************************************************************************************************************//
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
// Chaos.NaCL                                                                                                                                                                               // 
//                                                                                                                                                                                          //
// Chaos.NaCl is a cryptography library written in C#. It is based on djb's NaCl.                                                                                                           //
//                                                                                                                                                                                          //
// Public domain                                                                                                                                                                            //
//                                                                                                                                                                                          //
// License: Mixed MIT & Public Domain - https://github.com/CodesInChaos/Chaos.NaCl/blob/master/License.txt                                                                                  //
//                                                                                                                                                                                          //
// C# port + code by Christian Winnerlein (CodesInChaos)                                                                                                                                    //
//                                                                                                                                                                                          //
// Poly1305 in c                                                                                                                                                                            // 
//         written by Andrew M. (floodyberry)                                                                                                                                               //
//         original license: MIT or PUBLIC DOMAIN                                                                                                                                           //
//         https://github.com/floodyberry/poly1305-donna/blob/master/poly1305-donna-unrolled.c                                                                                              //
//                                                                                                                                                                                          //
// Curve25519 and Ed25519 in c                                                                                                                                                              //
//         written by Dan Bernstein(djb)                                                                                                                                                    //
//         public domain                                                                                                                                                                    //
//         from Ref10 in SUPERCOP http://bench.cr.yp.to/supercop.html                                                                                                                       //
//                                                                                                                                                                                          //
// (H) Salsa20 in c                                                                                                                                                                         //
//         written by Dan Bernstein(djb)                                                                                                                                                    //
//         public domain                                                                                                                                                                    //
//         from SUPERCOP http://bench.cr.yp.to/supercop.html                                                                                                                                //
//                                                                                                                                                                                          //
// SHA512                                                                                                                                                                                   //
//         written by Christian Winnerlein(CodesInChaos)                                                                                                                                    //
//         public domain                                                                                                                                                                    //
//         directly from the specification                                                                                                                                                  //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Nacl
{
    // Array8<UInt32> Poly1305 key
    // Array8<UInt64> SHA-512 state/output
    internal struct Array8<T>
    {
        public T x0;
        public T x1;
        public T x2;
        public T x3;
        public T x4;
        public T x5;
        public T x6;
        public T x7;
    }

    // Array16<UInt32> Salsa20 state
    // Array16<UInt64> SHA-512 block
    internal struct Array16<T>
    {
        public T x0;
        public T x1;
        public T x2;
        public T x3;
        public T x4;
        public T x5;
        public T x6;
        public T x7;
        public T x8;
        public T x9;
        public T x10;
        public T x11;
        public T x12;
        public T x13;
        public T x14;
        public T x15;
    }

    // Loops? Arrays? Never heard of that stuff
    // Library avoids unnecessary heap allocations and unsafe code
    // so this ugly code becomes necessary :(
    internal static class ByteIntegerConverter
    {
        #region Individual

        public static UInt32 LoadLittleEndian32(byte[] buf, int offset)
        {
            return
                (UInt32)(buf[offset + 0])
            | (((UInt32)(buf[offset + 1])) << 8)
            | (((UInt32)(buf[offset + 2])) << 16)
            | (((UInt32)(buf[offset + 3])) << 24);
        }

        public static void StoreLittleEndian32(byte[] buf, int offset, UInt32 value)
        {
            buf[offset + 0] = unchecked((byte)value);
            buf[offset + 1] = unchecked((byte)(value >> 8));
            buf[offset + 2] = unchecked((byte)(value >> 16));
            buf[offset + 3] = unchecked((byte)(value >> 24));
        }

        public static UInt64 LoadBigEndian64(byte[] buf, int offset)
        {
            return
                (UInt64)(buf[offset + 7])
                | (((UInt64)(buf[offset + 6])) << 8)
                | (((UInt64)(buf[offset + 5])) << 16)
                | (((UInt64)(buf[offset + 4])) << 24)
                | (((UInt64)(buf[offset + 3])) << 32)
                | (((UInt64)(buf[offset + 2])) << 40)
                | (((UInt64)(buf[offset + 1])) << 48)
                | (((UInt64)(buf[offset + 0])) << 56);
        }

        public static void StoreBigEndian64(byte[] buf, int offset, UInt64 value)
        {
            buf[offset + 7] = unchecked((byte)value);
            buf[offset + 6] = unchecked((byte)(value >> 8));
            buf[offset + 5] = unchecked((byte)(value >> 16));
            buf[offset + 4] = unchecked((byte)(value >> 24));
            buf[offset + 3] = unchecked((byte)(value >> 32));
            buf[offset + 2] = unchecked((byte)(value >> 40));
            buf[offset + 1] = unchecked((byte)(value >> 48));
            buf[offset + 0] = unchecked((byte)(value >> 56));
        }

        /*public static void XorLittleEndian32(byte[] buf, int offset, UInt32 value)
        {
            buf[offset + 0] ^= (byte)value;
            buf[offset + 1] ^= (byte)(value >> 8);
            buf[offset + 2] ^= (byte)(value >> 16);
            buf[offset + 3] ^= (byte)(value >> 24);
        }*/

        /*public static void XorLittleEndian32(byte[] output, int outputOffset, byte[] input, int inputOffset, UInt32 value)
        {
            output[outputOffset + 0] = (byte)(input[inputOffset + 0] ^ value);
            output[outputOffset + 1] = (byte)(input[inputOffset + 1] ^ (value >> 8));
            output[outputOffset + 2] = (byte)(input[inputOffset + 2] ^ (value >> 16));
            output[outputOffset + 3] = (byte)(input[inputOffset + 3] ^ (value >> 24));
        }*/

        #endregion

        #region Array8

        public static void Array8LoadLittleEndian32(out Array8<UInt32> output, byte[] input, int inputOffset)
        {
            output.x0 = LoadLittleEndian32(input, inputOffset + 0);
            output.x1 = LoadLittleEndian32(input, inputOffset + 4);
            output.x2 = LoadLittleEndian32(input, inputOffset + 8);
            output.x3 = LoadLittleEndian32(input, inputOffset + 12);
            output.x4 = LoadLittleEndian32(input, inputOffset + 16);
            output.x5 = LoadLittleEndian32(input, inputOffset + 20);
            output.x6 = LoadLittleEndian32(input, inputOffset + 24);
            output.x7 = LoadLittleEndian32(input, inputOffset + 28);
        }

        /*        public static void Array8LoadLittleEndian32(out Array8<uint> output, byte[] input, int inputOffset, int inputLength)
                {
        #if DEBUG
                    if (inputLength <= 0)
                        throw new ArgumentException();
        #endif
                    int inputEnd = inputOffset + inputLength;
                    UInt32 highestInt;
                    switch (inputLength & 3)
                    {
                        case 1:
                            highestInt = input[inputEnd - 1];
                            break;
                        case 2:
                            highestInt = (uint)(
                                (input[inputEnd - 1] << 8) |
                                (input[inputEnd - 2]));
                            break;
                        case 3:
                            highestInt = (uint)(
                                (input[inputEnd - 1] << 16) |
                                (input[inputEnd - 2] << 8) |
                                (input[inputEnd - 3]));
                            break;
                        case 0:
                            highestInt = (uint)(
                                (input[inputEnd - 1] << 24) |
                                (input[inputEnd - 2] << 16) |
                                (input[inputEnd - 3] << 8) |
                                (input[inputEnd - 4]));
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    switch ((inputLength - 1) >> 2)
                    {
                        case 7:
                            output.x7 = highestInt;
                            output.x6 = LoadLittleEndian32(input, inputOffset + 6 * 4);
                            output.x5 = LoadLittleEndian32(input, inputOffset + 5 * 4);
                            output.x4 = LoadLittleEndian32(input, inputOffset + 4 * 4);
                            output.x3 = LoadLittleEndian32(input, inputOffset + 3 * 4);
                            output.x2 = LoadLittleEndian32(input, inputOffset + 2 * 4);
                            output.x1 = LoadLittleEndian32(input, inputOffset + 1 * 4);
                            output.x0 = LoadLittleEndian32(input, inputOffset + 0 * 4);
                            return;
                        case 6:
                            output.x7 = 0;
                            output.x6 = highestInt;
                            output.x5 = LoadLittleEndian32(input, inputOffset + 5 * 4);
                            output.x4 = LoadLittleEndian32(input, inputOffset + 4 * 4);
                            output.x3 = LoadLittleEndian32(input, inputOffset + 3 * 4);
                            output.x2 = LoadLittleEndian32(input, inputOffset + 2 * 4);
                            output.x1 = LoadLittleEndian32(input, inputOffset + 1 * 4);
                            output.x0 = LoadLittleEndian32(input, inputOffset + 0 * 4);
                            return;
                        case 5:
                            output.x7 = 0;
                            output.x6 = 0;
                            output.x5 = highestInt;
                            output.x4 = LoadLittleEndian32(input, inputOffset + 4 * 4);
                            output.x3 = LoadLittleEndian32(input, inputOffset + 3 * 4);
                            output.x2 = LoadLittleEndian32(input, inputOffset + 2 * 4);
                            output.x1 = LoadLittleEndian32(input, inputOffset + 1 * 4);
                            output.x0 = LoadLittleEndian32(input, inputOffset + 0 * 4);
                            return;
                        case 4:
                            output.x7 = 0;
                            output.x6 = 0;
                            output.x5 = 0;
                            output.x4 = highestInt;
                            output.x3 = LoadLittleEndian32(input, inputOffset + 3 * 4);
                            output.x2 = LoadLittleEndian32(input, inputOffset + 2 * 4);
                            output.x1 = LoadLittleEndian32(input, inputOffset + 1 * 4);
                            output.x0 = LoadLittleEndian32(input, inputOffset + 0 * 4);
                            return;
                        case 3:
                            output.x7 = 0;
                            output.x6 = 0;
                            output.x5 = 0;
                            output.x4 = 0;
                            output.x3 = highestInt;
                            output.x2 = LoadLittleEndian32(input, inputOffset + 2 * 4);
                            output.x1 = LoadLittleEndian32(input, inputOffset + 1 * 4);
                            output.x0 = LoadLittleEndian32(input, inputOffset + 0 * 4);
                            return;
                        case 2:
                            output.x7 = 0;
                            output.x6 = 0;
                            output.x5 = 0;
                            output.x4 = 0;
                            output.x3 = 0;
                            output.x2 = highestInt;
                            output.x1 = LoadLittleEndian32(input, inputOffset + 1 * 4);
                            output.x0 = LoadLittleEndian32(input, inputOffset + 0 * 4);
                            return;
                        case 1:
                            output.x7 = 0;
                            output.x6 = 0;
                            output.x5 = 0;
                            output.x4 = 0;
                            output.x3 = 0;
                            output.x2 = 0;
                            output.x1 = highestInt;
                            output.x0 = LoadLittleEndian32(input, inputOffset + 0 * 4);
                            return;
                        case 0:
                            output.x7 = 0;
                            output.x6 = 0;
                            output.x5 = 0;
                            output.x4 = 0;
                            output.x3 = 0;
                            output.x2 = 0;
                            output.x1 = 0;
                            output.x0 = highestInt;
                            return;
                        default:
                            throw new InvalidOperationException();
                    }
                }*/

        /*public static void Array8XorLittleEndian(byte[] output, int outputOffset, byte[] input, int inputOffset, ref Array8<uint> keyStream, int length)
        {
#if DEBUG
            InternalAssert(length > 0);
#endif
            int outputEnd = outputOffset + length;
            UInt32 highestInt;
            switch ((length - 1) >> 2)
            {
                case 7:
                    highestInt = keyStream.x7;
                    XorLittleEndian32(output, outputOffset + 6 * 4, input, inputOffset + 6 * 4, keyStream.x6);
                    XorLittleEndian32(output, outputOffset + 5 * 4, input, inputOffset + 6 * 4, keyStream.x5);
                    XorLittleEndian32(output, outputOffset + 4 * 4, input, inputOffset + 6 * 4, keyStream.x4);
                    XorLittleEndian32(output, outputOffset + 3 * 4, input, inputOffset + 6 * 4, keyStream.x3);
                    XorLittleEndian32(output, outputOffset + 2 * 4, input, inputOffset + 6 * 4, keyStream.x2);
                    XorLittleEndian32(output, outputOffset + 1 * 4, input, inputOffset + 6 * 4, keyStream.x1);
                    XorLittleEndian32(output, outputOffset + 0 * 4, input, inputOffset + 6 * 4, keyStream.x0);
                    break;
                case 6:
                    highestInt = keyStream.x6;
                    XorLittleEndian32(output, outputOffset + 5 * 4, input, inputOffset + 6 * 4, keyStream.x5);
                    XorLittleEndian32(output, outputOffset + 4 * 4, input, inputOffset + 6 * 4, keyStream.x4);
                    XorLittleEndian32(output, outputOffset + 3 * 4, input, inputOffset + 6 * 4, keyStream.x3);
                    XorLittleEndian32(output, outputOffset + 2 * 4, input, inputOffset + 6 * 4, keyStream.x2);
                    XorLittleEndian32(output, outputOffset + 1 * 4, input, inputOffset + 6 * 4, keyStream.x1);
                    XorLittleEndian32(output, outputOffset + 0 * 4, input, inputOffset + 6 * 4, keyStream.x0);
                    break;
                case 5:
                    highestInt = keyStream.x5;
                    XorLittleEndian32(output, outputOffset + 4 * 4, input, inputOffset + 6 * 4, keyStream.x4);
                    XorLittleEndian32(output, outputOffset + 3 * 4, input, inputOffset + 6 * 4, keyStream.x3);
                    XorLittleEndian32(output, outputOffset + 2 * 4, input, inputOffset + 6 * 4, keyStream.x2);
                    XorLittleEndian32(output, outputOffset + 1 * 4, input, inputOffset + 6 * 4, keyStream.x1);
                    XorLittleEndian32(output, outputOffset + 0 * 4, input, inputOffset + 6 * 4, keyStream.x0);
                    break;
                case 4:
                    highestInt = keyStream.x4;
                    XorLittleEndian32(output, outputOffset + 3 * 4, input, inputOffset + 6 * 4, keyStream.x3);
                    XorLittleEndian32(output, outputOffset + 2 * 4, input, inputOffset + 6 * 4, keyStream.x2);
                    XorLittleEndian32(output, outputOffset + 1 * 4, input, inputOffset + 6 * 4, keyStream.x1);
                    XorLittleEndian32(output, outputOffset + 0 * 4, input, inputOffset + 6 * 4, keyStream.x0);
                    break;
                case 3:
                    highestInt = keyStream.x3;
                    XorLittleEndian32(output, outputOffset + 2 * 4, input, inputOffset + 6 * 4, keyStream.x2);
                    XorLittleEndian32(output, outputOffset + 1 * 4, input, inputOffset + 6 * 4, keyStream.x1);
                    XorLittleEndian32(output, outputOffset + 0 * 4, input, inputOffset + 6 * 4, keyStream.x0);
                    break;
                case 2:
                    highestInt = keyStream.x2;
                    XorLittleEndian32(output, outputOffset + 1 * 4, input, inputOffset + 6 * 4, keyStream.x1);
                    XorLittleEndian32(output, outputOffset + 0 * 4, input, inputOffset + 6 * 4, keyStream.x0);
                    break;
                case 1:
                    highestInt = keyStream.x1;
                    XorLittleEndian32(output, outputOffset + 0 * 4, input, inputOffset + 6 * 4, keyStream.x0);
                    break;
                case 0:
                    highestInt = keyStream.x0;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            switch (length & 3)
            {
                case 1:
                    output[outputEnd - 1] ^= (byte)highestInt;
                    break;
                case 2:
                    output[outputEnd - 1] ^= (byte)(highestInt >> 8);
                    output[outputEnd - 2] ^= (byte)highestInt;
                    break;
                case 3:
                    output[outputEnd - 1] ^= (byte)(highestInt >> 16);
                    output[outputEnd - 2] ^= (byte)(highestInt >> 8);
                    output[outputEnd - 3] ^= (byte)highestInt;
                    break;
                case 0:
                    output[outputEnd - 1] ^= (byte)(highestInt >> 24);
                    output[outputEnd - 2] ^= (byte)(highestInt >> 16);
                    output[outputEnd - 3] ^= (byte)(highestInt >> 8);
                    output[outputEnd - 4] ^= (byte)highestInt;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }*/

        /*public static void Array8StoreLittleEndian32(byte[] output, int outputOffset, ref Array8<uint> input)
        {
            StoreLittleEndian32(output, outputOffset + 0, input.x0);
            StoreLittleEndian32(output, outputOffset + 4, input.x1);
            StoreLittleEndian32(output, outputOffset + 8, input.x2);
            StoreLittleEndian32(output, outputOffset + 12, input.x3);
            StoreLittleEndian32(output, outputOffset + 16, input.x4);
            StoreLittleEndian32(output, outputOffset + 20, input.x5);
            StoreLittleEndian32(output, outputOffset + 24, input.x6);
            StoreLittleEndian32(output, outputOffset + 28, input.x7);
        }*/
        #endregion

        public static void Array16LoadBigEndian64(out Array16<UInt64> output, byte[] input, int inputOffset)
        {
            output.x0 = LoadBigEndian64(input, inputOffset + 0);
            output.x1 = LoadBigEndian64(input, inputOffset + 8);
            output.x2 = LoadBigEndian64(input, inputOffset + 16);
            output.x3 = LoadBigEndian64(input, inputOffset + 24);
            output.x4 = LoadBigEndian64(input, inputOffset + 32);
            output.x5 = LoadBigEndian64(input, inputOffset + 40);
            output.x6 = LoadBigEndian64(input, inputOffset + 48);
            output.x7 = LoadBigEndian64(input, inputOffset + 56);
            output.x8 = LoadBigEndian64(input, inputOffset + 64);
            output.x9 = LoadBigEndian64(input, inputOffset + 72);
            output.x10 = LoadBigEndian64(input, inputOffset + 80);
            output.x11 = LoadBigEndian64(input, inputOffset + 88);
            output.x12 = LoadBigEndian64(input, inputOffset + 96);
            output.x13 = LoadBigEndian64(input, inputOffset + 104);
            output.x14 = LoadBigEndian64(input, inputOffset + 112);
            output.x15 = LoadBigEndian64(input, inputOffset + 120);
        }

        // ToDo: Only used in tests. Remove?
        public static void Array16LoadLittleEndian32(out Array16<UInt32> output, byte[] input, int inputOffset)
        {
            output.x0 = LoadLittleEndian32(input, inputOffset + 0);
            output.x1 = LoadLittleEndian32(input, inputOffset + 4);
            output.x2 = LoadLittleEndian32(input, inputOffset + 8);
            output.x3 = LoadLittleEndian32(input, inputOffset + 12);
            output.x4 = LoadLittleEndian32(input, inputOffset + 16);
            output.x5 = LoadLittleEndian32(input, inputOffset + 20);
            output.x6 = LoadLittleEndian32(input, inputOffset + 24);
            output.x7 = LoadLittleEndian32(input, inputOffset + 28);
            output.x8 = LoadLittleEndian32(input, inputOffset + 32);
            output.x9 = LoadLittleEndian32(input, inputOffset + 36);
            output.x10 = LoadLittleEndian32(input, inputOffset + 40);
            output.x11 = LoadLittleEndian32(input, inputOffset + 44);
            output.x12 = LoadLittleEndian32(input, inputOffset + 48);
            output.x13 = LoadLittleEndian32(input, inputOffset + 52);
            output.x14 = LoadLittleEndian32(input, inputOffset + 56);
            output.x15 = LoadLittleEndian32(input, inputOffset + 60);
        }

        /*public static void Array16LoadLittleEndian32(out Array16<UInt32> output, byte[] input, int inputOffset, int inputLength)
        {
            Array8<UInt32> temp;
            if (inputLength > 32)
            {
                output.x0 = LoadLittleEndian32(input, inputOffset + 0);
                output.x1 = LoadLittleEndian32(input, inputOffset + 4);
                output.x2 = LoadLittleEndian32(input, inputOffset + 8);
                output.x3 = LoadLittleEndian32(input, inputOffset + 12);
                output.x4 = LoadLittleEndian32(input, inputOffset + 16);
                output.x5 = LoadLittleEndian32(input, inputOffset + 20);
                output.x6 = LoadLittleEndian32(input, inputOffset + 24);
                output.x7 = LoadLittleEndian32(input, inputOffset + 28);
                Array8LoadLittleEndian32(out temp, input, inputOffset + 32, inputLength - 32);
                output.x8 = temp.x0;
                output.x9 = temp.x1;
                output.x10 = temp.x2;
                output.x11 = temp.x3;
                output.x12 = temp.x4;
                output.x13 = temp.x5;
                output.x14 = temp.x6;
                output.x15 = temp.x7;
            }
            else
            {
                Array8LoadLittleEndian32(out temp, input, inputOffset, inputLength);
                output.x0 = temp.x0;
                output.x1 = temp.x1;
                output.x2 = temp.x2;
                output.x3 = temp.x3;
                output.x4 = temp.x4;
                output.x5 = temp.x5;
                output.x6 = temp.x6;
                output.x7 = temp.x7;
                output.x8 = 0;
                output.x9 = 0;
                output.x10 = 0;
                output.x11 = 0;
                output.x12 = 0;
                output.x13 = 0;
                output.x14 = 0;
                output.x15 = 0;
            }
        }*/

        public static void Array16StoreLittleEndian32(byte[] output, int outputOffset, ref Array16<UInt32> input)
        {
            StoreLittleEndian32(output, outputOffset + 0, input.x0);
            StoreLittleEndian32(output, outputOffset + 4, input.x1);
            StoreLittleEndian32(output, outputOffset + 8, input.x2);
            StoreLittleEndian32(output, outputOffset + 12, input.x3);
            StoreLittleEndian32(output, outputOffset + 16, input.x4);
            StoreLittleEndian32(output, outputOffset + 20, input.x5);
            StoreLittleEndian32(output, outputOffset + 24, input.x6);
            StoreLittleEndian32(output, outputOffset + 28, input.x7);
            StoreLittleEndian32(output, outputOffset + 32, input.x8);
            StoreLittleEndian32(output, outputOffset + 36, input.x9);
            StoreLittleEndian32(output, outputOffset + 40, input.x10);
            StoreLittleEndian32(output, outputOffset + 44, input.x11);
            StoreLittleEndian32(output, outputOffset + 48, input.x12);
            StoreLittleEndian32(output, outputOffset + 52, input.x13);
            StoreLittleEndian32(output, outputOffset + 56, input.x14);
            StoreLittleEndian32(output, outputOffset + 60, input.x15);
        }
    }
    internal static class InternalAssert
    {
        public static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("An assertion in Chaos.Crypto failed " + message);
        }
    }

    internal class Poly1305Donna
    {
        // written by floodyberry (Andrew M.)
        // original license: MIT or PUBLIC DOMAIN
        // https://github.com/floodyberry/poly1305-donna/blob/master/poly1305-donna-unrolled.c
        public static void poly1305_auth(byte[] output, int outputOffset, byte[] m, int mStart, int mLength, ref Array8<UInt32> key)
        {
            UInt32 t0, t1, t2, t3;
            UInt32 h0, h1, h2, h3, h4;
            UInt32 r0, r1, r2, r3, r4;
            UInt32 s1, s2, s3, s4;
            UInt32 b, nb;
            int j;
            UInt64 tt0, tt1, tt2, tt3, tt4;
            UInt64 f0, f1, f2, f3;
            UInt32 g0, g1, g2, g3, g4;
            UInt64 c;

            /* clamp key */
            t0 = key.x0;
            t1 = key.x1;
            t2 = key.x2;
            t3 = key.x3;

            /* precompute multipliers */
            r0 = t0 & 0x3ffffff; t0 >>= 26; t0 |= t1 << 6;
            r1 = t0 & 0x3ffff03; t1 >>= 20; t1 |= t2 << 12;
            r2 = t1 & 0x3ffc0ff; t2 >>= 14; t2 |= t3 << 18;
            r3 = t2 & 0x3f03fff; t3 >>= 8;
            r4 = t3 & 0x00fffff;

            s1 = r1 * 5;
            s2 = r2 * 5;
            s3 = r3 * 5;
            s4 = r4 * 5;

            /* init state */
            h0 = 0;
            h1 = 0;
            h2 = 0;
            h3 = 0;
            h4 = 0;

            /* full blocks */
            if (mLength < 16)
                goto poly1305_donna_atmost15bytes;

            poly1305_donna_16bytes:
            mStart += 16;
            mLength -= 16;

            t0 = ByteIntegerConverter.LoadLittleEndian32(m, mStart - 16);
            t1 = ByteIntegerConverter.LoadLittleEndian32(m, mStart - 12);
            t2 = ByteIntegerConverter.LoadLittleEndian32(m, mStart - 8);
            t3 = ByteIntegerConverter.LoadLittleEndian32(m, mStart - 4);

            //todo: looks like these can be simplified a bit
            h0 += t0 & 0x3ffffff;
            h1 += (uint)(((((UInt64)t1 << 32) | t0) >> 26) & 0x3ffffff);
            h2 += (uint)(((((UInt64)t2 << 32) | t1) >> 20) & 0x3ffffff);
            h3 += (uint)(((((UInt64)t3 << 32) | t2) >> 14) & 0x3ffffff);
            h4 += (t3 >> 8) | (1 << 24);


        poly1305_donna_mul:
            tt0 = (ulong)h0 * r0 + (ulong)h1 * s4 + (ulong)h2 * s3 + (ulong)h3 * s2 + (ulong)h4 * s1;
            tt1 = (ulong)h0 * r1 + (ulong)h1 * r0 + (ulong)h2 * s4 + (ulong)h3 * s3 + (ulong)h4 * s2;
            tt2 = (ulong)h0 * r2 + (ulong)h1 * r1 + (ulong)h2 * r0 + (ulong)h3 * s4 + (ulong)h4 * s3;
            tt3 = (ulong)h0 * r3 + (ulong)h1 * r2 + (ulong)h2 * r1 + (ulong)h3 * r0 + (ulong)h4 * s4;
            tt4 = (ulong)h0 * r4 + (ulong)h1 * r3 + (ulong)h2 * r2 + (ulong)h3 * r1 + (ulong)h4 * r0;

            unchecked
            {
                h0 = (UInt32)tt0 & 0x3ffffff; c = (tt0 >> 26);
                tt1 += c; h1 = (UInt32)tt1 & 0x3ffffff; b = (UInt32)(tt1 >> 26);
                tt2 += b; h2 = (UInt32)tt2 & 0x3ffffff; b = (UInt32)(tt2 >> 26);
                tt3 += b; h3 = (UInt32)tt3 & 0x3ffffff; b = (UInt32)(tt3 >> 26);
                tt4 += b; h4 = (UInt32)tt4 & 0x3ffffff; b = (UInt32)(tt4 >> 26);
            }
            h0 += b * 5;

            if (mLength >= 16)
                goto poly1305_donna_16bytes;

            /* final bytes */
            poly1305_donna_atmost15bytes:
            if (mLength == 0)
                goto poly1305_donna_finish;

            byte[] mp = new byte[16];//todo remove allocation

            for (j = 0; j < mLength; j++)
                mp[j] = m[mStart + j];
            mp[j++] = 1;
            for (; j < 16; j++)
                mp[j] = 0;
            mLength = 0;

            t0 = ByteIntegerConverter.LoadLittleEndian32(mp, 0);
            t1 = ByteIntegerConverter.LoadLittleEndian32(mp, 4);
            t2 = ByteIntegerConverter.LoadLittleEndian32(mp, 8);
            t3 = ByteIntegerConverter.LoadLittleEndian32(mp, 12);
            CryptoBytes.Wipe(mp);

            h0 += t0 & 0x3ffffff;
            h1 += (uint)(((((UInt64)t1 << 32) | t0) >> 26) & 0x3ffffff);
            h2 += (uint)(((((UInt64)t2 << 32) | t1) >> 20) & 0x3ffffff);
            h3 += (uint)(((((UInt64)t3 << 32) | t2) >> 14) & 0x3ffffff);
            h4 += t3 >> 8;

            goto poly1305_donna_mul;

        poly1305_donna_finish:
            b = h0 >> 26; h0 = h0 & 0x3ffffff;
            h1 += b; b = h1 >> 26; h1 = h1 & 0x3ffffff;
            h2 += b; b = h2 >> 26; h2 = h2 & 0x3ffffff;
            h3 += b; b = h3 >> 26; h3 = h3 & 0x3ffffff;
            h4 += b; b = h4 >> 26; h4 = h4 & 0x3ffffff;
            h0 += b * 5;

            g0 = h0 + 5; b = g0 >> 26; g0 &= 0x3ffffff;
            g1 = h1 + b; b = g1 >> 26; g1 &= 0x3ffffff;
            g2 = h2 + b; b = g2 >> 26; g2 &= 0x3ffffff;
            g3 = h3 + b; b = g3 >> 26; g3 &= 0x3ffffff;
            g4 = unchecked(h4 + b - (1 << 26));

            b = (g4 >> 31) - 1;
            nb = ~b;
            h0 = (h0 & nb) | (g0 & b);
            h1 = (h1 & nb) | (g1 & b);
            h2 = (h2 & nb) | (g2 & b);
            h3 = (h3 & nb) | (g3 & b);
            h4 = (h4 & nb) | (g4 & b);

            f0 = ((h0) | (h1 << 26)) + (UInt64)key.x4;
            f1 = ((h1 >> 6) | (h2 << 20)) + (UInt64)key.x5;
            f2 = ((h2 >> 12) | (h3 << 14)) + (UInt64)key.x6;
            f3 = ((h3 >> 18) | (h4 << 8)) + (UInt64)key.x7;

            unchecked
            {
                ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 0, (uint)f0); f1 += (f0 >> 32);
                ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 4, (uint)f1); f2 += (f1 >> 32);
                ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 8, (uint)f2); f3 += (f2 >> 32);
                ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 12, (uint)f3);
            }
        }
    }

    internal static class Sha512Internal
    {
        private static readonly UInt64[] K = new UInt64[]
            {
                0x428a2f98d728ae22,0x7137449123ef65cd,0xb5c0fbcfec4d3b2f,0xe9b5dba58189dbbc,
                0x3956c25bf348b538,0x59f111f1b605d019,0x923f82a4af194f9b,0xab1c5ed5da6d8118,
                0xd807aa98a3030242,0x12835b0145706fbe,0x243185be4ee4b28c,0x550c7dc3d5ffb4e2,
                0x72be5d74f27b896f,0x80deb1fe3b1696b1,0x9bdc06a725c71235,0xc19bf174cf692694,
                0xe49b69c19ef14ad2,0xefbe4786384f25e3,0x0fc19dc68b8cd5b5,0x240ca1cc77ac9c65,
                0x2de92c6f592b0275,0x4a7484aa6ea6e483,0x5cb0a9dcbd41fbd4,0x76f988da831153b5,
                0x983e5152ee66dfab,0xa831c66d2db43210,0xb00327c898fb213f,0xbf597fc7beef0ee4,
                0xc6e00bf33da88fc2,0xd5a79147930aa725,0x06ca6351e003826f,0x142929670a0e6e70,
                0x27b70a8546d22ffc,0x2e1b21385c26c926,0x4d2c6dfc5ac42aed,0x53380d139d95b3df,
                0x650a73548baf63de,0x766a0abb3c77b2a8,0x81c2c92e47edaee6,0x92722c851482353b,
                0xa2bfe8a14cf10364,0xa81a664bbc423001,0xc24b8b70d0f89791,0xc76c51a30654be30,
                0xd192e819d6ef5218,0xd69906245565a910,0xf40e35855771202a,0x106aa07032bbd1b8,
                0x19a4c116b8d2d0c8,0x1e376c085141ab53,0x2748774cdf8eeb99,0x34b0bcb5e19b48a8,
                0x391c0cb3c5c95a63,0x4ed8aa4ae3418acb,0x5b9cca4f7763e373,0x682e6ff3d6b2b8a3,
                0x748f82ee5defb2fc,0x78a5636f43172f60,0x84c87814a1f0ab72,0x8cc702081a6439ec,
                0x90befffa23631e28,0xa4506cebde82bde9,0xbef9a3f7b2c67915,0xc67178f2e372532b,
                0xca273eceea26619c,0xd186b8c721c0c207,0xeada7dd6cde0eb1e,0xf57d4f7fee6ed178,
                0x06f067aa72176fba,0x0a637dc5a2c898a6,0x113f9804bef90dae,0x1b710b35131c471b,
                0x28db77f523047d84,0x32caab7b40c72493,0x3c9ebe0a15c9bebc,0x431d67c49c100d4c,
                0x4cc5d4becb3e42b6,0x597f299cfc657e2a,0x5fcb6fab3ad6faec,0x6c44198c4a475817
            };

        internal static void Sha512Init(out Array8<UInt64> state)
        {
            state.x0 = 0x6a09e667f3bcc908;
            state.x1 = 0xbb67ae8584caa73b;
            state.x2 = 0x3c6ef372fe94f82b;
            state.x3 = 0xa54ff53a5f1d36f1;
            state.x4 = 0x510e527fade682d1;
            state.x5 = 0x9b05688c2b3e6c1f;
            state.x6 = 0x1f83d9abfb41bd6b;
            state.x7 = 0x5be0cd19137e2179;
        }

        internal static void Core(out Array8<UInt64> outputState, ref Array8<UInt64> inputState, ref Array16<UInt64> input)
        {
            unchecked
            {
                UInt64 a = inputState.x0;
                UInt64 b = inputState.x1;
                UInt64 c = inputState.x2;
                UInt64 d = inputState.x3;
                UInt64 e = inputState.x4;
                UInt64 f = inputState.x5;
                UInt64 g = inputState.x6;
                UInt64 h = inputState.x7;

                UInt64 w0 = input.x0;
                UInt64 w1 = input.x1;
                UInt64 w2 = input.x2;
                UInt64 w3 = input.x3;
                UInt64 w4 = input.x4;
                UInt64 w5 = input.x5;
                UInt64 w6 = input.x6;
                UInt64 w7 = input.x7;
                UInt64 w8 = input.x8;
                UInt64 w9 = input.x9;
                UInt64 w10 = input.x10;
                UInt64 w11 = input.x11;
                UInt64 w12 = input.x12;
                UInt64 w13 = input.x13;
                UInt64 w14 = input.x14;
                UInt64 w15 = input.x15;

                int t = 0;
                while (true)
                {
                    ulong t1, t2;

                    {//0
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w0;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//1
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w1;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//2
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w2;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//3
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w3;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//4
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w4;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//5
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w5;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//6
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w6;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//7
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w7;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//8
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w8;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//9
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w9;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//10
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w10;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//11
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w11;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//12
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w12;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//13
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w13;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//14
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w14;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    {//15
                        t1 = h +
                             ((e >> 14) ^ (e << (64 - 14)) ^ (e >> 18) ^ (e << (64 - 18)) ^ (e >> 41) ^ (e << (64 - 41))) +
                             //Sigma1(e)
                             ((e & f) ^ (~e & g)) + //Ch(e,f,g)
                             K[t] + w15;
                        t2 = ((a >> 28) ^ (a << (64 - 28)) ^ (a >> 34) ^ (a << (64 - 34)) ^ (a >> 39) ^ (a << (64 - 39))) +
                             //Sigma0(a)
                             ((a & b) ^ (a & c) ^ (b & c)); //Maj(a,b,c)
                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                        t++;
                    }
                    if (t == 80)
                        break;

                    w0 += ((w14 >> 19) ^ (w14 << (64 - 19)) ^ (w14 >> 61) ^ (w14 << (64 - 61)) ^ (w14 >> 6)) +
                          w9 +
                          ((w1 >> 1) ^ (w1 << (64 - 1)) ^ (w1 >> 8) ^ (w1 << (64 - 8)) ^ (w1 >> 7));
                    w1 += ((w15 >> 19) ^ (w15 << (64 - 19)) ^ (w15 >> 61) ^ (w15 << (64 - 61)) ^ (w15 >> 6)) +
                          w10 +
                          ((w2 >> 1) ^ (w2 << (64 - 1)) ^ (w2 >> 8) ^ (w2 << (64 - 8)) ^ (w2 >> 7));
                    w2 += ((w0 >> 19) ^ (w0 << (64 - 19)) ^ (w0 >> 61) ^ (w0 << (64 - 61)) ^ (w0 >> 6)) +
                          w11 +
                          ((w3 >> 1) ^ (w3 << (64 - 1)) ^ (w3 >> 8) ^ (w3 << (64 - 8)) ^ (w3 >> 7));
                    w3 += ((w1 >> 19) ^ (w1 << (64 - 19)) ^ (w1 >> 61) ^ (w1 << (64 - 61)) ^ (w1 >> 6)) +
                          w12 +
                          ((w4 >> 1) ^ (w4 << (64 - 1)) ^ (w4 >> 8) ^ (w4 << (64 - 8)) ^ (w4 >> 7));
                    w4 += ((w2 >> 19) ^ (w2 << (64 - 19)) ^ (w2 >> 61) ^ (w2 << (64 - 61)) ^ (w2 >> 6)) +
                          w13 +
                          ((w5 >> 1) ^ (w5 << (64 - 1)) ^ (w5 >> 8) ^ (w5 << (64 - 8)) ^ (w5 >> 7));
                    w5 += ((w3 >> 19) ^ (w3 << (64 - 19)) ^ (w3 >> 61) ^ (w3 << (64 - 61)) ^ (w3 >> 6)) +
                          w14 +
                          ((w6 >> 1) ^ (w6 << (64 - 1)) ^ (w6 >> 8) ^ (w6 << (64 - 8)) ^ (w6 >> 7));
                    w6 += ((w4 >> 19) ^ (w4 << (64 - 19)) ^ (w4 >> 61) ^ (w4 << (64 - 61)) ^ (w4 >> 6)) +
                          w15 +
                          ((w7 >> 1) ^ (w7 << (64 - 1)) ^ (w7 >> 8) ^ (w7 << (64 - 8)) ^ (w7 >> 7));
                    w7 += ((w5 >> 19) ^ (w5 << (64 - 19)) ^ (w5 >> 61) ^ (w5 << (64 - 61)) ^ (w5 >> 6)) +
                          w0 +
                          ((w8 >> 1) ^ (w8 << (64 - 1)) ^ (w8 >> 8) ^ (w8 << (64 - 8)) ^ (w8 >> 7));
                    w8 += ((w6 >> 19) ^ (w6 << (64 - 19)) ^ (w6 >> 61) ^ (w6 << (64 - 61)) ^ (w6 >> 6)) +
                          w1 +
                          ((w9 >> 1) ^ (w9 << (64 - 1)) ^ (w9 >> 8) ^ (w9 << (64 - 8)) ^ (w9 >> 7));
                    w9 += ((w7 >> 19) ^ (w7 << (64 - 19)) ^ (w7 >> 61) ^ (w7 << (64 - 61)) ^ (w7 >> 6)) +
                          w2 +
                          ((w10 >> 1) ^ (w10 << (64 - 1)) ^ (w10 >> 8) ^ (w10 << (64 - 8)) ^ (w10 >> 7));
                    w10 += ((w8 >> 19) ^ (w8 << (64 - 19)) ^ (w8 >> 61) ^ (w8 << (64 - 61)) ^ (w8 >> 6)) +
                           w3 +
                           ((w11 >> 1) ^ (w11 << (64 - 1)) ^ (w11 >> 8) ^ (w11 << (64 - 8)) ^ (w11 >> 7));
                    w11 += ((w9 >> 19) ^ (w9 << (64 - 19)) ^ (w9 >> 61) ^ (w9 << (64 - 61)) ^ (w9 >> 6)) +
                           w4 +
                           ((w12 >> 1) ^ (w12 << (64 - 1)) ^ (w12 >> 8) ^ (w12 << (64 - 8)) ^ (w12 >> 7));
                    w12 += ((w10 >> 19) ^ (w10 << (64 - 19)) ^ (w10 >> 61) ^ (w10 << (64 - 61)) ^ (w10 >> 6)) +
                           w5 +
                           ((w13 >> 1) ^ (w13 << (64 - 1)) ^ (w13 >> 8) ^ (w13 << (64 - 8)) ^ (w13 >> 7));
                    w13 += ((w11 >> 19) ^ (w11 << (64 - 19)) ^ (w11 >> 61) ^ (w11 << (64 - 61)) ^ (w11 >> 6)) +
                           w6 +
                           ((w14 >> 1) ^ (w14 << (64 - 1)) ^ (w14 >> 8) ^ (w14 << (64 - 8)) ^ (w14 >> 7));
                    w14 += ((w12 >> 19) ^ (w12 << (64 - 19)) ^ (w12 >> 61) ^ (w12 << (64 - 61)) ^ (w12 >> 6)) +
                           w7 +
                           ((w15 >> 1) ^ (w15 << (64 - 1)) ^ (w15 >> 8) ^ (w15 << (64 - 8)) ^ (w15 >> 7));
                    w15 += ((w13 >> 19) ^ (w13 << (64 - 19)) ^ (w13 >> 61) ^ (w13 << (64 - 61)) ^ (w13 >> 6)) +
                           w8 +
                           ((w0 >> 1) ^ (w0 << (64 - 1)) ^ (w0 >> 8) ^ (w0 << (64 - 8)) ^ (w0 >> 7));
                }

                outputState.x0 = inputState.x0 + a;
                outputState.x1 = inputState.x1 + b;
                outputState.x2 = inputState.x2 + c;
                outputState.x3 = inputState.x3 + d;
                outputState.x4 = inputState.x4 + e;
                outputState.x5 = inputState.x5 + f;
                outputState.x6 = inputState.x6 + g;
                outputState.x7 = inputState.x7 + h;
            }
        }
    }

    internal class Salsa20
    {
        public const uint SalsaConst0 = 0x61707865;
        public const uint SalsaConst1 = 0x3320646e;
        public const uint SalsaConst2 = 0x79622d32;
        public const uint SalsaConst3 = 0x6b206574;

        public static void HSalsa20(byte[] output, int outputOffset, byte[] key, int keyOffset, byte[] nonce, int nonceOffset)
        {
            Array16<UInt32> state;
            state.x0 = SalsaConst0;
            state.x1 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 0);
            state.x2 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 4);
            state.x3 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 8);
            state.x4 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 12);
            state.x5 = SalsaConst1;
            state.x6 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 0);
            state.x7 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 4);
            state.x8 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 8);
            state.x9 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 12);
            state.x10 = SalsaConst2;
            state.x11 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 16);
            state.x12 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 20);
            state.x13 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 24);
            state.x14 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 28);
            state.x15 = SalsaConst3;

            SalsaCore.HSalsa(out state, ref state, 20);

            ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 0, state.x0);
            ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 4, state.x5);
            ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 8, state.x10);
            ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 12, state.x15);
            ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 16, state.x6);
            ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 20, state.x7);
            ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 24, state.x8);
            ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 28, state.x9);
        }
    }

    internal static class SalsaCore
    {
        public static void HSalsa(out Array16<UInt32> output, ref Array16<UInt32> input, int rounds)
        {
            InternalAssert.Assert(rounds % 2 == 0, "Number of salsa rounds must be even");

            int doubleRounds = rounds / 2;

            UInt32 x0 = input.x0;
            UInt32 x1 = input.x1;
            UInt32 x2 = input.x2;
            UInt32 x3 = input.x3;
            UInt32 x4 = input.x4;
            UInt32 x5 = input.x5;
            UInt32 x6 = input.x6;
            UInt32 x7 = input.x7;
            UInt32 x8 = input.x8;
            UInt32 x9 = input.x9;
            UInt32 x10 = input.x10;
            UInt32 x11 = input.x11;
            UInt32 x12 = input.x12;
            UInt32 x13 = input.x13;
            UInt32 x14 = input.x14;
            UInt32 x15 = input.x15;

            unchecked
            {
                for (int i = 0; i < doubleRounds; i++)
                {
                    UInt32 y;

                    // row 0
                    y = x0 + x12;
                    x4 ^= (y << 7) | (y >> (32 - 7));
                    y = x4 + x0;
                    x8 ^= (y << 9) | (y >> (32 - 9));
                    y = x8 + x4;
                    x12 ^= (y << 13) | (y >> (32 - 13));
                    y = x12 + x8;
                    x0 ^= (y << 18) | (y >> (32 - 18));

                    // row 1
                    y = x5 + x1;
                    x9 ^= (y << 7) | (y >> (32 - 7));
                    y = x9 + x5;
                    x13 ^= (y << 9) | (y >> (32 - 9));
                    y = x13 + x9;
                    x1 ^= (y << 13) | (y >> (32 - 13));
                    y = x1 + x13;
                    x5 ^= (y << 18) | (y >> (32 - 18));

                    // row 2
                    y = x10 + x6;
                    x14 ^= (y << 7) | (y >> (32 - 7));
                    y = x14 + x10;
                    x2 ^= (y << 9) | (y >> (32 - 9));
                    y = x2 + x14;
                    x6 ^= (y << 13) | (y >> (32 - 13));
                    y = x6 + x2;
                    x10 ^= (y << 18) | (y >> (32 - 18));

                    // row 3
                    y = x15 + x11;
                    x3 ^= (y << 7) | (y >> (32 - 7));
                    y = x3 + x15;
                    x7 ^= (y << 9) | (y >> (32 - 9));
                    y = x7 + x3;
                    x11 ^= (y << 13) | (y >> (32 - 13));
                    y = x11 + x7;
                    x15 ^= (y << 18) | (y >> (32 - 18));

                    // column 0
                    y = x0 + x3;
                    x1 ^= (y << 7) | (y >> (32 - 7));
                    y = x1 + x0;
                    x2 ^= (y << 9) | (y >> (32 - 9));
                    y = x2 + x1;
                    x3 ^= (y << 13) | (y >> (32 - 13));
                    y = x3 + x2;
                    x0 ^= (y << 18) | (y >> (32 - 18));

                    // column 1
                    y = x5 + x4;
                    x6 ^= (y << 7) | (y >> (32 - 7));
                    y = x6 + x5;
                    x7 ^= (y << 9) | (y >> (32 - 9));
                    y = x7 + x6;
                    x4 ^= (y << 13) | (y >> (32 - 13));
                    y = x4 + x7;
                    x5 ^= (y << 18) | (y >> (32 - 18));

                    // column 2
                    y = x10 + x9;
                    x11 ^= (y << 7) | (y >> (32 - 7));
                    y = x11 + x10;
                    x8 ^= (y << 9) | (y >> (32 - 9));
                    y = x8 + x11;
                    x9 ^= (y << 13) | (y >> (32 - 13));
                    y = x9 + x8;
                    x10 ^= (y << 18) | (y >> (32 - 18));

                    // column 3
                    y = x15 + x14;
                    x12 ^= (y << 7) | (y >> (32 - 7));
                    y = x12 + x15;
                    x13 ^= (y << 9) | (y >> (32 - 9));
                    y = x13 + x12;
                    x14 ^= (y << 13) | (y >> (32 - 13));
                    y = x14 + x13;
                    x15 ^= (y << 18) | (y >> (32 - 18));
                }
            }

            output.x0 = x0;
            output.x1 = x1;
            output.x2 = x2;
            output.x3 = x3;
            output.x4 = x4;
            output.x5 = x5;
            output.x6 = x6;
            output.x7 = x7;
            output.x8 = x8;
            output.x9 = x9;
            output.x10 = x10;
            output.x11 = x11;
            output.x12 = x12;
            output.x13 = x13;
            output.x14 = x14;
            output.x15 = x15;
        }

        public static void Salsa(out Array16<UInt32> output, ref Array16<UInt32> input, int rounds)
        {
            HSalsa(out Array16<uint> temp, ref input, rounds);
            unchecked
            {
                output.x0 = temp.x0 + input.x0;
                output.x1 = temp.x1 + input.x1;
                output.x2 = temp.x2 + input.x2;
                output.x3 = temp.x3 + input.x3;
                output.x4 = temp.x4 + input.x4;
                output.x5 = temp.x5 + input.x5;
                output.x6 = temp.x6 + input.x6;
                output.x7 = temp.x7 + input.x7;
                output.x8 = temp.x8 + input.x8;
                output.x9 = temp.x9 + input.x9;
                output.x10 = temp.x10 + input.x10;
                output.x11 = temp.x11 + input.x11;
                output.x12 = temp.x12 + input.x12;
                output.x13 = temp.x13 + input.x13;
                output.x14 = temp.x14 + input.x14;
                output.x15 = temp.x15 + input.x15;
            }
        }

        /*public static void SalsaCore(int[] output, int outputOffset, int[] input, int inputOffset, int rounds)
        {
            if (rounds % 2 != 0)
                throw new ArgumentException("rounds must be even");
        }


static void store_littleendian(unsigned char *x,uint32 u)
{
  x[0] = u; u >>= 8;
  x[1] = u; u >>= 8;
  x[2] = u; u >>= 8;
  x[3] = u;
}

        public static void HSalsaCore(int[] output, int outputOffset, int[] input, int inputOffset, int rounds)
        {
            if (rounds % 2 != 0)
                throw new ArgumentException("rounds must be even");
            static uint32 rotate(uint32 u,int c)
{
  return (u << c) | (u >> (32 - c));
}



int crypto_core(
        unsigned char *out,
  const unsigned char *in,
  const unsigned char *k,
  const unsigned char *c
)
{
  uint32 x0, x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12, x13, x14, x15;
  int i;

  x0 = load_littleendian(c + 0);
  x1 = load_littleendian(k + 0);
  x2 = load_littleendian(k + 4);
  x3 = load_littleendian(k + 8);
  x4 = load_littleendian(k + 12);
  x5 = load_littleendian(c + 4);
  x6 = load_littleendian(in + 0);
  x7 = load_littleendian(in + 4);
  x8 = load_littleendian(in + 8);
  x9 = load_littleendian(in + 12);
  x10 = load_littleendian(c + 8);
  x11 = load_littleendian(k + 16);
  x12 = load_littleendian(k + 20);
  x13 = load_littleendian(k + 24);
  x14 = load_littleendian(k + 28);
  x15 = load_littleendian(c + 12);

  for (i = ROUNDS;i > 0;i -= 2) {
     x4 ^= rotate( x0+x12, 7);
     x8 ^= rotate( x4+ x0, 9);
    x12 ^= rotate( x8+ x4,13);
     x0 ^= rotate(x12+ x8,18);
     x9 ^= rotate( x5+ x1, 7);
    x13 ^= rotate( x9+ x5, 9);
     x1 ^= rotate(x13+ x9,13);
     x5 ^= rotate( x1+x13,18);
    x14 ^= rotate(x10+ x6, 7);
     x2 ^= rotate(x14+x10, 9);
     x6 ^= rotate( x2+x14,13);
    x10 ^= rotate( x6+ x2,18);
     x3 ^= rotate(x15+x11, 7);
     x7 ^= rotate( x3+x15, 9);
    x11 ^= rotate( x7+ x3,13);
    x15 ^= rotate(x11+ x7,18);
     x1 ^= rotate( x0+ x3, 7);
     x2 ^= rotate( x1+ x0, 9);
     x3 ^= rotate( x2+ x1,13);
     x0 ^= rotate( x3+ x2,18);
     x6 ^= rotate( x5+ x4, 7);
     x7 ^= rotate( x6+ x5, 9);
     x4 ^= rotate( x7+ x6,13);
     x5 ^= rotate( x4+ x7,18);
    x11 ^= rotate(x10+ x9, 7);
     x8 ^= rotate(x11+x10, 9);
     x9 ^= rotate( x8+x11,13);
    x10 ^= rotate( x9+ x8,18);
    x12 ^= rotate(x15+x14, 7);
    x13 ^= rotate(x12+x15, 9);
    x14 ^= rotate(x13+x12,13);
    x15 ^= rotate(x14+x13,18);
  }

  store_littleendian(out + 0,x0);
  store_littleendian(out + 4,x5);
  store_littleendian(out + 8,x10);
  store_littleendian(out + 12,x15);
  store_littleendian(out + 16,x6);
  store_littleendian(out + 20,x7);
  store_littleendian(out + 24,x8);
  store_littleendian(out + 28,x9);

  return 0;
}*/

    }
}
