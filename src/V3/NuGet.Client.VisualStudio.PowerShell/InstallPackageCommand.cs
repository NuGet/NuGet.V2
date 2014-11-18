﻿using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using NuGet.Client;
using NuGet.Client.Resolution;
using NuGet.Client.VisualStudio.PowerShell;
using NuGet.Versioning;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;


#if VS14
using Microsoft.VisualStudio.ProjectSystem.Interop;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
#endif

namespace NuGet.PowerShell.Commands
{
    /// <summary>
    /// This command installs the specified package into the specified project.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Install, "Package2")]
    public class InstallPackageCommand : ProcessPackageBaseCommand
    {
        private ResolutionContext _context;
        private string _version;

        public InstallPackageCommand() :
            base(ServiceLocator.GetInstance<IVsPackageSourceProvider>(),
                 ServiceLocator.GetInstance<IPackageRepositoryFactory>(),
                 ServiceLocator.GetInstance<SVsServiceProvider>(),
                 ServiceLocator.GetInstance<IVsPackageManagerFactory>(),
                 PackageActionType.Install)
        {
            this.PackageActionResolver = new ActionResolver(this.RepoManager.ActiveRepository, ResContext);
            this.PackageVersion = Version;
        }

        [Parameter(Position = 2)]
        public string Version
        {
            get
            {
                _version = VersionUtil.GetLastestVersionForPackage(this.RepoManager.ActiveRepository, this.Id);
                return _version;
            }
            set
            {
                _version = value;
            }
        }

        [Parameter]
        public DependencyBehavior DependencyBehavior { get; set; }

        [Parameter, Alias("Prerelease")]
        public SwitchParameter IncludePrerelease { get; set; }


        public ResolutionContext ResContext
        {
            get
            {
                _context = new ResolutionContext();
                _context.DependencyBehavior = DependencyBehavior;
                _context.AllowPrerelease = IncludePrerelease.IsPresent;
                return _context;
            }
        }
    }
} 