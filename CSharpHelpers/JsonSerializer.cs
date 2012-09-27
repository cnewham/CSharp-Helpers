using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace CSharpHelpers
{
    public static class JsonSerializer
    {
        /// <summary>
        /// Builds JSON string from key-value pair and deserializes to a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Dictionary<string, string> values)
        {
            var serializer = new JavaScriptSerializer();
            var jsonString = new StringBuilder("{");

            foreach (KeyValuePair<string,string> value in values)
            {
                jsonString.Append(FormatValuePair(value));
            }

            //Remove the last comma and close the json
            jsonString.Replace(",", "", jsonString.Length - 1, 1);
            jsonString.Append("}");

            string temp = jsonString.ToString();
            return serializer.Deserialize<T>(temp);
        }

        /// <summary>
        /// Deserialize a well-formed JSON string to a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string jsonString)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Formats key-value pair to well-formed JSON
        /// </summary>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        private static string FormatValuePair(KeyValuePair<string, string> valuePair )
        {
            //TODO: Cleanup illegal characters

            return string.Format("\"{0}\":\"{1}\",", valuePair.Key, valuePair.Value);
        }

    }
}
