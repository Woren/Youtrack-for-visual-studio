﻿//------------------------------------------------------------------------------
// <copyright file="YouTrackIssuesWindow.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using YouTrackClientVS.Contracts.Interfaces.ViewModels;
using YouTrackClientVS.Contracts.Interfaces.Views;
using YouTrackIssuesWindowContainer = YouTrackClientVS.UI.Views.YouTrackIssuesWindowContainer;

namespace YouTrackClientVS.VisualStudio.UI.Window
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("62C67AF1-674A-4390-8C86-32E80954D820")]
    public class YouTrackIssuesWindow : ToolWindowPane, IYouTrackIssuesWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YouTrackIssuesWindow"/> class.
        /// </summary>
        public YouTrackIssuesWindow() : base(null)
        {
            Caption = "YouTrack - Issues";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new YouTrackIssuesWindowContainer();
        }

        public IYouTrackIssuesWindowContainer Container => (YouTrackIssuesWindowContainer)Content;

        public void Close()
        {
            ((IVsWindowFrame)Frame).CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
        }
    }
}
