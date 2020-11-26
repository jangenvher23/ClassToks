using System;
using Xamarin.Forms;

namespace ClassToksV2.Models
{
    public class TokFeedTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TokCard { get; set; }
        public DataTemplate TokTile { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var type = item.ToString();
            return (type == "TokTile") ? TokTile : TokCard;
        }
    }
}