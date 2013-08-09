using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CSharpHelpers
{
    /// <summary>
    /// Encrypts/Decrypts strings and byte arrays. Original posting found here: http://stackoverflow.com/questions/165808/simple-2-way-encryption-for-c-sharp
    /// </summary>
    public static class SimpleAES
    {
        private static readonly byte[] Key = {203, 62, 119, 126, 89, 253, 33, 166, 30, 89, 197, 198, 187, 12, 181, 225, 80, 32, 90, 162, 195, 226, 71, 42, 124, 213, 130, 51, 127, 54, 191, 212};
        private static readonly byte[] Vector = {57, 36, 170, 120, 13, 42, 14, 79, 237, 121, 24, 68, 166, 0, 101, 111};
        private static readonly ICryptoTransform Encryptor, Decryptor;
        private static UTF8Encoding encoder;

        static SimpleAES()
        {
            var rm = new RijndaelManaged();
            Encryptor = rm.CreateEncryptor(Key, Vector);
            Decryptor = rm.CreateDecryptor(Key, Vector);
            encoder = new UTF8Encoding();
        }

        /// <summary>
        /// Generates Encryption Key
        /// </summary>
        /// <returns></returns>
        static public byte[] GenerateEncryptionKey()
        {
            //Generate a Key.
            var rm = new RijndaelManaged();
            rm.GenerateKey();
            return rm.Key;
        }

        /// <summary>
        /// Generates Encryption Vector
        /// </summary>
        /// <returns></returns>
        static public byte[] GenerateEncryptionVector()
        {
            //Generate a Vector
            var rm = new RijndaelManaged();
            rm.GenerateIV();
            return rm.IV;
        }

        /// <summary>
        /// Encrypts a string.
        /// </summary>
        /// <param name="unencrypted"></param>
        /// <returns>Encrypted string</returns>
        public static string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
        }

        /// <summary>
        /// Decrypts a string
        /// </summary>
        /// <param name="encrypted"></param>
        /// <returns>Decrypted string</returns>
        public static string Decrypt(string encrypted)
        {
            return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        /// <summary>
        /// Encrypts a byte stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, Encryptor);
        }

        /// <summary>
        /// Decrypts a byte stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] buffer)
        {
            return Transform(buffer, Decryptor);
        }

        /// <summary>
        /// Encrypts/Decrypts byte stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            var stream = new MemoryStream();
            using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
    }
}
