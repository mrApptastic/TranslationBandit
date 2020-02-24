using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TranslationBandit
{
    class ExcelBandit
    {
        public static List<TranslationModel> GetTranslations(string excelPath, string sheet)
        {
            var trans = new List<TranslationModel>();

            var pck = new OfficeOpenXml.ExcelPackage();
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

            return trans;
        }


    }
}
