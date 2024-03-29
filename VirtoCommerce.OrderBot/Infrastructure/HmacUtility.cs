﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VirtoCommerce.OrderBot.Infrastructure
{
    public class HmacUtility
    {
        public const string NameValueSeparator = "=";
        public const string ParameterSeparator = "&";

        public static string GetHashString(Func<byte[], HMAC> hmacFactory, string secretKey, NameValuePair[] parameters)
        {
            var data = BuildDataString(parameters);
            return ComputeHash(hmacFactory, secretKey, data);
        }
        public static string ComputeHash(Func<byte[], HMAC> hmacFactory, string secretKey, string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var keyBytes = ConvertHexStringToBytes(secretKey);
            return ComputeHash(hmacFactory, keyBytes, dataBytes);
        }

        public static string ComputeHash(Func<byte[], HMAC> hmacFactory, byte[] secretKey, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (var hmac = hmacFactory(secretKey))
            {
                var hash = hmac.ComputeHash(data, 0, data.Length);
                return ConvertBytesToHexString(hash);
            }
        }

        private static string BuildDataString(NameValuePair[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            var builder = new StringBuilder();

            var orderedParameters =
                parameters.Where(p => string.IsNullOrEmpty(p.Name) && !string.IsNullOrEmpty(p.Value))
                    .Union(
                        parameters.Where(p => !string.IsNullOrEmpty(p.Name) && !string.IsNullOrEmpty(p.Value))
                            .OrderBy(i => i.Name))
                    .ToList();

            foreach (var parameter in orderedParameters)
            {
                if (builder.Length > 0)
                {
                    builder.Append(ParameterSeparator);
                }

                if (!string.IsNullOrEmpty(parameter.Name))
                {
                    builder.Append(parameter.Name);
                    builder.Append(NameValueSeparator);
                }

                builder.Append(parameter.Value);
            }

            var data = builder.ToString();
            return data;
        }

        private static byte[] ConvertHexStringToBytes(string hexString)
        {
            return
                Enumerable.Range(0, hexString.Length)
                    .Where(i => i % 2 == 0)
                    .Select(i => Convert.ToByte(hexString.Substring(i, 2), 16))
                    .ToArray();
        }

        private static string ConvertBytesToHexString(byte[] bytes)
        {
            var builder = new StringBuilder();

            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }

    public class NameValuePair
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public NameValuePair(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("[");
            builder.Append(Name);
            builder.Append(", ");
            builder.Append(Value);
            builder.Append("]");
            return builder.ToString();
        }
    }

    public class ApiRequestSignature
    {
        public string AppId { get; set; }
        public string Hash { get; set; }
        public DateTime Timestamp { get; private set; }
        public string TimestampString { get; private set; }

        public ApiRequestSignature()
        {
            Timestamp = DateTime.UtcNow;
            TimestampString = Timestamp.ToString("o", CultureInfo.InvariantCulture);
        }

        public static bool TryParse(string input, out ApiRequestSignature parsedValue)
        {
            parsedValue = null;
            var success = false;

            if (input != null)
            {
                var parts = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3)
                {
                    if (parts[2].Length == 64)
                    {
                        DateTime timestamp;
                        if (DateTime.TryParseExact(
                            parts[1],
                            "o",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AdjustToUniversal,
                            out timestamp))
                        {
                            parsedValue = new ApiRequestSignature
                            {
                                AppId = parts[0],
                                TimestampString = parts[1],
                                Hash = parts[2],
                                Timestamp = timestamp,
                            };
                            success = true;
                        }
                    }
                }
            }

            return success;
        }

        public override string ToString()
        {
            return string.Join(";", AppId, TimestampString, Hash);
        }
    }
}
