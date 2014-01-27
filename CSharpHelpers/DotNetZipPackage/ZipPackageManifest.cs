using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CSharpHelpers.DotNetZipPackage
{
    [Serializable]
    [XmlType(TypeName = "manifest")]
    public class ZipPackageManifest
    {
        public ZipPackageManifest()
        {
            this.Items = new List<OfflineZipPackageItem<string, ZipPackageItemType>>();
        }

        [XmlElement(ElementName = "version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "items")]
        public List<OfflineZipPackageItem<string, ZipPackageItemType>> Items { get; set; } 


        /// <summary>
        /// Returns collection of items of a package type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<string> GetItemNames(ZipPackageItemType type)
        {
            var items = new List<string>();

            foreach (var item in this.Items)
                if (item.Value == type)
                    items.Add(item.Key);

            return items;
        }

        public bool Exists(ZipPackageItemType type)
        {
            return this.Items.Any(item => item.Value == type);
        }
    }
}
