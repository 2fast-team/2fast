﻿using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Project2FA.Core;
using Project2FA.Repository.Models;
using Project2FA.Repository.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOversal.Services.Serialization;

// based on
// https://github.com/stratumauth/app/blob/1f74c5fbfd88eb81a58b1ba71dc3423ac779dc06/Stratum.Core/src/Converter/AegisBackupConverter.cs

namespace Project2FA.Services.Importer
{
    public class AegisBackupImportService : IAegisBackupImportService
    {
        ISerializationService SerializationService { get; }
        public AegisBackupImportService(ISerializationService serializationService)
        {
            SerializationService = serializationService;
        }
        public Task<(List<TwoFACodeModel> accountList, bool successful)> ImportBackup(string content, byte[] bytePassword)
        {
            const string BaseAlgorithm = "AES";
            const string Mode = "GCM";
            const string Padding = "NoPadding";
            const string AlgorithmDescription = BaseAlgorithm + "/" + Mode + "/" + Padding;

            if (bytePassword is null)
            {
                var database = SerializationService.Deserialize<AegisDecryptedDatabase>(content);

                return Task.FromResult((CreateAccountCollection(database), true));
            }
            else
            {
                AegisModel<string> encryptedModel = SerializationService.Deserialize<AegisModel<string>>(content);

                // search for password slot
                if (encryptedModel.Header.Slots != null && encryptedModel.Header.Slots.Count > 0)
                {
                    var slot = encryptedModel.Header.Slots.Where(x => x.Type == AegisSlotType.Password).FirstOrDefault();
                    if (slot != null)
                    {
                        byte[] saltBytes = HexStringToByteArray(slot.Salt);

                        // SCrypt parameters
                        int n = slot.N;  // Cost factor
                        int r = slot.R;  // Block size
                        int p = slot.P;  // Parallelization factor

                        // Derive the key
                        int keySize = 32; // Key size in bytes
                        byte[] derivedKey = SCrypt.Generate(bytePassword, saltBytes, n, r, p, keySize);

                        // get data and initialisation vector
                        byte[] databaseBytes = Convert.FromBase64String(encryptedModel.Database);
                        byte[] ivBytes = Hex.Decode(encryptedModel.Header.Params.Nonce);
                        byte[] keyBytes = Hex.Decode(slot.Key);
                        byte[] macBytes = Hex.Decode(encryptedModel.Header.Params.Tag);

                        var masterKey = DecryptSlot(slot, bytePassword, AlgorithmDescription);
                        // decrypt
                        var decryptedBytes = DecryptAesGcm(masterKey, ivBytes, databaseBytes, macBytes, AlgorithmDescription);
                        var json = Encoding.UTF8.GetString(decryptedBytes);
                        var database = SerializationService.Deserialize<AegisDecryptedDatabase>(json);

                        return Task.FromResult((CreateAccountCollection(database), true));
                    }
                    else
                    {
                        return Task.FromResult((new List<TwoFACodeModel>(), false));
                    }
                }
                else
                {
                    return Task.FromResult((new List<TwoFACodeModel>(), false));
                }
            }
        }

        private List<TwoFACodeModel> CreateAccountCollection(AegisDecryptedDatabase database)
        {
            List<TwoFACodeModel> accountList = new List<TwoFACodeModel>();
            for (int i = 0; i < database.Entries.Count; i++)
            {
                if (database.Entries[i].Type == OTPType.totp.ToString() || database.Entries[i].Type == OTPType.steam.ToString())
                {
                    var model = new TwoFACodeModel
                    {
                        Label = database.Entries[i].Name,
                        TotpSize = database.Entries[i].Info.Digits,
                        Issuer = database.Entries[i].Issuer,
                        Period = database.Entries[i].Info.Period,
                        HashMode = database.Entries[i].Info.HashMode,
                        SecretByteArray = Encoding.UTF8.GetBytes(database.Entries[i].Info.Secret),
                        AccountIconName = DataService.Instance.GetIconForLabel(database.Entries[i].Name.ToLower())
                    };
                    if (string.IsNullOrWhiteSpace(model.Issuer))
                    {
                        model.Issuer = database.Entries[i].Name;
                    }
                    if (database.Entries[i].Type == OTPType.steam.ToString())
                    {
                        model.OTPType = OTPType.steam.ToString();
                    }
                    accountList.Add(model);
                }
                else
                {
                    accountList.Add(new TwoFACodeModel
                    {
                        Label = database.Entries[i].Name,
                        Issuer = database.Entries[i].Issuer,
                        AccountIconName = DataService.Instance.GetIconForLabel(database.Entries[i].Name.ToLower()),
                        IsEnabled = false,
                        IsChecked = false
                    });
                }
            }
            return accountList;
        }

        private byte[] DecryptSlot(AegisSlot slot, byte[] password, string algorithm)
        {
            var saltBytes = Hex.Decode(slot.Salt);
            var derivedKey = SCrypt.Generate(password, saltBytes, slot.N, slot.R, slot.P, 32);

            var ivBytes = Hex.Decode(slot.KeyParams.Nonce);
            var keyBytes = Hex.Decode(slot.Key);
            var macBytes = Hex.Decode(slot.KeyParams.Tag);

            return DecryptAesGcm(derivedKey, ivBytes, keyBytes, macBytes, algorithm);
        }

        private byte[] DecryptAesGcm(byte[] key, byte[] iv, byte[] data, byte[] mac, string algorithm)
        {
            var keyParameter = new ParametersWithIV(new KeyParameter(key), iv);
            var cipher = CipherUtilities.GetCipher(algorithm);
            cipher.Init(false, keyParameter);

            var authenticatedBytes = GetAuthenticatedBytes(data, mac);
            return cipher.DoFinal(authenticatedBytes);
        }


        private byte[] GetAuthenticatedBytes(byte[] payload, byte[] mac)
        {
            var result = new byte[payload.Length + mac.Length];
            System.Buffer.BlockCopy(payload, 0, result, 0, payload.Length);
            System.Buffer.BlockCopy(mac, 0, result, payload.Length, mac.Length);
            return result;
        }

        private byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
