using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using log4net.Config;

namespace RPBooksApiTestingFramework.Controllers
{
    static class PropertiesController
    {
        public static readonly ILog Logger = LogManager.GetLogger(typeof(TestController));

        /* This method figurs out the path to properties.ini file */
        public static string GetPropertiesPath()
        {
            //If environment variable is set for PROPERTIES_FILE_PATH, then use that. 
            //Otherwise, default to Application directory. 
            string path = Environment.GetEnvironmentVariable("PROPERTIES_FILE_PATH");
            if (string.IsNullOrEmpty(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory + "\\" + "properties.ini";
                //path = "properties.ini";
            }
            return path;
        }
        
        /* This method loads the content of properties.ini file to a Dictionary, dumps output to console 
         * INPUT    : string directory and file path to the properties file (e.g., properties.ini)
         * OUTPUT   : Dictionary<string, string> set of properties 
         */
        public static Dictionary<string, string> ReadProperties(string propsPath)
        {
            XmlConfigurator.Configure();
            Logger.Info("Reading properties from " + propsPath);
            var data = new Dictionary<string, string>();
            try
            {
                foreach (var row in File.ReadAllLines(propsPath))
                    if(row != "" && row.Contains("="))
                    data.Add(row.Split('=')[0], string.Join("=", row.Split('=').Skip(1).ToArray()));
                //foreach (KeyValuePair<string, string> kv in data)
                //    logger.Debug(kv.Key.ToString() + " = " + kv.Value.ToString());
            }
            catch (Exception e)
            {
                Logger.Error("Error reading properties file. Error message : " + e.Message);
                data = null;
            }
            return data;
        }
    }
}
