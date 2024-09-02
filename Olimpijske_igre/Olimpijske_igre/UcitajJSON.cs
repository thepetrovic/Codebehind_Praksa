using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Olimpijske_igre
{
    internal  class UcitajJSON
    {
        public string groupLocation = Directory.GetCurrentDirectory() + "\\json\\";
        
        public  T LoadJSON<T>(string file)
        {
            string jsonString = File.ReadAllText(groupLocation + file);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
       
        

    }
}
