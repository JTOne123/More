﻿namespace More.Composition.Hosting
{
    using ComponentModel;
    using System.Composition.Convention;
    using System.Composition.Hosting;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Controllers;
    using Web.Http.Services;

    internal sealed class WebApiConventions
    {
        private static readonly IRule<ImportParameter> DefaultImportParameterRule = new Rule<ImportParameter>( DefaultAction.None );

        internal IRule<ImportParameter> ImportParameterRule
        {
            get;
            set;
        } = DefaultImportParameterRule;

        internal void Configure( ContainerConfiguration configuration, ConventionBuilder conventions )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( conventions != null );

            var assembly = typeof( WebApiConventions ).Assembly;
            var decorators = new InterfaceSpecification( typeof( IDecoratorFactory<> ) );

            // export decorator factories
            conventions.ForTypesMatching( decorators.IsSatisfiedBy ).ExportInterfaces( t => t.Assembly == assembly );

            // export web api controllers
            var builder = conventions.ForTypesDerivedFrom<IHttpController>().Export();

            // if the default parameter rule is applied, then we're done
            if ( ImportParameterRule == DefaultImportParameterRule )
                return;

            var constructorRule = new ConstructorSelectionRule();

            builder.SelectConstructor( constructorRule.Evaluate, ( p, b ) => ImportParameterRule.Evaluate( new ImportParameter( p, b ) ) );
        }
    }
}