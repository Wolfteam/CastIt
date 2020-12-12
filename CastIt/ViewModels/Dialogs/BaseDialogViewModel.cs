using CastIt.Common;
using CastIt.Interfaces;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels.Dialogs
{
    public class BaseDialogViewModel : BaseViewModel
    {
        #region Members
        private string _title;
        private string _contentText;
        private string _okText;
        private string _cancelText;
        #endregion

        #region Properties
        public string AppName
            => AppConstants.AppName;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string ContentText
        {
            get => _contentText;
            set => SetProperty(ref _contentText, value);
        }

        public string OkText
        {
            get => _okText;
            set => SetProperty(ref _okText, value);
        }

        public string CancelText
        {
            get => _cancelText;
            set => SetProperty(ref _cancelText, value);
        }
        #endregion

        #region Commands
        public IMvxAsyncCommand OkCommand { get; set; }
        public IMvxAsyncCommand CloseCommand { get; set; }
        #endregion

        protected BaseDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger)
            : base(textProvider, messenger, logger)
        {
        }
    }

    public class BaseDialogViewModelResult<TResult> : BaseViewModelResult<TResult>
    {
        #region Members
        private string _title;
        private string _contentText;
        private string _okText;
        private string _cancelText;
        #endregion

        #region Properties
        public string AppName
            => AppConstants.AppName;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string ContentText
        {
            get => _contentText;
            set => SetProperty(ref _contentText, value);
        }

        public string OkText
        {
            get => _okText;
            set => SetProperty(ref _okText, value);
        }

        public string CancelText
        {
            get => _cancelText;
            set => SetProperty(ref _cancelText, value);
        }
        #endregion

        #region Commands
        public IMvxAsyncCommand OkCommand { get; set; }
        public IMvxAsyncCommand CloseCommand { get; set; }
        #endregion

        protected BaseDialogViewModelResult(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger)
            : base(textProvider, messenger, logger)
        {
        }
    }
}
