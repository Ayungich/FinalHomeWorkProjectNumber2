using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WorkWithCSV
{
    public static class CsvProcessing
    {
        static string? fPath; // Статическое поле для хранения пути к файлу

        public const int currentColumnsCount = 13;
        public static void SetFilePath(string? filePath) => fPath = filePath;
        
        public static bool DataCheck(in string[]? lines)
        {
            if (lines is null || lines.Length == 0) return false;

            int count = Checkers.SkipHeader(lines);

            for (int i = count; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(';');

                for (int j = 0; j < parts.Length; j++)
                {
                    parts[j] = parts[j].Trim('"');
                }

                if (parts.Length - 1 != currentColumnsCount) return false;

                CultureInfo cultureInfo = CultureInfo.InvariantCulture;

                if (!int.TryParse(parts[0], out _) ||
                    !long.TryParse(parts[2], out _) ||
                    !double.TryParse(parts[7], NumberStyles.Any, cultureInfo, out _) ||
                    !double.TryParse(parts[8], NumberStyles.Any, cultureInfo, out _) ||
                    !int.TryParse(parts[9], out _))
                {
                    return false;
                }
            }

            return true;
        }

        public static string CorrectStringMaker(string? inputString)
        {
            if (string.IsNullOrEmpty(inputString)) throw new ArgumentNullException("Input string is empty.");

            string[] fields = inputString.Split(';');

            if (fields.Length - 1 != currentColumnsCount) throw new ArgumentException("Incorrect data");

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            if (!int.TryParse(fields[0], out _) ||
                        !long.TryParse(fields[2], out _) ||
                        !double.TryParse(fields[7], NumberStyles.Any, cultureInfo, out _) ||
                        !double.TryParse(fields[8], NumberStyles.Any, cultureInfo, out _) ||
                        !int.TryParse(fields[9], out _))
            {
                throw new ArgumentException("Incorrect data.");
            }

            string outputString = string.Join(";", fields.Select(field => $"\"{field.Trim()}\"")) + ";";
            return outputString.TrimEnd(';', ' ').TrimEnd('"');
        }

        public static string CorrectStringMaker(in string[]? inputData)
        {
            if (inputData is null || inputData.Length == 0) throw new ArgumentNullException("Input string is empty");
            if (inputData.Length != currentColumnsCount) throw new ArgumentNullException("Invalid data in string");

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            if (!int.TryParse(inputData[0], out _) ||
                       !long.TryParse(inputData[2], out _) ||
                       !double.TryParse(inputData[7], NumberStyles.Any, cultureInfo, out _) ||
                       !double.TryParse(inputData[8], NumberStyles.Any, cultureInfo, out _) ||
                       !int.TryParse(inputData[9], out _))
            {
                throw new ArgumentException("Incorrect data.");
            }

            string outputString = string.Join(";", inputData.Select(field => $"\"{field.Trim()}\"")) + ";";

            return outputString;
        }

        public static string[] Read()
        {
            if (!File.Exists(fPath) || string.IsNullOrEmpty(fPath))
                throw new ArgumentNullException("The file does not exist");

            string[] csvFileLines = File.ReadAllLines(fPath);

            if (csvFileLines is null || csvFileLines.Length == 0) 
                throw new ArgumentNullException();

            for (int i = 0; i < csvFileLines.Length; i++)
            {
                csvFileLines[i].Split(';');
            }

            if (!DataCheck(in csvFileLines)) 
                throw new ArgumentException("Invalid data in file");

            return csvFileLines;
        }

        public static readonly string[] headerEng = { "ID", "Name", "global_id", "AdmArea", "District", "Address", "LocationDescription",
                "Longitude_WGS84", "Latitude_WGS84", "CarCapacity", "Mode", "geodata_center", "geoarea"};

        public static readonly string[] headerRu = { "Локальный идентификатор", "Наименование", "global_id", "Административный округ по адресу",
            "Район", "Адресный ориентир", "Уточнение местоположения", "Долгота в WGS-84", "Широта в WGS-84",
            "Количество парковочных мест", "Режим работы", "geodata_center", "geoarea"};

        public static void WriteHeader()
        {
            string completeHeaderEng = string.Join(";", headerEng.Select(field => $"\"{field.Trim()}\"")) + ";";
            string completeHeaderRu = string.Join(";", headerRu.Select(field => $"\"{field.Trim()}\"")) + ";";

            File.AppendAllText(fPath!, completeHeaderEng + Environment.NewLine);
            File.AppendAllText(fPath!, completeHeaderRu + Environment.NewLine);
        }

        public static void WriteHeader(string? inputText)
        {
            if (string.IsNullOrEmpty(inputText)) 
                    throw new ArgumentNullException("The file path is null or empty.");

            string completeHeaderEng = string.Join(";", headerEng.Select(field => $"\"{field.Trim()}\"")) + ";";
            string completeHeaderRu = string.Join(";", headerRu.Select(field => $"\"{field.Trim()}\"")) + ";";

            File.AppendAllText(inputText, completeHeaderEng + Environment.NewLine);
            File.AppendAllText(inputText, completeHeaderRu + Environment.NewLine);
        }

        public static void Write(string? inputData, string? newPath)
        {
            if (string.IsNullOrEmpty(inputData) || string.IsNullOrEmpty(newPath))
                throw new ArgumentException("The file path cannot be empty or null.");

            if (!File.Exists(newPath))
            {
                var fileStream = File.Create(newPath);
                WriteHeader(newPath);
                fileStream.Close();
            }

            File.AppendAllText(newPath, CorrectStringMaker(inputData) + Environment.NewLine);
        }

        public static void Write(in string[] inputData)
        {
            if (inputData is null || inputData.Length == 0)
                throw new ArgumentException("Input data is null or empty");

            if (!File.Exists(fPath))
            {
                File.Create(fPath!);
                WriteHeader();
                File.AppendAllText(fPath!, CorrectStringMaker(in inputData) + Environment.NewLine);
            }

            File.WriteAllText(fPath!, string.Empty);
            WriteHeader();
            File.AppendAllText(fPath!, CorrectStringMaker(in inputData) + Environment.NewLine);
        }
    }
    public static class DataProcessing
    {
        public static void SelectByField(in string[] inputData, string? fieldName, string? value)
        {
            ValidateInput(inputData, fieldName, value);

            string[] headerRu = ProcessHeader(inputData[1]);
            string[] header = ProcessHeader(inputData[0]);
            int columnIndex = Array.IndexOf(header, fieldName);

            if (columnIndex == -1)
                throw new ArgumentException($"Field {fieldName} not found");

            try
            {
                int count = Checkers.SkipHeader(in inputData);

                for (int i = count; i < inputData.Length; i++)
                {
                    string[] fields = ProcessFields(inputData[i]);

                    if (fields.Length > 0 && fields[columnIndex] == value)
                    {
                        PrintResults(in headerRu, in fields);
                    }
                }
            }
            finally
            {
                Console.ResetColor();
            }
        }
        public static void SelectByField(in string[] inputData, string? fieldNameFirst, string? fieldNameSecond, 
            string? valueFirst, string? valueSecond)
        {
            ValidateInput(inputData, fieldNameFirst, fieldNameSecond, valueFirst, valueSecond);

            string[] headerRu = ProcessHeader(inputData[1]);
            string[] header = ProcessHeader(inputData[0]);

            int columnIndexFirst = Array.IndexOf(header, fieldNameFirst);
            int columnIndexSecond = Array.IndexOf(header, fieldNameSecond);

            if (columnIndexFirst == -1 || columnIndexSecond == -1)
                    throw new ArgumentException($"Field not found.");

            try
            {
                int count = Checkers.SkipHeader(in inputData);

                for (int i = count; i < inputData.Length; i++)
                {
                    string[] fields = ProcessFields(inputData[i]);

                    if (fields.Length > 0 && 
                        (fields[columnIndexFirst] == valueFirst && 
                        fields[columnIndexSecond] == valueSecond))
                    {
                        PrintResults(in headerRu, in fields);
                    }
                }
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static void ValidateInput(in string[] inputData, string? fieldName, string? value)
        {
            if (inputData is null || 
                inputData.Length == 0 || 
                string.IsNullOrEmpty(fieldName) || 
                string.IsNullOrEmpty(value))
                throw new ArgumentException("Invalid data.");
        }

        private static void ValidateInput(in string[] inputData, string? fieldNameFirst, 
            string? fieldNameSecond, string? valueFirst, string? valueSecond)
        {
            if (inputData is null || 
                inputData.Length == 0 ||
                string.IsNullOrEmpty(fieldNameFirst) || 
                string.IsNullOrEmpty(fieldNameSecond) || 
                string.IsNullOrEmpty(valueFirst) || 
                string.IsNullOrEmpty(valueSecond))
                throw new ArgumentException("Invalid data.");
        }

        private static void ValidateInput(in string[] inputData, string? fieldName)
        {
            if (inputData is null ||
                inputData.Length == 0 ||
                string.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Invalid data.");
        }

        private static string[] ProcessHeader(string header)
        {
            string[] processedHeader = header.Split(';');
            Array.Resize(ref processedHeader, processedHeader.Length - 1);

            for (int j = 0; j < processedHeader.Length; j++)
            {
                processedHeader[j] = processedHeader[j].Trim('"');
            }

            return processedHeader;
        }

        private static string[] ProcessFields(string field)
        {
            string[] processedFields = field.Split(';');
            Array.Resize(ref processedFields, processedFields.Length - 1);

            for (int j = 0; j < processedFields.Length; j++)
            {
                processedFields[j] = processedFields[j].Trim('"');
            }

            return processedFields;
        }

        private static void PrintResults(in string[] headerEng, in string[] fields)
        {
            if (fields.Length == 13)
            {
                Console.WriteLine("-------------------------------------------------------------------------------------");

                for (int k = 0; k < fields.Length; k++)
                {
                    if (k == 6 || k == 11 || k == 12)
                        continue;

                    Console.WriteLine($"{headerEng[k]}: {fields[k]}");
                    Console.WriteLine("-------------------------------------------------------------------------------------");
                }

                Console.WriteLine();
            }

            else if (fields.Length == 14)
            {
                Console.WriteLine("-------------------------------------------------------------------------------------");

                for (int k = 0; k < fields.Length - 1; k++)
                {
                    if (k == 6 || k == 11 || k == 12)
                        continue;

                    Console.WriteLine($"{headerEng[k]}: {fields[k]}");
                    Console.WriteLine("-------------------------------------------------------------------------------------");
                }

                Console.WriteLine();
            }
        }
        public static void SortByField(in string[] inputData, string? fieldName, bool descendingOrder)
        {
            ValidateInput(in inputData, fieldName);
            
            string[] headerRu = ProcessHeader(inputData[1]);
            string[] header = ProcessHeader(inputData[0]);
            
            int columnIndex = Array.IndexOf(header, fieldName);
            int count = Checkers.SkipHeader(in inputData);

            if (columnIndex == -1)
                throw new ArgumentException($"Field {fieldName} not found");

            // Фильтрация и сортировка данных
            var filteredAndSortedData = inputData
                .Skip(count)
                .Select(line => line.Split(';').Select(field => field.Trim('"')).ToArray())
                .Where(fields => fields.Length > columnIndex)
                .ToArray();

            if (descendingOrder)
            {
                filteredAndSortedData = filteredAndSortedData
                    .OrderByDescending(fields => fields[columnIndex])
                    .ToArray();
            }
            else
            {
                filteredAndSortedData = filteredAndSortedData
                    .OrderBy(fields => fields[columnIndex])
                    .ToArray();
            }

            // Вывод отсортированных данных
            foreach (var fields in filteredAndSortedData)
            {
                PrintResults(in headerRu, in fields);
            }
        }
    }

    public class CustomException : Exception
    {
        public CustomException() : base() { }

        public CustomException(string message) : base(message) { }

        public CustomException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class CustomLogger
    {
        static string? fullLoggingFilePath;
        public static void SetFilePath(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) 
                    throw new ArgumentNullException("File path is null or empty.");

            fullLoggingFilePath = filePath;
        }
        public async Task LogAsync(bool loggingIsEnabled, Exception? exception)
        {
            if (exception is null || !loggingIsEnabled)
                return;

            var logMessage = $"{DateTime.Now}: {exception.Message}; {exception.Source}; {exception.StackTrace}";

            using StreamWriter sw = new StreamWriter(fullLoggingFilePath!, true);
            await sw.WriteLineAsync(logMessage + Environment.NewLine);
        }

        public static void LogFileClear() => File.Delete(fullLoggingFilePath!);
    }

    public class AdditionalFunctionality
    {
        public static void WriteLineColor(string? inputMessage, ConsoleColor color)
        {
            if (string.IsNullOrEmpty(inputMessage)) 
                    throw new ArgumentException("Input message is null or empty.");

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(inputMessage);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteColor(string? inputMessage, ConsoleColor color)
        {
            if (string.IsNullOrEmpty(inputMessage))
                throw new ArgumentException("Input message is null or empty.");

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(inputMessage);
            Console.ForegroundColor = originalColor;
        }

        public static void SlowWriteLine(string? inputMessage, ConsoleColor color)
        {
            if (string.IsNullOrEmpty(inputMessage)) 
                    throw new ArgumentNullException("Input message is null or empty.");

            foreach (var c in  inputMessage)
            {
                Thread.Sleep(30);
                WriteColor(c.ToString(), color);
            }
        }

        public static string[] ArrayDone()
        {
            string[] inputData = new string[CsvProcessing.currentColumnsCount];

            for (int i = 0; i < inputData.Length; i++)
            {
                inputData[i] = Checkers.StringIsNullOrEmptyCheck($"Enter value({CsvProcessing.headerEng[i]}): ");
            }

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            if (!int.TryParse(inputData[0], out _) ||
                    !long.TryParse(inputData[2], out _) ||
                    !double.TryParse(inputData[7], NumberStyles.Any, cultureInfo, out _) ||
                    !double.TryParse(inputData[8], NumberStyles.Any, cultureInfo, out _) ||
                    !int.TryParse(inputData[9], out _))
            {
                throw new ArgumentException("The data is incorrect.");
            }

            return inputData;
        }
    }

    public class Checkers
    {
        public static bool FileExtensionCheck(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) 
                throw new ArgumentNullException("The file name is null or empty.");

            if (!fileName.EndsWith(".csv")) return false;

            return true;


        }
        public static bool ValidateHeader()
        {
            string[] inputData = CsvProcessing.Read();

            string completeHeaderEng = string.Join(";", CsvProcessing.headerEng.Select(field => $"\"{field.Trim()}\"")) + ";";
            string completeHeaderRu = string.Join(";", CsvProcessing.headerRu.Select(field => $"\"{field.Trim()}\"")) + ";";

            if (string.Compare(inputData[0], completeHeaderEng) != 0 || 
                string.Compare(inputData[0], completeHeaderEng) != 0) 
                return false;

            return true;
        }

        public static bool IsValidAbsolutePath(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            if (!Path.IsPathRooted(filePath)) return false;
            if (!File.Exists(filePath)) return false;

            var fileName = Path.GetFileName(filePath);
            var directoryPath = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(directoryPath)) return false;

            byte[] pathBytes = Encoding.Default.GetBytes(filePath);
            // Проверка наличия кириллических символов в пути
            foreach (byte b in pathBytes)
            {
                // Диапазон кодов для кириллических символов
                if (b >= 0xC0 && b <= 0xFF) return true;
            }

            foreach (var symbol in Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(symbol)) return false;
            }

            foreach (var symbol in Path.GetInvalidPathChars())
            {
                if (directoryPath.Contains(symbol)) return false;
            }

            return true;
        }

        public static bool IsValidNewPath(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            if (!Path.IsPathRooted(filePath)) return false;

            var fileName = Path.GetFileName(filePath);
            var directoryPath = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(directoryPath)) return false;

            byte[] pathBytes = Encoding.Default.GetBytes(filePath);
            // Проверка наличия кириллических символов в пути
            foreach (byte b in pathBytes)
            {
                // Диапазон кодов для кириллических символов
                if (b >= 0xC0 && b <= 0xFF) return true;
            }

            foreach (var symbol in Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(symbol)) return false;
            }

            foreach (var symbol in Path.GetInvalidPathChars())
            {
                if (directoryPath.Contains(symbol)) return false;
            }

            return true;
        }

        public static int SkipHeader(in string[] lines)
        {
            int count = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(';');

                if (!int.TryParse(parts[0].Replace("\"", ""), out _)) count++;
            }

            return count;
        }
        public static string StringIsNullOrEmptyCheck(string outputText) // Проверка для строк
        {
            Console.WriteLine(outputText);
            bool IsNullOrEmpty;
            string output;
            do
            {
                output = Console.ReadLine()!;
                IsNullOrEmpty = string.IsNullOrEmpty(output);
                if (IsNullOrEmpty)
                {
                    AdditionalFunctionality.WriteLineColor("Error: The file path is not set! Please try again...", ConsoleColor.DarkRed);
                }
            } while (IsNullOrEmpty);
            return output;
        }
    }
}