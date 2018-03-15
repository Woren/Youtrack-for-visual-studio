﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
    [Export(typeof(ICreateRepositoriesDialogViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CreateRepositoriesDialogViewModel : ViewModelBase, ICreateRepositoriesDialogViewModel
    {
        private readonly IYouTrackClientService _youTrackClientService;
        private readonly IGitService _gitService;
        private readonly IFileService _fileService;
        private ReactiveCommand _createCommand;

        private string _errorMessage;
        private string _localPath;
        private string _name;
        private string _description;
        private bool _isPrivate;

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        [Required(AllowEmptyStrings = false)]
        [ValidatesViaMethod(AllowBlanks = false, AllowNull = false, Name = nameof(ClonePathHasSelectedRepositoryName), ErrorMessage = "Please enter repository name")]
        [ValidatesViaMethod(AllowBlanks = false, AllowNull = false, Name = nameof(ClonePathIsPath), ErrorMessage = "Clone Path must be a valid path")]
        [ValidatesViaMethod(AllowBlanks = false, AllowNull = false, Name = nameof(ClonePathNotExists), ErrorMessage = "Directory already exists")]
        public string LocalPath
        {
            get => _localPath;
            set => this.RaiseAndSetIfChanged(ref _localPath, value);
        }

        [Required]
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
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

        public ICommand CreateCommand => _createCommand;
        public IEnumerable<ReactiveCommand> ThrowableCommands => new List<ReactiveCommand> { _createCommand };

        [ImportingConstructor]
        public CreateRepositoriesDialogViewModel(
            IYouTrackClientService youTrackClientService,
            IGitService gitService,
            IFileService fileService
            )
        {
            _youTrackClientService = youTrackClientService;
            _gitService = gitService;
            _fileService = fileService;
            _isPrivate = true;

            var path = _gitService.GetDefaultRepoPath();
            LocalPath = !string.IsNullOrEmpty(path) ? path : Paths.DefaultRepositoryPath;
        }

        public void InitializeCommands()
        {
            _createCommand = ReactiveCommand.CreateFromTask(_ => Create(), CanExecuteCreateObservable());
        }

        protected override IEnumerable<IDisposable> SetupObservables()
        {
            this.WhenAnyValue(x => x.Name).Subscribe(_ => ForcePropertyValidation(nameof(LocalPath)));
            yield break;
        }

        private async Task Create()
        {
            var repository = new GitRemoteRepository { Name = Name, Description = Description, IsPrivate = IsPrivate };
            var remoteRepo = await _youTrackClientService.CreateRepositoryAsync(repository);
            _gitService.CloneRepository(remoteRepo.CloneUrl, remoteRepo.Name, LocalPath);
            OnClose();
        }

        private IObservable<bool> CanExecuteCreateObservable()
        {
            return ValidationObservable.Select(x => Unit.Default)
                .Merge(Changed.Select(x => Unit.Default))
                .Select(x => CanExecute()).StartWith(CanExecute());
        }

        private bool CanExecute()
        {
            return IsObjectValid();
        }

        protected void OnClose()
        {
            Closed?.Invoke(this, new EventArgs());
        }

        public bool ClonePathHasSelectedRepositoryName(string clonePath)
        {
            return !string.IsNullOrEmpty(Name);
        }

        public bool ClonePathNotExists(string clonePath)
        {
            if (string.IsNullOrEmpty(Name))
                return false;

            return !_fileService.Exists(Path.Combine(clonePath, Name));
        }

        public bool ClonePathIsPath(string clonePath)
        {
            return _fileService.IsPath(clonePath);
        }

        public event EventHandler Closed;

    }
}
