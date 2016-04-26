using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Assets.Scripts.IO {
    public class FileLoader {
        private static readonly string currentDirectory = Directory.GetCurrentDirectory();
        private static readonly string statementsDirectory = currentDirectory + @"\Assets\Statements\";
        
        public static List<string> GetLocations() { return File.ReadAllLines(statementsDirectory + @"\variables\locations.txt").ToList(); }
        public static List<string> GetNames() { return File.ReadAllLines(statementsDirectory + @"\variables\names.txt").ToList(); }
        public static List<string> GetParts() { return File.ReadAllLines(statementsDirectory + @"\variables\parts.txt").ToList(); }

        public static List<string> GetTemplates(ClueIdentifier identifier) {
            string filePath = @"templates\";
            string finalPath = @"D:\Projects\MSU\MSU-Game\Nomoreno\Deceptional\Assets\Statements\templates\";
            switch (identifier) {
                case ClueIdentifier.Accusatory:     filePath += @"accusation.txt"; break;
                case ClueIdentifier.Descriptive:    filePath += @"description.txt"; break;
                case ClueIdentifier.MurderLocation: filePath += @"murderLocation.txt"; break;
                case ClueIdentifier.PeopleLocation: filePath += @"peopleLocation.txt"; break;
                case ClueIdentifier.Pointer:        filePath += @"specificPointer.txt"; break;
                default: throw new Exception();
            }

            //return File.ReadAllLines(statementsDirectory + filePath).ToList();
            return File.ReadAllLines(statementsDirectory + filePath).ToList();
        }
    }
}
