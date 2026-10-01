using System.Formats.Asn1;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DevToolsHub.Services;

public sealed record CertificateInfo(
    int Version,
    string SerialNumber,
    string SignatureAlgorithm,
    string Issuer,
    string Subject,
    DateTimeOffset NotBefore,
    DateTimeOffset NotAfter,
    string PublicKeyAlgorithm,
    string? KeySize,
    IReadOnlyList<string> SubjectAltNames,
    bool? IsCa,
    string Sha256Fingerprint,
    string Sha1Fingerprint);

/// <summary>Minimal X.509 parser (X509Certificate2 is not available in browser WebAssembly).</summary>
public static partial class CertificateParser
{
    private static readonly Dictionary<string, string> NameOids = new()
    {
        ["2.5.4.3"] = "CN", ["2.5.4.6"] = "C", ["2.5.4.7"] = "L", ["2.5.4.8"] = "ST",
        ["2.5.4.10"] = "O", ["2.5.4.11"] = "OU", ["2.5.4.5"] = "SERIALNUMBER",
        ["1.2.840.113549.1.9.1"] = "E", ["0.9.2342.19200300.100.1.25"] = "DC",
    };

    private static readonly Dictionary<string, string> AlgorithmOids = new()
    {
        ["1.2.840.113549.1.1.1"] = "RSA",
        ["1.2.840.113549.1.1.5"] = "SHA1withRSA",
        ["1.2.840.113549.1.1.11"] = "SHA256withRSA",
        ["1.2.840.113549.1.1.12"] = "SHA384withRSA",
        ["1.2.840.113549.1.1.13"] = "SHA512withRSA",
        ["1.2.840.113549.1.1.10"] = "RSASSA-PSS",
        ["1.2.840.10045.2.1"] = "EC",
        ["1.2.840.10045.4.3.2"] = "SHA256withECDSA",
        ["1.2.840.10045.4.3.3"] = "SHA384withECDSA",
        ["1.2.840.10045.4.3.4"] = "SHA512withECDSA",
        ["1.3.101.112"] = "Ed25519",
        ["1.3.101.113"] = "Ed448",
    };

    private static readonly Dictionary<string, string> CurveOids = new()
    {
        ["1.2.840.10045.3.1.7"] = "P-256",
        ["1.3.132.0.34"] = "P-384",
        ["1.3.132.0.35"] = "P-521",
    };

    [GeneratedRegex("-----BEGIN CERTIFICATE-----(.*?)-----END CERTIFICATE-----", RegexOptions.Singleline)]
    private static partial Regex PemRegex();

    public static byte[] DecodePem(string text)
    {
        var match = PemRegex().Match(text);
        var body = match.Success ? match.Groups[1].Value : text;
        return Convert.FromBase64String(new string(body.Where(c => !char.IsWhiteSpace(c)).ToArray()));
    }

    public static CertificateInfo Parse(byte[] der)
    {
        var cert = new AsnReader(der, AsnEncodingRules.BER).ReadSequence();
        var tbs = cert.ReadSequence();

        var version = 1;
        var v0 = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        if (tbs.PeekTag().HasSameClassAndValue(v0))
            version = (int)tbs.ReadSequence(v0).ReadInteger() + 1;

        var serial = Convert.ToHexString(tbs.ReadIntegerBytes().Span);
        var sigAlg = ReadAlgorithm(tbs.ReadSequence(), out _);
        var issuer = ReadName(tbs.ReadSequence());

        var validity = tbs.ReadSequence();
        var notBefore = ReadTime(validity);
        var notAfter = ReadTime(validity);

        var subject = ReadName(tbs.ReadSequence());

        var spki = tbs.ReadSequence();
        var keyAlg = ReadAlgorithm(spki.ReadSequence(), out var keyParam);
        var keyBits = spki.ReadBitString(out _);
        var keySize = keyAlg switch
        {
            "RSA" => RsaKeySize(keyBits),
            "EC" => keyParam is not null && CurveOids.TryGetValue(keyParam, out var curve) ? curve : keyParam,
            _ => null,
        };

        var sans = new List<string>();
        bool? isCa = null;
        var ext3 = new Asn1Tag(TagClass.ContextSpecific, 3, isConstructed: true);
        while (tbs.HasData)
        {
            var tag = tbs.PeekTag();
            if (!tag.HasSameClassAndValue(ext3))
            {
                tbs.ReadEncodedValue();
                continue;
            }
            var extensions = tbs.ReadSequence(ext3).ReadSequence();
            while (extensions.HasData)
            {
                var ext = extensions.ReadSequence();
                var oid = ext.ReadObjectIdentifier();
                if (ext.PeekTag().HasSameClassAndValue(Asn1Tag.Boolean)) ext.ReadBoolean();
                var value = ext.ReadOctetString();
                if (oid == "2.5.29.17") sans.AddRange(ReadSubjectAltNames(value));
                else if (oid == "2.5.29.19") isCa = ReadIsCa(value);
            }
        }

        return new CertificateInfo(
            version, serial, sigAlg, issuer, subject, notBefore, notAfter, keyAlg, keySize, sans, isCa,
            Fingerprint(SHA256.HashData(der)), Fingerprint(SHA1.HashData(der)));
    }

    private static string ReadAlgorithm(AsnReader seq, out string? parameterOid)
    {
        var oid = seq.ReadObjectIdentifier();
        parameterOid = seq.HasData && seq.PeekTag().HasSameClassAndValue(Asn1Tag.ObjectIdentifier)
            ? seq.ReadObjectIdentifier()
            : null;
        return AlgorithmOids.TryGetValue(oid, out var name) ? name : oid;
    }

    private static string ReadName(AsnReader name)
    {
        var parts = new List<string>();
        while (name.HasData)
        {
            var set = name.ReadSetOf();
            while (set.HasData)
            {
                var atv = set.ReadSequence();
                var oid = atv.ReadObjectIdentifier();
                var tag = atv.PeekTag();
                string value;
                try
                {
                    value = atv.ReadCharacterString((UniversalTagNumber)tag.TagValue);
                }
                catch (Exception ex) when (ex is AsnContentException or ArgumentException)
                {
                    value = Convert.ToHexString(atv.ReadEncodedValue().Span);
                }
                parts.Add($"{(NameOids.TryGetValue(oid, out var key) ? key : oid)}={value}");
            }
        }
        return string.Join(", ", parts);
    }

    private static DateTimeOffset ReadTime(AsnReader reader) =>
        reader.PeekTag().HasSameClassAndValue(Asn1Tag.UtcTime) ? reader.ReadUtcTime() : reader.ReadGeneralizedTime();

    private static string RsaKeySize(byte[] keyBits)
    {
        var rsa = new AsnReader(keyBits, AsnEncodingRules.BER).ReadSequence();
        var modulus = rsa.ReadInteger();
        return $"{modulus.GetBitLength()} bit";
    }

    private static IEnumerable<string> ReadSubjectAltNames(byte[] value)
    {
        var names = new AsnReader(value, AsnEncodingRules.BER).ReadSequence();
        while (names.HasData)
        {
            var tag = names.PeekTag();
            if (tag.TagClass != TagClass.ContextSpecific)
            {
                names.ReadEncodedValue();
                continue;
            }
            switch (tag.TagValue)
            {
                case 1:
                    yield return "email:" + names.ReadCharacterString(UniversalTagNumber.IA5String, tag);
                    break;
                case 2:
                    yield return "DNS:" + names.ReadCharacterString(UniversalTagNumber.IA5String, tag);
                    break;
                case 6:
                    yield return "URI:" + names.ReadCharacterString(UniversalTagNumber.IA5String, tag);
                    break;
                case 7:
                    var ip = names.ReadOctetString(tag);
                    yield return "IP:" + new System.Net.IPAddress(ip);
                    break;
                default:
                    names.ReadEncodedValue();
                    break;
            }
        }
    }

    private static bool ReadIsCa(byte[] value)
    {
        var seq = new AsnReader(value, AsnEncodingRules.BER).ReadSequence();
        return seq.HasData && seq.PeekTag().HasSameClassAndValue(Asn1Tag.Boolean) && seq.ReadBoolean();
    }

    private static string Fingerprint(byte[] hash) =>
        string.Join(':', hash.Select(b => b.ToString("X2")));
}
