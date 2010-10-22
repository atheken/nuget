﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.ExtensionsExplorer.UI;
using Microsoft.VisualStudio.PlatformUI;
using NuPack.Dialog.Providers;
using NuPack.VisualStudio;
using DTEPackage = Microsoft.VisualStudio.Shell.Package;

namespace NuPack.Dialog.PackageManagerUI {

    public partial class PackageManagerWindow : DialogWindow, ILicenseWindowOpener {

        private const string F1Keyword = "vs.ExtensionManager";

        private readonly DTEPackage _ownerPackage;

        public PackageManagerWindow(DTEPackage ownerPackage)
            : base(F1Keyword) {

            InitializeComponent();

            System.Diagnostics.Debug.Assert(ownerPackage != null);
            _ownerPackage = ownerPackage;

            SetupProviders();
        }

        private void SetupProviders() {
            VsPackageManager packageManager = new VsPackageManager(DTEExtensions.DTE);
            EnvDTE.Project activeProject = DTEExtensions.DTE.GetActiveProject();

            IProjectManager projectManager = packageManager.GetProjectManager(activeProject);

            // The ExtensionsExplorer control display providers in reverse order.
            // We want the providers to appear as Installed - Online - Updates

            var updatesProvider = new UpdatesProvider(packageManager, projectManager, Resources);
            explorer.Providers.Add(updatesProvider);

            var onlineProvider = new OnlineProvider(packageManager, projectManager, Resources, PackageRepositoryFactory.Default, VsPackageSourceProvider.GetSourceProvider(DTEExtensions.DTE));
            explorer.Providers.Add(onlineProvider);

            var installedProvider = new InstalledProvider(packageManager, projectManager, Resources);
            explorer.Providers.Add(installedProvider);

            // make the Installed provider as selected by default
            explorer.SelectedProvider = installedProvider;
        }

        private void CanExecuteCommandOnPackage(object sender, CanExecuteRoutedEventArgs e) {

            if (OperationCoordinator.IsBusy) {
                e.CanExecute = false;
                return;
            }

            VSExtensionsExplorerCtl control = e.Source as VSExtensionsExplorerCtl;
            if (control == null) {
                e.CanExecute = false;
                return;
            }

            PackageItem selectedItem = control.SelectedExtension as PackageItem;
            if (selectedItem == null) {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = selectedItem.IsEnabled;
        }

        private void ExecutedPackageCommand(object sender, ExecutedRoutedEventArgs e) {
            if (OperationCoordinator.IsBusy) {
                return;
            }

            VSExtensionsExplorerCtl control = e.Source as VSExtensionsExplorerCtl;
            if (control == null) {
                return;
            }

            PackageItem selectedItem = control.SelectedExtension as PackageItem;
            if (selectedItem == null) {
                return;
            }

            PackagesProviderBase provider = control.SelectedProvider as PackagesProviderBase;
            if (provider != null) {
                provider.Execute(selectedItem, this);
            }
        }

        private void ExecutedClose(object sender, ExecutedRoutedEventArgs e) {
            this.Close();
        }

        private void ExecutedShowOptionsPage(object sender, ExecutedRoutedEventArgs e) {
            this.Close();
            _ownerPackage.ShowOptionPage(typeof(ToolsOptionsUI.ToolsOptionsPage));
        }

        private void ExecuteOpenLicenseLink(object sender, ExecutedRoutedEventArgs e) {
            VSExtensionsExplorerCtl control = e.Source as VSExtensionsExplorerCtl;
            if (control == null) {
                return;
            }

            PackageItem selectedItem = control.SelectedExtension as PackageItem;
            if (selectedItem == null) {
                return;
            }

            UriHelper.OpenLicenseLink(selectedItem.LicenseUrl);
            e.Handled = true;
        }

        private void ExecuteSetFocusOnSearchBox(object sender, ExecutedRoutedEventArgs e) {
            explorer.SetFocusOnSearchBox();
        }

        bool ILicenseWindowOpener.ShowLicenseWindow(IEnumerable<IPackage> dataContext) {
            var licenseWidow = new LicenseAcceptanceWindow() {
                Owner = this,
                DataContext = dataContext
            };

            bool? dialogResult = licenseWidow.ShowDialog();
            return dialogResult ?? false;
        }

        private void OnCategorySelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            PackagesTreeNodeBase selectedNode = explorer.SelectedExtensionTreeNode as PackagesTreeNodeBase;
            if (selectedNode != null) {
                // notify the selected node that it is opened.
                selectedNode.OnOpened();
            }
        }

        private void OnDialogWindowClosed(object sender, EventArgs e) {
            explorer.Providers.Clear();
        }
    }
}