using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TABGPatcher.VDF
{
    public class VDFFile
    {
        #region VARIABLES
        private Regex regNested = new Regex(@"\""(.*?)\""");
        private Regex regValuePair = new Regex(@"\""(.*?)\""\s*\""(.*?)\""");
        #endregion

        #region PROPERTIES
        public List<Element> RootElements { get; set; }
        #endregion

        #region CONSTRUCTORS
        public VDFFile(string filePath)
        {
            RootElements = new List<Element>();
            Parse(filePath);
        }
        #endregion

        #region METHODS
        public string ToVDF()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Element child in RootElements)
                builder.Append(child.ToVDF());
            return builder.ToString();
        }
        private void Parse(string filePath)
        {
            Element currentLevel = null;
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    string[] parts = line.Split('"');

                    if (regValuePair.Match(line).Success)
                    {
                        Element subElement = new Element();
                        subElement.Name = parts[1];
                        subElement.Value = parts[3];
                        subElement.Parent = currentLevel;
                        if (currentLevel == null)
                            RootElements.Add(subElement);
                        else
                            currentLevel.Children.Add(subElement);
                    }
                    else if (regNested.Match(line).Success)
                    {
                        Element nestedElement = new Element();
                        nestedElement.Name = parts[1];
                        nestedElement.Parent = currentLevel;
                        if (currentLevel == null)
                            RootElements.Add(nestedElement);
                        else
                            currentLevel.Children.Add(nestedElement);
                        currentLevel = nestedElement;
                    }
                    else if (line == "}")
                    {
                        currentLevel = currentLevel.Parent;
                    }
                    /*else if (line == "{")
                    {
                        //Nothing to do here
                    }*/
                }
            }
        }
        #endregion

        #region OPERATORS
        public Element this[int key]
        {
            get
            {
                return RootElements[key];
            }
        }
        public Element this[string key]
        {
            get
            {
                return RootElements.First(x => x.Name == key);
            }
        }
        #endregion
    }
}
