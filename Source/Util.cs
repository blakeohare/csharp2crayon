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

        public static string[] GatherFiles(string directory, string extension, ICollection<string> blackListedFiles)
        {
            List<string> output = new List<string>();
            extension = extension.ToLower();
            if (!extension.StartsWith('.'))
            {
                extension = "." + extension;
            }

            GatherFilesImpl(output, directory, "", extension);

            return output
                .Where<string>(path =>
                {
                    string canonicalPath = path.Replace('\\', '/');
                    foreach (string blackListItem in blackListedFiles)
                    {
                        if (canonicalPath.Contains(blackListItem))
                        {
                            return false;
                        }
                    }
                    return true;
                })
                .ToArray();
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

        private static readonly byte[] BUFFER = new byte[500];

        public static string GetTextResource(string path)
        {
            System.Reflection.Assembly asm = typeof(Util).Assembly;
            string name = asm.FullName.Split(',')[0].Trim();
            string asmPath = name + "." + path.Replace('/', '.');
            System.IO.Stream stream = asm.GetManifestResourceStream(asmPath);

            List<byte> builder = new List<byte>();
            int bytesRead = 0;
            do
            {
                bytesRead = stream.Read(BUFFER, 0, BUFFER.Length);
                if (bytesRead == BUFFER.Length) builder.AddRange(BUFFER);
                else
                {
                    for (int i = 0; i < bytesRead; ++i)
                    {
                        builder.Add(BUFFER[i]);
                    }
                }
            } while (bytesRead > 0);

            byte[] bytes;
            if (builder.Count >= 3 && builder[0] == 239 && builder[1] == 187 && builder[2] == 191)
            {
                bytes = builder.Skip(3).ToArray();
            }
            else
            {
                bytes = builder.ToArray();
            }

            string value = System.Text.Encoding.UTF8.GetString(bytes);
            return value;
        }
    }
}
