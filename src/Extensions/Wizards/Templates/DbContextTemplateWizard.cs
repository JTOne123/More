﻿namespace More.VisualStudio.Templates
{
    using EnvDTE;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Data.Core;
    using Microsoft.VisualStudio.Data.Services;
    using Microsoft.VisualStudio.DataTools.Interop;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VSDesigner.VSDesignerPackage;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml.Linq;
    using ViewModels;
    using Views;
    using IVsPackageInstaller = NuGet.VisualStudio.IVsPackageInstaller;
    using IVsPackageInstallerServices = NuGet.VisualStudio.IVsPackageInstallerServices;

    /// <summary>
    /// Represents a template wizard to create an Entity Framework (EF) DbContext with interface scaffolding.
    /// </summary>
    public class DbContextTemplateWizard : ItemTemplateWizard
    {
        private void InstallPackages( IComponentModel services, XElement packages, IDictionary<string, string> packageVersions )
        {
            Contract.Requires( services != null );
            Contract.Requires( packages != null );
            Contract.Requires( packageVersions != null );

            if ( packageVersions.Count == 0 )
                return;

            var extensionId = (string) packages.Attribute( "repositoryId" );
            var installer = services.GetService<IVsPackageInstaller>();
            var unzipped = false;
            var skipAssemblyReferences = false;
            var ignoreDependencies = false;

            // although it's less efficient, we install the packages one at a time to display status.
            // the mechanism to report back status is internal and can't be wired up without some
            // crafty reflection hacks.  this is a more straight forward alternative.
            foreach ( var entry in packageVersions )
            {
                var packageVersion = new Dictionary<string, string>()
                {
                    { entry.Key, entry.Value }
                };

                // provide user feedback
                DesignTimeEnvironment.StatusBar.Text = SR.PackageInstallStatus.FormatDefault( entry.Key, entry.Value );

                // install the package from the vsix location
                installer.InstallPackagesFromVSExtensionRepository( extensionId, unzipped, skipAssemblyReferences, ignoreDependencies, Project, packageVersion );
            }
        }

        private void InstallCompositionPackagesIfNeeded( IComponentModel services, IVsPackageInstallerServices nuget, Lazy<XElement> wizardData )
        {
            Contract.Requires( services != null );
            Contract.Requires( nuget != null );
            Contract.Requires( wizardData != null );

            // ensure composition is enabled
            if ( !GetBoolean( "$compose$" ) )
                return;

            var packages = wizardData.Value;
            var packageIds = new[] { "Microsoft.Composition", "More.Composition" };
            var packageVersions = new Dictionary<string, string>();

            // build collection of required packages and versions
            foreach ( var packageId in packageIds )
            {
                if ( nuget.IsPackageInstalled( Project, packageId ) )
                    continue;

                var packageVersion = ( from element in packages.Elements( "package" )
                                       let id = (string) element.Attribute( "id" )
                                       where id == packageId
                                       select (string) element.Attribute( "version" ) ).FirstOrDefault();

                if ( !string.IsNullOrEmpty( packageVersion ) )
                    packageVersions[packageId] = packageVersion;
            }

            InstallPackages( services, packages, packageVersions );
        }

        private void InstallEntityFrameworkPackageIfNeeded( IComponentModel services, IVsPackageInstallerServices nuget, Lazy<XElement> wizardData )
        {
            Contract.Requires( services != null );
            Contract.Requires( nuget != null );
            Contract.Requires( wizardData != null );

            // determine whether the package is already installed
            if ( nuget.IsPackageInstalled( Project, "EntityFramework" ) )
                return;

            var packages = wizardData.Value;
            var selectedId = GetString( "_SelectedEFVersion" );
            var packageVersion = ( from element in packages.Elements( "package" )
                                   let id = (string) element.Attribute( "id" )
                                   where id == selectedId
                                   select (string) element.Attribute( "version" ) ).FirstOrDefault();

            // package version unknown (should only happen if there's an error in the wizard or the vstemplate)
            if ( string.IsNullOrEmpty( packageVersion ) )
                return;

            var packageVersions = new Dictionary<string, string>()
            {
                { "EntityFramework", packageVersion }
            };

            InstallPackages( services, packages, packageVersions );
        }

        private ProjectItem GetOrCreateConfigFile()
        {
            var fileName = Project.GetConfigurationFileName();
            var comparer = StringComparer.OrdinalIgnoreCase;
            var configFile = Project.ProjectItems.Cast<ProjectItem>().FirstOrDefault( pi => comparer.Equals( pi.Name, fileName ) );

            // if a *.config file already exists, there's nothing to do
            if ( configFile != null )
                return configFile;

            var vb = Project.IsVisualBasic();

            // add *.config for a web application
            if ( Project.IsWebApp() )
                return AddFromTemplate( "WebConfig.zip", fileName, vb ? "{349C5854-65DF-11DA-9384-00065B846F21}" : "{349C5853-65DF-11DA-9384-00065B846F21}" );

            // add *.config for all other web project types
            var templateName = Project.IsWebProject() ? "WebConfig.zip" : ( vb ? "AppConfigurationInternal.zip" : "AppConfigInternal.zip" );
            return AddFromTemplate( templateName, fileName, Project.Kind );
        }

        private void UpdateConfigFileIfNeeded()
        {
            var cs = GetString( "$connectionString$" );

            if ( string.IsNullOrEmpty( cs ) )
                return;

            var name = GetString( "$connectionStringKey$" );
            var configFile = GetOrCreateConfigFile();
            var sourceControl = DesignTimeEnvironment.SourceControl;

            // check-out file if necessary
            if ( sourceControl.IsItemUnderSCC( configFile.Name ) && !sourceControl.IsItemCheckedOut( configFile.Name ) )
                sourceControl.CheckOutItem( configFile.Name );

            // get <connectionStrings/> element
            var path = configFile.FileNames[1];
            var xml = XDocument.Load( path );
            var configuration = xml.Root;
            var connectionStrings = configuration.Element( "connectionStrings" );

            // add <connectionStrings/> element as necessary
            if ( connectionStrings == null )
            {
                connectionStrings = new XElement( "connectionStrings" );
                configuration.Add( connectionStrings );
            }

            // try to find existing <add/> element with matching name
            var add = connectionStrings.Elements( "add" ).FirstOrDefault( e => (string) e.Attribute( "name" ) == name );

            // add or update <add/> element
            if ( add == null )
            {
                var providerName = GetString( "$providerName$", "System.Data.SqlClient" );
                add = new XElement( "add", new XAttribute( "name", name ), new XAttribute( "connectionString", cs ), new XAttribute( "providerName", providerName ) );
                connectionStrings.Add( add );
            }
            else
            {
                // if a provider name isn't provided, use the current value (if any) or assume the default value
                var providerName = GetString( "$providerName$", (string) add.Attribute( "providerName" ) ?? "System.Data.SqlClient" );
                add.SetAttributeValue( "connectionString", cs );
                add.SetAttributeValue( "providerName", providerName );
            }

            // save changes
            xml.Save( path );
        }

        /// <summary>
        /// Occurs after a project item finished being generated.
        /// </summary>
        /// <param name="projectItem">The generated <see cref="ProjectItem">project item</see>.</param>
        public override void ProjectItemFinishedGenerating( ProjectItem projectItem )
        {
            if ( projectItem == null )
                return;

            projectItem.Properties.Item( "CustomTool" ).Value = "DbContextGenerator";
            DesignTimeEnvironment.StatusBar.Text = SR.EvaluatingPackages;

            var services = Context.GetRequiredService<IComponentModel>();
            var nuget = services.GetService<IVsPackageInstallerServices>();
            var wizardData = new Lazy<XElement>( () => XDocument.Parse( GetString( "$packagedata$", "<packages />" ) ).Root );

            InstallCompositionPackagesIfNeeded( services, nuget, wizardData );
            InstallEntityFrameworkPackageIfNeeded( services, nuget, wizardData );
            UpdateConfigFileIfNeeded();
        }

        private DbContextReplacementsMapper CreateMapper( DbContextItemTemplateWizardViewModel model )
        {
            Contract.Requires( model != null );
            Contract.Ensures( Contract.Result<DbContextReplacementsMapper>() != null );

            var dataProviderManager = Context.GetRequiredService<IVsDataProviderManager>();
            var providerMapper = new Lazy<IDTAdoDotNetProviderMapper>( Context.GetRequiredService<IDTAdoDotNetProviderMapper> );
            var dataExplorerConnectionManager = Context.GetRequiredService<IVsDataExplorerConnectionManager>();
            Func<IVsDataConnectionManager> dataConnectionManagerFactory = Context.GetRequiredService<IVsDataConnectionManager>;
            IGlobalConnectionService globalConnectionService;

            // we honor the global connection service if available, but we don't need it
            Context.TryGetService( out globalConnectionService );

            return new DbContextReplacementsMapper( Project, Context, dataProviderManager, globalConnectionService, providerMapper, dataExplorerConnectionManager, dataConnectionManagerFactory );
        }

        private DbContextItemTemplateWizard CreateView( DbContextItemTemplateWizardViewModel model, IVsUIShell shell )
        {
            Contract.Requires( model != null );
            Contract.Requires( shell != null );
            Contract.Ensures( Contract.Result<DbContextItemTemplateWizard>() != null );

            var projectInfo = new ProjectInformation( Project );
            var dataConnectionDialogFactory = new Lazy<IVsDataConnectionDialogFactory>( Context.GetRequiredService<IVsDataConnectionDialogFactory> );
            var dataExplorerConnectionManager = new Lazy<IVsDataExplorerConnectionManager>( Context.GetRequiredService<IVsDataExplorerConnectionManager> );

            return new DbContextItemTemplateWizard( model, projectInfo, shell, dataConnectionDialogFactory, dataExplorerConnectionManager );
        }

        /// <summary>
        /// Attempts to run the template wizard.
        /// </summary>
        /// <param name="shell">The <see cref="IVsUIShell">shell</see> associated with the wizard.</param>
        /// <returns>True if the wizard completed successfully; otherwise, false if the wizard was canceled.</returns>
        protected override bool TryRunWizard( IVsUIShell shell )
        {
            Arg.NotNull( shell, nameof( shell ) );

            var model = new DbContextItemTemplateWizardViewModel();
            var mapper = CreateMapper( model );
            var view = CreateView( model, shell );
            var statusBar = DesignTimeEnvironment.StatusBar;

            // the mapping relies on visual studio enumerating the available data sources,
            // which could take a while. we [seemingly] cannot run this in the background
            // so provide the user with some feedback to let them know we are doing work.
            statusBar.Text = SR.StatusInitializingDataSources;
            mapper.Map( Context.Replacements, model );
            statusBar.Clear();

            if ( !( view.ShowDialog( shell ) ?? false ) )
                return false;

            // map model back to replacements
            mapper.Map( model, Context.Replacements );
            return true;
        }
    }
}
