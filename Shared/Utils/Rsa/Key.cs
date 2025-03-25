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
            var dBytes = type == KeyType.PRIVATE ? d.ToByteArray() : Array.Empty<byte>();

            // Lưu độ dài nBytes dưới dạng 4 byte big-endian
            int nLength = nBytes.Length;
            byte[] nLengthBytes = new byte[4];
            nLengthBytes[0] = (byte)(nLength >> 24);
            nLengthBytes[1] = (byte)(nLength >> 16);
            nLengthBytes[2] = (byte)(nLength >> 8);
            nLengthBytes[3] = (byte)(nLength);

            var combined = new byte[1 + nLengthBytes.Length + nBytes.Length + dBytes.Length];
            combined[0] = (byte)type;
            Buffer.BlockCopy(nLengthBytes, 0, combined, 1, nLengthBytes.Length);
            Buffer.BlockCopy(nBytes, 0, combined, 1 + nLengthBytes.Length, nBytes.Length);
            Buffer.BlockCopy(dBytes, 0, combined, 1 + nLengthBytes.Length + nBytes.Length, dBytes.Length);

            return combined;
        }
        
        public static Key FromBytes(byte[] data)
        {
            if (data.Length < 5)
            {
                throw new ArgumentException("Invalid key data.");
            }

            var type = (KeyType)data[0];
            int nLength = (data[1] << 24) | (data[2] << 16) | (data[3] << 8) | data[4];

            if (nLength <= 0 || data.Length < 1 + 4 + nLength)
            {
                throw new ArgumentException("Invalid key data: incorrect n length.");
            }

            var nBytes = new byte[nLength];
            Buffer.BlockCopy(data, 1 + 4, nBytes, 0, nLength);
            var n = new BigInteger(nBytes);

            if (type == KeyType.PUBLIC)
            {
                return new Key(n, type);
            }
            else
            {
                int dOffset = 1 + 4 + nLength;
                int dLength = data.Length - dOffset;

                if (dLength <= 0)
                {
                    throw new ArgumentException("Invalid key data: missing d bytes for private key.");
                }

                var dBytes = new byte[dLength];
                Buffer.BlockCopy(data, dOffset, dBytes, 0, dLength);
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