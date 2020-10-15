using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;

namespace TranslationBandit
{
    class XlfBandit
    {
        public static List<string> GenerateLanguageFiles(string path, List<TranslationModel> translations)
        {
            List<string> misingTrans = new List<string>();
            string mainPath = path.Substring(0, path.LastIndexOf(@"\"));
            string originalFile = path.Split(@"\")[path.Split(@"\").Length - 1].Split('.')[0];

            foreach (var language in translations)
            {
                if (language.LanguageCode == "langKey")
                {
                    continue;
                }

                List<string> surplusTrans = language.Translations.Select(x => x.Text).Distinct().ToList();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                var transUnit = xmlDoc.GetElementsByTagName("trans-unit");

                for (int i = 0; i < transUnit.Count; i++)
                {
                     string sourceText = transUnit[i].FirstChild.InnerText.Trim();

                    /* Find index of source translation (Index 0 in translations). */
                    int index = translations[0].Translations.FindIndex(x => x.Text.Trim().ToLower() == sourceText.Trim().ToLower());

                    /* Handle Interpolations */
                    if (index < 0 && transUnit[i].FirstChild.InnerXml.Contains("INTERPOLATION"))
                    {
                        string text = transUnit[i].FirstChild.InnerXml.Replace(" xmlns=\"urn:oasis:names:tc:xliff:document:1.2\" ", "");

                        index = translations[0].Translations.FindIndex(x => x.Text == text);
                    }

                    /* Handle ICU Expressions */
                    string ICUText = "";
                                        
                    if (index < 0 && sourceText.Contains("VAR_SELECT"))
                    {
                        try
                        {
                            ICUText = sourceText;
                            string[] baseUnits = sourceText.Split('}');

                            for (int j = 0; j < baseUnits.Count(); j++)
                            {
                                string key = "";
                                string trans = "";

                                if (j != 0)
                                {
                                    key = baseUnits[j].Split('{')[0].Trim();
                                    trans = baseUnits[j].Split('{')[baseUnits[j].Split('{').Count() - 1].Trim();
                                } else {
                                    key = baseUnits[j].Split(',')[baseUnits[j].Split(',').Count() - 1].Trim();
                                    trans = key.Split('{')[0].Trim();
                                    key = key.Split('{')[1].Trim();
                                }
                                
                                if (key.Length > 0)
                                {
                                    int transIndex = translations[0].Translations.FindIndex(x => x.Text.Trim().ToLower() == key.Trim().ToLower());
                                    if (transIndex > -1)
                                    {
                                        string realTrans = language.Translations[transIndex].Text;
                                        ICUText = ICUText.Replace("{" + trans + "}", "{" + realTrans + "}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("No translation for " + key + " in " + language.LanguageCode);
                                    }
                                }
                                
                            }
                        } catch (Exception expt)
                        {
                            Console.WriteLine(expt.Message);
                            continue;
                        }
                    }        
         
                string targetText = "";
                
                if (index > -1)
                {

                    if (index < language.Translations.Count)
                    {
                        string text = language.Translations[index].Text;

                        if (text.Length > 0)
                        {
                            targetText = text;
                            if (surplusTrans.Contains(text))
                            {
                                surplusTrans.Remove(text);
                            }
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
                } else if (ICUText.Length > 0) {
                    targetText = ICUText;
                }
                else
                {
                    if (misingTrans.FindIndex(x => x == sourceText) == -1)
                    {
                        misingTrans.Add(sourceText);
                        Console.WriteLine(sourceText + " needs a source translation.");
                    }
                }

                /* Fallback to English */
                if (targetText.Length == 0) {
                    if (index > 0 && index < language.Translations.Count) {
                        var en = translations.Where(x => x.LanguageCode.ToLower() == "en").FirstOrDefault();
                        if (en != null) {
                            targetText = en.Translations[index].Text;
                        }
                    } 
                    ///* Fallback to Key */
                    //if (targetText.Length == 0)
                    //{
                    //    targetText = "";
                    //}
                }

                /* Insert target node */
                XmlNode newNode = xmlDoc.CreateElement("target", xmlDoc.DocumentElement.NamespaceURI);

                xmlDoc.GetElementsByTagName("trans-unit")[i].InsertAfter(newNode, xmlDoc.GetElementsByTagName("trans-unit")[i].FirstChild);

                xmlDoc.GetElementsByTagName("target")[i].InnerXml = targetText;
                }

                //foreach (var sur in surplusTrans)
                //{
                //    Console.WriteLine(sur + " translation is not used in " + language.LanguageCode + ".");
                //}

                string langaugeFilePath = mainPath + @"\" + originalFile + "." + language.LanguageCode + ".xlf";

                xmlDoc.Save(langaugeFilePath);
            }

            return misingTrans;
        }
    }
}
