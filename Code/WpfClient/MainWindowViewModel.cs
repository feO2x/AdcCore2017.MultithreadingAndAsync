﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Input;
using Light.GuardClauses;
using Light.ViewModels;

namespace WpfClient
{
    public sealed class MainWindowViewModel : BaseNotifyPropertyChanged, INotifyDataErrorInfo, IRaiseErrorsChanged
    {
        private readonly DelegateCommand _callApiCommand;
        private readonly WebApiPerformanceManager _performanceManager;
        private readonly ValidationManager<string> _validationManager = new ValidationManager<string>();
        private bool _isBusy;
        private bool _isCallingAsynchronousApi;
        private int _numberOfCalls;
        private string _numberOfCallsText;
        private string _resultText;

        public MainWindowViewModel(WebApiPerformanceManager performanceManager)
        {
            _performanceManager = performanceManager.MustNotBeNull(nameof(performanceManager));
            _callApiCommand = new DelegateCommand(CallApi, () => CanCallApi);
            _numberOfCalls = 500;
            _numberOfCallsText = _numberOfCalls.ToString();
            _isCallingAsynchronousApi = true;
        }

        public string NumberOfCallsText
        {
            get => _numberOfCallsText;
            set
            {
                if (this.SetIfDifferent(ref _numberOfCallsText, value) == false)
                    return;
                _validationManager.Validate(value, ParseNumberOfCalls, this);
                _callApiCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsCallingAsynchronousApi
        {
            get => _isCallingAsynchronousApi;
            set => this.SetIfDifferent(ref _isCallingAsynchronousApi, value);
        }

        public ICommand CallApiCommand => _callApiCommand;

        public bool CanCallApi => HasErrors == false && IsBusy == false;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                this.Set(out _isBusy, value);
                _callApiCommand.RaiseCanExecuteChanged();
            }
        }

        public string ResultText
        {
            get => _resultText;
            private set => this.Set(out _resultText, value);
        }

        public IEnumerable GetErrors(string propertyName) => _validationManager.GetErrors(propertyName);

        public bool HasErrors => _validationManager.HasErrors;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public void OnErrorsChanged(string propertyName) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        private ValidationResult<string> ParseNumberOfCalls(string value)
        {
            if (value.IsNullOrWhiteSpace())
                return "Number of Calls must not be empty.";

            if (int.TryParse(value, out var numberOfCalls) == false)
                return "Number of Calls must be a numeric positive value.";

            if (numberOfCalls < 10)
                return "Number of Calls must be positive.";

            _numberOfCalls = numberOfCalls;
            return ValidationResult<string>.Valid;
        }

        private async void CallApi()
        {
            IsBusy = true;
            var results = await _performanceManager.MeasureApiCallsAsync(_isCallingAsynchronousApi, _numberOfCalls);
            IsBusy = false;

            ResultText = results.ToString();
        }
    }
}