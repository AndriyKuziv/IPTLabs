using IPTLab2.Data;
using IPTLab2.FileWorks;
using IPTLab2.Menus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2.Algorithms
{
    public class RCFive
    {
        private readonly int wordSize = 32;
        private readonly int rounds = 20;
        private readonly int keySize = 16;

        private readonly int blockSize = 8;

        private uint[] S;

        public RCFive(string password, int wordSize, int rounds, int keySize)
        {
            byte[] key = Encoding.UTF8.GetBytes(password);

            if (key.Length != keySize) key = key[..keySize];

            Setup(key);

            this.wordSize = wordSize;
            this.rounds = rounds;
            this.keySize = keySize;
            this.blockSize = wordSize / 8 * 2;
        }

        private void Setup(byte[] key)
        {
            int c = key.Length / (wordSize / 8);
            int t = 2 * (rounds + 1);

            S = new uint[t];

            uint Pw = 0xb7e15163;
            uint Qw = 0x9e3779b9;

            S[0] = Pw;
            for (int kk = 1; kk < t; kk++)
            {
                S[kk] = S[kk - 1] + Qw;
            }

            int iA = 0;
            int iB = 0;
            uint[] L = new uint[c * 3];
            for (int k = 0; k < c * 3; k++)
            {
                uint A = BitConverter.ToUInt32(key, iA);
                uint B = BitConverter.ToUInt32(key, iB);

                L[k] = A + B;
                iA = (iA + 4) % key.Length;
                iB = (iB + 4) % key.Length;
            }

            int i = 0, j = 0;
            for (int k = 0; k < 3 * Math.Max(t, c); k++)
            {
                S[i] = ROTL((S[i] + L[j]), 3);
                i = (i + 1) % t;
                j = (j + 1) % c;
            }
        }

        private uint ROTL(uint val, int shift)
        {
            return (val << shift) | (val >> (wordSize - shift));
        }

        private uint ROTR(uint val, int shift)
        {
            return (val >> shift) | (val << (wordSize - shift));
        }

        public byte[] EncryptFile(string filename)
        {
            var pt = FileWorksCrypto.ReadFile(filename);
            var genPars = FileWorksRandGen.ReadConfig(RandGenMenu.configFilePath);

            return ExecuteEncryption(pt, genPars);
        }

        public byte[] ExecuteEncryption(byte[] pt, GeneratorParams genPars)
        {
            int paddedLength = (pt.Length % blockSize == 0) ? pt.Length :
                pt.Length + (blockSize - (pt.Length % blockSize));

            byte[] paddedData = new byte[paddedLength];
            Array.Copy(pt, paddedData, pt.Length);

            byte[] iv = GetIV(genPars, blockSize);

            byte[] encryptedData = new byte[paddedLength + blockSize];
            iv.CopyTo(encryptedData, 0);

            byte[] prevBlock = new byte[blockSize];
            iv.CopyTo(prevBlock, 0);

            for (int i = 0; i < paddedLength; i += blockSize)
            {
                byte[] currBlock = new byte[blockSize];
                Array.Copy(paddedData, i, currBlock, 0, blockSize);

                for (int j = 0; j < 8; j++)
                {
                    currBlock[j] ^= prevBlock[j];
                }

                byte[] encryptedBlock = Encrypt(currBlock);
                encryptedBlock.CopyTo(prevBlock, 0);


                encryptedBlock.CopyTo(encryptedData, i + blockSize);
            }

            return encryptedData;
        }

        private byte[] Encrypt(byte[] block)
        {
            uint A = BitConverter.ToUInt32(block, 0);
            uint B = BitConverter.ToUInt32(block, 4);

            A += S[0];
            B += S[1];

            for (int i = 1; i <= rounds; i++)
            {
                A = ROTL(A ^ B, (int)B) + S[2 * i];
                B = ROTL(B ^ A, (int)A) + S[2 * i + 1];
            }

            byte[] encryptedBlock = new byte[blockSize];
            BitConverter.GetBytes(A).CopyTo(encryptedBlock, 0);
            BitConverter.GetBytes(B).CopyTo(encryptedBlock, 4);

            return encryptedBlock;
        }

        public byte[] DecryptFile(string filename)
        {
            var ct = FileWorksCrypto.ReadFile(filename);

            return ExecuteDecryption(ct);
        }

        public byte[] ExecuteDecryption(byte[] ct)
        {
            byte[] iv = new byte[blockSize];
            Array.Copy(ct, 0, iv, 0, blockSize);

            byte[] decryptedData = new byte[ct.Length - blockSize];
            byte[] prevBlock = iv;

            for (int i = blockSize; i < ct.Length; i += blockSize)
            {
                byte[] currBlock = new byte[blockSize];
                Array.Copy(ct, i, currBlock, 0, blockSize);

                byte[] decryptedBlock = Decrypt(currBlock);

                for (int j = 0; j < blockSize; j++)
                {
                    decryptedBlock[j] ^= prevBlock[j];
                }

                currBlock.CopyTo(prevBlock, 0);

                decryptedBlock.CopyTo(decryptedData, i - blockSize);
            }

            return decryptedData;
        }

        private byte[] Decrypt(byte[] block)
        {
            uint A = BitConverter.ToUInt32(block, 0);
            uint B = BitConverter.ToUInt32(block, 4);

            for (int i = rounds; i > 0; i--)
            {
                B = ROTR(B - S[2 * i + 1], (int)A) ^ A;
                A = ROTR(A - S[2 * i], (int)B) ^ B;
            }

            B -= S[1];
            A -= S[0];

            byte[] decryptedBlock = new byte[blockSize];
            BitConverter.GetBytes(A).CopyTo(decryptedBlock, 0);
            BitConverter.GetBytes(B).CopyTo(decryptedBlock, 4);

            return decryptedBlock;
        }

        private byte[] GetIV(GeneratorParams pars, long length)
        {
            return RandGen.GenerBytes(pars, length);
        }
    }
}
