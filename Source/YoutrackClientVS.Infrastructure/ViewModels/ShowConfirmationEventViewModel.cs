﻿using System.Windows.Input;
using ReactiveUI;
using YouTrackClientVS.Contracts.Events;

namespace YouTrackClientVS.Infrastructure.ViewModels
{
    public class ShowConfirmationEventViewModel : ReactiveObject
    {
        private ShowConfirmationEvent _event;
        public ICommand SetResultCommand { get; private set; }

        public ShowConfirmationEvent Event
        {
            get { return _event; }
            set { this.RaiseAndSetIfChanged(ref _event, value); }
        }

        public ShowConfirmationEventViewModel()
        {
            SetCommands();
        }

        private void SetCommands()
        {
            var setResultCommand = ReactiveCommand.Create<bool>(confirmation =>
            {
                if (confirmation)
                    Event.Command.Execute(null);

                Event = null;
            });

            SetResultCommand = setResultCommand;
        }
    }
}