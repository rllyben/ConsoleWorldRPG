using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Models
{
    public class UserAccount
    {
        public const string UserDir = "Data/users/";
        public const string SaveDir = "Data/saves/";
        public string Username { get; set; }
        public string Password { get; set; } // optionally hash later
        public List<string> CharacterNames { get; set; } = new();
    }

}
