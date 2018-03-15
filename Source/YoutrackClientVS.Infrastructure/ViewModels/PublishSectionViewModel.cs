﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using YouTrackClientVS.Contracts.Interfaces.Services;
using YouTrackClientVS.Contracts.Interfaces.ViewModels;
using YouTrackClientVS.Contracts.Models.GitClientModels;

namespace YouTrackClientVS.Infrastructure.ViewModels
{
    [Export(typeof(IPublishSectionViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PublishSectionViewModel : ViewModelBase, IPublishSectionViewModel
    {
        private IYouTrackClientService _youTrackClientService;
        private IGitService _gitService;
        private readonly IUserInformationService _userInformationService;
        private readonly IGitWatcher _gitWatcher;
        private ReactiveCommand _publishRepositoryCommand;
        private ReactiveCommand _initializeCommand;
        private string _repositoryName;
        private string _description;
        private bool _isPrivate;
        private string _errorMessage;
        private bool _isLoading;
        private List<string> _owners;
        private string _selectedOwner;

        public ICommand PublishRepositoryCommand => _publishRepositoryCommand;
        public ICommand InitializeCommand => _initializeCommand;

        [Required]
        public string RepositoryName
        {
            get => _repositoryName;
            set => this.RaiseAndSetIfChanged(ref _repositoryName, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        public bool IsPrivate
        {
            get => _isPrivate;
            set => this.RaiseAndSetIfChanged(ref _isPrivate, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public List<string> Owners
        {
            get => _owners;
            set => this.RaiseAndSetIfChanged(ref _owners, value);
        }

        [Required]
        public string SelectedOwner
        {
            get => _selectedOwner;
            set => this.RaiseAndSetIfChanged(ref _selectedOwner, value);
        }


        public IEnumerable<ReactiveCommand> ThrowableCommands => new[] { _publishRepositoryCommand, _initializeCommand };
        public IEnumerable<ReactiveCommand> LoadingCommands => new[] { _publishRepositoryCommand, _initializeCommand };


        [ImportingConstructor]
        public PublishSectionViewModel(
            IYouTrackClientService youTrackClientService,
            IGitService gitService,
            IFileService fileService,
            IUserInformationService userInformationService,
            IGitWatcher gitWatcher
            )
        {
            _youTrackClientService = youTrackClientService;
            _gitService = gitService;
            _userInformationService = userInformationService;
            _gitWatcher = gitWatcher;
            _isPrivate = true;
        }

        public void InitializeCommands()
        {
            _publishRepositoryCommand = ReactiveCommand.CreateFromTask(_ => PublishRepository(), CanPublishRepository());
            _initializeCommand = ReactiveCommand.CreateFromTask(_ => CreateOwners());
        }

        private async Task CreateOwners()
        {
            if (!_userInformationService.ConnectionData.IsLoggedIn)
                return;

            var teamNames = (await _youTrackClientService.GetTeams()).Select(x => x.Name).ToList();
            teamNames.Insert(0, _userInformationService.ConnectionData.UserName);
            Owners = teamNames;
            SelectedOwner = Owners.FirstOrDefault();
        }

        private async Task PublishRepository()
        {
            var gitRemoteRepository = new GitRemoteRepository()
            {
                Name = RepositoryName.Replace(' ', '-'),
                Description = Description,
                IsPrivate = IsPrivate,
                Owner = SelectedOwner
            };

            var remoteRepo = await _youTrackClientService.CreateRepositoryAsync(gitRemoteRepository);
            _gitService.PublishRepository(remoteRepo);
            _gitWatcher.Refresh();
        }

        private IObservable<bool> CanPublishRepository()
        {
            return ValidationObservable.Select(x => Unit.Default)
                .Merge(Changed.Select(x => Unit.Default))
                .Select(x => CanExecute()).StartWith(CanExecute());
        }

        private bool CanExecute()
        {
            return IsObjectValid();
        }
    }
}
