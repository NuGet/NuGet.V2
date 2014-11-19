﻿using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

using EnvDTE;
using NuGet.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace NuGet.PowerShell.Commands
{
    /// <summary>
    /// This cmdlet returns the list of project names in the current solution, 
    /// which is used for tab expansion.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Project2", DefaultParameterSetName = ParameterSetByName)]
    [OutputType(typeof(Project))]
    public class GetProjectCommand : NuGetPowerShellBaseCommand
    {
        private const string ParameterSetByName = "ByName";
        private const string ParameterSetAllProjects = "AllProjects";

        private readonly ISolutionManager _solutionManager;

        public GetProjectCommand() :
            base(ServiceLocator.GetInstance<IVsPackageSourceProvider>(),
                 ServiceLocator.GetInstance<IPackageRepositoryFactory>(),
                 ServiceLocator.GetInstance<SVsServiceProvider>(),
                 ServiceLocator.GetInstance<IVsPackageManagerFactory>(),
                 ServiceLocator.GetInstance<IHttpClientEvents>())
        {
        }

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSetByName, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "PowerShell API requirement")]
        public string[] Name { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParameterSetAllProjects)]
        public SwitchParameter All { get; set; }

        protected override void ProcessRecordCore()
        {
            if (!SolutionManager.IsSolutionOpen)
            {
                ErrorHandler.ThrowSolutionNotOpenTerminatingError();
            }

            if (All.IsPresent)
            {
                WriteObject(_solutionManager.GetProjects(), enumerateCollection: true);
            }
            else
            {
                // No name specified; return default project (if not null)
                if (Name == null)
                {
                    Project defaultProject = _solutionManager.DefaultProject;
                    if (defaultProject != null)
                    {
                        WriteObject(defaultProject);
                    }
                }
                else
                {
                    // get all projects matching name(s) - handles wildcards
                    WriteObject(GetProjectsByName(Name), enumerateCollection: true);
                }
            }
        }

        private object GetProjectsByName(string[] Name)
        {
            throw new System.NotImplementedException();
        }
    }
}
