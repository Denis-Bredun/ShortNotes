﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Data_Organizer.Interfaces;
using Data_Organizer.MVVM.Models;
using System.Globalization;

namespace Data_Organizer.MVVM.ViewModels
{
    public partial class MainPageViewModel : ObservableObject, IDisposable
    {
        private readonly INotificationService _notificationService;
        private readonly IOpenAIAPIRequestService _openAIAPIRequestService;
        private readonly IClipboardService _clipboardService;
        private Action<string>? _transcriptionUpdatedHandler;

        [ObservableProperty]
        private FeatureModel _selectedFeature;
        [ObservableProperty]
        private LanguageModel _selectedLanguage;
        [ObservableProperty]
        private string _outputText;
        [ObservableProperty]
        private bool _isReadOnly;
        [ObservableProperty]
        private string _editButtonImageSource;
        [ObservableProperty]
        private string _playButtonImageSource;
        [ObservableProperty]
        private bool _isLoading;

        public IFeatureService FeatureService { get; }
        public ICultureInfoService CultureInfoService { get; }
        public IAudioTranscriptorService AudioTranscriptorService { get; }

        public MainPageViewModel(
            IFeatureService featureService,
            ICultureInfoService cultureInfoService,
            INotificationService notificationService,
            IAudioTranscriptorService audioTranscriptorService,
            IOpenAIAPIRequestService openAIAPIRequestService,
            IClipboardService clipboardService)
        {
            FeatureService = featureService;
            CultureInfoService = cultureInfoService;
            AudioTranscriptorService = audioTranscriptorService;

            _notificationService = notificationService;
            _openAIAPIRequestService = openAIAPIRequestService;
            _clipboardService = clipboardService;

            SetDefaultProperties();
        }

        private void SetDefaultProperties()
        {
            IsReadOnly = true;
            EditButtonImageSource = "disabled_edit_mode.svg";
            PlayButtonImageSource = "start_record.svg";
        }

        [RelayCommand]
        public async Task PlayFeature()
        {
            if (SelectedFeature.Title.Contains("Транскрипція"))
                PlayTranscription();
            else if (SelectedFeature.Title.Contains("Конспект"))
                await PlayAISummary();
        }

        private async Task PlayAISummary()
        {
            if (string.IsNullOrWhiteSpace(OutputText))
            {
                await _notificationService.ShowToastAsync("Тези не можуть бути зроблені з пустоти!");
                return;
            }

            UnsubscribeFromTranscriptionUpdates();

            IsLoading = true;

            var responseResult = await _openAIAPIRequestService.GetSummaryAsync(OutputText);

            if (responseResult != null)
                OutputText += $"\n-----------------------\n{responseResult.Result}";

            IsLoading = false;
        }

        private void PlayTranscription()
        {
            SetTranscriptionUpdatedHandlerIfNecessary();

            if (!AudioTranscriptorService.IsListening)
            {
                var cultureInfo = CultureInfo.GetCultureInfo(SelectedLanguage.CultureCode);
                AudioTranscriptorService.StartListeningAsync(cultureInfo);
            }
            else
                AudioTranscriptorService.StopListening();
        }

        private void SetTranscriptionUpdatedHandlerIfNecessary()
        {
            if (_transcriptionUpdatedHandler == null)
            {
                _transcriptionUpdatedHandler = text => OutputText = text;
                AudioTranscriptorService.OnTranscriptionUpdated += _transcriptionUpdatedHandler;
            }
        }

        public void SwitchPlayButtonImage(bool isListening)
        {
            if (isListening)
                PlayButtonImageSource = "pause_record.svg";
            else
                PlayButtonImageSource = "start_record.svg";
        }

        [RelayCommand]
        public async Task CopyOutputText()
        {
            if (string.IsNullOrWhiteSpace(OutputText))
            {
                await _notificationService.ShowToastAsync("Нема чого копіювати...");
                return;
            }

            await _clipboardService.AddAsync(OutputText);

            await _notificationService.ShowToastAsync("Дані були успішно скопійовані!");
        }

        [RelayCommand]
        public async Task PasteText()
        {
            var action = await _notificationService.ShowActionSheetAsync("Як бажаєте вставити дані?", ["Вставити в кінець", "Замінити весь текст"]);

            switch (action)
            {
                case "Вставити в кінець":
                    OutputText += "\n";
                    OutputText += await _clipboardService.GetLastDataAsync();
                    await _notificationService.ShowToastAsync("Дані були успішно вставлені в кінець!");
                    break;
                case "Замінити весь текст":
                    OutputText = await _clipboardService.GetLastDataAsync();
                    await _notificationService.ShowToastAsync("Дані успішно замінили весь текст!");
                    break;
                default:
                    return;
            }
        }

        [RelayCommand]
        public async Task CleanEditor()
        {
            bool isConfirmed = await _notificationService.ShowConfirmationDialogAsync("Ви дійсно бажаєте очистити текстове поле?");

            if (!isConfirmed)
                return;

            OutputText = "";

            await _notificationService.ShowToastAsync("Текстове поле було очищено!");
        }

        [RelayCommand]
        public async Task SwitchEditMode()
        {
            bool wasEditModeEnabled = false;

            if (IsReadOnly)
                wasEditModeEnabled = await EnableEditMode();
            else
                await DisableEditMode();

            SwitchEditButtonImage(wasEditModeEnabled);
        }

        private void SwitchEditButtonImage(bool wasEditModeEnabled)
        {
            if (wasEditModeEnabled)
                EditButtonImageSource = "enabled_edit_mode.svg";
            else
                EditButtonImageSource = "disabled_edit_mode.svg";
        }

        public async Task<bool> EnableEditMode()
        {
            bool isConfirmed = await _notificationService.ShowConfirmationDialogAsync("Ви дійсно бажаєте увійти в режим редагування?");

            if (!isConfirmed)
                return false;

            IsReadOnly = false;

            await _notificationService.ShowToastAsync("Ви увійшли в режим редагування!");
            return true;
        }

        public async Task DisableEditMode()
        {
            IsReadOnly = true;

            await _notificationService.ShowToastAsync("Ви вийшли з режиму редагування!");
        }

        public void Dispose()
        {
            AudioTranscriptorService.StopListening();
            UnsubscribeFromTranscriptionUpdates();
        }

        private void UnsubscribeFromTranscriptionUpdates()
        {
            if (_transcriptionUpdatedHandler != null)
            {
                AudioTranscriptorService.OnTranscriptionUpdated -= _transcriptionUpdatedHandler;
                _transcriptionUpdatedHandler = null;
            }
        }
    }
}
