using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TranslationBandit
{
    class Program
    {
        static bool terminate = false;
        static string space = @"%"; /* Blank Spaces in Paths must be replaced with this character */
        // C:\Workspace\Web%Share\WebBooking\src\i18n\messages.xlf C:\Workspace\Web%Share\TranslationBandit\Translations.xlsx WebBooking

        static void Main(string[] args)
        {
            while (!terminate)
            {
                var batchParams = args;
                if (batchParams.Length >= 3)
                {                    
                    RockTheBoat(batchParams[0], batchParams[1], batchParams[2]);
                    terminate = true;
                }
                else
                {
                    Console.WriteLine("Type path to .xlf file, .xlsx file and excel tab. Type 'Q' to quit.");
                    string input = Console.ReadLine();

                    if (input.ToLower() == "q")
                    {
                        terminate = true;
                    }
                    else
                    {
                        string[] param = input.Split(' ');

                        if (param.Length >= 3)
                        {
                            RockTheBoat(param[0], param[1], param[2]);
                        }
                        else
                        {
                            Console.WriteLine("Too few input parameters");
                        }
                        
                    }
                }               

            }
        }

        static void RockTheBoat (string basePath, string excelPath, string excelTab)
        {
            basePath = basePath.Replace(space, " ");

            if (basePath.Length < 8)
            {
                Console.WriteLine("Path too short");
            }
            else if (basePath.Substring(basePath.Length - 4).ToLower() != ".xlf")
            {
                Console.WriteLine("Wrong file extension");
            }
            else
            {
                try
                {
                    List<TranslationModel> Translations = ExcelBandit.GetTranslations(excelPath.Replace(space, " "), excelTab.Replace(space, " "));

                    if (Translations != null && Translations.Count > 0)
                    {
                        XlfBandit.GenerateLanguageFiles(basePath, Translations);
                        Console.WriteLine("Translations were succesfully extracted!");
                    }
                    else
                    {
                        Console.WriteLine("Translation file could not be found");
                    }
                }
                catch (Exception expt)
                {
                    Console.WriteLine(expt.Message);
                }
            }
        }
    }
}
