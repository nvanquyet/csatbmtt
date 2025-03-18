using System.Text;
using Shared.AppSettings;
using Shared.Security.Interface;
using Shared.Utils;

namespace Shared.Security.Algorithm;

public class DesAlgorithm : IEncryptionAlgorithm
{
    
    #region  Core Algorithm
    
    private static readonly int[] PC1 = new int[]
    {
        57, 49, 41, 33, 25, 17, 9,
        1, 58, 50, 42, 34, 26, 18,
        10, 2, 59, 51, 43, 35, 27,
        19, 11, 3, 60, 52, 44, 36,

        63, 55, 47, 39, 31, 23, 15,
        7, 62, 54, 46, 38, 30, 22,
        14, 6, 61, 53, 45, 37, 29,
        21, 13, 5, 28, 20, 12, 4
    };

    private static readonly int[] PC2 = new int[]
    {
        14, 17, 11, 24, 1, 5,
        3, 28, 15, 6, 21, 10,
        23, 19, 12, 4, 26, 8,
        16, 7, 27, 20, 13, 2,

        41, 52, 31, 37, 47, 55,
        30, 40, 51, 45, 33, 48,
        44, 49, 39, 56, 34, 53,
        46, 42, 50, 36, 29, 32
    };

    private static readonly int[] E = new int[]
    {
        32, 1, 2, 3, 4, 5,
        4, 5, 6, 7, 8, 9,
        8, 9, 10, 11, 12, 13,
        12, 13, 14, 15, 16, 17,
        16, 17, 18, 19, 20, 21,
        20, 21, 22, 23, 24, 25,
        24, 25, 26, 27, 28, 29,
        28, 29, 30, 31, 32, 1
    };

    private static readonly int[] S1 = new int[]
    {
        14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7,
        0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8,
        4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0,
        15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13
    };

    private static readonly int[] S2 = new int[]
    {
        15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10,
        3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5,
        0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15,
        13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9
    };

    private static readonly int[] S3 = new int[]
    {
        10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8,
        13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1,
        13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7,
        1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12
    };


    private static readonly int[] S4 = new int[]
    {
        7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15,
        13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9,
        10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4,
        3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14
    };

    private static readonly int[] S5 = new int[]
    {
        2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9,
        14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6,
        4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14,
        11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3
    };

    private static readonly int[] S6 = new int[]
    {
        12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11,
        10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8,
        9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6,
        4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13
    };

    private static readonly int[] S7 = new int[]
    {
        4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1,
        13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6,
        1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2,
        6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12
    };

    private static readonly int[] S8 = new int[]
    {
        13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7,
        1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2,
        7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8,
        2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11
    };

    private static readonly int[] P = new int[]
    {
        16, 7, 20, 21, 29, 12, 28, 17,
        1, 15, 23, 26, 5, 18, 31, 10,
        2, 8, 24, 14, 32, 27, 3, 9,
        19, 13, 30, 6, 22, 11, 4, 25
    };
    private static readonly int[] IP = new int[]
    {
        58,  50,  42,  34,  26,  18,  10,  2,
        60,  52,  44,  36,  28,  20,  12,  4,
        62,  54,  46,  38,  30,  22,  14,  6,
        64,  56,  48,  40,  32,  24,  16,  8,
        57,  49,  41,  33,  25,  17,  9,   1,
        59,  51,  43,  35,  27,  19,  11,  3,
        61,  53,  45,  37,  29,  21,  13,  5,
        63,  55,  47,  39,  31,  23,  15,  7

    };
    private static int[] IP2 = new int[]
    {
        40,  8,   48,  16,  56,  24,  64,  32,
        39,  7,   47,  15,  55,  23,  63,  31,
        38,  6,   46,  14,  54,  22,  62,  30,
        37,  5,   45,  13,  53,  21,  61,  29,
        36,  4,   44,  12,  52,  20,  60,  28,
        35,  3,   43,  11,  51,  19,  59,  27,
        34,  2,   42,  10,  50,  18,  58,  26,
        33,  1,   41,   9,  49,  17,  57,  25

    };

    public byte[] Encrypt(byte[] textbytes, byte[] key)
    {
        //input
        byte[] holderL = new byte[4];
        byte[] holderR = new byte[4];
        byte[] tmp = new byte[8];
        //expanding bitarrays to fit (56 bits for key,64 bit blocks for plaintext
        if ((textbytes.Length & 7) != 0)
        {
            byte[] temp = new byte[textbytes.Length + textbytes.Length % 8];
            textbytes.CopyTo(temp, 0);
            textbytes = temp;
        }
        int blockcount = textbytes.Length / 8;

        byte[][] subkeys = GenerateSubkeys(key);

        for (int stage = 1; stage <= 16; stage++)
        {
            //get subkey
            key = subkeys[stage - 1];

            // Split the block into halves
            for (int blocknum = 0; blocknum < blockcount; blocknum++)
            {
                if (stage == 1)
                {
                    Array.Copy(textbytes, blocknum * 8, tmp, 0, 8);
                    tmp = UseTable(tmp, IP);
                    Array.Copy(tmp, 0, textbytes, blocknum * 8, 8);
                }
                // Load the block
                for (int pointer = 0; pointer < 4; pointer++)
                {
                    holderL[pointer] = textbytes[pointer + blocknum * 8];
                    holderR[pointer] = textbytes[pointer + blocknum * 8 + 4];
                }

                byte[] buffholder = holderR;
                //Function F
                holderR = UseTable(holderR, E);
                holderR = ByteUtils.XORBytes(holderR, key);
                holderR = Sbox(holderR);
                holderR = UseTable(holderR, P);
                //End F

                //XOR operation
                holderR = ByteUtils.XORBytes(holderR, holderL);

                holderL = buffholder;

                // Store block
                for (int pointer = 0; pointer < 4; pointer++)
                {
                    textbytes[pointer + blocknum * 8] = holderL[pointer];
                    textbytes[pointer + blocknum * 8 + 4] = holderR[pointer];
                    if (stage == 16)
                    {
                        textbytes[pointer + blocknum * 8] = holderR[pointer];
                        textbytes[pointer + blocknum * 8 + 4] = holderL[pointer];

                    }
                }
                if (stage == 16)
                {
                    Array.Copy(textbytes, blocknum * 8, tmp, 0, 8);
                    tmp = UseTable(tmp, IP2);
                    Array.Copy(tmp, 0, textbytes, blocknum * 8, 8);
                }
            }

        }



        return textbytes;
    }

    public byte[] Decrypt(byte[] textbytes, byte[] key)
    {
        byte[] holderL = new byte[4];
        byte[] holderR = new byte[4];
        byte[] tmp = new byte[8];
        // Expanding bit arrays to fit (56 bits for the key, 64-bit blocks for plaintext)
        if ((textbytes.Length & 7) != 0)
        {
            byte[] temp = new byte[textbytes.Length + textbytes.Length % 8];
            textbytes.CopyTo(temp, 0);
            textbytes = temp;
        }
        int blockcount = textbytes.Length / 8;

        byte[][] subkeys = GenerateSubkeys(key);

        for (int stage = 1; stage <= 16; stage++)
        {
            //generate subkey
            key = subkeys[16 - stage];

            // Split the block into halves
            for (int blocknum = 0; blocknum < blockcount; blocknum++)
            {
                if (stage == 1)
                {
                    Array.Copy(textbytes, blocknum * 8, tmp, 0, 8);
                    tmp = UseTable(tmp, IP);
                    Array.Copy(tmp, 0, textbytes, blocknum * 8, 8);
                }
                for (int pointer = 0; pointer < 4; pointer++)
                {
                    holderL[pointer] = textbytes[pointer + blocknum * 8];
                    holderR[pointer] = textbytes[pointer + blocknum * 8 + 4];
                }


                byte[] buffholder = holderR;
                //Function F
                holderR = UseTable(holderR, E);
                holderR = ByteUtils.XORBytes(holderR, key);
                holderR = Sbox(holderR);
                holderR = UseTable(holderR, P);
                //End F

                //xorowanie
                holderR = ByteUtils.XORBytes(holderR, holderL);

                holderL = buffholder;

                //XOR operation
                for (int pointer = 0; pointer < 4; pointer++)
                {
                    textbytes[pointer + blocknum * 8] = holderL[pointer];
                    textbytes[pointer + blocknum * 8 + 4] = holderR[pointer];
                    if (stage == 16)
                    {
                        textbytes[pointer + blocknum * 8] = holderR[pointer];
                        textbytes[pointer + blocknum * 8 + 4] = holderL[pointer];
                    }
                }
                if (stage == 16)
                {
                    Array.Copy(textbytes, blocknum * 8, tmp, 0, 8);
                    tmp = UseTable(tmp, IP2);
                    Array.Copy(tmp, 0, textbytes, blocknum * 8, 8);
                }
            }
        }
        return textbytes;
    }

    public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }
    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
    private static byte[] UseTable(byte[] arr, int[] table)
    {
        var len = table.Length;
        var bytenum = (len - 1) / 8 + 1;
        var output = new byte[bytenum];
        for (int i = 0; i < len; i++)
        {
            int val = ByteUtils.GetBitAt(arr, table[i] - 1);
            ByteUtils.SetBitAt(output, i, val);
        }

        return output;
    }

    private byte[] GlueKey(byte[] arrC, byte[] arrD)
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

    private byte[] Sbox(byte[] input)
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
            halfofbyte = (byte)Sboxpicker(col, row, section); //4 bits (or if I shift by 4 during the next insertion, will it be correct?)
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

    private static int Sboxpicker(int col, int row, int index)
    {
        int result = 0;
        switch (index + 1)
        {
            case 1:
                result = S1[col + row * 16];
                break;
            case 2:
                result = S2[col + row * 16];
                break;
            case 3:
                result = S3[col + row * 16];
                break;
            case 4:
                result = S4[col + row * 16];
                break;
            case 5:
                result = S5[col + row * 16];
                break;
            case 6:
                result = S6[col + row * 16];
                break;
            case 7:
                result = S7[col + row * 16];
                break;
            case 8:
                result = S8[col + row * 16];
                break;
        }

        return result;
    }

    public byte[][] GenerateSubkeys(byte[] keybytes)
    {
        byte[][] subkeyarray = new byte[16][];
        keybytes = UseTable(keybytes, PC1); // This cuts 56 bits and shortens keybytes, so there is no need to trim it earlier.

        byte[] keybytesC = ByteUtils.SelectBits(keybytes, 0, 28);
        byte[] keybytesD = ByteUtils.SelectBits(keybytes, 28, 28);
        for (int i = 1; i <= 16; i++)
        {
            if (i == 1 || i == 2 || i == 9 || i == 16)
            {
                keybytesC = ByteUtils.RotateLeft(keybytesC, 28, 1);
                keybytesD = ByteUtils.RotateLeft(keybytesD, 28, 1);

                keybytes = UseTable(GlueKey(keybytesC, keybytesD), PC2);
                subkeyarray[i - 1] = keybytes;
            }
            else
            {
                keybytesC = ByteUtils.RotateLeft(keybytesC, 28, 2);
                keybytesD = ByteUtils.RotateLeft(keybytesD, 28, 2);

                keybytes = UseTable(GlueKey(keybytesC, keybytesD), PC2);
                subkeyarray[i - 1] = keybytes;
            }
        }
        return subkeyarray;
    }

    public static String Test()
    {

        byte x, y;
        x = 1;
        y = 3;
        for (int i = 0; i < 4; i++)
        {
            byte a;
            a = (byte)(x & y);
            x = (byte)(x << 1);
        }
        return "";
    }
    
    
    #endregion

    private byte[]? _key;
    public DesAlgorithm()
    {
        GenerateKey(Config.KeyEncryptionLength);
    }
    
    public byte[] EncryptKey => _key ??= GenerateKey(Config.KeyEncryptionLength);
    public byte[] DecryptKey => _key ??= GenerateKey(Config.KeyEncryptionLength);

    public byte[] GenerateKey(int length)
    {
        return ByteUtils.GetBytesFromString("anhyeuem");
    }
}

