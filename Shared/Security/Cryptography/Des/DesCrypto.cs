using Shared.Security.Cryptography.Des;
using Shared.Utils;

namespace Shared.Security.Cryptography.Des;

public static class DesCrypto
{
    public static byte[]? Encrypt(byte[]? textBytes, byte[] key)
    {
        //Todo: padding
        int blockSize = 8;
        int padLength = blockSize - (textBytes.Length % blockSize);
        byte[]? paddedData = new byte[textBytes.Length + padLength];
        Array.Copy(textBytes, paddedData, textBytes.Length);
        
        // Gán giá trị padding
        for (int i = textBytes.Length; i < paddedData.Length; i++)
        {
            paddedData[i] = (byte)padLength;
        }
        textBytes = paddedData;
        //Todo: Encrypt
        byte[] holderL = new byte[4];
        byte[] holderR = new byte[4];
        byte[] tmp = new byte[8];
        //expanding bitarrays to fit (56 bits for key,64 bit blocks for plaintext
        if ((textBytes.Length & 7) != 0)
        {
            byte[]? temp = new byte[textBytes.Length + textBytes.Length % 8];
            textBytes.CopyTo(temp, 0);
            textBytes = temp;
        }
    
        int blockcount = textBytes.Length / 8;
    
        byte[][] subkeys = GenerateSubKeys(key);
    
        for (int stage = 1; stage <= 16; stage++)
        {
            //get subkey
            var keybytes = subkeys[stage - 1];
    
            // Split the block into halves
            for (int blocknum = 0; blocknum < blockcount; blocknum++)
            {
                if (stage == 1)
                {
                    Array.Copy(textBytes, blocknum * 8, tmp, 0, 8);
                    tmp = UseTable(tmp, CryptoConst.GetConst(Permutation.Ip));
                    Array.Copy(tmp, 0, textBytes, blocknum * 8, 8);
                }
    
                // Load the block
                for (int pointer = 0; pointer < 4; pointer++)
                {
                    holderL[pointer] = textBytes[pointer + blocknum * 8];
                    holderR[pointer] = textBytes[pointer + blocknum * 8 + 4];
                }
    
                byte[] buffholder = holderR;
                //Function F
                holderR = UseTable(holderR, CryptoConst.GetConst(Permutation.F));
                holderR = ByteUtils.XORBytes(holderR, keybytes);
                holderR = SBox(holderR);
                holderR = UseTable(holderR, CryptoConst.GetConst(Permutation.P));
                //End F
    
                //XOR operation
                holderR = ByteUtils.XORBytes(holderR, holderL);
    
                holderL = buffholder;
    
                // Store block
                for (int pointer = 0; pointer < 4; pointer++)
                {
                    textBytes[pointer + blocknum * 8] = holderL[pointer];
                    textBytes[pointer + blocknum * 8 + 4] = holderR[pointer];
                    if (stage == 16)
                    {
                        textBytes[pointer + blocknum * 8] = holderR[pointer];
                        textBytes[pointer + blocknum * 8 + 4] = holderL[pointer];
                    }
                }
    
                if (stage == 16)
                {
                    Array.Copy(textBytes, blocknum * 8, tmp, 0, 8);
                    tmp = UseTable(tmp, CryptoConst.GetConst(Permutation.IpInverse));
                    Array.Copy(tmp, 0, textBytes, blocknum * 8, 8);
                }
            }
        }
    
    
        return textBytes;
    }
    
    public static byte[]? Decrypt(byte[]? textBytes, byte[] key)
    {
        //Todo: Decrypt
        byte[] holderL = new byte[4];
        byte[] holderR = new byte[4];
        byte[] tmp = new byte[8];
        // Expanding bit arrays to fit (56 bits for the key, 64-bit blocks for plaintext)
        if ((textBytes.Length & 7) != 0)
        {
            byte[]? temp = new byte[textBytes.Length + textBytes.Length % 8];
            textBytes.CopyTo(temp, 0);
            textBytes = temp;
        }
    
        int blockcount = textBytes.Length / 8;
    
        byte[][] subkeys = GenerateSubKeys(key);
    
        for (int stage = 1; stage <= 16; stage++)
        {
            //generate subkey
            var keybytes = subkeys[16 - stage];
    
            // Split the block into halves
            for (int blocknum = 0; blocknum < blockcount; blocknum++)
            {
                if (stage == 1)
                {
                    Array.Copy(textBytes, blocknum * 8, tmp, 0, 8);
                    tmp = UseTable(tmp, CryptoConst.GetConst(Permutation.Ip));
                    Array.Copy(tmp, 0, textBytes, blocknum * 8, 8);
                }
    
                for (int pointer = 0; pointer < 4; pointer++)
                {
                    holderL[pointer] = textBytes[pointer + blocknum * 8];
                    holderR[pointer] = textBytes[pointer + blocknum * 8 + 4];
                }
    
    
                byte[] buffholder = holderR;
                //Function F
                holderR = UseTable(holderR, CryptoConst.GetConst(Permutation.F));
                holderR = ByteUtils.XORBytes(holderR, keybytes);
                holderR = SBox(holderR);
                holderR = UseTable(holderR, CryptoConst.GetConst(Permutation.P));
                //End F
    
                //xorowanie
                holderR = ByteUtils.XORBytes(holderR, holderL);
    
                holderL = buffholder;
    
                //XOR operation
                for (int pointer = 0; pointer < 4; pointer++)
                {
                    textBytes[pointer + blocknum * 8] = holderL[pointer];
                    textBytes[pointer + blocknum * 8 + 4] = holderR[pointer];
                    if (stage == 16)
                    {
                        textBytes[pointer + blocknum * 8] = holderR[pointer];
                        textBytes[pointer + blocknum * 8 + 4] = holderL[pointer];
                    }
                }
    
                if (stage == 16)
                {
                    Array.Copy(textBytes, blocknum * 8, tmp, 0, 8);
                    tmp = UseTable(tmp, CryptoConst.GetConst(Permutation.IpInverse));
                    Array.Copy(tmp, 0, textBytes, blocknum * 8, 8);
                }
            }
        }
        
        
        //Todo: Remove padding
        // Loại bỏ padding PKCS7
        if (textBytes.Length == 0)
            return textBytes;

        int padLength = textBytes[textBytes.Length - 1];

        // Kiểm tra tính hợp lệ của padding
        if (padLength < 1 || padLength > 8 || padLength > textBytes.Length)
        {
            throw new Exception("Padding không hợp lệ.");
        }

        // Kiểm tra tất cả byte padding có giá trị padLength
        for (int i = textBytes.Length - padLength; i < textBytes.Length; i++)
        {
            if (textBytes[i] != padLength)
            {
                throw new Exception("Padding không đúng chuẩn PKCS7.");
            }
        }

        // Loại bỏ padding
        byte[]? result = new byte[textBytes.Length - padLength];
        Array.Copy(textBytes, result, result.Length);
        return result;
        //return textBytes;
    }
    
    private static byte[] UseTable(byte[] arr, int[] table)
    {
        int len = table.Length;
        int bytenum = (len - 1) / 8 + 1;
        byte[] output = new byte[bytenum];
        for (int i = 0; i < len; i++)
        {
            int val = ByteUtils.GetBitAt(arr, table[i] - 1);
            ByteUtils.SetBitAt(output, i, val);
        }
    
        return output;
    }
    
    private static byte[] GlueKey(byte[] arrC, byte[] arrD)
    {
        byte[] result = new byte[7];
        for (int i = 0; i < 3; i++)
        {
            result[i] = arrC[i];
        }
    
        for (int i = 0; i < 4; i++)
        {
            int val = ByteUtils.GetBitAt(arrC, 24 + i); //24-27
            ByteUtils.SetBitAt(result, 24 + i, val);
        }
    
        for (int i = 0; i < 28; i++)
        {
            int val = ByteUtils.GetBitAt(arrD, i);
            ByteUtils.SetBitAt(result, 28 + i, val);
        }
    
        return result;
    }
    
    private static byte[] SBox(byte[] input)
    {
        byte[] output = new byte[4];
        int row;
        int col;
        byte halfofbyte;
        // Input = 48 bits (expanded E after XOR with the key)
        // Output = 32 bits after S-box substitution
        // 48/6 = 8 sections to process
        // 1. Set row and col bytes (from each 6 bits of input)
        // 2. Convert row and col to integer values
        // 3. Use sboxpicker to get the values
        // 4. Pack the value into 4 bits (section*4) so that after this, the entire input is reduced
        // 5. Use 2 variables to create a complete bit from the 2 returned values
        for (int section = 0; section < 8; section++)
        {
            row = ByteUtils.GetBitAt(input, section * 6) << 1;
            row += ByteUtils.GetBitAt(input, section * 6 + 5);
            col = 0;
            for (int colbit = 0; colbit < 4; colbit++)
            {
                col += ByteUtils.GetBitAt(input, section * 6 + colbit + 1) << 3 - colbit;
            }
    
            // Check if the number is even, i.e., if the first bit is 0 (if it's odd, it MUST be 1)
            halfofbyte =
                (byte)SBoxPicker(col, row,
                    section); //4 bits (or if I shift by 4 during the next insertion, will it be correct?)
            if ((section & 1) == 0)
            {
                output[section / 2] += (byte)(halfofbyte << 4);
            }
            else
            {
                output[section / 2] += halfofbyte;
            }
        }
    
        return output;
    }
    
    private static int SBoxPicker(int col, int row, int index) => CryptoConst.GetSBoxConst(index)[row, col];
    private static byte[][] GenerateSubKeys(byte[] keyBytes)
    {
        var subkeyarray = new byte[16][];
        keyBytes = UseTable(keyBytes,
            CryptoConst.GetConst(Permutation.Pc1)); // This cuts 56 bits and shortens keybytes, so there is no need to trim it earlier.
    
        var keybytesC = ByteUtils.SelectBits(keyBytes, 0, 28);
        var keybytesD = ByteUtils.SelectBits(keyBytes, 28, 28);
        for (int i = 1; i <= 16; i++)
        {
            if (i == 1 || i == 2 || i == 9 || i == 16)
            {
                keybytesC = ByteUtils.RotateLeft(keybytesC, 28, 1);
                keybytesD = ByteUtils.RotateLeft(keybytesD, 28, 1);
    
                keyBytes = UseTable(GlueKey(keybytesC, keybytesD), CryptoConst.GetConst(Permutation.Pc2));
                subkeyarray[i - 1] = keyBytes;
            }
            else
            {
                keybytesC = ByteUtils.RotateLeft(keybytesC, 28, 2);
                keybytesD = ByteUtils.RotateLeft(keybytesD, 28, 2);
    
                keyBytes = UseTable(GlueKey(keybytesC, keybytesD), CryptoConst.GetConst(Permutation.Pc2));
                subkeyarray[i - 1] = keyBytes;
            }
        }
    
        return subkeyarray;
    }
    
}