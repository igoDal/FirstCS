using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class User
    {
        [JsonProperty("Username")]
        public string Userame { get; set; }
        [JsonProperty("Password")]
        public string Password { get; set; }
        [JsonProperty("Role")]
        public string Role { get; set; }
    }
}
