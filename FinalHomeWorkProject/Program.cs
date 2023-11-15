// КДЗ №2. Журавель Никита Алексеевич. БПИ238(2). Вариант 16

using WorkWithCSV;

namespace FinalHomeWorkProject
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                
                Console.WriteLine("Enable error logging?(Y|N)");

                ConsoleKeyInfo enableLoggingKey = Console.ReadKey(true);
                bool loggingIsEnabled = false;

                if (enableLoggingKey.Key == ConsoleKey.Y)
                {
                    CustomLogger.SetFilePath(Path.Combine("../../../../", "Logs.txt"));
                    loggingIsEnabled = true;
                    AdditionalFunctionality.SlowWriteLine("Logging is enabled...", ConsoleColor.Green);
                }

                else AdditionalFunctionality.SlowWriteLine("Logging is not enabled...", ConsoleColor.Red);

                Thread.Sleep(500);
                Console.Clear();

                try
                {
                    string filePath = Checkers.StringIsNullOrEmptyCheck("Enter the absolute path to the file: ");

                    if (!Checkers.FileExtensionCheck(Path.GetFileName(filePath))) 
                            throw new ArgumentException("File extension is incorrect.");

                    if (!Checkers.IsValidAbsolutePath(filePath))
                        throw new ArgumentException("File path is not correct.");

                    CsvProcessing.SetFilePath(filePath);
                    string[] dataFromCsvFile = CsvProcessing.Read();

                    if (!Checkers.ValidateHeader()) 
                            throw new ArgumentException("The file contains incorrect data.");

                    AdditionalFunctionality.SlowWriteLine("\nDone...\n", ConsoleColor.Green);
                    Thread.Sleep(150);
                    AdditionalFunctionality.SlowWriteLine($"The file path is: {filePath}", ConsoleColor.Green);
                    Thread.Sleep(500);
                    Console.Clear();

                    AdditionalFunctionality.WriteLineColor("Choose the number of the menu item to start the action: \n", ConsoleColor.Magenta);
                    Console.WriteLine("1. Make a selection by value AdmArea\r\n" +
                                      "2. Make a selection by value CarCapacity\r\n" +
                                      "3. Make a selection by values District и Mode\r\n" +
                                      "4. Sort the table by value CarCapacity (to increase capacity)\r\n" +
                                      "5. Sort the table by value CarCapacity (to reduce the capacity)\r\n" +
                                      "6. Add data to a file(data row)\r\n" +
                                      "7. Overwrite the data file (array with data)\r\n" +
                                      "8. Exit the program\n");

                    ConsoleKeyInfo menuKey = Console.ReadKey(true);
                    switch (menuKey.Key)
                    {
                        case ConsoleKey.D1:
                            Console.Clear();
                            string selectingValueD1 = Checkers.StringIsNullOrEmptyCheck("Enter the value for the selection:");
                            DataProcessing.SelectByField(in dataFromCsvFile, "AdmArea", selectingValueD1);
                            break;

                        case ConsoleKey.D2:
                            Console.Clear();
                            string selectingValueD2 = Checkers.StringIsNullOrEmptyCheck("Enter the value for the selection:");
                            DataProcessing.SelectByField(in dataFromCsvFile, "CarCapacity", selectingValueD2);
                            break;

                        case ConsoleKey.D3:
                            Console.Clear();
                            string selectingValueFirst = Checkers.StringIsNullOrEmptyCheck("Enter the first value for the selection(district):");
                            string selectingValueSecond = Checkers.StringIsNullOrEmptyCheck("Enter the second value for the selection(mode):");
                            Console.WriteLine();
 
                            DataProcessing.SelectByField(in dataFromCsvFile, "District", "Mode", 
                                selectingValueFirst, selectingValueSecond);
                            break;

                        case ConsoleKey.D4:
                            Console.Clear();
                            DataProcessing.SortByField(in dataFromCsvFile, "CarCapacity", false);
                            break;

                        case ConsoleKey.D5:
                            Console.Clear();
                            DataProcessing.SortByField(in dataFromCsvFile, "CarCapacity", true);
                            break;

                        case ConsoleKey.D6:
                            Console.Clear();
                            string newFilePath = Checkers.StringIsNullOrEmptyCheck("Enter new file path:");
                            string fileName = Checkers.StringIsNullOrEmptyCheck("\nEnter file name:");
 
                            if (!Checkers.FileExtensionCheck(fileName)) 
                                    throw new ArgumentException("File extension is incorrect.");

                            string completePath = Path.Combine(newFilePath, fileName);

                            if (!Checkers.IsValidNewPath(completePath))
                                throw new ArgumentException("The new file path is incorrect.");

                            AdditionalFunctionality.SlowWriteLine($"\nFile path: {completePath}", ConsoleColor.Green);

                            Thread.Sleep(500);
                            Console.Clear();
      
                            string inputData = Checkers.StringIsNullOrEmptyCheck("Enter a string in this format: \r\n" +
                                                                                 "ID(int);Name;global_id(long);AdmArea;" +
                                                                                 "District;Address;LocationDescription;" +
                                                                                 "Longitude_WGS84(double);Latitude_WGS84(double);CarCapacity(int);" +
                                                                                 "Mode;geodata_center;geoarea;\n");

                            CsvProcessing.Write(inputData, completePath);
                            AdditionalFunctionality.SlowWriteLine($"\nThe file was successfully written " +
                                $"along the path: {completePath}\n", ConsoleColor.Green);
                            break;
                           
                        case ConsoleKey.D7:
                            Thread.Sleep(150);
                            Console.Clear();

                            Console.WriteLine("Fill in the array with the data: \n");

                            Console.WriteLine("Enter data in this format: \r\n" +
                                              "ID(int);Name;global_id(long);AdmArea;" +
                                              "District;Address;LocationDescription;" +
                                              "Longitude_WGS84(double);Latitude_WGS84(double);CarCapacity(int);" +
                                              "Mode;geodata_center;geoarea;\n");

                            string[] inputUserData = AdditionalFunctionality.ArrayDone();
                            CsvProcessing.Write(in inputUserData);
                            AdditionalFunctionality.WriteLineColor("Data has been successfully recorded", ConsoleColor.Green);
                            break; 

                        case ConsoleKey.D8:
                            AdditionalFunctionality.SlowWriteLine("Exiting program...........", ConsoleColor.Magenta);
                            Environment.Exit(0);
                            break;

                        default:
                            AdditionalFunctionality.WriteLineColor("You haven't chosen anything.", ConsoleColor.Red);
                            break;
                    }
                }
                
                catch (ArgumentException ex)
                {
                    AdditionalFunctionality.WriteLineColor($"\n{ex.Message}", ConsoleColor.Red);

                    if (loggingIsEnabled == true)
                    {
                        CustomLogger log = new();
                        Task.Run(async () => await log.LogAsync(loggingIsEnabled, ex)).Wait();
                    }
                }

                catch (Exception ex)
                {
                    AdditionalFunctionality.WriteLineColor($"\n{ex.Message}", ConsoleColor.Red);

                    if (loggingIsEnabled == true)
                    {
                        CustomLogger log = new();
                        Task.Run(async () => await log.LogAsync(loggingIsEnabled, ex)).Wait();
                    }
                }

                AdditionalFunctionality.WriteLineColor("\nDo you want to clear the log file?(Y|N)", ConsoleColor.Cyan);
                ConsoleKeyInfo clearKey = Console.ReadKey(true);

                if (clearKey.Key == ConsoleKey.Y)
                {
                    CustomLogger.LogFileClear();
                    AdditionalFunctionality.WriteLineColor("The log file has been successfully deleted", ConsoleColor.Green);
                }
                
                AdditionalFunctionality.WriteLineColor("\nPress any key to restart program or \'Escape\' to close...", ConsoleColor.Green);

            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}