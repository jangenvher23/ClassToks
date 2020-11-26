using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Tokkepedia.iOS.ViewModels
{
    public class PickerViewModel : UIPickerViewModel
    {
        public EventHandler ValueChanged;
        private List<string> _myItems;
        protected int selectedIndex = 0;

        public PickerViewModel(List<string> items)
        {
            _myItems = items;
        }

        public string SelectedItem
        {
            get { return _myItems[selectedIndex]; }
        }

        public override nint GetComponentCount(UIPickerView picker)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView picker, nint component)
        {
            return _myItems.Count;
        }

        public override string GetTitle(UIPickerView picker, nint row, nint component)
        {
            return _myItems[(int)row];
        }

        public override void Selected(UIPickerView picker, nint row, nint component)
        {
            selectedIndex = (int)row;
            ValueChanged?.Invoke(null, null);
        }
    }
}