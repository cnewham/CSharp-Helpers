using System;
using System.Xml.Serialization;

namespace CSharpHelpers.DotNetZipPackage
{
    [Serializable]
    [XmlType(TypeName = "item")]
    public class OfflineZipPackageItem<TKey,TValue>
    {
        private OfflineZipPackageItem()
        {
        }

        public OfflineZipPackageItem(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        [XmlElement(ElementName = "name")]
        public TKey Key { get; set; }

        [XmlElement(ElementName = "type")]
        public TValue Value { get; set; }
    }
}
