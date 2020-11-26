using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tokkepedia.iOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditTokPage : ContentPage
    {
        int numDetails = 3;

#if _CLASSTOKS
        ClassTokModel classTok = new ClassTokModel();
#else
        TokModel tok = new TokModel();
#endif

        public AddEditTokPage(TokViewModel _tok = null)
        {
            InitializeComponent();

            //if (_tok != null)
            //{

            //}
            //else
            //{
            //    tok = _tok;
            //}

            //BindingContext = tok;
        }

        private async void btnAddEditTok_Clicked(object sender, EventArgs e)
        {
#if _CLASSTOKS
            #region Get from text enties
            classTok.UserId = Settings.GetUserModel().UserId;
            classTok.Label = "classtok";
            classTok.Category = txtCategory.Text;
            classTok.PrimaryFieldText = txtPrimary.Text;
            classTok.Notes = txtNotes.Text;

            #region Secondary field only 
            if (!(classTok?.IsDetailBased ?? false) && !(classTok?.IsMegaTok ?? false)) //Basic (not detailed or mega classTok)
            {
                classTok.SecondaryFieldText = txtSecondary.Text;

                classTok.Details = null;
                classTok.IsMegaTok = null;
            }
            else if ((classTok?.IsDetailBased ?? false) && !(classTok?.IsMegaTok ?? false)) //Detailed (not basic or mega classTok)
            {
                classTok.SecondaryFieldText = null;
                classTok.IsMegaTok = null;
            }
            else if (classTok?.IsMegaTok ?? false) //Mega
            {
                classTok.SecondaryFieldText = null;
                classTok.Details = null;
            }
            classTok.SecondaryFieldText = txtSecondary.Text;
            #endregion
            #endregion

            //API
            await ClassService.Instance.AddClassToksAsync(classTok);
#else
            #region Get from text enties
            tok.Category = txtCategory.Text;
            tok.PrimaryFieldText = txtPrimary.Text;
            tok.Notes = txtNotes.Text;

            #region Secondary field only 
            if (!(tok?.IsDetailBased ?? false) && !(tok?.IsMegaTok ?? false)) //Basic (not detailed or mega tok)
            {
                tok.SecondaryFieldText = txtSecondary.Text;

                tok.Details = null;
                tok.IsMegaTok = null;
            }
            else if ((tok?.IsDetailBased ?? false) && !(tok?.IsMegaTok ?? false)) //Detailed (not basic or mega tok)
            {
                tok.SecondaryFieldText = null;
                tok.IsMegaTok = null;
            }
            else if (tok?.IsMegaTok ?? false) //Mega
            {
                tok.SecondaryFieldText = null;
                tok.Details = null;
            }
            tok.SecondaryFieldText = txtSecondary.Text;
            #endregion
            #endregion

            //API
            await Tokkepedia.Shared.Services.TokService.Instance.CreateTokAsync(tok);
#endif
        }

        private void pckTokGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (sender as Picker).SelectedItem.ToString();
            if (item == "Detailed")
            {
                stkDetails.IsVisible = true;
                scrollDetails.IsVisible = true;
                stkAddRemoveDetails.IsVisible = true;

                stkSections.IsVisible = false;
                lblSecondary.IsVisible = false;
                txtSecondary.IsVisible = false;
            }
            else if (item == "Basic")
            {
                stkSections.IsVisible = false;
                stkDetails.IsVisible = false;
                scrollDetails.IsVisible = false;
                stkAddRemoveDetails.IsVisible = false;

                lblSecondary.IsVisible = true;
                txtSecondary.IsVisible = true;
            }
            else //Mega
            {
                stkSections.IsVisible = true;
                stkDetails.IsVisible = false;
                scrollDetails.IsVisible = false;
                stkAddRemoveDetails.IsVisible = false;

                lblSecondary.IsVisible = false;
                txtSecondary.IsVisible = false;
            }
        }

        private async void btnAddDetail_Clicked(object sender, EventArgs e)
        {
            await SetDetailVisibility(numDetails + 1, true);

            numDetails++;
            //Maximum of 3 details
            btnRemoveDetail.IsVisible = true;
            if (numDetails >= 10)
                btnAddDetail.IsVisible = false;
        }

        private async void btnRemoveDetail_Clicked(object sender, EventArgs e)
        {
            await SetDetailVisibility(numDetails, false);

            numDetails--;
            //Minimum of 3 details
            btnAddDetail.IsVisible = true;
            if (numDetails <= 3)
                btnRemoveDetail.IsVisible = false;
        }

        private async Task SetDetailVisibility(int detailNum, bool isVisible)
        {
            switch (detailNum)
            {
                case 4:
                    lblDetail4.IsVisible = isVisible;
                    txtDetail4.IsVisible = isVisible;
                    lblDetail4Counter.IsVisible = isVisible;
                    if (isVisible) await scrollDetails.ScrollToAsync(lblDetail4Counter, ScrollToPosition.End, false); else await scrollDetails.ScrollToAsync(lblDetail3Counter, ScrollToPosition.End, false);
                    break;
                case 5:
                    lblDetail5.IsVisible = isVisible;
                    txtDetail5.IsVisible = isVisible;
                    lblDetail5Counter.IsVisible = isVisible;
                    if (isVisible) await scrollDetails.ScrollToAsync(lblDetail5Counter, ScrollToPosition.End, false); else await scrollDetails.ScrollToAsync(lblDetail4Counter, ScrollToPosition.End, false);
                    break;
                case 6:
                    lblDetail6.IsVisible = isVisible;
                    txtDetail6.IsVisible = isVisible;
                    lblDetail6Counter.IsVisible = isVisible;
                    if (isVisible) await scrollDetails.ScrollToAsync(lblDetail6Counter, ScrollToPosition.End, false); else await scrollDetails.ScrollToAsync(lblDetail5Counter, ScrollToPosition.End, false);
                    break;
                case 7:
                    lblDetail7.IsVisible = isVisible;
                    txtDetail7.IsVisible = isVisible;
                    lblDetail7Counter.IsVisible = isVisible;
                    if (isVisible) await scrollDetails.ScrollToAsync(lblDetail7Counter, ScrollToPosition.End, false); else await scrollDetails.ScrollToAsync(lblDetail6Counter, ScrollToPosition.End, false);
                    break;
                case 8:
                    lblDetail8.IsVisible = isVisible;
                    txtDetail8.IsVisible = isVisible;
                    lblDetail8Counter.IsVisible = isVisible;
                    if (isVisible) await scrollDetails.ScrollToAsync(lblDetail8Counter, ScrollToPosition.End, false); else await scrollDetails.ScrollToAsync(lblDetail7Counter, ScrollToPosition.End, false);
                    break;
                case 9:
                    lblDetail9.IsVisible = isVisible;
                    txtDetail9.IsVisible = isVisible;
                    lblDetail9Counter.IsVisible = isVisible;
                    if (isVisible) await scrollDetails.ScrollToAsync(lblDetail9Counter, ScrollToPosition.End, false); else await scrollDetails.ScrollToAsync(lblDetail8Counter, ScrollToPosition.End, false);
                    break;
                case 10:
                    lblDetail10.IsVisible = isVisible;
                    txtDetail10.IsVisible = isVisible;
                    lblDetail10Counter.IsVisible = isVisible;
                    if (isVisible) await scrollDetails.ScrollToAsync(lblDetail10Counter, ScrollToPosition.End, false); else await scrollDetails.ScrollToAsync(lblDetail9Counter, ScrollToPosition.End, false);
                    break;
            }
        }
    }

    
}