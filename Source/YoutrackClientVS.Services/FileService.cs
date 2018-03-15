﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using YouTrackClientVS.Contracts.Interfaces.Services;
using YouTrackClientVS.Contracts.Models;

namespace YouTrackClientVS.Services
{
    [Export(typeof(IFileService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FileService : IFileService
    {
        public void DeleteDirectory(string path)
        {
            if (Exists(path))
                Directory.Delete(path, true);
        }

        public void Save(string path, string content)
        {
            var file = new FileInfo(path);
            file.Directory?.Create();
            File.WriteAllText(file.FullName, content);
        }

        public string Read(string path)
        {
            return File.ReadAllText(path);
        }


        public Result<string> OpenSaveDialog(string filterPattern)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = filterPattern
            };

            return dialog.ShowDialog() == DialogResult.OK ? Result<string>.Success(dialog.FileName) : Result<string>.Fail();
        }

        public Result<string> OpenDirectoryDialog(string selectedPath, string title = null)
        {
            using (var folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.RootFolder = Environment.SpecialFolder.Desktop;
                folderBrowser.SelectedPath = selectedPath;
                folderBrowser.ShowNewFolderButton = true;

                if (title != null)
                {
                    folderBrowser.Description = title;
                }

                return folderBrowser.ShowDialog() == DialogResult.OK ? Result<string>.Success(folderBrowser.SelectedPath) : Result<string>.Fail();
            }
        }

        public bool IsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return Path.IsPathRooted(path) && path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
        }

        public bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return Directory.Exists(path);
        }

    }
}
