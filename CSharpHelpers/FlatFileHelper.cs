using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpHelpers
{
    public class FlatFileHelper
    {
        #region Constructor
        public FlatFileHelper(string FileName)
        {
            this.FileName = FileName;
            this.Bytes = 0;

            this.GetFileSize();
        }
        #endregion

        #region Properties
        public string FileName { get; set; }
        public int Bytes { get; set; }

        public char Delimiter { get; set; }
        public bool RemoveQuotes { get; set; }
        public bool HasHeaders { get; set; }

        private long _Size;
        public long Size
        {
            get
            {
                return _Size;
            }
        }
        #endregion

        #region Private Methods
        private void GetFileSize()
        {
            FileInfo File = new FileInfo(this.FileName);

            this._Size = File.Length;
        }
        #endregion


        #region Public Methods

        /// <summary>
        /// Reads the file until the end
        /// </summary>
        /// <returns></returns>
        public List<string[]> ReadFile()
        {
            List<string[]> table = new List<string[]>();

            using (StreamReader stream = new StreamReader(this.FileName))
            {
                string line;

                while ((line = stream.ReadLine()) != null)
                {
                    string[] row;
                    if (this.RemoveQuotes) { line.Replace("\"", ""); }

                    row = line.Split(this.Delimiter);

                    table.Add(row);
                }

            }

            return table;
        }

        /// <summary>
        /// Read line by line until EOF. Returns single line of data
        /// </summary>
        /// <returns></returns>
        public string[] ReadLine()
        {
            string[] result;

            using (StreamReader stream = new StreamReader(this.FileName))
            {

                // check HasHeaders flag and discard first line
                if (HasHeaders && this.Bytes == 0)
                {
                    this.Bytes = stream.ReadLine().Length + 2;
                }

                char[] buffer = new char[this.Bytes];
                stream.Read(buffer, 0, this.Bytes);

                string line;

                while ((line = stream.ReadLine()) != null)
                {
                    if (this.RemoveQuotes) { line.Replace("\"", ""); }

                    result = line.Split(this.Delimiter);

                    if (line != null)
                    {
                        this.Bytes += line.Length + 2;
                        return result;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
