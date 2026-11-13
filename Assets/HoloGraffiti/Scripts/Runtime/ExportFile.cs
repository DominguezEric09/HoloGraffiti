using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace HoloGraffiti
{
    // Shared by the exporters so OBJ and GLB files are named the same way.
    public static class ExportFile
    {
        /// Returns a path like "<directory>/drawing_2026-11-13_14-30-05.obj".
        /// directory defaults to persistentDataPath, the one folder Android always
        /// lets us write to. The folder is created if it doesn't exist yet.
        public static string NewPath(string directory, string extension)
        {
            if (string.IsNullOrEmpty(directory))
                directory = Application.persistentDataPath;

            Directory.CreateDirectory(directory);

            // A timestamp in the name makes each export unique.
            // InvariantCulture ensures the date format is the same on every machine.
            string timestamp = DateTime.Now.ToString(
                "yyyy-MM-dd_HH-mm-ss",
                CultureInfo.InvariantCulture
            );
            return Path.Combine(directory, $"drawing_{timestamp}.{extension}");
        }
    }
}
