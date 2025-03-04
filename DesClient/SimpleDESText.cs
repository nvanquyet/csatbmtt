// using System.Text;
//
// namespace DesClient;
//
// static class SimpleDesText
// {
//     private static readonly int[] Ip = { 58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4,
//         62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8, 57, 49, 41, 33, 25, 17, 9, 1,
//         59, 51, 43, 35, 27, 19, 11, 3, 61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7 };
//
//     private static readonly int[] Fp = { 40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31,
//         38, 6, 46, 14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29, 36, 4, 44, 12, 52, 20, 60, 28,
//         35, 3, 43, 11, 51, 19, 59, 27, 34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9, 49, 17, 57, 25 };
//
//     private static ulong Permute(ulong input, int[] table, int tableSize)
//     {
//         ulong output = 0;
//         for (int i = 0; i < tableSize; i++)
//         {
//             output <<= 1;
//             output |= (input >> (64 - table[i])) & 1;
//         }
//         return output;
//     }
//
//     private static uint Feistel(uint R, ulong key)
//     {
//         return R ^ (uint)key;
//     }
//
//     private static ulong EncryptBlock(ulong plainText, ulong key)
//     {
//         ulong permuted = Permute(plainText, Ip, 64);
//         uint L = (uint)(permuted >> 32);
//         uint R = (uint)(permuted & 0xFFFFFFFF);
//
//         for (int i = 0; i < 16; i++)
//         {
//             uint temp = R;
//             R = L ^ Feistel(R, key);
//             L = temp;
//         }
//
//         ulong combined = ((ulong)R << 32) | L;
//         return Permute(combined, Fp, 64);
//     }
//
//     private static ulong DecryptBlock(ulong cipherText, ulong key)
//     {
//         ulong permuted = Permute(cipherText, Ip, 64);
//         uint L = (uint)(permuted >> 32);
//         uint R = (uint)(permuted & 0xFFFFFFFF);
//
//         for (int i = 0; i < 16; i++)
//         {
//             uint temp = L;
//             L = R ^ Feistel(L, key);
//             R = temp;
//         }
//
//         ulong combined = ((ulong)R << 32) | L;
//         return Permute(combined, Fp, 64);
//     }
//
//     private static byte[] PadMessage(byte[] data)
//     {
//         int padSize = 8 - (data.Length % 8);
//         Array.Resize(ref data, data.Length + padSize);
//         return data;
//     }
//
//     private static string EncryptText(string plainText, ulong key)
//     {
//         byte[] data = Encoding.UTF8.GetBytes(plainText);
//         data = PadMessage(data);
//
//         byte[] encryptedData = new byte[data.Length];
//
//         for (int i = 0; i < data.Length; i += 8)
//         {
//             ulong block = BitConverter.ToUInt64(data, i);
//             ulong encryptedBlock = EncryptBlock(block, key);
//             Array.Copy(BitConverter.GetBytes(encryptedBlock), 0, encryptedData, i, 8);
//         }
//
//         return Convert.ToBase64String(encryptedData);
//     }
//
//     private static string DecryptText(string cipherText, ulong key)
//     {
//         byte[] encryptedData = Convert.FromBase64String(cipherText);
//         byte[] decryptedData = new byte[encryptedData.Length];
//
//         for (int i = 0; i < encryptedData.Length; i += 8)
//         {
//             ulong block = BitConverter.ToUInt64(encryptedData, i);
//             ulong decryptedBlock = DecryptBlock(block, key);
//             Array.Copy(BitConverter.GetBytes(decryptedBlock), 0, decryptedData, i, 8);
//         }
//
//         return Encoding.UTF8.GetString(decryptedData).TrimEnd('\0'); 
//     }
//
//     public static void Main()
//     {
//         string message = "HelloDES!";
//         ulong key = 0x133457799BBCDFF1;
//
//         string encrypted = EncryptText(message, key);
//         Console.WriteLine("Ciphertext: " + encrypted);
//
//         string decrypted = DecryptText(encrypted, key);
//         Console.WriteLine("Decrypted Text: " + decrypted);
//     }
// }