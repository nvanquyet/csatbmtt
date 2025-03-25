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
            var dBytes = type == KeyType.PRIVATE ? d.ToByteArray() : [];

            // Kết hợp các mảng byte
            var combined = new byte[nBytes.Length + dBytes.Length + 1];
            combined[0] = (byte)type;  // Lưu kiểu khóa (PUBLIC / PRIVATE)
            Buffer.BlockCopy(nBytes, 0, combined, 1, nBytes.Length);
            Buffer.BlockCopy(dBytes, 0, combined, 1 + nBytes.Length, dBytes.Length);

            return combined;
        }
        
        public static Key FromBytes(byte[] data)
        {
            if (data.Length < 2)
            {
                throw new ArgumentException("Invalid key data.");
            }

            // Đọc kiểu khóa từ byte đầu tiên
            var type = (KeyType)data[0];

            // Tách n từ dữ liệu còn lại
            var halfLength = (data.Length - 1) / 2;
            var nBytes = new byte[halfLength];
            var dBytes = new byte[halfLength];

            Buffer.BlockCopy(data, 1, nBytes, 0, halfLength);
            Buffer.BlockCopy(data, 1 + halfLength, dBytes, 0, halfLength);

            var n = new BigInteger(nBytes);
            var d = new BigInteger(dBytes);

            return type == KeyType.PUBLIC ? new Key(n, type) : new Key(n, type, d);
        }

    }

    public enum KeyType
    {
        PUBLIC,
        PRIVATE
    }
}