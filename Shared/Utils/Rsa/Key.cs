using System.Numerics;
using System.Runtime.Serialization;

namespace Shared.Utils.Rsa
{
    public class Constants
    {
        //The "e" value for low compute time RSA encryption.
        //Only has two bits of value 1.
        public static int e = 0x10001;
    }

    /// <summary>
    /// Wrapper KeyPair class, for the case when people generate keys locally.
    /// </summary>
    [DataContract]
    [Serializable]
    public sealed class KeyPair
    {
        //After assignment, the keys cannot be touched.
        [DataMember] public readonly Key private_;
        [DataMember] public readonly Key public_;

        public KeyPair(Key private__, Key public__)
        {
            private_ = private__;
            public_ = public__;
        }

        /// <summary>
        /// Returns a keypair based on the calculated n and d values from RSA.
        /// </summary>
        /// <param name="n">The "n" value from RSA calculations.</param>
        /// <param name="d">The "d" value from RSA calculations.</param>
        /// <returns></returns>
        public static KeyPair Generate(BigInteger n, BigInteger d)
        {
            Key public_ = new Key(n, KeyType.PUBLIC);
            Key private_ = new Key(n, KeyType.PRIVATE, d);
            Logger.LogInfo($"Generated public key: {public_.n.ToByteArray().Length} bytes");
            Logger.LogInfo($"Generated private key: {private_.n.ToByteArray().Length} bytes");
            return new KeyPair(private_, public_);
        }
    }


    /// <summary>
    /// Class to contain RSA key values for public and private keys. All values readonly and protected
    /// after construction, type set on construction.
    /// </summary>
    [DataContract(Name = "Key", Namespace = "Shared.Utils.Rsa")]
    [Serializable]
    public class Key
    {
        //Hidden key constants, n and e are public key variables.
        [DataMember(Name = "n")] public BigInteger n { get; set; }
        [DataMember(Name = "e")] public int e = Constants.e;


        //Optional null variable D.
        //This should never be shared as a DataMember, by principle this should not be passed over a network.
        public readonly BigInteger d;

        //Variable for key type.
        [DataMember(Name = "type")] public KeyType type { get; set; }

        //Constructor that sets values once, values then permanently unwriteable.
        public Key(BigInteger n_, KeyType type_, BigInteger d_)
        {
            //Catching edge cases for invalid input.
            if (type_ == KeyType.PRIVATE && d_ < 2)
            {
                throw new Exception("Constructed as private, but invalid d value provided.");
            }

            //Setting values.
            n = n_;
            type = type_;
            d = d_;
        }

        //Overload constructor for key with no d value.
        public Key(BigInteger n_, KeyType type_)
        {
            //Catching edge cases for invalid input.
            if (type_ == KeyType.PRIVATE)
            {
                throw new Exception("Constructed as private, but no d value provided.");
            }

            //Setting values.
            n = n_;
            type = type_;
        }
        
        public byte[] GetBytes()
        {
            var nBytes = n.ToByteArray();
            // Với public key, không có d
            var dBytes = type == KeyType.PRIVATE ? d.ToByteArray() : [];

            // Chuyển độ dài của nBytes thành 4 byte (int)
            var nLengthBytes = BitConverter.GetBytes(nBytes.Length);

            // Kết hợp: [1 byte kiểu][4 byte độ dài n][nBytes][dBytes]
            int totalLength = 1 + nLengthBytes.Length + nBytes.Length + dBytes.Length;
            var combined = new byte[totalLength];

            combined[0] = (byte)type;
            Buffer.BlockCopy(nLengthBytes, 0, combined, 1, nLengthBytes.Length);
            Buffer.BlockCopy(nBytes, 0, combined, 1 + nLengthBytes.Length, nBytes.Length);
            if (dBytes.Length > 0)
            {
                Buffer.BlockCopy(dBytes, 0, combined, 1 + nLengthBytes.Length + nBytes.Length, dBytes.Length);
            }
            return combined;
        }
        
        public static Key FromBytes(byte[] data)
        {
            if (data.Length < 5) // 1 byte kiểu + 4 byte độ dài n
            {
                throw new ArgumentException("Invalid key data.");
            }

            // Đọc kiểu khóa từ byte đầu tiên
            var type = (KeyType)data[0];

            // Đọc 4 byte tiếp theo để lấy độ dài của n
            int nLength = BitConverter.ToInt32(data, 1);
            if (nLength <= 0 || data.Length < 1 + 4 + nLength)
            {
                throw new ArgumentException("Invalid key data: incorrect n length.");
            }

            // Đọc nBytes
            var nBytes = new byte[nLength];
            Buffer.BlockCopy(data, 1 + 4, nBytes, 0, nLength);
            var n = new BigInteger(nBytes);

            if (type == KeyType.PUBLIC)
            {
                return new Key(n, type);
            }
            else
            {
                // Với private key, phần còn lại của mảng là dBytes
                int dLength = data.Length - (1 + 4 + nLength);
                if (dLength <= 0)
                {
                    throw new ArgumentException("Invalid key data: missing d bytes for private key.");
                }
                var dBytes = new byte[dLength];
                Buffer.BlockCopy(data, 1 + 4 + nLength, dBytes, 0, dLength);
                var d = new BigInteger(dBytes);
                return new Key(n, type, d);
            }
        }


    }

    public enum KeyType
    {
        PUBLIC,
        PRIVATE
    }
}