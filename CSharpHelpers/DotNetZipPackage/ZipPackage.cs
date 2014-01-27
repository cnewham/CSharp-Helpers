using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

namespace CSharpHelpers.DotNetZipPackage
{
    /// <summary>
    /// Used for reading and writing of offline zip packages
    /// </summary>
    public class ZipPackage : IDisposable
    {
        #region Constructors
        /// <summary>
        /// Reads an existing extracted temp archive
        /// TODO: Change this to extract files to temp directory and access them directly in the Extract method
        /// </summary>
        /// <param name="tempPath"></param>
        public ZipPackage(string tempPath)
        {
            var ms = new MemoryStream();

            using (var file = new FileStream(tempPath, FileMode.Open))
            {
                file.CopyTo(ms);
            }

            this.Items = ms;
            this.ParseItemManifest();
        }

        /// <summary>
        /// Reads an existing Items
        /// </summary>
        /// <param name="zipPackage"></param>
        public ZipPackage(Stream zipPackage)
        {
            this.Items = zipPackage;
            this.ParseItemManifest();
        }

        /// <summary>
        /// Builds a new Package
        /// </summary>
        public ZipPackage()
        {
            this.Items = new MemoryStream();
            this.ItemManifest = new ZipPackageManifest();
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Zip file path in temp directory
        /// </summary>
        public string TempZipFilePath { get; set; }

        /// <summary>
        /// Directory that stores package content
        /// </summary>
        public string TempDirectory
        {
            get { return Path.GetDirectoryName(this.TempZipFilePath); }
        }

        /// <summary>
        /// Determines if items are being read from the local disk or in memory
        /// </summary>
        private bool TempZipFile { get; set; }

        /// <summary>
        /// Zipped stream of Items
        /// </summary>
        private Stream Items { get; set; }

        /// <summary>
        /// Package manfifest mapping to package type
        /// </summary>
        public ZipPackageManifest ItemManifest { get; set; }
       
        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts item manifest from zip package's comments
        /// </summary>
        private void ParseItemManifest()
        {
            if (this.TempZipFile)
            {
                using (var file = new FileStream(this.TempZipFilePath, FileMode.Open))
                {
                    using (ZipFile zip = ZipFile.Read(file))
                    {
                        this.ParseItemManifest(zip.Comment);
                    }
                }
            }
            else
            {
                using (var manifest = this.Extract("manifest.xml"))
                {
                    using (var reader = new StreamReader(manifest))
                    {
                        this.ParseItemManifest(reader.ReadToEnd());
                    }
                }
            }
        }

        /// <summary>
        /// Parent to a manifest xml file
        /// </summary>
        /// <param name="manifestXml"></param>
        private void ParseItemManifest(string manifestXml)
        {
            try
            {
                this.ItemManifest = XmlSerializer.DeserializeXml<ZipPackageManifest>(manifestXml);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Configuration manifest is missing or corrupted. Please generate new configuration export and try again", ex);
            }
        }

        /// <summary>
        /// Stores item manifest in zip package's comments
        /// </summary>
        private void UpdateItemManifest()
        {
            if (this.TempZipFile)
                throw new ApplicationException("ZipPackage is read-only when reading from temp directory");

            if (this.ItemManifest.Items.Count == 0)
                throw new ApplicationException("No items to add to package manifest");

            var manifestXml = XmlSerializer.SerializeXml(this.ItemManifest);

            using (var ms = new MemoryStream())
            {
                using (ZipFile zip = this.Items.Length == 0 ? new ZipFile() : ZipFile.Read(this.Items.Reset()))
                {
                    if (zip.ContainsEntry("manifest.xml"))
                        zip.UpdateEntry("manifest.xml", XmlSerializer.StringToUTF8ByteArray(manifestXml));
                    else
                        zip.AddEntry("manifest.xml", XmlSerializer.StringToUTF8ByteArray(manifestXml));

                    zip.Save(ms);
                }

                this.Items = new MemoryStream();
                ms.Reset().CopyTo(this.Items);
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Extracts and saves items and manifest to temp directory
        /// </summary>
        public void SaveTempExtract()
        {
            try
            {
                if (this.Items == null || this.Items.Length == 0)
                    throw new ApplicationException("No items to be extracted");

                using (ZipFile zip = ZipFile.Read(this.Items.Reset()))
                {
                    zip.ExtractAll(this.TempDirectory, ExtractExistingFileAction.OverwriteSilently);
                }

                this.TempZipFile = true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Files could not be extracted to temp directory: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Adds an item to Items
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="item"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public void Add(ZipPackageItemType itemType, string itemName, Stream item)
        {
            if (this.TempZipFile)
                throw new ApplicationException("ZipPackage is read-only when reading from temp directory");

            using (var ms = new MemoryStream())
            {
                using (ZipFile zip = this.Items.Length == 0 ? new ZipFile() : ZipFile.Read(this.Items.Reset()))
                {
                    zip.AddEntry(itemName, item.Reset());
                    zip.Save(ms);
                }

                this.Items = new MemoryStream();
                ms.Reset().CopyTo(this.Items);
            }

            this.ItemManifest.Items.Add(new OfflineZipPackageItem<string, ZipPackageItemType>(itemName, itemType));

            this.UpdateItemManifest();
        }
        
        /// <summary>
        /// Returns an open stream from Items for the named item
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public Stream Extract(string itemName)
        {
            Stream ms = new MemoryStream();

            if (this.TempZipFile)
            {
                using (var file = new FileStream(this.TempZipFilePath, FileMode.Open))
                {
                    file.CopyTo(ms);
                }
            }
            else
            {
                using (ZipFile zip = ZipFile.Read(this.Items.Reset()))
                {
                    ZipEntry entry = zip[itemName];
                    if (entry != null)
                        entry.Extract(ms);
                }                
            }

            return ms.Reset();
        }

        /// <summary>
        /// Returns an open stream of the first instance of the item type
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public Stream Extract(ZipPackageItemType itemType)
        {
            foreach (var item in this.ItemManifest.Items)
                if (item.Value == itemType)
                    return this.Extract(item.Key);

            throw new ApplicationException(string.Format("No package items of type \"{0}\" exist in imported file", itemType));
        }

        /// <summary>
        /// Returns a collection of open streams of a specific type
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public IEnumerable<Stream> ExtractAll(ZipPackageItemType itemType)
        {
            foreach (var item in this.ItemManifest.Items)
                if (item.Value == itemType)
                    yield return this.Extract(item.Key);
        }

        /// <summary>
        /// Returns a collection of file names in Items
        /// </summary>
        /// <returns></returns>
        public List<string> GetItemNames()
        {
            var items = new List<string>();

            using (ZipFile zip = ZipFile.Read(this.Items.Reset()))
            {
                foreach (ZipEntry entry in zip.Entries)
                    items.Add(entry.FileName);
            }

            return items;
        }

        /// <summary>
        /// Returns a Byte[] of the zip package
        /// </summary>
        /// <returns>byte[]</returns>
        public byte[] ToArray()
        {
            return (this.Items as MemoryStream).ToArray();
        }

        /// <summary>
        /// Returns the package stream
        /// </summary>
        /// <returns></returns>
        public Stream ToStream()
        {
            return this.Items.Reset();
        }
        
        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (this.Items != null)
                this.Items.Dispose();
        }
        #endregion
    }

    public static class StreamExtenstions
    {
        /// <summary>
        /// Resets read position
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Stream Reset(this Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;

            return stream;
        }
    }

}
