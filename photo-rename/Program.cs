using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photo_rename
{
    class Program
    {
        private const int PropertyTagDateTime = 0x0132;
        private static ASCIIEncoding Encoding = new ASCIIEncoding();

        static void Main(string[] args)
        {
            // TODO: Accept via command line
            var inputDir = Directory.GetCurrentDirectory();
            // TODO: Accept via command line
            var outputDir = Directory.GetCurrentDirectory();

            var inputFiles = Directory.GetFiles(inputDir);

            // Default output format is %outputdir%\{year}\{month}\{day}\{filename}
            // TODO: Copy files to output - optional

            foreach (var file in inputFiles)
            {
                try
                {
                    var imageDate = ExtractDateTime(file);

                    var imagePath = CombinePath(
                        CombinePath(
                            CombinePath(outputDir, imageDate.Year.ToString(CultureInfo.InvariantCulture))
                            , imageDate.Month.ToString("D2", CultureInfo.InvariantCulture)),
                        imageDate.Day.ToString("D2", CultureInfo.InvariantCulture));

                    // File.Move requires a full path
                    File.Move(file, Path.Combine(imagePath, Path.GetFileName(file)));
                }
                catch (Exception e)
                {
                    // Not an image
                    Debug.WriteLine(e);
                }
            }
        }

        private static DateTime ExtractDateTime(string file)
        {
            using (var fileStream = File.OpenRead(file))
            {
                using (var image = Image.FromStream(fileStream, false, false))
                {
                    var dateTimeProp = image.GetPropertyItem(PropertyTagDateTime);
                    var dateString = Encoding.GetString(dateTimeProp.Value, 0, dateTimeProp.Len - 1);
                    return DateTime.ParseExact(dateString, "yyyy:MM:d H:m:s", CultureInfo.InvariantCulture);
                }
            }
        }

        private static string CombinePath(string root, string sub)
        {
            var path = Path.Combine(root, sub);
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
