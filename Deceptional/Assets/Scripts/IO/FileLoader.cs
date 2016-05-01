﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Assets.Scripts.IO {
    public class FileLoader {
        //private static readonly string statementsDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"\Assets\Statements");

        /// <summary>
        /// Opens the file containing location names and reads them into a string list, seperating on new-line characters.
        /// </summary>
        /// <returns>A list of all defined locations.</returns>
        public static List<string> GetLocations() {
            // Loads the text document as a TextAsset, Unity's text-file object.
            var locations = UnityEngine.Resources.Load<UnityEngine.TextAsset>(@"Statements/variables/locations");
            // Splits the text document at linebreaks, removes empty entries and returns the data as a list.
            return locations.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        /// <summary>
        /// Opens the file containing NPC names and reads them into a string list, seperating on new-line characters.
        /// Differentiates between male and female names.
        /// </summary>
        /// <returns>A list of all defined NPC names</returns>
        public static List<string> GetNames(bool maleNames) {
            var names = UnityEngine.Resources.Load<UnityEngine.TextAsset>(@"Statements/variables/names");
            string[] namesArray = names.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (maleNames) {
                return namesArray.Where(s => s.StartsWith("[M]")).Select(s => s.Replace("[M]", "").Trim()).ToList();
            } else {
                return namesArray.Where(s => s.StartsWith("[F]")).Select(s => s.Replace("[F]", "").Trim()).ToList();
            }
        }
        public static List<string> GetParts() {
            var parts = UnityEngine.Resources.Load<UnityEngine.TextAsset>(@"Statements/variables/parts");
            return parts.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static List<string> GetTemplates(ClueIdentifier identifier) {
            string filePath = @"Statements/templates/";
            switch (identifier) {
                case ClueIdentifier.Accusatory:     filePath += @"accusation"; break;
                case ClueIdentifier.Descriptive:    filePath += @"description"; break;
                case ClueIdentifier.MurderLocation: filePath += @"murderLocation"; break;
                case ClueIdentifier.PeopleLocation: filePath += @"peopleLocation"; break;
                case ClueIdentifier.Pointer:        filePath += @"specificPointer"; break;
                default: throw new Exception();
            }

            var templates = UnityEngine.Resources.Load<UnityEngine.TextAsset>(filePath);
            return templates.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
