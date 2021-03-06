﻿namespace More.VisualStudio.Templates
{
    using EnvDTE;
    using Microsoft.VisualStudio.Shell.Interop;
    using More.VisualStudio.ViewModels;
    using More.VisualStudio.Views;
    using System;

    /// <summary>
    /// Represents a template wizard to create a view using the Model-View-ViewModel (MVVM) pattern.
    /// </summary>
    [CLSCompliant( false )]
    public class ViewTemplateWizard : ItemTemplateWizard
    {
        bool createNewViewModel;
        bool viewModelAdded;
        string viewModelTemplateKey;

        /// <summary>
        /// Occurs after a project item finished being generated.
        /// </summary>
        /// <param name="projectItem">The generated <see cref="ProjectItem">project item</see>.</param>
        public override void ProjectItemFinishedGenerating( ProjectItem projectItem )
        {
            if ( !createNewViewModel || viewModelAdded || projectItem == null )
            {
                return;
            }

            viewModelAdded = true;

            var viewModelName = GetString( "$viewmodel$" );
            var viewModelTemplate = GetString( viewModelTemplateKey );

            if ( string.IsNullOrEmpty( viewModelName ) || string.IsNullOrEmpty( viewModelTemplate ) )
            {
                return;
            }

            var language = projectItem.ContainingProject.GetTemplateLanguage();

            AddFromTemplate( "ViewModels", viewModelTemplate, viewModelName, language );
        }

        /// <summary>
        /// Attempts to run the template wizard.
        /// </summary>
        /// <param name="shell">The <see cref="IVsUIShell">shell</see> associated with the wizard.</param>
        /// <returns>True if the wizard completed successfully; otherwise, false if the wizard was canceled.</returns>
        protected override bool TryRunWizard( IVsUIShell shell )
        {
            Arg.NotNull( shell, nameof( shell ) );

            createNewViewModel = false;

            var mapper = new ViewReplacementsMapper( Project );
            var model = new ViewItemTemplateWizardViewModel();

            mapper.Map( Context.Replacements, model );

            if ( Context.IsInteractive )
            {
                var projectInfo = new ProjectInformation( Project );
                var view = new ViewItemTemplateWizard( model, projectInfo );

                if ( !( view.ShowDialog( shell ) ?? false ) )
                {
                    return false;
                }
            }

            mapper.Map( model, Context.Replacements );
            createNewViewModel = model.ViewModelOption == 1;
            viewModelTemplateKey = model.IsTopLevelSupported && model.IsTopLevel ? "_topLevelViewModelTemplateName" : "_viewModelTemplateName";

            return true;
        }
    }
}