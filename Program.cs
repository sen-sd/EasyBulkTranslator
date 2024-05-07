using Google.Cloud.Translation.V2;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        string sourceDirectory = @"C:\Work\Sen\Projects\Source";
        string targetDirectory = @"C:\Work\Sen\Projects\Dest";
        string apiKey = ""; // Fill your key
        TranslationClient client = TranslationClient.CreateFromApiKey(apiKey);
        string targetLanguage = "en"; // Set your target language code

        foreach (string file in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
        {
            // Determine if the file is a Markdown file
            bool isMarkdown = Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase);

            // Get the relative directory path for translation
            string relativeDirPath = Path.GetDirectoryName(file.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar));
            string translatedDirPath = TranslateAndFormatPath(relativeDirPath, client, targetLanguage);

            // Construct the new file path, preserving the file name and extension
            string newFilePath = Path.Combine(targetDirectory, translatedDirPath, Path.GetFileName(file));

            // Ensure the target directory exists
            string newFileDirectory = Path.GetDirectoryName(newFilePath);
            Directory.CreateDirectory(newFileDirectory);

            if (isMarkdown)
            {
                // Translate the content of the file line by line
                using (var reader = new StreamReader(file, Encoding.UTF8))
                using (var writer = new StreamWriter(newFilePath, false, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var translatedLine = client.TranslateText(line, targetLanguage).TranslatedText;
                        writer.WriteLine(translatedLine);
                    }
                }
            }
            else
            {
                // Copy non-Markdown files directly
                File.Copy(file, newFilePath, overwrite: true);
            }
        }

        Console.WriteLine("Processing completed.");
    }

    static string TranslateAndFormatPath(string path, TranslationClient client, string targetLanguage)
    {
        var pathSegments = path.Split(Path.DirectorySeparatorChar);
        for (int i = 0; i < pathSegments.Length; i++)
        {
            // Translate the directory names only
            if (i < pathSegments.Length - 1) // Ignore file name for translation
            {
                var translatedSegment = client.TranslateText(pathSegments[i], targetLanguage).TranslatedText;
                translatedSegment = Regex.Replace(translatedSegment, @"\s+", "_");
                pathSegments[i] = $"{pathSegments[i]}({translatedSegment})";
            }
        }

        return string.Join(Path.DirectorySeparatorChar.ToString(), pathSegments);
    }
}
