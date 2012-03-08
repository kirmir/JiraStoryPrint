using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace JiraStoryPrint
{
    /// <summary>
    /// <see cref="DragEventArgs"/> extensions for retrieving file names.
    /// </summary>
    public static class DragEventExtensions
    {
        /// <summary>
        /// Gets the file names with specified extensions from the <see cref="DragEventArgs"/> object.
        /// </summary>
        /// <param name="args">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        /// <param name="extensions">The file name extensions.</param>
        /// <returns>The file names with one of the specified extensions or
        /// <c>null</c> if event data doesn't contain file names.</returns>
        public static IEnumerable<string> GetFileNamesWithExtension(this DragEventArgs args, params string[] extensions)
        {
            var fileNames = args.Data as DataObject;
            if (fileNames == null || !fileNames.ContainsFileDropList())
            {
                return null;
            }

            return fileNames.GetFileDropList()
                            .Cast<string>()
                            .Where(name => fileNameHasExtension(name, extensions));
        }

        /// <summary>
        /// Gets the first file name with specified extensions from the <see cref="DragEventArgs"/> object.
        /// </summary>
        /// <param name="args">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        /// <param name="extensions">The file name extensions.</param>
        /// <returns>The file names with one of the specified extensions or
        /// <c>null</c> if event data doesn't contain file names.</returns>
        public static string GetFirstFileNameWithExtension(this DragEventArgs args, params string[] extensions)
        {
            var fileNames = GetFileNamesWithExtension(args, extensions);
            return fileNames == null ? null : (fileNames.FirstOrDefault() ?? string.Empty);
        }

        /// <summary>
        /// Checks that file name has one of the specified extensions.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="extensions">The file name extensions.</param>
        /// <returns><c>true</c> if file name has one of the specified extensions; otherwise, <c>false</c>.</returns>
        private static bool fileNameHasExtension(string fileName, params string[] extensions)
        {
            var extension = Path.GetExtension(fileName);
            if (extensions == null)
            {
                return string.IsNullOrEmpty(extension);
            }

            extension = string.IsNullOrEmpty(extension) ? string.Empty : extension.Substring(1).ToLower();
            return extensions.Any(ext => ext.ToLower() == extension);
        }
    }
}
