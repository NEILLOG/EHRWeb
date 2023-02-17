using BASE.Models.DB;
using BASE.Service.Base;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BASE.Service
{
    public static class EncryptService
    {
        /// <summary>
        /// AES 加密，參考: https://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
        /// </summary>
        public static class AES
        {
            #region "定義加密字串變數"
            private static readonly byte[] saltBytes = new byte[] { 156, 165, 69, 196, 56, 86, 53, 62 };
            private const string pw = "7_wolf";
            #endregion

            /// <summary>
            /// Encrypt Bytes with PasswordBytes
            /// </summary>
            /// <param name="bytesToBeEncrypted"></param>
            /// <param name="passwordBytes"></param>
            /// <returns></returns>
            public static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
            {
                byte[]? encryptedBytes = null;

                // Set your salt here, change it to meet your flavor:
                // The salt bytes must be at least 8 bytes.

                using (MemoryStream ms = new MemoryStream())
                {
                    using (Aes AES = Aes.Create())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }

                return encryptedBytes;
            }

            /// <summary>
            /// Decrypt encrypted bytes with PasswordBytes
            /// </summary>
            /// <param name="bytesToBeDecrypted"></param>
            /// <param name="passwordBytes"></param>
            /// <returns></returns>
            public static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
            {
                byte[]? decryptedBytes = null;

                // Set your salt here, change it to meet your flavor:
                // The salt bytes must be at least 8 bytes.

                using (MemoryStream ms = new MemoryStream())
                {
                    using (Aes AES = Aes.Create())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }

                return decryptedBytes;
            }

            public static byte[] GetRandomBytes()
            {
                int saltLength = GetSaltLength();
                byte[] ba = new byte[saltLength];
                RandomNumberGenerator.Create().GetBytes(ba);
                
                return ba;
            }

            public static int GetSaltLength()
            {
                return 8;
            }

            /// <summary>
            /// AES Encrypt
            /// </summary>
            /// <param name="input"></param>
            /// <param name="url_encode">是否要 url encode</param>
            /// <returns></returns>
            public static string? Encrypt(string? input, bool url_encode = false)
            {
                if (input == null)
                {
                    return null;
                }
                else
                {
                    // Get the bytes of the string
                    byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(pw);

                    // Hash the password with SHA256
                    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                    byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

                    string result = BitConverter.ToString(bytesEncrypted.ToArray()).Replace("-", string.Empty);

                    if (url_encode)
                    {
                        result = WebUtility.UrlEncode(result);
                    }

                    return result;
                }
            }

            /// <summary>
            /// AES Decrypt
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string? Decrypt(string? input)
            {
                try
                {
                    if (input == null)
                    {
                        return null;
                    }
                    else
                    {
                        // Get the bytes of the string
                        byte[] bytesToBeDecrypted = new byte[input.Length / 2];
                        int j = 0;
                        for (int i = 0; i < input.Length / 2; i++)
                        {
                            bytesToBeDecrypted[i] = Byte.Parse(input[j].ToString() + input[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                            j += 2;
                        }

                        byte[] passwordBytes = Encoding.UTF8.GetBytes(pw);
                        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                        byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

                        string result = Encoding.UTF8.GetString(bytesDecrypted);

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    return string.Empty;
                }

            }


            /// <summary>
            /// Base 64 AES Encrypt
            /// </summary>
            /// <param name="input"></param>
            /// <param name="url_encode">是否要 url encode</param>
            /// <returns></returns>
            public static string? Base64Encrypt(string? input, bool url_encode = false)
            {
                if (input == null)
                {
                    return null;
                }
                else
                {
                    // Get the bytes of the string
                    byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(pw);

                    // Hash the password with SHA256
                    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                    byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

                    string result = Convert.ToBase64String(bytesEncrypted);

                    if (url_encode)
                    {
                        result = WebUtility.UrlEncode(result);
                    }

                    return result;
                }

            }

            /// <summary>
            /// AES Decrypt
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string? Base64Decrypt(string input)
            {
                try
                {
                    if (input == null)
                    {
                        return null;
                    }
                    else
                    {
                        // Get the bytes of the string
                        byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
                        byte[] passwordBytes = Encoding.UTF8.GetBytes(pw);
                        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                        byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

                        string result = Encoding.UTF8.GetString(bytesDecrypted);

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    return string.Empty;
                }

            }

            public static string RandomizedEncrypt(string? text, bool url_encode = false)
            {
                if (text == null)
                {
                    return null;
                }
                else
                {
                    byte[] baPwd = Encoding.UTF8.GetBytes(pw);

                    // Hash the password with SHA256
                    byte[] baPwdHash = SHA256.Create().ComputeHash(baPwd);

                    byte[] baText = Encoding.UTF8.GetBytes(text);

                    //byte[] baSalt = GetRandomBytes();  //隨機值
                    byte[] baSalt = new byte[] { 11, 228, 42, 63, 235, 203, 79, 147 };  //因無障礙檢測Freego的執行個體會造成每次讀取頁面都不同，導致永遠檢測不完，故檢測時須使用固定值
                    byte[] baEncrypted = new byte[baSalt.Length + baText.Length];

                    // Combine Salt + Text
                    for (int i = 0; i < baSalt.Length; i++)
                        baEncrypted[i] = baSalt[i];
                    for (int i = 0; i < baText.Length; i++)
                        baEncrypted[i + baSalt.Length] = baText[i];

                    baEncrypted = Encrypt(baEncrypted, baPwdHash);

                    string result = BitConverter.ToString(baEncrypted.ToArray()).Replace("-", string.Empty);

                    if (url_encode)
                    {
                        result = WebUtility.UrlEncode(result);
                    }

                    return result;
                }

            }

            public static string RandomizedDecrypt(string? text)
            {
                try
                {
                    if (text == null)
                    {
                        return null;
                    }
                    else
                    {
                        byte[] baPwd = Encoding.UTF8.GetBytes(pw);

                        // Hash the password with SHA256
                        byte[] baPwdHash = SHA256.Create().ComputeHash(baPwd);

                        byte[] baText = new byte[text.Length / 2];
                        int j = 0;
                        for (int i = 0; i < text.Length / 2; i++)
                        {
                            baText[i] = Byte.Parse(text[j].ToString() + text[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                            j += 2;
                        }

                        byte[] baDecrypted = Decrypt(baText, baPwdHash);

                        // Remove salt
                        int saltLength = GetSaltLength();
                        byte[] baResult = new byte[baDecrypted.Length - saltLength];
                        for (int i = 0; i < baResult.Length; i++)
                            baResult[i] = baDecrypted[i + saltLength];

                        string result = Encoding.UTF8.GetString(baResult);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    return String.Empty;
                }

            }

            /// <summary>
            /// AES Encrypt（每次結果均會不同）
            /// </summary>
            /// <param name="text"></param>
            /// <param name="url_encode">是否要 url encode</param>
            /// <returns></returns>
            public static string Randomized64Encrypt(string? text, bool url_encode = false)
            {
                if (text == null)
                {
                    return null;
                }
                else
                {
                    byte[] baPwd = Encoding.UTF8.GetBytes(pw);

                    // Hash the password with SHA256
                    byte[] baPwdHash = SHA256.Create().ComputeHash(baPwd);

                    byte[] baText = Encoding.UTF8.GetBytes(text);

                    byte[] baSalt = GetRandomBytes();
                    byte[] baEncrypted = new byte[baSalt.Length + baText.Length];

                    // Combine Salt + Text
                    for (int i = 0; i < baSalt.Length; i++)
                        baEncrypted[i] = baSalt[i];
                    for (int i = 0; i < baText.Length; i++)
                        baEncrypted[i + baSalt.Length] = baText[i];

                    baEncrypted = Encrypt(baEncrypted, baPwdHash);

                    string result = Convert.ToBase64String(baEncrypted);

                    if (url_encode)
                    {
                        result = WebUtility.UrlEncode(result);
                    }

                    return result;
                }
            }

            /// <summary>
            /// AES Decrypt（針對摻雜隨機碼的 AES 加密）
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string Randomized64Decrypt(string? text)
            {
                try
                {
                    if (text == null)
                    {
                        return null;
                    }
                    else
                    {
                        byte[] baPwd = Encoding.UTF8.GetBytes(pw);

                        // Hash the password with SHA256
                        byte[] baPwdHash = SHA256.Create().ComputeHash(baPwd);

                        byte[] baText = Convert.FromBase64String(text);

                        byte[] baDecrypted = Decrypt(baText, baPwdHash);

                        // Remove salt
                        int saltLength = GetSaltLength();
                        byte[] baResult = new byte[baDecrypted.Length - saltLength];
                        for (int i = 0; i < baResult.Length; i++)
                            baResult[i] = baDecrypted[i + saltLength];

                        string result = Encoding.UTF8.GetString(baResult);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    return String.Empty;
                }

            }

            /// <summary>
            /// 檔案加密
            /// </summary>
            /// <param name="original_file_path">欲加密檔案位置（實體路徑）</param>
            /// <param name="encrypted_file_path">加密後檔案位置（實體路徑）</param>
            public static void FileEncrypt(string original_file_path, string encrypted_file_path)
            {
                string file = original_file_path;

                byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(pw);

                // Hash the password with SHA256
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

                string fileEncrypted = encrypted_file_path;

                File.WriteAllBytes(fileEncrypted, bytesEncrypted);
            }

            /// <summary>
            /// 檔案解密
            /// </summary>
            /// <param name="encrypted_file_path">欲解密檔案位置（實體路徑）</param>
            /// <param name="decrypted_file_path">解密後檔案位置（實體路徑）</param>
            public static void FileDecrypt(string encrypted_file_path, string decrypted_file_path)
            {
                string fileEncrypted = encrypted_file_path;

                byte[] bytesToBeDecrypted = File.ReadAllBytes(fileEncrypted);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(pw);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

                string file = decrypted_file_path;
                File.WriteAllBytes(file, bytesDecrypted);
            }
        }


    }
}
