using System.IO;

namespace RPBooksApiTestingFramework.Controllers
{
    public class JsonController
    {
        /* This method facilitates loading json string given a path to a .json file 
         * INPUT    : string directory path to the json file (filename.json included)
         * OUTPUT   : string json payload
         */
         /// <summary>
         /// some summary
         /// </summary>
         /// <param name="filePath">Enter the file nmae here</param>
         /// <returns>string of json</returns>
        public string LoadJsonAsString(string filePath)
        {
            string json;
            using (StreamReader r = new StreamReader(filePath))
            {
                json = r.ReadToEnd();
            }
            return json;
        }
    }
}