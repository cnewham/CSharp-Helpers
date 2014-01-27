using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CSharpHelpers
{
    public static class XmlSerializer
    {

        public static T DeserializeXml<T>(string xmlString)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (var xmlStringReader = new StringReader(xmlString))
                return (T)serializer.Deserialize(xmlStringReader);
        }

        public static string SerializeXml<T>(T obj)
        {
            //Serialize the object
            var xmlize = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

            xmlize.Serialize(xmlTextWriter, obj);

            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;

            string XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());

            //Close all the memory objects
            xmlTextWriter.Close();
            //xmlTextWriter.Flush();
            memoryStream.Close();
            memoryStream.Dispose();

            //replace the tab charecter
            XmlizedString = XmlizedString.Trim();
            ﻿XmlizedString = XmlizedString.Replace("﻿", "");

             return XmlizedString;
        }

        public static String UTF8ByteArrayToString(Byte[] characters)
        {

            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        public static Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }
    }
}
