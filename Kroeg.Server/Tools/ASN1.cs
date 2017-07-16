﻿using Asn1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Kroeg.Server.Tools
{
    public class ASN1
    {
        public static RSA ToRSA(byte[] data)
        {
            var node = Asn1Node.ReadNode(data);
            Debug.Assert(node.Is(Asn1UniversalNodeType.Sequence));

            var header = node.Nodes[0];
            Debug.Assert(header.Is(Asn1UniversalNodeType.Sequence));

            Debug.Assert((header.Nodes[0] as Asn1ObjectIdentifier).Value == "1.2.840.113549.1.1.1");

            var rsaSequence = Asn1Node.ReadNode((node.Nodes[1] as Asn1BitString).Data);

            var modulus = (rsaSequence.Nodes[0] as Asn1Integer).Value;
            var exponent = (rsaSequence.Nodes[1] as Asn1Integer).Value;
            var prms = new RSAParameters { Modulus = modulus, Exponent = exponent };
            var rsa = RSA.Create();
            rsa.ImportParameters(prms);
            return rsa;
        }

        public static byte[] FromRSA(RSA rsa)
        {
            var prms = rsa.ExportParameters(false);

            var modulus = new Asn1Integer((new byte[] { 0x00 }.Concat(prms.Modulus)).ToArray());
            var exponent = new Asn1Integer(prms.Exponent);

            var oidheader = new Asn1Sequence();
            oidheader.Nodes.Add(new Asn1ObjectIdentifier("1.2.840.113549.1.1.1"));
            oidheader.Nodes.Add(new Asn1Null());

            var rsaSequence = new Asn1Sequence();
            rsaSequence.Nodes.Add(modulus);
            rsaSequence.Nodes.Add(exponent);

            var bitString = new Asn1BitString(rsaSequence.GetBytes());

            var result = new Asn1Sequence();
            result.Nodes.Add(oidheader);
            result.Nodes.Add(bitString);

            return result.GetBytes();
        }
    }
}
