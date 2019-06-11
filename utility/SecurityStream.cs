using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DialogMaker
{
    public class SecurityStream : System.IO.Stream
    {
        static Rijndael rijndael = new RijndaelManaged();

        System.IO.Stream baseStream;

        private static readonly byte[] SALT = new byte[] { 0x20, 0xaf, 0xff, 0x10, 0xad, 0xed, 0x8a, 0xee, 0xae, 0x6c, 0x07, 0xae, 0x4d, 0x18, 0x22, 0xfc };

        public static byte[] Encrypt(byte[] plain, int offset, int len)
        {
            byte[] aout = new byte[len];

            ICryptoTransform icte = rijndael.CreateEncryptor();


            for (int i = offset; i < len; i += 4096)
            {
                int rt = len - i;
                if (rt > 4096) rt = 4096;

                byte[] zeroblock = icte.TransformFinalBlock(plain, i, 4096);

                Array.Copy(zeroblock, 0, aout, i, rt);
            }

            return aout;
        }
        public static void Decrypt(byte[] aout, byte[] cipher, int offset, int len)
        {
            ICryptoTransform ictd = rijndael.CreateDecryptor();

            for (int i = offset; i < len; i += 4096)
            {
                int rt = len - i;
                if (rt > 4096) rt = 4096;
                byte[] zeroblock = ictd.TransformFinalBlock(cipher, i, 4096);
                Array.Copy(zeroblock, 0, aout, i, rt);
            }
        }

        public SecurityStream(System.IO.Stream operableStream, string password = "release")
        {

            baseStream = operableStream;
            rijndael.Mode = CipherMode.CFB;
            rijndael.Padding = PaddingMode.None;
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, SALT);
            rijndael.Key = pdb.GetBytes(32);
            rijndael.IV = pdb.GetBytes(16);
            //
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] r = new byte[count - offset];
            int readed = baseStream.Read(r, offset, count);
            Decrypt(buffer, r, 0, readed);

            return readed;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            buffer = Encrypt(buffer, offset, count);
            baseStream.Write(buffer, 0, buffer.Length);
        }
        public override bool CanRead
        {
            get { return baseStream.CanRead; }
        }
        public override bool CanSeek
        {
            get { return baseStream.CanSeek; }
        }
        public override bool CanTimeout
        {
            get
            {
                return base.CanTimeout;
            }
        }
        public override long Length
        {
            get { return baseStream.Length; }
        }
        public override bool CanWrite
        {
            get { return baseStream.CanWrite; }
        }
        public override long Position
        {
            get
            {
                return baseStream.Position;
            }
            set
            {
                baseStream.Position = value;
            }
        }
        public override void Flush()
        {
            baseStream.Flush();
        }
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {

            return 0;

            //return baseStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();

            //baseStream.SetLength(value);
        }
        public override void Close()
        {
            baseStream.Close();
        }
    }
}
