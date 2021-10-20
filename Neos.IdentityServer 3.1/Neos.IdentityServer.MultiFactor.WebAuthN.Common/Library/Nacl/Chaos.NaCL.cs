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
using System.Runtime.CompilerServices;

#pragma warning disable 618

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Nacl
{

    public static class CryptoBytes
    {
        public static bool ConstantTimeEquals(byte[] x, byte[] y)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");
            if (x.Length != y.Length)
                throw new ArgumentException("x.Length must equal y.Length");
            return InternalConstantTimeEquals(x, 0, y, 0, x.Length) != 0;
        }

        public static bool ConstantTimeEquals(ArraySegment<byte> x, ArraySegment<byte> y)
        {
            if (x.Array == null)
                throw new ArgumentNullException("x.Array");
            if (y.Array == null)
                throw new ArgumentNullException("y.Array");
            if (x.Count != y.Count)
                throw new ArgumentException("x.Count must equal y.Count");

            return InternalConstantTimeEquals(x.Array, x.Offset, y.Array, y.Offset, x.Count) != 0;
        }

        public static bool ConstantTimeEquals(byte[] x, int xOffset, byte[] y, int yOffset, int length)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (xOffset < 0)
                throw new ArgumentOutOfRangeException("xOffset", "xOffset < 0");
            if (y == null)
                throw new ArgumentNullException("y");
            if (yOffset < 0)
                throw new ArgumentOutOfRangeException("yOffset", "yOffset < 0");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "length < 0");
            if (x.Length - xOffset < length)
                throw new ArgumentException("xOffset + length > x.Length");
            if (y.Length - yOffset < length)
                throw new ArgumentException("yOffset + length > y.Length");

            return InternalConstantTimeEquals(x, xOffset, y, yOffset, length) != 0;
        }

        private static uint InternalConstantTimeEquals(byte[] x, int xOffset, byte[] y, int yOffset, int length)
        {
            int differentbits = 0;
            for (int i = 0; i < length; i++)
                differentbits |= x[xOffset + i] ^ y[yOffset + i];
            return (1 & (unchecked((uint)differentbits - 1) >> 8));
        }

        public static void Wipe(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            InternalWipe(data, 0, data.Length);
        }

        public static void Wipe(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Requires count >= 0");
            if ((uint)offset + (uint)count > (uint)data.Length)
                throw new ArgumentException("Requires offset + count <= data.Length");
            InternalWipe(data, offset, count);
        }

        public static void Wipe(ArraySegment<byte> data)
        {
            if (data.Array == null)
                throw new ArgumentNullException("data.Array");
            InternalWipe(data.Array, data.Offset, data.Count);
        }

        // Secure wiping is hard
        // * the GC can move around and copy memory
        //   Perhaps this can be avoided by using unmanaged memory or by fixing the position of the array in memory
        // * Swap files and error dumps can contain secret information
        //   It seems possible to lock memory in RAM, no idea about error dumps
        // * Compiler could optimize out the wiping if it knows that data won't be read back
        //   I hope this is enough, suppressing inlining
        //   but perhaps `RtlSecureZeroMemory` is needed
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void InternalWipe(byte[] data, int offset, int count)
        {
            Array.Clear(data, offset, count);
        }

        // shallow wipe of structs
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void InternalWipe<T>(ref T data)
            where T : struct
        {
            data = default(T);
        }

        // constant time hex conversion
        // see http://stackoverflow.com/a/14333437/445517
        //
        // An explanation of the weird bit fiddling:
        //
        // 1. `bytes[i] >> 4` extracts the high nibble of a byte  
        //   `bytes[i] & 0xF` extracts the low nibble of a byte
        // 2. `b - 10`  
        //    is `< 0` for values `b < 10`, which will become a decimal digit  
        //    is `>= 0` for values `b > 10`, which will become a letter from `A` to `F`.
        // 3. Using `i >> 31` on a signed 32 bit integer extracts the sign, thanks to sign extension.
        //    It will be `-1` for `i < 0` and `0` for `i >= 0`.
        // 4. Combining 2) and 3), shows that `(b-10)>>31` will be `0` for letters and `-1` for digits.
        // 5. Looking at the case for letters, the last summand becomes `0`, and `b` is in the range 10 to 15. We want to map it to `A`(65) to `F`(70), which implies adding 55 (`'A'-10`).
        // 6. Looking at the case for digits, we want to adapt the last summand so it maps `b` from the range 0 to 9 to the range `0`(48) to `9`(57). This means it needs to become -7 (`'0' - 55`).  
        // Now we could just multiply with 7. But since -1 is represented by all bits being 1, we can instead use `& -7` since `(0 & -7) == 0` and `(-1 & -7) == -7`.
        //
        // Some further considerations:
        //
        // * I didn't use a second loop variable to index into `c`, since measurement shows that calculating it from `i` is cheaper. 
        // * Using exactly `i < bytes.Length` as upper bound of the loop allows the JITter to eliminate bounds checks on `bytes[i]`, so I chose that variant.
        // * Making `b` an int avoids unnecessary conversions from and to byte.
        public static string ToHexStringUpper(byte[] data)
        {
            if (data == null)
                return null;
            char[] c = new char[data.Length * 2];
            int b;
            for (int i = 0; i < data.Length; i++)
            {
                b = data[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = data[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        // Explanation is similar to ToHexStringUpper
        // constant 55 -> 87 and -7 -> -39 to compensate for the offset 32 between lowercase and uppercase letters
        public static string ToHexStringLower(byte[] data)
        {
            if (data == null)
                return null;
            char[] c = new char[data.Length * 2];
            int b;
            for (int i = 0; i < data.Length; i++)
            {
                b = data[i] >> 4;
                c[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
                b = data[i] & 0xF;
                c[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
            }
            return new string(c);
        }

        public static byte[] FromHexString(string hexString)
        {
            if (hexString == null)
                return null;
            if (hexString.Length % 2 != 0)
                throw new FormatException("The hex string is invalid because it has an odd length");
            byte[] result = new byte[hexString.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return result;
        }

        public static string ToBase64String(byte[] data)
        {
            if (data == null)
                return null;
            return Convert.ToBase64String(data);
        }

        public static byte[] FromBase64String(string s)
        {
            if (s == null)
                return null;
            return Convert.FromBase64String(s);
        }
    }

    public static class Ed25519
    {
        public static readonly int PublicKeySizeInBytes = 32;
        public static readonly int SignatureSizeInBytes = 64;
        public static readonly int ExpandedPrivateKeySizeInBytes = 32 * 2;
        public static readonly int PrivateKeySeedSizeInBytes = 32;
        public static readonly int SharedKeySizeInBytes = 32;

        public static bool Verify(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> publicKey)
        {
            if (signature.Count != SignatureSizeInBytes)
                throw new ArgumentException(string.Format("Signature size must be {0}", SignatureSizeInBytes), "signature.Count");
            if (publicKey.Count != PublicKeySizeInBytes)
                throw new ArgumentException(string.Format("Public key size must be {0}", PublicKeySizeInBytes), "publicKey.Count");
            return Ed25519Operations.crypto_sign_verify(signature.Array, signature.Offset, message.Array, message.Offset, message.Count, publicKey.Array, publicKey.Offset);
        }

        public static bool Verify(byte[] signature, byte[] message, byte[] publicKey)
        {
            if (signature == null)
                throw new ArgumentNullException("signature");
            if (message == null)
                throw new ArgumentNullException("message");
            if (publicKey == null)
                throw new ArgumentNullException("publicKey");
            if (signature.Length != SignatureSizeInBytes)
                throw new ArgumentException(string.Format("Signature size must be {0}", SignatureSizeInBytes), "signature.Length");
            if (publicKey.Length != PublicKeySizeInBytes)
                throw new ArgumentException(string.Format("Public key size must be {0}", PublicKeySizeInBytes), "publicKey.Length");
            return Ed25519Operations.crypto_sign_verify(signature, 0, message, 0, message.Length, publicKey, 0);
        }

        public static void Sign(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> expandedPrivateKey)
        {
            if (signature.Array == null)
                throw new ArgumentNullException("signature.Array");
            if (signature.Count != SignatureSizeInBytes)
                throw new ArgumentException("signature.Count");
            if (expandedPrivateKey.Array == null)
                throw new ArgumentNullException("expandedPrivateKey.Array");
            if (expandedPrivateKey.Count != ExpandedPrivateKeySizeInBytes)
                throw new ArgumentException("expandedPrivateKey.Count");
            if (message.Array == null)
                throw new ArgumentNullException("message.Array");
            Ed25519Operations.crypto_sign2(signature.Array, signature.Offset, message.Array, message.Offset, message.Count, expandedPrivateKey.Array, expandedPrivateKey.Offset);
        }

        public static byte[] Sign(byte[] message, byte[] expandedPrivateKey)
        {
            byte[] signature = new byte[SignatureSizeInBytes];
            Sign(new ArraySegment<byte>(signature), new ArraySegment<byte>(message), new ArraySegment<byte>(expandedPrivateKey));
            return signature;
        }

        public static byte[] PublicKeyFromSeed(byte[] privateKeySeed)
        {
            KeyPairFromSeed(out byte[] publicKey, out byte[] privateKey, privateKeySeed);
            CryptoBytes.Wipe(privateKey);
            return publicKey;
        }

        public static byte[] ExpandedPrivateKeyFromSeed(byte[] privateKeySeed)
        {
            KeyPairFromSeed(out byte[] publicKey, out byte[] privateKey, privateKeySeed);
            CryptoBytes.Wipe(publicKey);
            return privateKey;
        }

        public static void KeyPairFromSeed(out byte[] publicKey, out byte[] expandedPrivateKey, byte[] privateKeySeed)
        {
            if (privateKeySeed == null)
                throw new ArgumentNullException("privateKeySeed");
            if (privateKeySeed.Length != PrivateKeySeedSizeInBytes)
                throw new ArgumentException("privateKeySeed");
            byte[] pk = new byte[PublicKeySizeInBytes];
            byte[] sk = new byte[ExpandedPrivateKeySizeInBytes];
            Ed25519Operations.crypto_sign_keypair(pk, 0, sk, 0, privateKeySeed, 0);
            publicKey = pk;
            expandedPrivateKey = sk;
        }

        public static void KeyPairFromSeed(ArraySegment<byte> publicKey, ArraySegment<byte> expandedPrivateKey, ArraySegment<byte> privateKeySeed)
        {
            if (publicKey.Array == null)
                throw new ArgumentNullException("publicKey.Array");
            if (expandedPrivateKey.Array == null)
                throw new ArgumentNullException("expandedPrivateKey.Array");
            if (privateKeySeed.Array == null)
                throw new ArgumentNullException("privateKeySeed.Array");
            if (publicKey.Count != PublicKeySizeInBytes)
                throw new ArgumentException("publicKey.Count");
            if (expandedPrivateKey.Count != ExpandedPrivateKeySizeInBytes)
                throw new ArgumentException("expandedPrivateKey.Count");
            if (privateKeySeed.Count != PrivateKeySeedSizeInBytes)
                throw new ArgumentException("privateKeySeed.Count");
            Ed25519Operations.crypto_sign_keypair(
                publicKey.Array, publicKey.Offset,
                expandedPrivateKey.Array, expandedPrivateKey.Offset,
                privateKeySeed.Array, privateKeySeed.Offset);
        }

        [Obsolete("Needs more testing")]
        public static byte[] KeyExchange(byte[] publicKey, byte[] privateKey)
        {
            byte[] sharedKey = new byte[SharedKeySizeInBytes];
            KeyExchange(new ArraySegment<byte>(sharedKey), new ArraySegment<byte>(publicKey), new ArraySegment<byte>(privateKey));
            return sharedKey;
        }

        [Obsolete("Needs more testing")]
        public static void KeyExchange(ArraySegment<byte> sharedKey, ArraySegment<byte> publicKey, ArraySegment<byte> privateKey)
        {
            if (sharedKey.Array == null)
                throw new ArgumentNullException("sharedKey.Array");
            if (publicKey.Array == null)
                throw new ArgumentNullException("publicKey.Array");
            if (privateKey.Array == null)
                throw new ArgumentNullException("privateKey");
            if (sharedKey.Count != 32)
                throw new ArgumentException("sharedKey.Count != 32");
            if (publicKey.Count != 32)
                throw new ArgumentException("publicKey.Count != 32");
            if (privateKey.Count != 64)
                throw new ArgumentException("privateKey.Count != 64");
            FieldOperations.fe_frombytes(out FieldElement edwardsY, publicKey.Array, publicKey.Offset);
            FieldOperations.fe_1(out FieldElement edwardsZ);
            MontgomeryCurve25519.EdwardsToMontgomeryX(out FieldElement montgomeryX, ref edwardsY, ref edwardsZ);
            byte[] h = Sha512.Hash(privateKey.Array, privateKey.Offset, 32);//ToDo: Remove alloc
            ScalarOperations.sc_clamp(h, 0);
            MontgomeryOperations.scalarmult(out FieldElement sharedMontgomeryX, h, 0, ref montgomeryX);
            CryptoBytes.Wipe(h);
            FieldOperations.fe_tobytes(sharedKey.Array, sharedKey.Offset, ref sharedMontgomeryX);
            MontgomeryCurve25519.KeyExchangeOutputHashNaCl(sharedKey.Array, sharedKey.Offset);
        }
    }

    // This class is mainly for compatibility with NaCl's Curve25519 implementation
    // If you don't need that compatibility, use Ed25519.KeyExchange
    public static class MontgomeryCurve25519
    {
        public static readonly int PublicKeySizeInBytes = 32;
        public static readonly int PrivateKeySizeInBytes = 32;
        public static readonly int SharedKeySizeInBytes = 32;

        public static byte[] GetPublicKey(byte[] privateKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException("privateKey");
            if (privateKey.Length != PrivateKeySizeInBytes)
                throw new ArgumentException("privateKey.Length must be 32");
            byte[] publicKey = new byte[32];
            GetPublicKey(new ArraySegment<byte>(publicKey), new ArraySegment<byte>(privateKey));
            return publicKey;
        }

        static readonly byte[] _basePoint = new byte[32]
        {
            9, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0 ,0, 0, 0, 0, 0,
            0, 0, 0 ,0, 0, 0, 0, 0,
            0, 0, 0 ,0, 0, 0, 0, 0
        };

        public static void GetPublicKey(ArraySegment<byte> publicKey, ArraySegment<byte> privateKey)
        {
            if (publicKey.Array == null)
                throw new ArgumentNullException("publicKey.Array");
            if (privateKey.Array == null)
                throw new ArgumentNullException("privateKey.Array");
            if (publicKey.Count != PublicKeySizeInBytes)
                throw new ArgumentException("privateKey.Count must be 32");
            if (privateKey.Count != PrivateKeySizeInBytes)
                throw new ArgumentException("privateKey.Count must be 32");

            // hack: abusing publicKey as temporary storage
            // todo: remove hack
            for (int i = 0; i < 32; i++)
            {
                publicKey.Array[publicKey.Offset + i] = privateKey.Array[privateKey.Offset + i];
            }
            ScalarOperations.sc_clamp(publicKey.Array, publicKey.Offset);

            GroupOperations.ge_scalarmult_base(out GroupElementP3 A, publicKey.Array, publicKey.Offset);
            EdwardsToMontgomeryX(out FieldElement publicKeyFE, ref A.Y, ref A.Z);
            FieldOperations.fe_tobytes(publicKey.Array, publicKey.Offset, ref publicKeyFE);
        }

        // hashes like the Curve25519 paper says
        internal static void KeyExchangeOutputHashCurve25519Paper(byte[] sharedKey, int offset)
        {
            //c = Curve25519output
            const UInt32 c0 = 'C' | 'u' << 8 | 'r' << 16 | (UInt32)'v' << 24;
            const UInt32 c1 = 'e' | '2' << 8 | '5' << 16 | (UInt32)'5' << 24;
            const UInt32 c2 = '1' | '9' << 8 | 'o' << 16 | (UInt32)'u' << 24;
            const UInt32 c3 = 't' | 'p' << 8 | 'u' << 16 | (UInt32)'t' << 24;

            Array16<UInt32> salsaState;
            salsaState.x0 = c0;
            salsaState.x1 = ByteIntegerConverter.LoadLittleEndian32(sharedKey, offset + 0);
            salsaState.x2 = 0;
            salsaState.x3 = ByteIntegerConverter.LoadLittleEndian32(sharedKey, offset + 4);
            salsaState.x4 = ByteIntegerConverter.LoadLittleEndian32(sharedKey, offset + 8);
            salsaState.x5 = c1;
            salsaState.x6 = ByteIntegerConverter.LoadLittleEndian32(sharedKey, offset + 12);
            salsaState.x7 = 0;
            salsaState.x8 = 0;
            salsaState.x9 = ByteIntegerConverter.LoadLittleEndian32(sharedKey, offset + 16);
            salsaState.x10 = c2;
            salsaState.x11 = ByteIntegerConverter.LoadLittleEndian32(sharedKey, offset + 20);
            salsaState.x12 = ByteIntegerConverter.LoadLittleEndian32(sharedKey, offset + 24);
            salsaState.x13 = 0;
            salsaState.x14 = ByteIntegerConverter.LoadLittleEndian32(sharedKey, offset + 28);
            salsaState.x15 = c3;
            SalsaCore.Salsa(out salsaState, ref salsaState, 20);

            ByteIntegerConverter.StoreLittleEndian32(sharedKey, offset + 0, salsaState.x0);
            ByteIntegerConverter.StoreLittleEndian32(sharedKey, offset + 4, salsaState.x1);
            ByteIntegerConverter.StoreLittleEndian32(sharedKey, offset + 8, salsaState.x2);
            ByteIntegerConverter.StoreLittleEndian32(sharedKey, offset + 12, salsaState.x3);
            ByteIntegerConverter.StoreLittleEndian32(sharedKey, offset + 16, salsaState.x4);
            ByteIntegerConverter.StoreLittleEndian32(sharedKey, offset + 20, salsaState.x5);
            ByteIntegerConverter.StoreLittleEndian32(sharedKey, offset + 24, salsaState.x6);
            ByteIntegerConverter.StoreLittleEndian32(sharedKey, offset + 28, salsaState.x7);
        }

        private static readonly byte[] _zero16 = new byte[16];

        // hashes like the NaCl paper says instead i.e. HSalsa(x,0)
        internal static void KeyExchangeOutputHashNaCl(byte[] sharedKey, int offset)
        {
            Salsa20.HSalsa20(sharedKey, offset, sharedKey, offset, _zero16, 0);
        }

        public static byte[] KeyExchange(byte[] publicKey, byte[] privateKey)
        {
            byte[] sharedKey = new byte[SharedKeySizeInBytes];
            KeyExchange(new ArraySegment<byte>(sharedKey), new ArraySegment<byte>(publicKey), new ArraySegment<byte>(privateKey));
            return sharedKey;
        }

        public static void KeyExchange(ArraySegment<byte> sharedKey, ArraySegment<byte> publicKey, ArraySegment<byte> privateKey)
        {
            if (sharedKey.Array == null)
                throw new ArgumentNullException("sharedKey.Array");
            if (publicKey.Array == null)
                throw new ArgumentNullException("publicKey.Array");
            if (privateKey.Array == null)
                throw new ArgumentNullException("privateKey");
            if (sharedKey.Count != 32)
                throw new ArgumentException("sharedKey.Count != 32");
            if (publicKey.Count != 32)
                throw new ArgumentException("publicKey.Count != 32");
            if (privateKey.Count != 32)
                throw new ArgumentException("privateKey.Count != 32");
            MontgomeryOperations.scalarmult(sharedKey.Array, sharedKey.Offset, privateKey.Array, privateKey.Offset, publicKey.Array, publicKey.Offset);
            KeyExchangeOutputHashNaCl(sharedKey.Array, sharedKey.Offset);
        }

        internal static void EdwardsToMontgomeryX(out FieldElement montgomeryX, ref FieldElement edwardsY, ref FieldElement edwardsZ)
        {
            FieldOperations.fe_add(out FieldElement tempX, ref edwardsZ, ref edwardsY);
            FieldOperations.fe_sub(out FieldElement tempZ, ref edwardsZ, ref edwardsY);
            FieldOperations.fe_invert(out tempZ, ref tempZ);
            FieldOperations.fe_mul(out montgomeryX, ref tempX, ref tempZ);
        }
    }

    public abstract class OneTimeAuth
    {
        private static readonly Poly1305 _poly1305 = new Poly1305();

        public abstract int KeySizeInBytes { get; }
        public abstract int SignatureSizeInBytes { get; }

        public abstract byte[] Sign(byte[] message, byte[] key);
        public abstract void Sign(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> key);
        public abstract bool Verify(byte[] signature, byte[] message, byte[] key);
        public abstract bool Verify(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> key);

        public static OneTimeAuth Poly1305 { get { return _poly1305; } }
    }

    internal sealed class Poly1305 : OneTimeAuth
    {
        public override int KeySizeInBytes
        {
            get { return 32; }
        }

        public override int SignatureSizeInBytes
        {
            get { return 16; }
        }

        public override byte[] Sign(byte[] message, byte[] key)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (key == null)
                throw new ArgumentNullException("key");
            if (key.Length != 32)
                throw new ArgumentException("Invalid key size", "key");

            byte[] result = new byte[16];
            ByteIntegerConverter.Array8LoadLittleEndian32(out Array8<uint> internalKey, key, 0);
            Poly1305Donna.poly1305_auth(result, 0, message, 0, message.Length, ref internalKey);
            return result;
        }

        public override void Sign(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> key)
        {
            if (signature.Array == null)
                throw new ArgumentNullException("signature.Array");
            if (message.Array == null)
                throw new ArgumentNullException("message.Array");
            if (key.Array == null)
                throw new ArgumentNullException("key.Array");
            if (key.Count != 32)
                throw new ArgumentException("Invalid key size", "key");
            if (signature.Count != 16)
                throw new ArgumentException("Invalid signature size", "signature");

            ByteIntegerConverter.Array8LoadLittleEndian32(out Array8<uint> internalKey, key.Array, key.Offset);
            Poly1305Donna.poly1305_auth(signature.Array, signature.Offset, message.Array, message.Offset, message.Count, ref internalKey);
        }

        public override bool Verify(byte[] signature, byte[] message, byte[] key)
        {
            if (signature == null)
                throw new ArgumentNullException("signature");
            if (message == null)
                throw new ArgumentNullException("message");
            if (key == null)
                throw new ArgumentNullException("key");
            if (signature.Length != 16)
                throw new ArgumentException("Invalid signature size", "signature");
            if (key.Length != 32)
                throw new ArgumentException("Invalid key size", "key");

            byte[] tempBytes = new byte[16];//todo: remove allocation
            ByteIntegerConverter.Array8LoadLittleEndian32(out Array8<uint> internalKey, key, 0);
            Poly1305Donna.poly1305_auth(tempBytes, 0, message, 0, message.Length, ref internalKey);
            return CryptoBytes.ConstantTimeEquals(tempBytes, signature);
        }

        public override bool Verify(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> key)
        {
            if (signature.Array == null)
                throw new ArgumentNullException("signature.Array");
            if (message.Array == null)
                throw new ArgumentNullException("message.Array");
            if (key.Array == null)
                throw new ArgumentNullException("key.Array");
            if (key.Count != 32)
                throw new ArgumentException("Invalid key size", "key");
            if (signature.Count != 16)
                throw new ArgumentException("Invalid signature size", "signature");

            byte[] tempBytes = new byte[16];//todo: remove allocation
            ByteIntegerConverter.Array8LoadLittleEndian32(out Array8<uint> internalKey, key.Array, key.Offset);
            Poly1305Donna.poly1305_auth(tempBytes, 0, message.Array, message.Offset, message.Count, ref internalKey);
            return CryptoBytes.ConstantTimeEquals(new ArraySegment<byte>(tempBytes), signature);
        }
    }

    public class Sha512
    {
        private Array8<UInt64> _state;
        private readonly byte[] _buffer;
        private ulong _totalBytes;
        public const int BlockSize = 128;
        private static readonly byte[] _padding = new byte[] { 0x80 };

        public Sha512()
        {
            _buffer = new byte[BlockSize];//todo: remove allocation
            Init();
        }

        public void Init()
        {
            Sha512Internal.Sha512Init(out _state);
            _totalBytes = 0;
        }

        public void Update(ArraySegment<byte> data)
        {
            if (data.Array == null)
                throw new ArgumentNullException("data.Array");
            Update(data.Array, data.Offset, data.Count);
        }

        public void Update(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (data.Length - offset < count)
                throw new ArgumentException("Requires offset + count <= data.Length");

            Array16<ulong> block;
            int bytesInBuffer = (int)_totalBytes & (BlockSize - 1);
            _totalBytes += (uint)count;

            if (_totalBytes >= ulong.MaxValue / 8)
                throw new InvalidOperationException("Too much data");
            // Fill existing buffer
            if (bytesInBuffer != 0)
            {
                int toCopy = Math.Min(BlockSize - bytesInBuffer, count);
                Buffer.BlockCopy(data, offset, _buffer, bytesInBuffer, toCopy);
                offset += toCopy;
                count -= toCopy;
                bytesInBuffer += toCopy;
                if (bytesInBuffer == BlockSize)
                {
                    ByteIntegerConverter.Array16LoadBigEndian64(out block, _buffer, 0);
                    Sha512Internal.Core(out _state, ref _state, ref block);
                    CryptoBytes.InternalWipe(_buffer, 0, _buffer.Length);
                    bytesInBuffer = 0;
                }
            }
            // Hash complete blocks without copying
            while (count >= BlockSize)
            {
                ByteIntegerConverter.Array16LoadBigEndian64(out block, data, offset);
                Sha512Internal.Core(out _state, ref _state, ref block);
                offset += BlockSize;
                count -= BlockSize;
            }
            // Copy remainder into buffer
            if (count > 0)
            {
                Buffer.BlockCopy(data, offset, _buffer, bytesInBuffer, count);
            }
        }

        public void Finish(ArraySegment<byte> output)
        {
            if (output.Array == null)
                throw new ArgumentNullException("output.Array");
            if (output.Count != 64)
                throw new ArgumentException("output.Count must be 64");

            Update(_padding, 0, _padding.Length);
            ByteIntegerConverter.Array16LoadBigEndian64(out Array16<ulong> block, _buffer, 0);
            CryptoBytes.InternalWipe(_buffer, 0, _buffer.Length);
            int bytesInBuffer = (int)_totalBytes & (BlockSize - 1);
            if (bytesInBuffer > BlockSize - 16)
            {
                Sha512Internal.Core(out _state, ref _state, ref block);
                block = default(Array16<ulong>);
            }
            block.x15 = (_totalBytes - 1) * 8;
            Sha512Internal.Core(out _state, ref _state, ref block);

            ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 0, _state.x0);
            ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 8, _state.x1);
            ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 16, _state.x2);
            ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 24, _state.x3);
            ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 32, _state.x4);
            ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 40, _state.x5);
            ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 48, _state.x6);
            ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 56, _state.x7);
            _state = default(Array8<ulong>);
        }

        public byte[] Finish()
        {
            byte[] result = new byte[64];
            Finish(new ArraySegment<byte>(result));
            return result;
        }

        public static byte[] Hash(byte[] data)
        {
            return Hash(data, 0, data.Length);
        }

        public static byte[] Hash(byte[] data, int offset, int count)
        {
            Sha512 hasher = new Sha512();
            hasher.Update(data, offset, count);
            return hasher.Finish();
        }
    }

    public static class XSalsa20Poly1305
    {
        public static readonly int KeySizeInBytes = 32;
        public static readonly int NonceSizeInBytes = 24;
        public static readonly int MacSizeInBytes = 16;

        public static byte[] Encrypt(byte[] message, byte[] key, byte[] nonce)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (key == null)
                throw new ArgumentNullException("key");
            if (nonce == null)
                throw new ArgumentNullException("nonce");
            if (key.Length != KeySizeInBytes)
                throw new ArgumentException("key.Length != 32");
            if (nonce.Length != NonceSizeInBytes)
                throw new ArgumentException("nonce.Length != 24");

            byte[] ciphertext = new byte[message.Length + MacSizeInBytes];
            EncryptInternal(ciphertext, 0, message, 0, message.Length, key, 0, nonce, 0);
            return ciphertext;
        }

        public static void Encrypt(ArraySegment<byte> ciphertext, ArraySegment<byte> message, ArraySegment<byte> key, ArraySegment<byte> nonce)
        {
            if (key.Count != KeySizeInBytes)
                throw new ArgumentException("key.Length != 32");
            if (nonce.Count != NonceSizeInBytes)
                throw new ArgumentException("nonce.Length != 24");
            if (ciphertext.Count != message.Count + MacSizeInBytes)
                throw new ArgumentException("ciphertext.Count != message.Count + 16");
            EncryptInternal(ciphertext.Array, ciphertext.Offset, message.Array, message.Offset, message.Count, key.Array, key.Offset, nonce.Array, nonce.Offset);
        }

        /// <summary>
        /// Decrypts the ciphertext and verifies its authenticity
        /// </summary>
        /// <returns>Plaintext if MAC validation succeeds, null if the data is invalid.</returns>
        public static byte[] TryDecrypt(byte[] ciphertext, byte[] key, byte[] nonce)
        {
            if (ciphertext == null)
                throw new ArgumentNullException("ciphertext");
            if (key == null)
                throw new ArgumentNullException("key");
            if (nonce == null)
                throw new ArgumentNullException("nonce");
            if (key.Length != KeySizeInBytes)
                throw new ArgumentException("key.Length != 32");
            if (nonce.Length != NonceSizeInBytes)
                throw new ArgumentException("nonce.Length != 24");

            if (ciphertext.Length < MacSizeInBytes)
                return null;
            byte[] plaintext = new byte[ciphertext.Length - MacSizeInBytes];
            bool success = DecryptInternal(plaintext, 0, ciphertext, 0, ciphertext.Length, key, 0, nonce, 0);
            if (success)
                return plaintext;
            else
                return null;
        }

        /// <summary>
        /// Decrypts the ciphertext and verifies its authenticity
        /// </summary>
        /// <param name="message">Plaintext if authentication succeeded, all zero if authentication failed, unmodified if argument verification fails</param>
        /// <param name="ciphertext"></param>
        /// <param name="key">Symmetric key. Must be identical to key specified for encryption.</param>
        /// <param name="nonce">Must be identical to nonce specified for encryption.</param>
        /// <returns>true if ciphertext is authentic, false otherwise</returns>
        public static bool TryDecrypt(ArraySegment<byte> message, ArraySegment<byte> ciphertext, ArraySegment<byte> key, ArraySegment<byte> nonce)
        {
            if (key.Count != KeySizeInBytes)
                throw new ArgumentException("key.Length != 32");
            if (nonce.Count != NonceSizeInBytes)
                throw new ArgumentException("nonce.Length != 24");
            if (ciphertext.Count != message.Count + MacSizeInBytes)
                throw new ArgumentException("ciphertext.Count != message.Count + 16");

            return DecryptInternal(message.Array, message.Offset, ciphertext.Array, ciphertext.Offset, ciphertext.Count, key.Array, key.Offset, nonce.Array, nonce.Offset);
        }

        private static void PrepareInternalKey(out Array16<UInt32> internalKey, byte[] key, int keyOffset, byte[] nonce, int nonceOffset)
        {
            internalKey.x0 = Salsa20.SalsaConst0;
            internalKey.x1 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 0);
            internalKey.x2 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 4);
            internalKey.x3 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 8);
            internalKey.x4 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 12);
            internalKey.x5 = Salsa20.SalsaConst1;
            internalKey.x6 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 0);
            internalKey.x7 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 4);
            internalKey.x8 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 8);
            internalKey.x9 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 12);
            internalKey.x10 = Salsa20.SalsaConst2;
            internalKey.x11 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 16);
            internalKey.x12 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 20);
            internalKey.x13 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 24);
            internalKey.x14 = ByteIntegerConverter.LoadLittleEndian32(key, keyOffset + 28);
            internalKey.x15 = Salsa20.SalsaConst3;
            SalsaCore.HSalsa(out internalKey, ref internalKey, 20);

            //key
            internalKey.x1 = internalKey.x0;
            internalKey.x2 = internalKey.x5;
            internalKey.x3 = internalKey.x10;
            internalKey.x4 = internalKey.x15;
            internalKey.x11 = internalKey.x6;
            internalKey.x12 = internalKey.x7;
            internalKey.x13 = internalKey.x8;
            internalKey.x14 = internalKey.x9;
            //const
            internalKey.x0 = Salsa20.SalsaConst0;
            internalKey.x5 = Salsa20.SalsaConst1;
            internalKey.x10 = Salsa20.SalsaConst2;
            internalKey.x15 = Salsa20.SalsaConst3;
            //nonce
            internalKey.x6 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 16);
            internalKey.x7 = ByteIntegerConverter.LoadLittleEndian32(nonce, nonceOffset + 20);
            //offset
            internalKey.x8 = 0;
            internalKey.x9 = 0;
        }

        private static bool DecryptInternal(byte[] plaintext, int plaintextOffset, byte[] ciphertext, int ciphertextOffset, int ciphertextLength, byte[] key, int keyOffset, byte[] nonce, int nonceOffset)
        {
            int plaintextLength = ciphertextLength - MacSizeInBytes;
            PrepareInternalKey(out Array16<uint> internalKey, key, keyOffset, nonce, nonceOffset);

            Array16<UInt32> temp;
            byte[] tempBytes = new byte[64];//todo: remove allocation

            // first iteration
            {
                SalsaCore.Salsa(out temp, ref internalKey, 20);

                //first half is for Poly1305
                Array8<UInt32> poly1305Key;
                poly1305Key.x0 = temp.x0;
                poly1305Key.x1 = temp.x1;
                poly1305Key.x2 = temp.x2;
                poly1305Key.x3 = temp.x3;
                poly1305Key.x4 = temp.x4;
                poly1305Key.x5 = temp.x5;
                poly1305Key.x6 = temp.x6;
                poly1305Key.x7 = temp.x7;

                // compute MAC
                Poly1305Donna.poly1305_auth(tempBytes, 0, ciphertext, ciphertextOffset + 16, plaintextLength, ref poly1305Key);
                if (!CryptoBytes.ConstantTimeEquals(tempBytes, 0, ciphertext, ciphertextOffset, MacSizeInBytes))
                {
                    Array.Clear(plaintext, plaintextOffset, plaintextLength);
                    return false;
                }

                // rest for the message
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 0, temp.x8);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 4, temp.x9);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 8, temp.x10);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 12, temp.x11);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 16, temp.x12);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 20, temp.x13);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 24, temp.x14);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 28, temp.x15);
                int count = Math.Min(32, plaintextLength);
                for (int i = 0; i < count; i++)
                    plaintext[plaintextOffset + i] = (byte)(ciphertext[MacSizeInBytes + ciphertextOffset + i] ^ tempBytes[i]);
            }

            // later iterations
            int blockOffset = 32;
            while (blockOffset < plaintextLength)
            {
                internalKey.x8++;
                SalsaCore.Salsa(out temp, ref internalKey, 20);
                ByteIntegerConverter.Array16StoreLittleEndian32(tempBytes, 0, ref temp);
                int count = Math.Min(64, plaintextLength - blockOffset);
                for (int i = 0; i < count; i++)
                    plaintext[plaintextOffset + blockOffset + i] = (byte)(ciphertext[16 + ciphertextOffset + blockOffset + i] ^ tempBytes[i]);
                blockOffset += 64;
            }
            return true;
        }

        private static void EncryptInternal(byte[] ciphertext, int ciphertextOffset, byte[] message, int messageOffset, int messageLength, byte[] key, int keyOffset, byte[] nonce, int nonceOffset)
        {
            PrepareInternalKey(out Array16<uint> internalKey, key, keyOffset, nonce, nonceOffset);

            Array16<UInt32> temp;
            byte[] tempBytes = new byte[64];//todo: remove allocation
            Array8<UInt32> poly1305Key;

            // first iteration
            {
                SalsaCore.Salsa(out temp, ref internalKey, 20);

                //first half is for Poly1305
                poly1305Key.x0 = temp.x0;
                poly1305Key.x1 = temp.x1;
                poly1305Key.x2 = temp.x2;
                poly1305Key.x3 = temp.x3;
                poly1305Key.x4 = temp.x4;
                poly1305Key.x5 = temp.x5;
                poly1305Key.x6 = temp.x6;
                poly1305Key.x7 = temp.x7;

                // second half for the message
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 0, temp.x8);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 4, temp.x9);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 8, temp.x10);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 12, temp.x11);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 16, temp.x12);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 20, temp.x13);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 24, temp.x14);
                ByteIntegerConverter.StoreLittleEndian32(tempBytes, 28, temp.x15);
                int count = Math.Min(32, messageLength);
                for (int i = 0; i < count; i++)
                    ciphertext[16 + ciphertextOffset + i] = (byte)(message[messageOffset + i] ^ tempBytes[i]);
            }

            // later iterations
            int blockOffset = 32;
            while (blockOffset < messageLength)
            {
                internalKey.x8++;
                SalsaCore.Salsa(out temp, ref internalKey, 20);
                ByteIntegerConverter.Array16StoreLittleEndian32(tempBytes, 0, ref temp);
                int count = Math.Min(64, messageLength - blockOffset);
                for (int i = 0; i < count; i++)
                    ciphertext[16 + ciphertextOffset + blockOffset + i] = (byte)(message[messageOffset + blockOffset + i] ^ tempBytes[i]);
                blockOffset += 64;
            }

            // compute MAC
            Poly1305Donna.poly1305_auth(ciphertext, ciphertextOffset, ciphertext, ciphertextOffset + 16, messageLength, ref poly1305Key);
        }
    }
}
