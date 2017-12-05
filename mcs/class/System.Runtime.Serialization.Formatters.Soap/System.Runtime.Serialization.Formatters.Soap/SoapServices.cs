// created on 24/04/2003 at 15:35
//
//	System.Runtime.Serialization.Formatters.Soap.SoapReader
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

namespace System.Runtime.Remoting
{
    internal class SoapServices
    {
        internal static void DecodeXmlNamespaceForClrTypeNamespace(string namespaceURI, out string typeNamespace, out string assemblyName)
        {
            throw new NotImplementedException();
        }

        internal static string CodeXmlNamespaceForClrTypeNamespace(string typeNamespace, string v)
        {
            throw new NotImplementedException();
        }

        public static string XmlNsForClrType { get; set; }
    }
}