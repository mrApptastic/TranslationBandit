using System;
using System.Collections.Generic;
using System.Text;

namespace TranslationBandit
{
    public class TranslationModel
    {
        public string LanguageCode { get; set; }
        public List<TranslationTextModel> Translations { get; set; }
    }

    public class TranslationTextModel
    {
        public string Text { get; set; }
    }
}
