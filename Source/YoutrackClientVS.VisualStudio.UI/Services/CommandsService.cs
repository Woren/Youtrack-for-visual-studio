﻿using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReactiveUI;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.IO;
using System.Reactive.Linq;
using YouTrackClientVS.Contracts.Interfaces.Services;
using YouTrackClientVS.Contracts.Interfaces.ViewModels;
using YouTrackClientVS.Contracts.Interfaces.Views;
using YouTrackClientVS.Contracts.Models;
using YouTrackClientVS.Infrastructure.Extensions;
using YouTrackClientVS.Infrastructure.ViewModels;
using YouTrackClientVS.VisualStudio.UI.Window;

namespace YouTrackClientVS.VisualStudio.UI.Services
{
    [Export(typeof(ICommandsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CommandsService : ICommandsService
    {
        private readonly ExportFactory<IDiffWindowControlViewModel> _diffFactory;
        private readonly ExportFactory<IYouTrackIssuesWindowContainerViewModel> _pqFactory;
        private readonly IPageNavigationService<IYouTrackIssuesWindow> _navigationService;
        private Package _package;


        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 257;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f4810f5c-ed7a-4cc3-9cbf-d3e19125792b");


        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => _package;

        [ImportingConstructor]
        public CommandsService(
            ExportFactory<IDiffWindowControlViewModel> diffFactory,
            ExportFactory<IYouTrackIssuesWindowContainerViewModel> pqFactory,
            IPageNavigationService<IYouTrackIssuesWindow> navigationService)

        {
            _diffFactory = diffFactory;
            _pqFactory = pqFactory;
            _navigationService = navigationService;
        }

        public void Initialize(object package)
        {
            _package = (Package)package;


            if (!(ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            ) return;

            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(ShowYouTrackIssuesWindow, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowYouTrackIssuesWindow(object sender, EventArgs e)
        {
            ShowYouTrackIssuesWindow();
        }


        public void ShowYouTrackIssuesWindow()
        {
            var window = ShowWindow<YouTrackIssuesWindow>();
            var vm = _pqFactory.CreateExport().Value;
            var view = window.Content as IYouTrackIssuesWindowContainer;
            view.DataContext = vm;
            view.Window = window;
            vm.Initialize();
            _navigationService.ClearNavigationHistory();
            _navigationService.Navigate<IYouTrackIssuesMainView>();
        }

        public void ShowDiffWindow(FileDiffModel parameter, int id)

        {
            var
                window = ShowWindow<DiffWindow>(
                    id); // todo id is generated in filediff, we can use more elegant solution to prevent duplicates. It won't work in cases like user reopen detail view
            if (window.ViewModel == null)
            {
                var vm = (DiffWindowControlViewModel)_diffFactory.CreateExport().Value;
                window.ViewModel = vm;
                vm.Initialize(parameter);
                vm.WhenAnyValue(x => x.FileDiff).Where(x => x != null)
                    .Subscribe(x => window.Caption = $"Diff ({x.DisplayFileName})");
            }
        }

        private TWindow ShowWindow<TWindow>(int id = 0) where TWindow : class
        {
            if (_package == null)
                throw new Exception("Package wasn't initialized");

            var window = _package.FindToolWindow(typeof(TWindow), id, true);

            if (window?.Frame == null)
                throw new NotSupportedException("Cannot create window");

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return window as TWindow;
        }

        public object ShowSideBySideDiffWindow(
            string content1,
            string content2,
            string fileDisplayName1,
            string fileDisplayName2,
            string caption,
            string tooltip,
            object vsFrame //todo thisvsFrame is pretty ugly. Find a better way to check if window already exists. It's object due to dependencies (it's just easier)
        )
        {
            (string file1, string file2) = CreateTempFiles(content1, content2);

            try
            {
                var differenceService = Package.GetGlobalService(typeof(SVsDifferenceService)) as IVsDifferenceService;

                var options = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_DetectBinaryFiles |
                              __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary |
                              __VSDIFFSERVICEOPTIONS.VSDIFFOPT_RightFileIsTemporary;

                (vsFrame as IVsWindowFrame)?.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);

                return differenceService.OpenComparisonWindow2(
                    file1,
                    file2,
                    caption,
                    tooltip,
                    fileDisplayName1,
                    fileDisplayName2,
                    null,
                    null,
                    (uint)options
                );
            }
            finally
            {
                if (File.Exists(file1))
                    File.Delete(file1);
                if (File.Exists(file2))
                    File.Delete(file2);
            }
        }


        private static (string file1, string file2) CreateTempFiles(string content1, string content2)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "YouTrackClientVs");
            Directory.CreateDirectory(tempDir);

            var tempPath1 = Path.Combine(tempDir, Guid.NewGuid().ToString());
            var tempPath2 = Path.Combine(tempDir, Guid.NewGuid().ToString());

            File.WriteAllText(tempPath1, content1);
            File.WriteAllText(tempPath2, content2);

            return (tempPath1, tempPath2);
        }
    }
}