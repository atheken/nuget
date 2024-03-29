﻿using EnvDTE;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Designers;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MsBuildProject = Microsoft.Build.Evaluation.Project;
using System.Diagnostics.CodeAnalysis;

namespace NuGet.VisualStudio
{
    [Export(typeof(IPackageRestoreManager))]
    internal class PackageRestoreManager : IPackageRestoreManager
    {
        private static readonly string NuGetExeFile = Path.Combine(VsConstants.NuGetSolutionSettingsFolder, "NuGet.exe");
        private static readonly string NuGetTargetsFile = Path.Combine(VsConstants.NuGetSolutionSettingsFolder, "NuGet.targets");
        private const string NuGetBuildPackageName = "NuGet.Build";
        private const string NuGetCommandLinePackageName = "NuGet.CommandLine";

        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly IPackageSourceProvider _packageSourceProvider;
        private readonly ISolutionManager _solutionManager;
        private readonly IPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IVsThreadedWaitDialogFactory _waitDialogFactory;
        private readonly IPackageRepository _localCacheRepository;
        private readonly IVsPackageManagerFactory _packageManagerFactory;
        private readonly DTE _dte;
        private readonly ISettings _settings;
        private IPackageRepository _officialNuGetRepository;

        [ImportingConstructor]
        public PackageRestoreManager(
            ISolutionManager solutionManager,
            IFileSystemProvider fileSystemProvider,
            IPackageRepositoryFactory packageRepositoryFactory,
            IVsPackageManagerFactory packageManagerFactory,
            IVsPackageSourceProvider packageSourceProvider,
            IVsPackageInstallerEvents packageInstallerEvents,
            ISettings settings) :
            this(ServiceLocator.GetInstance<DTE>(),
                 solutionManager,
                 fileSystemProvider,
                 packageRepositoryFactory,
                 packageSourceProvider,
                 packageManagerFactory,
                 packageInstallerEvents,
                 MachineCache.Default,
                 ServiceLocator.GetGlobalService<SVsThreadedWaitDialogFactory, IVsThreadedWaitDialogFactory>(),
                 settings)
        {
        }

        internal PackageRestoreManager(
            DTE dte,
            ISolutionManager solutionManager,
            IFileSystemProvider fileSystemProvider,
            IPackageRepositoryFactory packageRepositoryFactory,
            IPackageSourceProvider packageSourceProvider,
            IVsPackageManagerFactory packageManagerFactory,
            IVsPackageInstallerEvents packageInstallerEvents,
            IPackageRepository localCacheRepository,
            IVsThreadedWaitDialogFactory waitDialogFactory,
            ISettings settings)
        {
            Debug.Assert(solutionManager != null);
            _dte = dte;
            _fileSystemProvider = fileSystemProvider;
            _solutionManager = solutionManager;
            _packageRepositoryFactory = packageRepositoryFactory;
            _packageSourceProvider = packageSourceProvider;
            _waitDialogFactory = waitDialogFactory;
            _packageManagerFactory = packageManagerFactory;
            _localCacheRepository = localCacheRepository;
            _settings = settings;
            _solutionManager.ProjectAdded += OnProjectAdded;
            _solutionManager.SolutionOpened += OnSolutionOpenedOrClosed;
            _solutionManager.SolutionClosed += OnSolutionOpenedOrClosed;
            packageInstallerEvents.PackageReferenceAdded += OnPackageReferenceAdded;
        }

        public bool IsCurrentSolutionEnabledForRestore
        {
            get
            {
                if (!_solutionManager.IsSolutionOpen)
                {
                    return false;
                }

                string solutionDirectory = _solutionManager.SolutionDirectory;
                if (String.IsNullOrEmpty(solutionDirectory))
                {
                    return false;
                }

                IFileSystem fileSystem = _fileSystemProvider.GetFileSystem(solutionDirectory);
                return fileSystem.FileExists(NuGetExeFile) &&
                       fileSystem.FileExists(NuGetTargetsFile);
            }
        }

        public void EnableCurrentSolutionForRestore(bool fromActivation)
        {
            if (!_solutionManager.IsSolutionOpen)
            {
                throw new InvalidOperationException(VsResources.SolutionNotAvailable);
            }

            if (fromActivation)
            {
                // if not in quiet mode, ask user for confirmation before proceeding
                bool? result = MessageHelper.ShowQueryMessage(
                    VsResources.PackageRestoreConfirmation,
                    VsResources.DialogTitle,
                    showCancelButton: false);
                if (result != true)
                {
                    return;
                }
            }

            Exception exception = null;

            IVsThreadedWaitDialog2 waitDialog;
            _waitDialogFactory.CreateInstance(out waitDialog);
            try
            {
                waitDialog.StartWaitDialog(
                    VsResources.DialogTitle,
                    VsResources.PackageRestoreWaitMessage,
                    String.Empty,
                    varStatusBmpAnim: null,
                    szStatusBarText: null,
                    iDelayToShowDialog: 0,
                    fIsCancelable: false,
                    fShowMarqueeProgress: true);

                if (fromActivation)
                {
                    // only enable package restore consent if this is called as a result of user enabling package restore
                    SetPackageRestoreConsent();
                }

                EnablePackageRestore(fromActivation);
            }
            catch (Exception ex)
            {
                exception = ex;
                ExceptionHelper.WriteToActivityLog(exception);
            }
            finally
            {
                int canceled;
                waitDialog.EndWaitDialog(out canceled);
            }

            if (fromActivation)
            {
                if (exception != null)
                {
                    // show error message
                    MessageHelper.ShowErrorMessage(
                        VsResources.PackageRestoreErrorMessage +
                            Environment.NewLine +
                            Environment.NewLine +
                            ExceptionUtility.Unwrap(exception).Message,
                        VsResources.DialogTitle);
                }
                else
                {
                    // show success message
                    MessageHelper.ShowInfoMessage(
                        VsResources.PackageRestoreCompleted,
                        VsResources.DialogTitle);
                }
            }
        }

        public event EventHandler<PackagesMissingStatusEventArgs> PackagesMissingStatusChanged = delegate { };

        public Task RestoreMissingPackages()
        {
            TaskScheduler uiScheduler;
            try
            {
                uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
            catch (InvalidOperationException)
            {
                // this exception occurs during unit tests
                uiScheduler = TaskScheduler.Default;
            }

            Task task = Task.Factory.StartNew(() =>
            {
                IVsPackageManager packageManager = _packageManagerFactory.CreatePackageManager();
                IPackageRepository localRepository = packageManager.LocalRepository;
                var projectReferences = GetAllPackageReferences(packageManager);
                foreach (var reference in projectReferences)
                {
                    if (!localRepository.Exists(reference.Id, reference.Version))
                    {
                        packageManager.InstallPackage(reference.Id, reference.Version, ignoreDependencies: true, allowPrereleaseVersions: true);
                    }
                }
            });

            task.ContinueWith(originalTask =>
            {
                if (originalTask.IsFaulted)
                {
                    ExceptionHelper.WriteToActivityLog(originalTask.Exception);
                }
                else
                {
                    // we don't allow canceling
                    Debug.Assert(!originalTask.IsCanceled);

                    // after we're done with restoring packages, do the check again
                    CheckForMissingPackages();
                }
            }, uiScheduler);

            return task;
        }

        public void CheckForMissingPackages()
        {
            // This method is called by both Solution Opened and Solution Closed event handlers.
            // In the case of Solution Closed event, the IsCurrentSolutionEnabledForRestore is false,
            // and so we won't do the unnecessary work of checking for package references.
            bool missing = IsCurrentSolutionEnabledForRestore && CheckForMissingPackagesCore();
            PackagesMissingStatusChanged(this, new PackagesMissingStatusEventArgs(missing));
        }

        private void EnablePackageRestore(bool fromActivation)
        {
            EnsureNuGetBuild(fromActivation);

            IVsPackageManager packageManager = _packageManagerFactory.CreatePackageManager();
            foreach (Project project in _solutionManager.GetProjects())
            {
                EnablePackageRestore(project, packageManager);
            }
        }

        private void SetPackageRestoreConsent()
        {
            var consent = new PackageRestoreConsent(_settings);
            if (!consent.IsGranted)
            {
                consent.IsGrantedInSettings = true;
            }
        }

        private void EnablePackageRestore(Project project, IVsPackageManager packageManager)
        {
            var projectManager = packageManager.GetProjectManager(project);
            var projectPackageReferences = GetPackageReferences(projectManager);
            if (projectPackageReferences.IsEmpty())
            {
                // don't enable package restore for the project if it doesn't have at least one 
                // nuget package installed
                return;
            }

            EnablePackageRestore(project);
        }

        private void EnablePackageRestore(Project project)
        {
            if (project.IsWebSite())
            {
                // Can't do anything with Website
                // Also, the Javascript Metro project system has some weird bugs 
                // that cause havoc with the package restore mechanism
                return;
            }

            if (project.IsJavaScriptProject())
            {
                EnablePackageRestoreForJavaScriptProject(project);
            }
            else
            {
                MsBuildProject buildProject = project.AsMSBuildProject();
                EnablePackageRestore(project, buildProject, saveProjectWhenDone: true);
            }
        }

        // This method will only be called in VS 2012
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnablePackageRestoreForJavaScriptProject(Project javaScriptProject)
        {
            IVsHierarchy hierarchy = javaScriptProject.ToVsHierarchy();
            IVsBrowseObjectContext context = hierarchy as IVsBrowseObjectContext;
            if (context != null)
            {
                var service = context.UnconfiguredProject.ProjectService.Services.DirectAccessService;
                if (service != null)
                {
                    service.Write(
                        context.UnconfiguredProject.FullPath,
                        dwa =>
                        {
                            MsBuildProject buildProject = dwa.GetProject(context.UnconfiguredProject.Services.SuggestedConfiguredProject);

                            // When inside the Write lock, calling Project.Save() will cause a deadlock.
                            // Thus we will save it after and outside of the Write lock.
                            EnablePackageRestore(javaScriptProject, buildProject, saveProjectWhenDone: false);
                        },
                        ProjectAccess.Read | ProjectAccess.Write);

                    javaScriptProject.Save();
                }
            }
        }

        private void EnablePackageRestore(Project project, MsBuildProject buildProject, bool saveProjectWhenDone)
        {
            AddSolutionDirProperty(buildProject);
            AddNuGetTargets(buildProject);
            SetMsBuildProjectProperty(buildProject, "RestorePackages", "true");
            if (saveProjectWhenDone)
            {
                project.Save();
            }
        }

        private void AddNuGetTargets(MsBuildProject buildProject)
        {
            string targetsPath = Path.Combine(@"$(SolutionDir)", NuGetTargetsFile);

            // adds an <Import> element to this project file.
            if (buildProject.Xml.Imports == null ||
                buildProject.Xml.Imports.All(import => !targetsPath.Equals(import.Project, StringComparison.OrdinalIgnoreCase)))
            {
                buildProject.Xml.AddImport(targetsPath);
                buildProject.ReevaluateIfNecessary();
            }
        }

        private void AddSolutionDirProperty(MsBuildProject buildProject)
        {
            const string solutiondir = "SolutionDir";

            if (buildProject.Xml.Properties == null ||
                buildProject.Xml.Properties.All(p => p.Name != solutiondir))
            {
                string relativeSolutionPath = PathUtility.GetRelativePath(
                    buildProject.FullPath,
                    PathUtility.EnsureTrailingSlash(_solutionManager.SolutionDirectory));
                relativeSolutionPath = PathUtility.EnsureTrailingSlash(relativeSolutionPath);
                
                var solutionDirProperty = buildProject.Xml.AddProperty(solutiondir, relativeSolutionPath);
                solutionDirProperty.Condition =
                    String.Format(
                        CultureInfo.InvariantCulture,
                        @"$({0}) == '' Or $({0}) == '*Undefined*'",
                        solutiondir);
            }
        }

        private static void SetMsBuildProjectProperty(MsBuildProject buildProject, string name, string value)
        {
            if (!value.Equals(buildProject.GetPropertyValue(name), StringComparison.OrdinalIgnoreCase))
            {
                buildProject.SetProperty(name, value);
            }
        }

        private void EnsureNuGetBuild(bool fromActivation)
        {
            string solutionDirectory = _solutionManager.SolutionDirectory;
            string nugetFolderPath = Path.Combine(solutionDirectory, VsConstants.NuGetSolutionSettingsFolder);

            IFileSystem fileSystem = _fileSystemProvider.GetFileSystem(solutionDirectory);

            if (!fileSystem.DirectoryExists(VsConstants.NuGetSolutionSettingsFolder) ||
                !fileSystem.FileExists(NuGetExeFile) ||
                !fileSystem.FileExists(NuGetTargetsFile))
            {
                // download NuGet.Build and NuGet.CommandLine packages into the .nuget folder
                IPackageRepository repository = _packageSourceProvider.GetAggregate(_packageRepositoryFactory, ignoreFailingRepositories: true);

                var installPackages = new string[] { NuGetBuildPackageName, NuGetCommandLinePackageName };
                foreach (var packageId in installPackages)
                {
                    IPackage package = GetPackage(repository, packageId, fromActivation);
                    if (package == null)
                    {
                        throw new InvalidOperationException(
                            String.Format(
                                CultureInfo.CurrentCulture,
                                VsResources.PackageRestoreDownloadPackageFailed,
                                packageId));
                    }

                    fileSystem.AddFiles(package.GetFiles(Constants.ToolsDirectory), VsConstants.NuGetSolutionSettingsFolder, preserveFilePath: false);
                }

                // IMPORTANT: do this BEFORE adding the .nuget folder to solution so that 
                // the generated .nuget\nuget.config is included in the solution folder too. 
                DisableSourceControlMode();

                // now add the .nuget folder to the solution as a solution folder.
                _dte.Solution.AddFolderToSolution(VsConstants.NuGetSolutionSettingsFolder, nugetFolderPath);
            }
        }

        /// <summary>
        /// Try to retrieve the package with the specified Id from machine cache first. 
        /// If not found, download it from the specified repository and add to machine cache.
        /// </summary>
        private IPackage GetPackage(IPackageRepository repository, string packageId, bool fromActivation)
        {
            // first, find the package from the remote repository
            IPackage package = repository.FindPackage(packageId, version: null, allowPrereleaseVersions: true, allowUnlisted: false);

            if (package == null && fromActivation)
            {
                // if we can't find the package from the remote repositories, look for it
                // from nuget.org feed, provided that it's not already specified in one of the remote repositories
                if (!ContainsSource(_packageSourceProvider, NuGetConstants.DefaultFeedUrl) &&
                    !ContainsSource(_packageSourceProvider, NuGetConstants.V2LegacyFeedUrl))
                {
                    if (_officialNuGetRepository == null)
                    {
                        _officialNuGetRepository = _packageRepositoryFactory.CreateRepository(NuGetConstants.DefaultFeedUrl);
                    }

                    package = _officialNuGetRepository.FindPackage(packageId, version: null, allowPrereleaseVersions: true, allowUnlisted: false);
                }
            }

            bool fromCache = false;

            // if package == null, we use whatever version is in the machine cache
            IPackage cachedPackage = _localCacheRepository.FindPackage(packageId, package != null ? package.Version : null);
            if (cachedPackage != null)
            {
                var dataServicePackage = package as DataServicePackage;
                if (dataServicePackage != null)
                {
                    var cachedHash = cachedPackage.GetHash(dataServicePackage.PackageHashAlgorithm);
                    if (!dataServicePackage.PackageHash.Equals(cachedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        // if the remote package has the same hash as with the one in the machine cache, use the one from machine cache
                        package = cachedPackage;
                        fromCache = true;
                    }
                    else
                    {
                        // if the hash has changed, delete the stale package
                        _localCacheRepository.RemovePackage(cachedPackage);
                    }
                }
                else if (package == null)
                {
                    // in this case, we didn't find the package from remote repository.
                    // fallback to using the one in the machine cache.
                    package = cachedPackage;
                    fromCache = true;
                }
            }

            if (package == null)
            {
                return null;
            }

            if (!fromCache)
            {
                _localCacheRepository.AddPackage(package);

                // swap to the Zip package to avoid potential downloading package twice
                package = _localCacheRepository.FindPackage(package.Id, package.Version);
                Debug.Assert(package != null);
            }

            return package;
        }

        private static bool ContainsSource(IPackageSourceProvider provider, string source)
        {
            return provider.GetEnabledPackageSources().Any(p => p.Source.Equals(source, StringComparison.OrdinalIgnoreCase));
        }

        private void DisableSourceControlMode()
        {
            // get the settings for this solution
            var nugetFolder = Path.Combine(_solutionManager.SolutionDirectory, VsConstants.NuGetSolutionSettingsFolder);
            var settings = new Settings(_fileSystemProvider.GetFileSystem(nugetFolder));
            settings.DisableSourceControlMode();
        }

        private void OnProjectAdded(object sender, ProjectEventArgs e)
        {
            if (IsCurrentSolutionEnabledForRestore)
            {
                EnablePackageRestore(e.Project, _packageManagerFactory.CreatePackageManager());
                CheckForMissingPackages();
            }
        }

        private void OnPackageReferenceAdded(IVsPackageMetadata metadata)
        {
            if (IsCurrentSolutionEnabledForRestore)
            {
                var packageMetadata = (VsPackageMetadata)metadata;
                var fileSystem = packageMetadata.FileSystem as IVsProjectSystem;
                if (fileSystem != null)
                {
                    var project = _solutionManager.GetProject(fileSystem.UniqueName);
                    if (project != null)
                    {
                        // in this case, we know that this project has at least one nuget package,
                        // so enable package restore straight away
                        EnablePackageRestore(project);
                    }
                }
            }
        }

        private void OnSolutionOpenedOrClosed(object sender, EventArgs e)
        {
            // We need to do the check even on Solution Closed because, let's say if the yellow Update bar
            // is showing and the user closes the solution; in that case, we want to hide the Update bar.
            CheckForMissingPackages();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't care about exception handling here.")]
        private bool CheckForMissingPackagesCore()
        {
            // this can happen during unit tests
            if (_packageManagerFactory == null)
            {
                return false;
            }

            try
            {
                IVsPackageManager packageManager = _packageManagerFactory.CreatePackageManager();
                IPackageRepository localRepository = packageManager.LocalRepository;
                var projectReferences = GetAllPackageReferences(packageManager);
                return projectReferences.Any(reference => !localRepository.Exists(reference.Id, reference.Version));
            }
            catch (Exception exception)
            {
                // if an exception happens during the check, assume no missing packages and move on.
                ExceptionHelper.WriteToActivityLog(exception);
                return false;
            }
        }

        /// <summary>
        /// Gets all package references in all projects of the current solution plus package 
        /// references specified in the solution packages.config
        /// </summary>
        private IEnumerable<PackageReference> GetAllPackageReferences(IVsPackageManager packageManager)
        {
            IEnumerable<PackageReference> projectReferences = from project in _solutionManager.GetProjects()
                                                              from reference in GetPackageReferences(packageManager.GetProjectManager(project))
                                                              select reference;

            var localRepository = packageManager.LocalRepository as SharedPackageRepository;
            if (localRepository != null)
            {
                IEnumerable<PackageReference> solutionReferences = localRepository.PackageReferenceFile.GetPackageReferences();
                return projectReferences.Concat(solutionReferences).Distinct();
            }

            return projectReferences.Distinct();
        }

        /// <summary>
        /// Gets the package references of the specified project.
        /// </summary>
        private IEnumerable<PackageReference> GetPackageReferences(IProjectManager projectManager)
        {
            var packageRepository = projectManager.LocalRepository as PackageReferenceRepository;
            if (packageRepository != null)
            {
                return packageRepository.ReferenceFile.GetPackageReferences();
            }

            return new PackageReference[0];
        }
    }
}