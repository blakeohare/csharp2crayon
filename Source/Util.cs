using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon
{
    public static class Util
    {
        public static string ReadFile(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public static string[] GatherFiles(string directory, string extension)
        {
            List<string> output = new List<string>();
            extension = extension.ToLower();
            if (!extension.StartsWith('.'))
            {
                extension = "." + extension;
            }

            GatherFilesImpl(output, directory, "", extension);

            return output.ToArray();
        }

        private static void GatherFilesImpl(List<string> output, string rootDirectory, string relative, string extension)
        {
            string currentDir = rootDirectory;
            string prefix = relative;
            if (relative.Length > 0)
            {
                currentDir = System.IO.Path.Combine(rootDirectory, relative);
                prefix = prefix + System.IO.Path.DirectorySeparatorChar;
            }
            foreach (string fullPath in System.IO.Directory.GetFiles(currentDir))
            {
                string path = System.IO.Path.GetFileName(fullPath);
                if (path.ToLower().EndsWith(extension))
                {
                    output.Add(prefix + path);
                }
            }
            foreach (string fullPath in System.IO.Directory.GetDirectories(currentDir))
            {
                string path = System.IO.Path.GetFileName(fullPath);
                if (path == "bin" || path == "obj") continue;
                GatherFilesImpl(output, rootDirectory, prefix + path, extension);
            }
        }
    }
}
