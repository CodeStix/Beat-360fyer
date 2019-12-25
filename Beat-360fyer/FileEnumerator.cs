using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public class FileEnumerator
    {
        public static IEnumerable<string> GetFilesRecursive(string root, string searchPattern)
        {
            string[] files = Directory.GetFiles(root, searchPattern, SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
                yield return null;
            foreach (string file in files)
                yield return file;

            foreach (string dir in Directory.GetDirectories(root))
                foreach (string file in GetFilesRecursive(dir, searchPattern))
                    yield return file;
        }
    }
}
