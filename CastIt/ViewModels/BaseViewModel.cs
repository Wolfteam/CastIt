﻿using CastIt.Interfaces;
using CastIt.Interfaces.ViewModels;
using CastIt.Models.Messages;
using Microsoft.Extensions.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using MvvmCross.ViewModels.Result;
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
        public ILogger Logger { get; }
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");
        #endregion

        protected BaseViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger)
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
        public ILogger Logger { get; }
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");
        #endregion

        protected BaseViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger)
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

    public abstract class BaseViewModelResult<TResult> : MvxResultSettingViewModel<TResult>, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public ILogger Logger { get; }
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");
        #endregion

        protected BaseViewModelResult(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxResultViewModelManager resultViewModelManager)
            : base(resultViewModelManager)
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

    public abstract class BaseViewModelResult<TParam, TResult> : MvxResultSettingViewModel<TParam, TResult>, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public ILogger Logger { get; }
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");
        #endregion

        protected BaseViewModelResult(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxResultViewModelManager resultViewModelManager)
            : base(resultViewModelManager)
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

    public abstract class BasePopupViewModel : BaseViewModel
    {
        protected BasePopupViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger)
            : base(textProvider, messenger, logger)
        {
        }

        //We override these because a popup will call this methods multiple times and we may loose the subscriptions
        public override void ViewAppeared()
        {
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
        }
    }

    public abstract class BaseViewModelResultAwaiting<TResult> : MvxResultAwaitingViewModel<TResult>, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public ILogger Logger { get; }
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key)
            ?? throw new Exception($"{key} was not found in the resources file");
        #endregion

        protected BaseViewModelResultAwaiting(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxResultViewModelManager resultViewModelManager)
            : base(resultViewModelManager)
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
