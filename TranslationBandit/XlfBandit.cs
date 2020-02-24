using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TranslationBandit
{
    class XlfBandit
    {
        public static void GenerateLanguageFiles(string path, List<TranslationModel> translations)
        {
            
            string mainPath = path.Substring(0, path.LastIndexOf(@"\"));
            string originalFile = path.Split(@"\")[path.Split(@"\").Length - 1].Split('.')[0];

            foreach (var language in translations)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                var transUnit = xmlDoc.GetElementsByTagName("trans-unit");

                for (int i = 0; i < transUnit.Count; i++)
                {
                    string sourceText = transUnit[i].FirstChild.InnerText;
                    /* Find index of source translation (Index 0 in translations). */
                    int index = translations[0].Translations.FindIndex(x => x.Text.Trim().ToLower() == sourceText.Trim().ToLower());
                    string targetText = "";
                    if (index > -1)
                    {
                        if (index < language.Translations.Count)
                        {
                            string text = language.Translations[index].Text;

                            if (text.Length > 0)
                            {
                                targetText = text;
                            }
                            else
                            {
                                Console.WriteLine(sourceText + " needs a target translation in " + language.LanguageCode + ".");
                            }
                        }
                        else
                        {
                            Console.WriteLine(sourceText + " needs a target translation in " + language.LanguageCode + ".");
                        }
                    }
                    else
                    {
                        Console.WriteLine(sourceText + " needs a source translation.");
                    }
                    
                   XmlNode newNode = xmlDoc.CreateElement("target", xmlDoc.DocumentElement.NamespaceURI);

                    xmlDoc.GetElementsByTagName("trans-unit")[i].InsertAfter(newNode, xmlDoc.GetElementsByTagName("trans-unit")[i].FirstChild);

                    xmlDoc.GetElementsByTagName("target")[i].InnerText = targetText;
                }
                                           
                string langaugeFilePath = mainPath + @"\" + originalFile + "." + language.LanguageCode + ".xlf";

                xmlDoc.Save(langaugeFilePath);
            }
        }
    }
}
