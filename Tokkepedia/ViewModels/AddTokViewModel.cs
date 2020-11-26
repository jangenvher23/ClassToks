using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokkepedia.Shared.Helpers;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Android.App;
using System.Windows.Input;
using Com.Github.Aakira.Expandablelayout;
using Android.Support.Design.Widget;
using Android.Text;

namespace Tokkepedia.ViewModels
{
    public class AddTokViewModel : ViewModelBase
    {
        #region Properties
        public TokModel TokModel { get; set; }
        public List<TokTypeList> TokGroup { get; set; }
        public ObservableCollection<TokModel> TokModelCollection { get; private set; }
        #endregion

        #region Commands

        #endregion
        public AddTokViewModel()
        {
            TokModel = new TokModel();
            TokGroup = TokGroupHelper.TokGroups.OrderBy(item => item.TokGroup).ToList();
            TokModelCollection = new ObservableCollection<TokModel>();
            TokModelCollection.Clear();

            TokModelCollection.Add(TokModel);
        }
        #region Methods/Events

        #endregion
    }
}