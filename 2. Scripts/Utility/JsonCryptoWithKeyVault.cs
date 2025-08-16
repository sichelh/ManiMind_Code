using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

// AES-GCM 기반 JSON 암복호화 유틸.
public static class JsonCrypto
{
    private const byte VER_GCM = 0x01;      // [ver][nonce(12)][cipher][tag(16)]
    private const byte VER_CBC_HMAC = 0x02; // [ver][iv(16)][cipher][tag(32)]

#if AESGCM_FORCE_CBC
    private static readonly bool gcmSupported = false;
#else
    private static readonly bool gcmSupported = IsGcmSupported();
#endif

    public static string EncryptJson(string json, byte[] key)
    {
        byte[] plain = Encoding.UTF8.GetBytes(json);

        if (gcmSupported)
        {
            byte[] nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);

            byte[] cipher = new byte[plain.Length];
            byte[] tag    = new byte[16];

            try
            {
                using (AesGcm gcm = new(key))
                {
                    gcm.Encrypt(nonce, plain, cipher, tag);
                }

                byte[] blob = new byte[1 + nonce.Length + cipher.Length + tag.Length];
                blob[0] = VER_GCM;
                Buffer.BlockCopy(nonce, 0, blob, 1, nonce.Length);
                Buffer.BlockCopy(cipher, 0, blob, 1 + nonce.Length, cipher.Length);
                Buffer.BlockCopy(tag, 0, blob, 1 + nonce.Length + cipher.Length, tag.Length);
                return Convert.ToBase64String(blob);
            }
            catch (PlatformNotSupportedException)
            {
                // 아래 CBC로 폴백
            }
        }

        // === AES-CBC + HMAC-SHA256 (Encrypt-then-MAC) ===
        using (HMACSHA256 hk = new(key))
        {
            byte[] encKey = hk.ComputeHash(Encoding.UTF8.GetBytes("enc"));
            byte[] macKey = hk.ComputeHash(Encoding.UTF8.GetBytes("mac"));

            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = encKey;
                aes.IV = RandomBytes(16);

                using (ICryptoTransform enc = aes.CreateEncryptor())
                {
                    byte[] cipher = enc.TransformFinalBlock(plain, 0, plain.Length);

                    using (HMACSHA256 hmac = new(macKey))
                    {
                        byte[] header = new byte[1 + aes.IV.Length + cipher.Length];
                        header[0] = VER_CBC_HMAC;
                        Buffer.BlockCopy(aes.IV, 0, header, 1, aes.IV.Length);
                        Buffer.BlockCopy(cipher, 0, header, 1 + aes.IV.Length, cipher.Length);

                        byte[] tag = hmac.ComputeHash(header);

                        byte[] blob = new byte[header.Length + tag.Length];
                        Buffer.BlockCopy(header, 0, blob, 0, header.Length);
                        Buffer.BlockCopy(tag, 0, blob, header.Length, tag.Length);
                        return Convert.ToBase64String(blob);
                    }
                }
            }
        }
    }

    public static string DecryptJson(string blobBase64, byte[] key)
    {
        byte[] data = Convert.FromBase64String(blobBase64);
        if (data.Length < 1)
        {
            throw new ArgumentException("Invalid blob.");
        }

        byte ver = data[0];

        if (ver == VER_GCM)
        {
            if (data.Length < 1 + 12 + 16)
            {
                throw new ArgumentException("Invalid GCM blob.");
            }

            int nonceLen  = 12;
            int tagLen    = 16;
            int cipherLen = data.Length - 1 - nonceLen - tagLen;

            byte[] nonce  = new byte[nonceLen];
            byte[] cipher = new byte[cipherLen];
            byte[] tag    = new byte[tagLen];

            Buffer.BlockCopy(data, 1, nonce, 0, nonceLen);
            Buffer.BlockCopy(data, 1 + nonceLen, cipher, 0, cipherLen);
            Buffer.BlockCopy(data, 1 + nonceLen + cipherLen, tag, 0, tagLen);

            using (AesGcm gcm = new(key))
            {
                byte[] plain = new byte[cipherLen];
                gcm.Decrypt(nonce, cipher, tag, plain);
                return Encoding.UTF8.GetString(plain);
            }
        }
        else if (ver == VER_CBC_HMAC)
        {
            if (data.Length < 1 + 16 + 32)
            {
                throw new ArgumentException("Invalid CBC blob.");
            }

            int ivLen     = 16;
            int tagLen    = 32;
            int cipherLen = data.Length - 1 - ivLen - tagLen;

            byte[] iv     = new byte[ivLen];
            byte[] cipher = new byte[cipherLen];
            byte[] tag    = new byte[tagLen];

            Buffer.BlockCopy(data, 1, iv, 0, ivLen);
            Buffer.BlockCopy(data, 1 + ivLen, cipher, 0, cipherLen);
            Buffer.BlockCopy(data, 1 + ivLen + cipherLen, tag, 0, tagLen);

            using (HMACSHA256 hk = new(key))
            {
                byte[] encKey = hk.ComputeHash(Encoding.UTF8.GetBytes("enc"));
                byte[] macKey = hk.ComputeHash(Encoding.UTF8.GetBytes("mac"));

                using (HMACSHA256 hmac = new(macKey))
                {
                    byte[] header = new byte[1 + ivLen + cipherLen];
                    header[0] = VER_CBC_HMAC;
                    Buffer.BlockCopy(iv, 0, header, 1, ivLen);
                    Buffer.BlockCopy(cipher, 0, header, 1 + ivLen, cipherLen);
                    byte[] calc = hmac.ComputeHash(header);
                    if (!FixedTimeEquals(calc, tag))
                    {
                        throw new CryptographicException("Auth tag mismatch");
                    }
                }

                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = encKey;
                    aes.IV = iv;

                    using (ICryptoTransform dec = aes.CreateDecryptor())
                    {
                        byte[] plain = dec.TransformFinalBlock(cipher, 0, cipher.Length);
                        return Encoding.UTF8.GetString(plain);
                    }
                }
            }
        }

        throw new ArgumentException("Unknown blob version.");
    }

    private static bool IsGcmSupported()
    {
        try
        {
            using AesGcm test = new(new byte[16]);
            return true;
        }
        catch { return false; }
    }

    private static byte[] RandomBytes(int len)
    {
        byte[] b = new byte[len];
        RandomNumberGenerator.Fill(b);
        return b;
    }

    private static bool FixedTimeEquals(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length)
        {
            return false;
        }

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }

        return diff == 0;
    }
}

// 키 보관 인터페이스.
public interface IKeyVault
{
    // alias에 해당하는 32바이트 키를 반환합니다. 없으면 생성 후 보관합니다.
    byte[] GetOrCreateKey(string alias);
}

// 플랫폼별 KeyVault 생성 팩토리.
public static class KeyVaultFactory
{
    public static IKeyVault Create()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return new AndroidKeyVault();
#else
        return new FileVault(); // 단순 파일 보관(임시 대안).
#endif
    }
}

#if UNITY_ANDROID
// Android: Keystore의 RSA 키쌍으로 32바이트 키를 래핑하여 저장합니다.
public sealed class AndroidKeyVault : IKeyVault
{
    private readonly string rootPath = Path.Combine(Application.persistentDataPath, "kv");
    private const string BridgeClass = "com.sparta_five_group.security.KeystoreBridge";

    public byte[] GetOrCreateKey(string alias)
    {
        Directory.CreateDirectory(rootPath);
        string path = Path.Combine(rootPath, alias + "_wrapped.bin");

        try
        {
            using (AndroidJavaClass bridge = new(BridgeClass))
            {
                bool ok = bridge.CallStatic<bool>("ensureKeyPair", alias);
                Debug.Log($"[AKV] ensureKeyPair({alias})={ok}");

                if (File.Exists(path))
                {
                    byte[] wrapped = File.ReadAllBytes(path);
                    return bridge.CallStatic<byte[]>("unwrap", alias, wrapped);
                }
                else
                {
                    byte[] key = new byte[32];
                    RandomNumberGenerator.Fill(key);
                    byte[] wrapped = bridge.CallStatic<byte[]>("wrap", alias, key);
                    File.WriteAllBytes(path, wrapped);
                    return key;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AKV] Bridge load/call failed (class={BridgeClass}, alias={alias}): {e}");
            throw; // 상위에서 처리
        }
    }
}
#endif
// 기타 플랫폼 임시 대안: 암호화 없이 파일에 저장(개발용). 실제 릴리즈에선 사용 금지.
public sealed class FileVault : IKeyVault
{
    private readonly string rootPath = Path.Combine(Application.persistentDataPath, "kv");

    public byte[] GetOrCreateKey(string alias)
    {
        Directory.CreateDirectory(rootPath);
        string path = Path.Combine(rootPath, alias + ".raw");
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }

        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);
        File.WriteAllBytes(path, key);
        return key;
    }
}

// 사용 예.
public sealed class JsonSecureStore
{
    // JSON을 암호화하여 파일에 저장합니다.
    public static void Save(string alias, string json)
    {
        IKeyVault vault = KeyVaultFactory.Create();
        byte[]    key   = vault.GetOrCreateKey(alias);
        string    blob  = JsonCrypto.EncryptJson(json, key);
        string    path  = Path.Combine(Application.persistentDataPath, alias + ".jenc");
        File.WriteAllText(path, blob, Encoding.UTF8);
    }

    // 파일을 복호화하여 JSON을 반환합니다.
    public static string Load(string _alias)
    {
        string path = Path.Combine(Application.persistentDataPath, _alias + ".jenc");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(path);
        }

        IKeyVault vault = KeyVaultFactory.Create();
        byte[]    key   = vault.GetOrCreateKey(_alias);
        string    blob  = File.ReadAllText(path, Encoding.UTF8);
        return JsonCrypto.DecryptJson(blob, key);
    }
}

#if UNITY_ANDROID && !UNITY_EDITOR
public static class AndroidKeystoreDiag
{
    private const string BridgeClass = "com.sparta_five_group.security.KeystoreBridge";

    public static void RunOnce(string alias)
    {
        try
        {
            AndroidJavaClass bridge = new(BridgeClass);

            // 0) 키쌍 보장(없으면 생성)
            bool ensured = bridge.CallStatic<bool>("ensureKeyPair", alias);
            Debug.Log($"[KeystoreDiag] ensureKeyPair={ensured}");

            // 1) 존재 여부
            bool exists = bridge.CallStatic<bool>("hasAlias", alias);
            Debug.Log($"[KeystoreDiag] hasAlias={exists}");

            // 2) 키 정보(알고리즘/사이즈/하드웨어 백업 여부)
            string info = bridge.CallStatic<string>("getKeyInfo", alias);
            Debug.Log($"[KeystoreDiag] getKeyInfo: {info}");

            // 3) 실제 래핑/언래핑 테스트
            bool roundtrip = bridge.CallStatic<bool>("selfTest", alias);
            Debug.Log($"[KeystoreDiag] wrap/unwrap roundtrip={roundtrip}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[KeystoreDiag] error: {e}");
        }
    }
}
#endif