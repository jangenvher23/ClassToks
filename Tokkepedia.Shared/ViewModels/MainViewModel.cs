using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tokkepedia.Shared.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _message;
        private string _previousMessage;

        public MainViewModel()
        {
            MessageCommand = new RelayCommand<string>(SubmitMessage);
        }

        public RelayCommand<string> MessageCommand { get; private set; }

        private void SubmitMessage(string message)
        {
            PreviousMessage = message;
        }

        public string PreviousMessage
        {
            get { return _previousMessage; }
            set
            {
                _previousMessage = value;
                RaisePropertyChanged(propertyName: nameof(PreviousMessage));
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged(propertyName: nameof(Message));
            }
        }
    }
}
