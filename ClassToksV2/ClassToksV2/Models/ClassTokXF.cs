using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Tokkepedia.Shared.Models;
using Xamarin.Forms;

namespace ClassToksV2.Models
{
    public class ClassTokXF : ClassTokModel
    {
        [JsonIgnore]
        public Color ColorXF { get; set; }

        [JsonIgnore]
        public FontAttributes FontAttributesPrimary { get; set; } = FontAttributes.Bold;

        [JsonIgnore]
        public FontAttributes FontAttributesTitleSubaccount { get; set; } = FontAttributes.None;

        [JsonIgnore]
        public FontAttributes FontAttributesRow3 { get; set; } = FontAttributes.None;

        [JsonIgnore]
        public int FontSizeRow3 { get; set; } = 18;

        [JsonIgnore]
        public bool IsAddMode { get; set; }

        [JsonIgnore]
        public bool IsBasic { get; set; } = false;
        [JsonIgnore]
        public bool IsDetailed { get; set; } = false;
        [JsonIgnore]
        public bool IsSectioned { get; set; } = false;

        [JsonIgnore]
        public bool HasImage { get; set; }

        [JsonIgnore]
        public bool HasNotes { get; set; }

        [JsonIgnore]
        public bool HasEnglishTranslation { get; set; } = false;

        [JsonIgnore]
        public string ViewMore { get; set; } = "View More";

        [JsonIgnore]
        public bool HasViewMore { get; set; } = true;

        [JsonIgnore]
        public string Row0 { get; set; }

        [JsonIgnore]
        public string Row1 { get; set; }

        [JsonIgnore]
        public string Row2 { get; set; }

        [JsonIgnore]
        public string Row3 { get; set; }

        [JsonIgnore]
        public string Row4 { get; set; }



        [JsonIgnore]
        public List<TokDetailXF> DetailsXF { get; set; }

        [JsonIgnore]
        public List<TokDetailXF> EnglishDetailsXF { get; set; }

        [JsonIgnore]
        public string Detail1 { get; set; }

        [JsonIgnore]
        public bool HasDetail1 { get; set; } = false;

        [JsonIgnore]
        public string Detail2 { get; set; }

        [JsonIgnore]
        public bool HasDetail2 { get; set; } = false;

        [JsonIgnore]
        public string Detail3 { get; set; }

        [JsonIgnore]
        public bool HasDetail3 { get; set; } = false;

        [JsonIgnore]
        public string Detail4 { get; set; }

        [JsonIgnore]
        public bool HasDetail4 { get; set; } = false;

        [JsonIgnore]
        public string SectionTitle1 { get; set; }

        [JsonIgnore]
        public bool HasSectionTitle1 { get; set; } = false;

        [JsonIgnore]
        public string SectionTitle2 { get; set; }

        [JsonIgnore]
        public bool HasSectionTitle2 { get; set; } = false;

        [JsonIgnore]
        public string SectionTitle3 { get; set; }

        [JsonIgnore]
        public bool HasSectionTitle3 { get; set; } = false;

        [JsonIgnore]
        public string SectionTitle4 { get; set; }

        [JsonIgnore]
        public bool HasSectionTitle4 { get; set; } = false;
    }

    public class TokDetailXF
    {
        [JsonIgnore]
        public string Detail { get; set; }

        [JsonIgnore]
        public string DetailLabel { get; set; }
    }

    public static class ClassToksXFExtensions
    {
        #region Process Item
        const int PrimaryCharMax = 35;
        const int SecondaryCharMax = 40;
        public static ClassTokXF ProcessItem(this ClassTokXF item)
        {
            item.HasEnglishTranslation = !item.IsEnglish;

            item.IsBasic = item.TokGroup == "Basic" || (!item.IsDetailBased && !item.IsMega) ? true : false;
            item.IsDetailed = item.TokGroup == "Detailed" || (item.IsDetailBased && !item.IsMega) ? true : false;
            item.IsSectioned = item.TokGroup == "Mega" || (!item.IsDetailBased && item.IsMega) ? true : false;

            item.Row0 = Truncate(item.PrimaryFieldText, PrimaryCharMax);//.Substring(0, (item.PrimaryFieldText.Length >= PrimaryCharMax) ? PrimaryCharMax - 1 : item.PrimaryFieldText.Length - 1);


            //Basic Tok
            if (item.IsBasic && !string.IsNullOrEmpty(item.SecondaryFieldText))
            {
                item.HasNotes = true;

                if (!item.IsEnglish) //Not english
                {
                    item.Row1 = item.SecondaryFieldText;
                    item.Row2 = item.EnglishPrimaryFieldText;
                    item.FontAttributesRow3 = FontAttributes.Bold;
                    item.FontSizeRow3 = 16;
                    item.Row3 = item.EnglishSecondaryFieldText;
                }
                else //English
                {
                    int rows = (int)Math.Ceiling(item.SecondaryFieldText.Length / (SecondaryCharMax * 1.0));
                    rows = (rows > 3) ? 3 : rows;

                    var content = item.SecondaryFieldText.WordWrapSplit(rows, SecondaryCharMax);
                    item.Row1 = rows >= 1 ? content[0] : null;
                    item.Row2 = rows >= 2 ? content[1] : null;
                    item.Row3 = rows >= 3 ? content[2] : null;
                }
                item.HasViewMore = item.SecondaryFieldText.Length > (SecondaryCharMax * 3) ? true : false;
            }
            else if (item.IsDetailed)
            {
                item.IsDetailBased = true;
                item.HasNotes = false;

                //Mobile bilingual and english both shows 3 rows
                item.Detail1 = item.Details.Length > 0 ? item.Details?[0] : null;
                item.HasDetail1 = !string.IsNullOrEmpty(item.Detail1) ? true : false;
                item.Detail2 = item.Details.Length > 1 ? item.Details?[1] : null;
                item.HasDetail2 = !string.IsNullOrEmpty(item.Detail2) ? true : false;
                item.Detail3 = item.Details.Length > 2 ? item.Details?[2] : null;
                item.HasDetail3 = !string.IsNullOrEmpty(item.Detail3) ? true : false;
                item.Detail4 = item.Details.Length > 3 ? item.Details?[3] : null;
                item.HasDetail4 = !string.IsNullOrEmpty(item.Detail4) ? true : false;

                if (!string.IsNullOrEmpty(item.Detail1))
                    item.Row1 = Truncate(item.Detail1, SecondaryCharMax);

                if (!string.IsNullOrEmpty(item.Detail2))
                    item.Row2 = Truncate(item.Detail2, SecondaryCharMax);

                if (!string.IsNullOrEmpty(item.Detail3))
                    item.Row3 = Truncate(item.Detail3, SecondaryCharMax);

                if (!string.IsNullOrEmpty(item.Detail4))
                    item.Row4 = Truncate(item.Detail4, SecondaryCharMax);

                int detailCount = item.Details.Where(x => !string.IsNullOrEmpty(x)).Count();
                for (int j = 0; j < detailCount; ++j)
                {
                    if (!string.IsNullOrEmpty(item.Details?[j]))
                        item.Details[j] = $"{item.Details?[j]}";
                }
                item.Details = item.Details.Take(detailCount).ToArray();
                item.ViewMore = $"View {detailCount} Details";
            }
            else if (item?.IsSectioned ?? false)
            {
                item.IsMegaTok = true;
                item.HasNotes = false;

                if (item.SectionTitles.Length > 0 && !string.IsNullOrEmpty(item.SectionTitles?[0]))
                    item.Row1 = Truncate(item.SectionTitles?[0], SecondaryCharMax);

                if (item.SectionTitles.Length > 1 && !string.IsNullOrEmpty(item.SectionTitles?[1]))
                    item.Row2 = Truncate(item.SectionTitles?[1], SecondaryCharMax);
                item.HasSectionTitle2 = !string.IsNullOrEmpty(item.Row2) ? true : false;

                if (item.SectionTitles.Length > 2 && !string.IsNullOrEmpty(item.SectionTitles?[2]))
                    item.Row3 = Truncate(item.SectionTitles?[2], SecondaryCharMax);
                item.HasSectionTitle3 = !string.IsNullOrEmpty(item.Row3) ? true : false;

                if (item.SectionTitles.Length > 3 && !string.IsNullOrEmpty(item.SectionTitles?[3]))
                    item.Row4 = Truncate(item.SectionTitles?[3], SecondaryCharMax);
                item.HasSectionTitle4 = !string.IsNullOrEmpty(item.Row4) ? true : false;

                item.ViewMore = $"View {item.SectionCount} Sections";            
            }
            if (string.IsNullOrEmpty(item.Notes))
                item.HasNotes = false;

            item.HasImage = !string.IsNullOrEmpty(item.Image) ? true : false;
            if (item.HasImage)
            {
                item.IsDetailed = false;
                item.IsMega = false;

                item.FontAttributesTitleSubaccount = FontAttributes.Bold;
                item.FontAttributesPrimary = FontAttributes.None;
            }

            return item;
        }

        //https://stackoverflow.com/a/1613918 
        private static string Truncate(string text, int max)
        {
            //If less than or equal to max characters in row: do not truncate.
            //Otherwise take the max characters and add 3 dots to the end
            if (!string.IsNullOrEmpty(text) && text.Length > max)
            {
                text = text.Substring(0, max);
                text += "...";
            }

            return text;
        }

        public static string[] WordWrapSplit(this string text, int rows, int rowMax)
        {
            string[] rowContent = new string[rows];

            //Double and triple not supported
            string textRemaining = text.Trim().Replace("   ", " ").Replace("  ", " ").Replace("   ", " ");

            for (int i = 0; i < rows; ++i)
            {
                int rowLength = textRemaining.Length > rowMax ? rowMax : textRemaining.Length;
                string subtext = textRemaining.Substring(0, rowLength);

                if (textRemaining.Length > rowMax)
                {
                    string lastCharLeftPart = textRemaining.Substring(subtext.Length - 1, 1);
                    string firstCharRightPart = textRemaining.Substring(subtext.Length, 1);

                    if (lastCharLeftPart != " " && firstCharRightPart != " ")
                    {
                        //Cut out until space found
                        int charIndex = subtext.Length;
                        string charFound = "";
                        do
                        {
                            //Read and remove if space
                            if (charIndex < 1) break;
                            charFound = subtext.Substring(charIndex - 1, 1); //"space bar":
                             --charIndex;
                        }
                        while (charFound != " ");
                        subtext = subtext.Substring(0, charIndex); //space

                        //i.e "space bar": charIndex = 5, so start at 6. Then bar = 3 length. To get 3: 9 - charIndex (5) = 4 - 1 = 3.
                        textRemaining = textRemaining.Substring(charIndex + 1, textRemaining.Length - charIndex - 1); //bar
                    }
                }

                rowContent[i] = subtext;

                //Last row
                if (textRemaining.Length > 0 && i == rows - 1 && rows >= 3)
                {
                    rowContent[i] += "...";
                }
            }

            return rowContent;
        }
        #endregion

        public static bool IsAllEmpty(this string[] set)
        {
            for (int i = 0; i < set.Length; ++i)
            {
                if (!string.IsNullOrEmpty(set[i]))
                    return false;
            }
            return true;
        }
    }
}