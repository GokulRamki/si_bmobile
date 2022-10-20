using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Summary description for encryption
/// </summary>

    public class AesEncryptor : Aes
    {
        private Aes aes;

        public AesEncryptor()
        {
            aes = Create();
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return aes.CreateEncryptor(rgbKey, rgbIV);
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return aes.CreateDecryptor(rgbKey, rgbIV);
        }

        public override void GenerateKey()
        {
            aes.GenerateKey();
        }

        public override void GenerateIV()
        {
            aes.GenerateIV();
        }
    }



   