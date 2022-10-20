using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Configuration;
using System.Text;


    public class TripleDESImplementation
    {
        //Encryption Key
        private byte[] EncryptionKey { get; set; }
        // The Initialization Vector for the DES encryption routine
        private byte[] IV { get; set; }

        /// <summary>
        /// Constructor for TripleDESImplementation class
        /// </summary>
        /// <param name="encryptionKey">The 24-byte encryption key (24 character ASCII)</param>
        /// <param name="IV">The 8-byte DES encryption initialization vector (8 characters ASCII)</param>
        public TripleDESImplementation()
        {
            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
            string encryptionKey = (string)settingsReader.GetValue("3des_encKey", typeof(String));
            string IV = (string)settingsReader.GetValue("3des_iv", typeof(String));

            if (string.IsNullOrEmpty(encryptionKey))
            {
                throw new ArgumentNullException("'encryptionKey' parameter cannot be null.", "encryptionKey");
            }
            if (string.IsNullOrEmpty(IV))
            {
                throw new ArgumentException("'IV' parameter cannot be null or empty.", "IV");
            }

            EncryptionKey = Encoding.ASCII.GetBytes(encryptionKey);
            // Ensures length of 24 for encryption key
            Trace.Assert(EncryptionKey.Length == 24, "Encryption key must be exactly 24 characters of ASCII text (24 bytes)");

            this.IV = Encoding.ASCII.GetBytes(IV);
            // Ensures length of 8 for init. vector
            Trace.Assert(IV.Length == 8, "Init. vector must be exactly 8 characters of ASCII text (8 bytes)");
        }

        /// <summary>
        /// Encrypts a text block
        /// </summary>
        public string Encrypt(string textToEncrypt)
        {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = EncryptionKey;
            tdes.IV = IV;

            byte[] buffer = Encoding.ASCII.GetBytes(textToEncrypt);
            return Convert.ToBase64String(tdes.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
        }

        /// <summary>
        /// Decrypts an encrypted text block
        /// </summary>
        public string Decrypt(string textToDecrypt)
        {
            byte[] buffer = Convert.FromBase64String(textToDecrypt);

            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.Key = EncryptionKey;
            des.IV = IV;

            return Encoding.ASCII.GetString(des.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length));
        }
    }

