﻿using CastIt.ViewModels;
using CastIt.ViewModels.Items;
using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Views
{
    [MvxViewFor(typeof(MainViewModel))]
    public partial class MainPage : MvxWpfView<MainViewModel>
    {
        //TODO: IF YOU DRAG OUT OF THE WINDOW, THE SEPARATORS ARE SHOWN

        private IMvxInteraction _closeAppRequest;
        public IMvxInteraction CloseAppRequest
        {
            get => _closeAppRequest;
            set
            {
                if (_closeAppRequest != null)
                    _closeAppRequest.Requested -= (sender, args) => WindowButtons.CloseApp();

                _closeAppRequest = value;
                if (value != null)
                    _closeAppRequest.Requested += (sender, args) => WindowButtons.CloseApp();
            }
        }

        private IMvxInteraction<(double, double)> _setWindowWithAndHeightRequest;
        public IMvxInteraction<(double, double)> SetWindowWithAndHeightRequest
        {
            get => _setWindowWithAndHeightRequest;
            set
            {
                if (_setWindowWithAndHeightRequest != null)
                    _setWindowWithAndHeightRequest.Requested -= SetWindowWidthAndHeight;

                _setWindowWithAndHeightRequest = value;
                if (value != null)
                    _setWindowWithAndHeightRequest.Requested += SetWindowWidthAndHeight;
            }
        }

        public MainPage()
        {
            InitializeComponent();

            var set = this.CreateBindingSet<MainPage, MainViewModel>();
            set.Bind(this).For(v => v.SetWindowWithAndHeightRequest).To(vm => vm.SetWindowWidthAndHeight).OneWay();
            set.Bind(this).For(v => v.CloseAppRequest).To(vm => vm.CloseApp).OneWay();
            set.Apply();
        }

        public Dictionary<PlayListItemViewModel, int> GetTabsPosition()
            => PlayListTabControl.GetOrderedHeaders()
                .ToDictionary(a => (a.Content as PlayListItemViewModel), a => a.LogicalIndex);

        private void SetWindowWidthAndHeight(object sender, MvxValueEventArgs<(double, double)> e)
        {
            //TODO: SOMETIMES, THE INTERACTION IS NOT BEING RAISED
            var window = System.Windows.Application.Current.MainWindow;
            window.Width = e.Value.Item1;
            window.Height = e.Value.Item2;
        }
    }
}
