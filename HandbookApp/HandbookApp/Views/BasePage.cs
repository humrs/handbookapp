﻿//
//  Copyright 2016  R. Stanley Hum <r.stanley.hum@gmail.com>
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System.Reactive.Disposables;
using HandbookApp.ViewModels;
using ReactiveUI;
using Xamarin.Forms;


namespace HandbookApp.Views
{
    public abstract class BasePage<TViewModel> : ContentPage, IViewFor<TViewModel>
        where TViewModel : class, ICustomBaseViewModel
    {
        protected readonly CompositeDisposable subscriptionDisposibles = new CompositeDisposable();

        public BasePage()
        {
            SetupViewElements();
        }

        public static readonly BindableProperty ViewModelProperty = 
            BindableProperty.Create(
                propertyName: "ViewModelProperty",
                returnType: typeof(TViewModel),
                declaringType: typeof(BasePage<TViewModel>),
                defaultValue: null,
                defaultBindingMode: BindingMode.OneWay);

        public TViewModel ViewModel
        {
            get { return (TViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
         
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        protected virtual void SetupViewElements() { }

        protected virtual void SetupObservables() { }

        protected virtual void SetupSubscriptions() { }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SetupObservables();

            SetupSubscriptions();

            ViewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            subscriptionDisposibles.Clear();

            ViewModel.OnDisappearing();

        }
    }
}
