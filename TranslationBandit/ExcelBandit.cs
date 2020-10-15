using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TranslationBandit
{
    class ExcelBandit
    {
        public static List<TranslationModel> GetTranslations(string excelPath, string sheet)
        {
            List<TranslationModel> trans = ExtractTranslations(excelPath, sheet);
            List<TranslationModel> common = ExtractTranslations(excelPath, "Common");            

            foreach (var c in common) {
                var module = trans.Find(x => x.LanguageCode == c.LanguageCode);

                foreach (var t in c.Translations)
                {
                    module.Translations.Add(t);
                }
            }

            return trans;
        }            

        public static List<TranslationModel> ExtractTranslations(string excelPath, string sheet)
        {
            var trans = new List<TranslationModel>();

            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                pck.Load(File.OpenRead(excelPath));
                var ws = pck.Workbook.Worksheets[sheet];

                var start = ws.Dimension.Start;
                var end = ws.Dimension.End;

                /* Rows */
                for (int row = start.Row; row <= end.Row; row++)
                {
                    /* Cells */
                    for (int col = start.Column; col <= end.Column; col++)
                    {
                        string text = ws.Cells[row, col].Text;

                        if (row != 1)
                        {
                            if (text.Length > 0)
                            {
                                trans[col - 1].Translations.Add(new TranslationTextModel()
                                {
                                    Text = text
                                });
                            }
                        }
                        else
                        {
                            if (text.Length > 0)
                            {
                                trans.Add(new TranslationModel()
                                {
                                    LanguageCode = text,
                                    Translations = new List<TranslationTextModel>()
                                });
                            }
                        }
                    }
                }

            }

            return trans;
        }

        public static void AddMissingSourceTexts(List<string> translations, string excelPath, string sheet)
        {
            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                pck.Load(File.OpenRead(excelPath));
                var ws = pck.Workbook.Worksheets[sheet];

                int newStart = ws.Dimension.End.Row + translations.Count;
                ws.InsertRow(newStart, 1);

                for (int i = 0; i < translations.Count; i++)
                {
                    string text = translations[i];
                    ws.Cells[newStart + i, 1].Value = text;
                    Console.WriteLine("Source translation added for " + text);
                }

                pck.SaveAs(new FileInfo(excelPath));
            }
        }
    }
}
