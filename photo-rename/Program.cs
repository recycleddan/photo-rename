using Mono.Options;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace photo_rename
{
    class Program
    {
        private const int PropertyTagDateTime = 0x0132;
        private static ASCIIEncoding Encoding = new ASCIIEncoding();

        static void Main(string[] args)
        {
            var inputDir = Directory.GetCurrentDirectory();
            var outputDir = Directory.GetCurrentDirectory();
            bool copy = false;
            bool recursive = false;

            var s = new OptionSet()
            {
                {"input=|in=", v => inputDir = v },
                {"output=|out=", v => outputDir = v },
                {"copy|c", v => copy = (v != null) },
                {"recursive|r", v => recursive = (v != null) }
            };

            s.Parse(args);

            var inputFiles = Directory.GetFiles(inputDir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            // Default output format is %outputdir%\{year}\{month}\{day}\{filename}
            // TODO: Copy files to output - optional

            foreach (var file in inputFiles)
            {
                try
                {
                    var imageDate = ExtractDateTime(file);

                    var imagePath = Path.Combine(
                                        outputDir,
                                        imageDate.Year.ToString(CultureInfo.InvariantCulture),
                                        imageDate.Month.ToString("D2", CultureInfo.InvariantCulture),
                                        imageDate.Day.ToString("D2", CultureInfo.InvariantCulture));
                    Directory.CreateDirectory(imagePath);

                    // File.Copy requires a full path
                    File.Copy(file, Path.Combine(imagePath, Path.GetFileName(file)));
                    if(!copy)
                    {
                        File.Delete(file);
                    }
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
    }
}
