﻿using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using YouTrackClientVS.Contracts.Interfaces.Services;
using YouTrackClientVS.Infrastructure.Extensions;

namespace YouTrackClientVS.VisualStudio.UI
{
    [Export(typeof(IAppServiceProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AppServiceProvider : IAppServiceProvider
    {
        private readonly IServiceProvider _globalServiceProvider;
        private readonly ExportProvider _appExportProvider;
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private readonly CompositionContainer _compositionContainer;

        private readonly Dictionary<string, OwnedComposablePart> _composableParts =
            new Dictionary<string, OwnedComposablePart>();

        private readonly Version _currentVersion;
        private readonly List<Func<Type, object>> _serviceImporters;

        public IServiceProvider YouTrackServiceProvider { get; set; }

        [ImportingConstructor]
        public AppServiceProvider([Import(typeof(SVsServiceProvider))] IServiceProvider globalServiceProvider)
        {
            _currentVersion = typeof(AppServiceProvider).Assembly.GetName().Version;
            _globalServiceProvider = globalServiceProvider;

            var componentModel = globalServiceProvider.GetService<SComponentModel, IComponentModel>();
            _appExportProvider = componentModel.DefaultExportProvider;
            _compositionContainer =
                new CompositionContainer(new ComposablePartExportProvider { SourceProvider = _appExportProvider });
            _disposables.Add(_compositionContainer);

            _serviceImporters = new List<Func<Type, object>>()
            {
                GetServiceFromCompositionContainer,
                GetServiceFromExportProvider,
                _globalServiceProvider.GetService,
                serviceType => YouTrackServiceProvider?.GetService(serviceType)
            };
        }

        public TService GetService<TService>() where TService : class
        {
            return GetService(typeof(TService)) as TService;
        }

        public object GetService(Type serviceType)
        {
            foreach (var serviceImporter in _serviceImporters)
            {
                var result = serviceImporter(serviceType);
                if (result is IDisposable)
                    _disposables.Add(result as IDisposable); //todo this will hold all objects

                if (result != null)
                    return result;
            }

            return null;
        }

        private object GetServiceFromCompositionContainer(Type type)
        {
            var contract = AttributedModelServices.GetContractName(type);
            return _compositionContainer.GetExportedValueOrDefault<object>(contract);
        }


        private object GetServiceFromExportProvider(Type type)
        {
            var contract = AttributedModelServices.GetContractName(type);
            //TODO we don't have to match it with assembly and version as long as we don't support vs multiple versions i.e 14.0 and 15.0
            return _appExportProvider
                .GetExportedValues<object>(contract) //TODO below condition is wrong?
                .FirstOrDefault(x =>
                    !contract.StartsWith("GitClientVS.", StringComparison.OrdinalIgnoreCase) ||
                    x.GetType().Assembly.GetName().Version == _currentVersion);
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                try
                {
                    disposable?.Dispose();
                }
                catch (Exception)
                {
                    // ignore ex on dispose
                }
        }


        public void AddService<TService>(TService obj, object owner)
        {
            var contract = AttributedModelServices.GetContractName(typeof(TService));

            RemoveService(typeof(TService), null);

            var batch = new CompositionBatch();
            var part = batch.AddExportedValue(contract, obj);

            _composableParts.Add(contract, new OwnedComposablePart { Owner = owner, Part = part });
            _compositionContainer.Compose(batch);
        }


        public void RemoveService(Type t, object owner)
        {
            var contract = AttributedModelServices.GetContractName(t);

            OwnedComposablePart part;
            if (_composableParts.TryGetValue(contract, out part))
            {
                if (owner != null && part.Owner != owner)
                    return;
                _composableParts.Remove(contract);
                var batch = new CompositionBatch();
                batch.RemovePart(part.Part);
                _compositionContainer.Compose(batch);
            }
        }

        private class OwnedComposablePart
        {
            public object Owner { get; set; }
            public ComposablePart Part { get; set; }
        }
    }
}