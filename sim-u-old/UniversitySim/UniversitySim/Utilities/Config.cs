using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniversitySim.Utilities
{
    /// <summary>
    /// This class loads a configuration file from disk.
    /// It basically creates a key value collection of strings.
    /// </summary>
    public class Config
    {
        // The dictionary
        Dictionary<string, Section> configSections;
        
        /// <summary>
        /// Load the configuration file. Store it in configValues.
        /// </summary>
        /// <param name="filename">Path to config file relative to program.</param>
        public Config(string filename)
        {
            configSections = new Dictionary<string, Section>();

            using (var fin = new StreamReader(filename))
            {
                this.configSections[Constants.DEFAULT] = new Section(Constants.DEFAULT);
                string sectionName = Constants.DEFAULT;

                while (!fin.EndOfStream)
                {
                    string line = fin.ReadLine().Trim();

                    // skip comments and blank lines
                    if (line.StartsWith(";") || string.IsNullOrEmpty(line.Trim()))
                    {
                        continue;
                    }

                    // check for section headers
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        string newSection = line.Substring(1, line.Length - 2);

                        if (this.configSections.ContainsKey(newSection))
                        {
                            throw new Exception("CONFIG EXCEPTION: Repeated section header in file " + filename + ". Please remove it. Section: " + newSection);
                        }
                        else
                        {
                            this.configSections[newSection] = new Section(newSection);
                            sectionName = newSection;
                        }
                    }
                    // try parsing it as X=Y
                    else if (line.Contains('='))
                    {
                        int lineSplit = line.IndexOf('=');
                        string key = line.Substring(0, lineSplit);
                        string value = line.Substring(lineSplit + 1);
                        this.configSections[sectionName].Add(key.Trim(), value.Trim());
                    }
                    else
                    {
                        throw new Exception("CONFIG EXCEPTION: Invalid configuration line. Unknown syntax  in file " + filename + " for line: " + line);
                    }
                }
            }
        }

        /// <summary>
        /// Return a list of section names
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> SectionNames()
        {
            return this.configSections.Keys;
        }
        
        /// <summary>
        /// Try to read a configuration value from the file.
        /// </summary>
        /// <param name="name">Name of value</param>
        /// <param name="defaultValue">Default value if the name can't be found or parsed in the file</param>
        /// <returns></returns>
        public string GetStringValue(string name, string defaultValue)
        {
            return GetStringValue(Constants.DEFAULT, name, defaultValue);
        }

        public string GetStringValue(string section, string name, string defaultValue)
        {
            if (!this.configSections.ContainsKey(section))
            {
                return defaultValue;
            }

            return this.configSections[section].GetStringValue(name, defaultValue);
        }

        /// <summary>
        /// Try to read an integer value from the file.
        /// </summary>
        /// <param name="name">Name of value</param>
        /// <param name="defaultValue">Default value if the name can't be found or parsed in the file</param>
        /// <returns></returns>
        public int GetIntValue(string name, int defaultValue)
        {
            return this.GetIntValue(Constants.DEFAULT, name, defaultValue);
        }
        public int GetIntValue(string section, string name, int defaultValue)
        {
            if (!this.configSections.ContainsKey(section))
            {
                return defaultValue;
            }

            return this.configSections[section].GetIntValue(name, defaultValue);
        }

        /// <summary>
        /// Try to read a double value from the file.
        /// </summary>
        /// <param name="name">Name of value</param>
        /// <param name="defaultValue">Default value if the name can't be found or parsed in the file</param>
        /// <returns></returns>
        public double GetDoubleValue(string name, double defaultValue)
        {
            return GetDoubleValue(Constants.DEFAULT, name, defaultValue);
        }
        public double GetDoubleValue(string section, string name, double defaultValue)
        {
            if (!this.configSections.ContainsKey(section))
            {
                return defaultValue;
            }

            return this.configSections[section].GetDoubleValue(name, defaultValue);
        }

        /// <summary>
        /// Try to read a double value from the file.
        /// </summary>
        /// <param name="name">Name of value</param>
        /// <param name="defaultValue">Default value if the name can't be found or parsed in the file</param>
        /// <returns></returns>
        public bool GetBoolValue(string name, bool defaultValue)
        {
            return this.GetBoolValue(Constants.DEFAULT, name, defaultValue);
        }
        public bool GetBoolValue(string section, string name, bool defaultValue)
        {
            if (!this.configSections.ContainsKey(section))
            {
                return defaultValue;
            }

            return this.configSections[section].GetBoolValue(name, defaultValue);
        }

        /// <summary>
        /// This class describes a section in the config
        /// </summary>
        internal class Section
        {
            public string Name { get; set; }

            Dictionary<string, string> configValues;

            /// <summary>
            /// Create a new section
            /// </summary>
            /// <param name="name"></param>
            public Section(string name)
            {
                this.Name = name;

                this.configValues = new Dictionary<string, string>();
            }

            /// <summary>
            /// Add a value to this section
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public void Add(string name, string value)
            {
                this.configValues.Add(name, value);
            }

            /// <summary>
            /// Try to read a configuration value from the file.
            /// </summary>
            /// <param name="name">Name of value</param>
            /// <param name="defaultValue">Default value if the name can't be found or parsed in the file</param>
            /// <returns></returns>
            public string GetStringValue(string name, string defaultValue)
            {
                if (!configValues.ContainsKey(name))
                {
                    return defaultValue;
                }

                return configValues[name];
            }

            /// <summary>
            /// Try to read an integer value from the file.
            /// </summary>
            /// <param name="name">Name of value</param>
            /// <param name="defaultValue">Default value if the name can't be found or parsed in the file</param>
            /// <returns></returns>
            public int GetIntValue(string name, int defaultValue)
            {
                int retVal;
                if (!configValues.ContainsKey(name) || !int.TryParse(configValues[name], out retVal))
                {
                    return defaultValue;
                }

                return retVal;
            }

            /// <summary>
            /// Try to read a double value from the file.
            /// </summary>
            /// <param name="name">Name of value</param>
            /// <param name="defaultValue">Default value if the name can't be found or parsed in the file</param>
            /// <returns></returns>
            public double GetDoubleValue(string name, double defaultValue)
            {
                double retVal;
                if (!configValues.ContainsKey(name) || !double.TryParse(configValues[name], out retVal))
                {
                    return defaultValue;
                }

                return retVal;
            }

            /// <summary>
            /// Try to read a double value from the file.
            /// </summary>
            /// <param name="name">Name of value</param>
            /// <param name="defaultValue">Default value if the name can't be found or parsed in the file</param>
            /// <returns></returns>
            public bool GetBoolValue(string name, bool defaultValue)
            {
                bool retVal;
                if (!configValues.ContainsKey(name) || !bool.TryParse(configValues[name], out retVal))
                {
                    return defaultValue;
                }

                return retVal;
            }
        }
    }
}
