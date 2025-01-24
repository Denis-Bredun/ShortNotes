﻿using CommunityToolkit.Mvvm.ComponentModel;
using Data_Organizer.Interfaces;
using Data_Organizer.MVVM.Models;

namespace Data_Organizer.MVVM.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private FeatureModel _selectedFeature;

        public IFeatureService FeatureService { get; }
        public ICultureInfoService CultureInfoService { get; }

        public MainPageViewModel(
            IFeatureService featureService,
            ICultureInfoService cultureInfoService)
        {
            FeatureService = featureService;
            CultureInfoService = cultureInfoService;
        }
    }
}
