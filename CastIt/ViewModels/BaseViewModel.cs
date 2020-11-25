using CastIt.Interfaces;
using CastIt.Interfaces.ViewModels;
using CastIt.Models.Messages;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;

namespace CastIt.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public IMvxLog Logger { get; }
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");
        #endregion

        protected BaseViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLog logger)
        {
            TextProvider = textProvider;
            Messenger = messenger;
            Logger = logger;

            RegisterMessages();
            SetCommands();
        }

        public virtual void SetCommands()
        {
        }

        public virtual void RegisterMessages()
        {
            SubscriptionTokens.Add(Messenger.Subscribe<AppLanguageChangedMessage>(_ => RaiseAllPropertiesChanged()));
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            if (SubscriptionTokens.Count == 0)
            {
                RegisterMessages();
            }
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            base.ViewDestroy(viewFinishing);
            if (!viewFinishing)
                return;
            foreach (var token in SubscriptionTokens)
            {
                token.Dispose();
            }
            SubscriptionTokens.Clear();
        }

        public string GetText(string key)
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");

        public string GetText(string key, params string[] args)
            => TextProvider.GetText(string.Empty, string.Empty, key, args)
            ?? throw new Exception($"{key} was not found in the resources file");
    }

    public abstract class BaseViewModel<TParameter> : MvxViewModel<TParameter>, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public IMvxLog Logger { get; }
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");
        #endregion

        protected BaseViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLog logger)
        {
            TextProvider = textProvider;
            Messenger = messenger;
            Logger = logger;

            RegisterMessages();
            SetCommands();
        }

        public virtual void SetCommands()
        {
        }

        public virtual void RegisterMessages()
        {
            SubscriptionTokens.Add(Messenger.Subscribe<AppLanguageChangedMessage>(_ => RaiseAllPropertiesChanged()));
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            if (SubscriptionTokens.Count == 0)
            {
                RegisterMessages();
            }
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            base.ViewDestroy(viewFinishing);
            if (!viewFinishing)
                return;
            foreach (var token in SubscriptionTokens)
            {
                token.Dispose();
            }
            SubscriptionTokens.Clear();
        }

        public string GetText(string key)
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");

        public string GetText(string key, params string[] args)
            => TextProvider.GetText(string.Empty, string.Empty, key, args)
            ?? throw new Exception($"{key} was not found in the resources file");
    }

    public abstract class BaseViewModelResult<TResult> : MvxViewModelResult<TResult>, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public IMvxLog Logger { get; }
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");
        #endregion

        protected BaseViewModelResult(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLog logger)
        {
            TextProvider = textProvider;
            Messenger = messenger;
            Logger = logger;

            RegisterMessages();
            SetCommands();
        }

        public virtual void SetCommands()
        {
        }

        public virtual void RegisterMessages()
        {
            SubscriptionTokens.Add(Messenger.Subscribe<AppLanguageChangedMessage>(_ => RaiseAllPropertiesChanged()));
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            if (SubscriptionTokens.Count == 0)
            {
                RegisterMessages();
            }
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            base.ViewDestroy(viewFinishing);
            if (!viewFinishing)
                return;
            foreach (var token in SubscriptionTokens)
            {
                token.Dispose();
            }
            SubscriptionTokens.Clear();
        }

        public string GetText(string key)
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");

        public string GetText(string key, params string[] args)
            => TextProvider.GetText(string.Empty, string.Empty, key, args)
            ?? throw new Exception($"{key} was not found in the resources file");
    }
}
