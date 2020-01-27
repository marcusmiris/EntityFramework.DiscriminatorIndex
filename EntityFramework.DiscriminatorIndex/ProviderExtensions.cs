using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Sql;
using System.Linq;

namespace EntityFramework.DiscriminatorIndex
{
    /// <summary>
    ///     Classe usada para explorar o conteúdo do dependency resolver do Entity Framework.
    /// </summary>
    public static class ProviderExtensions
    {

        public static IEnumerable<string> GetAvailableProviders(this DbMigrationsConfiguration configuration)
        {
            // Marcus Miris @ 19/Out/2016:
            // ... tudo isso apenas para basicamente retornar a string "System.Data.SqlClient" sem hardcode...
            // Explicando: o Entity Framework tem vários Dependencies Resolvers internos para resolver 
            // diversas questões. Uma delas são os providers registrados. Os providers podem ser registrados
            // [1] no AppConfig; [2] no DbMigrationConfiguration; [3] No DbConfiguration.
            // O código abaixo varre todos os resolvers do Entity Framework à procura dos providers.
            var defaultProviders = from resolver in configuration.GetDependencyResolver().GetInternalResolvers()
                                   where resolver is SingletonDependencyResolver<Func<MigrationSqlGenerator>>
                                   let keyPredicate = resolver.GetPrivateField<Func<object, bool>>(@"_keyPredicate")
                                   select keyPredicate.Target.GetPrivateFieldValue("key") as string;
            
            // retorna também os providers que foram registrados pelo caller.
            var customProviders = from keyValuePair in configuration.GetPrivateField<Dictionary<string, MigrationSqlGenerator>>("_sqlGenerators")
                                  select keyValuePair.Key;

            return defaultProviders
                .Union(customProviders)
                .DefaultIfEmpty("System.Data.SqlClient");
        }

        #region ' private '

        /// <summary>
        ///     Recupera o <see cref="IDbDependencyResolver"/> utilizado
        ///     pelo <see cref="DbMigrationsConfiguration"/> informado.
        /// </summary>
        private static IDbDependencyResolver GetDependencyResolver(this DbMigrationsConfiguration configuration) 
            => configuration.GetPrivateField<Lazy<IDbDependencyResolver>>("_resolver")?.Value;

        /// <summary>
        ///     Retorna em uma lista flat os resolvers agragados no resolver informado.
        /// </summary>
        private static IEnumerable<IDbDependencyResolver> GetInternalResolvers(
            this IDbDependencyResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            if (IsCompositeResolver(resolver)) return ExplodeCompositeResolver(resolver);
            else if (IsResolverChain(resolver)) return ExplodeResolverChain(resolver);
            else if (IsRootDependencyResolver(resolver)) return ExplodeRootDepencendyResolver(resolver);
            else if (IsDbProviderServices(resolver)) return ExplodeDbProviderServices(resolver);
            else return new []{ resolver };
            //    throw new NotImplementedException();
        }

        private static bool IsCompositeResolver(IDbDependencyResolver resolver)
        {
            var type = resolver.GetType();
            return type.IsGenericType
                && type.GetGenericTypeDefinition().FullName == @"System.Data.Entity.Infrastructure.DependencyResolution.CompositeResolver`2";
        }

        private static bool IsResolverChain(IDbDependencyResolver resolver)
        {
            return resolver.GetType()
                .FullName
                .Equals(@"System.Data.Entity.Infrastructure.DependencyResolution.ResolverChain");
        }

        private static bool IsRootDependencyResolver(IDbDependencyResolver resolver)
        {
            return resolver.GetType()
                .FullName
                .Equals(@"System.Data.Entity.Infrastructure.DependencyResolution.RootDependencyResolver");
        }

        private static bool IsDbProviderServices(IDbDependencyResolver resolver)
        {
            return resolver is DbProviderServices;
        }

        private static IEnumerable<IDbDependencyResolver> ExplodeCompositeResolver(
            IDbDependencyResolver compositeResolver)
        {
            var type = compositeResolver.GetType();

            var first = (IDbDependencyResolver) type.GetProperty("First").GetValue(compositeResolver, null);
            var second = (IDbDependencyResolver)type.GetProperty("Second").GetValue(compositeResolver, null);

            return GetInternalResolvers(first)
                .Concat(GetInternalResolvers(second));
        }

        private static IEnumerable<IDbDependencyResolver> ExplodeResolverChain(
            IDbDependencyResolver resolverChain)
        {
            var type = resolverChain.GetType();
            var resolvers = (IEnumerable<IDbDependencyResolver>) type.GetProperty(@"Resolvers").GetValue(resolverChain, null);

            return resolvers.SelectMany(GetInternalResolvers);
        }

        private static IEnumerable<IDbDependencyResolver> ExplodeRootDepencendyResolver(
            IDbDependencyResolver rootDependencyResolver)
        {
            var defaultProviderResolvers = (IDbDependencyResolver) rootDependencyResolver.GetPrivateFieldValue("_defaultProviderResolvers");
            var defaultResolvers = (IDbDependencyResolver) rootDependencyResolver.GetPrivateFieldValue("_defaultResolvers");
            var resolvers = (IDbDependencyResolver) rootDependencyResolver.GetPrivateFieldValue("_resolvers");

            return GetInternalResolvers(defaultProviderResolvers)
                .Concat(GetInternalResolvers(defaultResolvers))
                .Concat(GetInternalResolvers(resolvers));
        }

        private static IEnumerable<IDbDependencyResolver> ExplodeDbProviderServices(
            IDbDependencyResolver resolver)
        {
            var dbProviderServices = resolver as DbProviderServices;
            if (dbProviderServices == null) throw new ArgumentException(@"Resolver informado não é um DbProviderServices válido.", nameof(resolver));

            var internalResolver = dbProviderServices
                .GetPrivateField<IDbDependencyResolver>("_resolvers")
                ;

            return GetInternalResolvers(internalResolver);
        }

        #endregion

    }
}
