﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.LanguageServices.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using InternalUtilities = Microsoft.Internal.VisualStudio.PlatformUI.Utilities;
using IOleCommandTarget = Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget;
using OLECMD = Microsoft.VisualStudio.OLE.Interop.OLECMD;
using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace Microsoft.VisualStudio.LanguageServices.DocumentOutline
{
    /// <summary>
    /// Interaction logic for DocumentOutlineView.xaml
    /// All operations happen on the UI thread for visual studio
    /// </summary>
    internal sealed partial class DocumentOutlineView : UserControl, IOleCommandTarget, IDisposable, IVsWindowSearch
    {
        private readonly IThreadingContext _threadingContext;
        private readonly IGlobalOptionService _globalOptionService;
        private readonly VsCodeWindowViewTracker _viewTracker;
        private readonly DocumentOutlineViewModel _viewModel;
        private readonly IVsToolbarTrayHost _toolbarTrayHost;
        private readonly IVsWindowSearchHost _windowSearchHost;

        public DocumentOutlineView(
            IVsUIShell4 uiShell,
            IVsWindowSearchHostFactory windowSearchHostFactory,
            IThreadingContext threadingContext,
            IGlobalOptionService globalOptionService,
            VsCodeWindowViewTracker viewTracker,
            DocumentOutlineViewModel viewModel)
        {
            _threadingContext = threadingContext;
            _globalOptionService = globalOptionService;
            _viewTracker = viewTracker;
            _viewModel = viewModel;

            DataContext = _viewModel;
            InitializeComponent();
            UpdateSort(_globalOptionService.GetOption(DocumentOutlineOptionsStorage.DocumentOutlineSortOrder), userSelected: false);

            ErrorHandler.ThrowOnFailure(uiShell.CreateToolbarTray(this, out _toolbarTrayHost));
            ErrorHandler.ThrowOnFailure(_toolbarTrayHost.AddToolbar(Guids.RoslynGroupId, ID.RoslynCommands.DocumentOutlineToolbar));

            ErrorHandler.ThrowOnFailure(_toolbarTrayHost.GetToolbarTray(out var toolbarTray));
            ErrorHandler.ThrowOnFailure(toolbarTray.GetUIObject(out var uiObject));
            ErrorHandler.ThrowOnFailure(((IVsUIWpfElement)uiObject).GetFrameworkElement(out var frameworkElement));
            Commands.Content = frameworkElement;

            _windowSearchHost = windowSearchHostFactory.CreateWindowSearchHost(SearchHost);
            _windowSearchHost.SetupSearch(this);

            viewTracker.CaretMovedOrActiveViewChanged += ViewTracker_CaretMovedOrActiveViewChanged;
        }

        public void Dispose()
        {
            _toolbarTrayHost.Close();
            _windowSearchHost.TerminateSearch();
            _viewTracker.CaretMovedOrActiveViewChanged -= ViewTracker_CaretMovedOrActiveViewChanged;
            _viewModel.Dispose();
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == Guids.RoslynGroupId)
            {
                for (var i = 0; i < cCmds; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case ID.RoslynCommands.DocumentOutlineExpandAll:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            break;

                        case ID.RoslynCommands.DocumentOutlineCollapseAll:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            break;

                        case ID.RoslynCommands.DocumentOutlineSortByName:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            if (_viewModel.SortOption == SortOption.Name)
                                prgCmds[i].cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

                            break;

                        case ID.RoslynCommands.DocumentOutlineSortByOrder:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            if (_viewModel.SortOption == SortOption.Location)
                                prgCmds[i].cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

                            break;

                        case ID.RoslynCommands.DocumentOutlineSortByType:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            if (_viewModel.SortOption == SortOption.Type)
                                prgCmds[i].cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

                            break;

                        default:
                            prgCmds[i].cmdf = 0;
                            break;
                    }
                }

                return VSConstants.S_OK;
            }

            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == Guids.RoslynGroupId)
            {
                switch (nCmdID)
                {
                    case ID.RoslynCommands.DocumentOutlineExpandAll:
                        _viewModel.ExpandOrCollapseAll(true);
                        return VSConstants.S_OK;

                    case ID.RoslynCommands.DocumentOutlineCollapseAll:
                        _viewModel.ExpandOrCollapseAll(false);
                        return VSConstants.S_OK;

                    case ID.RoslynCommands.DocumentOutlineSortByName:
                        UpdateSort(SortOption.Name, userSelected: true);
                        return VSConstants.S_OK;

                    case ID.RoslynCommands.DocumentOutlineSortByOrder:
                        UpdateSort(SortOption.Location, userSelected: true);
                        return VSConstants.S_OK;

                    case ID.RoslynCommands.DocumentOutlineSortByType:
                        UpdateSort(SortOption.Type, userSelected: true);
                        return VSConstants.S_OK;
                }
            }

            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        bool IVsWindowSearch.SearchEnabled => true;

        Guid IVsWindowSearch.Category => Guids.DocumentOutlineSearchCategoryId;

        IVsEnumWindowSearchFilters? IVsWindowSearch.SearchFiltersEnum => null;

        IVsEnumWindowSearchOptions? IVsWindowSearch.SearchOptionsEnum => null;

        IVsSearchTask IVsWindowSearch.CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            _viewModel.SearchText = pSearchQuery.SearchString;
            return new VsSearchTask(dwCookie, pSearchQuery, pSearchCallback);
        }

        void IVsWindowSearch.ClearSearch()
        {
            _viewModel.SearchText = "";
        }

        void IVsWindowSearch.ProvideSearchSettings(IVsUIDataSource pSearchSettings)
        {
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.ControlMaxWidth, uint.MaxValue);
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.SearchStartType, (uint)VSSEARCHSTARTTYPE.SST_DELAYED);
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.SearchStartDelay, (uint)100);
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.SearchUseMRU, true);
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.PrefixFilterMRUItems, false);
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.MaximumMRUItems, (uint)25);
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.SearchWatermark, ServicesVSResources.Document_Outline_Search);
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.SearchPopupAutoDropdown, false);
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.ControlBorderThickness, "1");
            InternalUtilities.SetValue(pSearchSettings, SearchSettingsDataSource.PropertyNames.SearchProgressType, (uint)VSSEARCHPROGRESSTYPE.SPT_INDETERMINATE);
        }

        bool IVsWindowSearch.OnNavigationKeyDown(uint dwNavigationKey, uint dwModifiers)
        {
            // By default we are not interesting in intercepting navigation keys, so return "not handled"
            return false;
        }

        private void UpdateSort(SortOption sortOption, bool userSelected)
        {
            _threadingContext.ThrowIfNotOnUIThread();

            if (userSelected)
            {
                // Log which sort option was used and save it back to the global options
                Logger.Log(sortOption switch
                {
                    SortOption.Name => FunctionId.DocumentOutline_SortByName,
                    SortOption.Location => FunctionId.DocumentOutline_SortByOrder,
                    SortOption.Type => FunctionId.DocumentOutline_SortByType,
                    _ => throw new NotImplementedException(),
                }, logLevel: LogLevel.Information);

                _globalOptionService.SetGlobalOption(DocumentOutlineOptionsStorage.DocumentOutlineSortOrder, sortOption);
            }

            // "DocumentSymbolItems" is the key name we specified for our CollectionViewSource in the XAML file
            var collectionView = ((CollectionViewSource)FindResource("DocumentSymbolItems")).View;

            // Defer changes until all the properties have been set
            using (var _ = collectionView.DeferRefresh())
            {
                // Update top-level sorting options for our tree view
                UpdateSortDescription(collectionView.SortDescriptions, sortOption);

                // Set the sort option property to begin live-sorting
                _viewModel.SortOption = sortOption;
            }

            // Queue a refresh now that everything is set.
            collectionView.Refresh();
        }

        private static ImmutableArray<SortDescription> NameSortDescriptions { get; } =
            ImmutableArray.Create(new SortDescription(
                $"{nameof(DocumentSymbolDataViewModel.Data)}.{nameof(DocumentSymbolDataViewModel.Data.Name)}",
                ListSortDirection.Ascending));
        private static ImmutableArray<SortDescription> LocationSortDescriptions { get; } =
            ImmutableArray.Create(new SortDescription(
                $"{nameof(DocumentSymbolDataViewModel.Data)}.{nameof(DocumentSymbolDataViewModel.Data.RangeSpan)}.{nameof(DocumentSymbolDataViewModel.Data.RangeSpan.Start)}.{nameof(DocumentSymbolDataViewModel.Data.RangeSpan.Start.Position)}",
                ListSortDirection.Ascending));
        private static ImmutableArray<SortDescription> TypeSortDescriptions { get; } = ImmutableArray.Create(
            new SortDescription(
                $"{nameof(DocumentSymbolDataViewModel.Data)}.{nameof(DocumentSymbolDataViewModel.Data.SymbolKind)}",
                ListSortDirection.Ascending),
            new SortDescription(
                $"{nameof(DocumentSymbolDataViewModel.Data)}.{nameof(DocumentSymbolDataViewModel.Data.Name)}",
                ListSortDirection.Ascending));

        public static void UpdateSortDescription(SortDescriptionCollection sortDescriptions, SortOption sortOption)
        {
            sortDescriptions.Clear();
            var newSortDescriptions = sortOption switch
            {
                SortOption.Name => NameSortDescriptions,
                SortOption.Location => LocationSortDescriptions,
                SortOption.Type => TypeSortDescriptions,
                _ => throw new InvalidOperationException(),
            };

            foreach (var newSortDescription in newSortDescriptions)
            {
                sortDescriptions.Add(newSortDescription);
            }
        }

        /// <summary>
        /// When a symbol node in the window is selected via the keyboard, move the caret to its position in the latest active text view.
        /// </summary>
        private void SymbolTreeItem_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            _threadingContext.ThrowIfNotOnUIThread();

            if (!_viewModel.IsNavigating && e.OriginalSource is TreeViewItem { DataContext: DocumentSymbolDataViewModel symbolModel })
            {
                // This is a user-initiated navigation, and we need to prevent reentrancy.  Specifically: when a user
                // does click on an item, we do navigate, and that does move the caret. This part happens synchronously.
                // So we do want to block navigation in that case.
                _viewModel.IsNavigating = true;
                try
                {
                    var textView = _viewTracker.GetActiveView();
                    textView.TryMoveCaretToAndEnsureVisible(
                        symbolModel.Data.SelectionRangeSpan.TranslateTo(textView.TextSnapshot, SpanTrackingMode.EdgeInclusive).Start);
                }
                finally
                {
                    _viewModel.IsNavigating = false;
                }
            }
        }

        /// <summary>
        /// On caret position change, highlight the corresponding symbol node in the window and update the view.
        /// </summary>
        private void ViewTracker_CaretMovedOrActiveViewChanged(object sender, EventArgs e)
        {
            _threadingContext.ThrowIfNotOnUIThread();
            _viewModel.ExpandAndSelectItemAtCaretPosition();
        }
    }
}
