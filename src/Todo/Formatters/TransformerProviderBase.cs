using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Optional;
using Todo.ViewModels.Out;

namespace Todo.Formatters
{
    public abstract class TransformerProviderBase<TRepresentation>
    {
        private readonly List<Tuple<Type, Func<object, TRepresentation>>> _transformers 
            = new List<Tuple<Type, Func<object, TRepresentation>>>();

        protected TransformerProviderBase()
        {
            this._transformers.Add(Tuple.Create<Type, Func<object, TRepresentation>>(typeof(TodoVM), TodoVmBridge));
            this._transformers.Add(Tuple.Create<Type, Func<object, TRepresentation>>(typeof(TodoCollectionVm), CollectionVmBridge));
        }

        public Option<TRepresentation, string> From(object vm)
        {
            var transformer = this._transformers
                .Where(t => t.Item1.IsInstanceOfType(vm))
                .Select(t => t.Item2)
                .FirstOrDefault()
                .SomeNotNull($"no Siren transformer for {vm.GetType()}");

            return transformer.Map(x => x(vm));
        }

        private TRepresentation TodoVmBridge(object vm)
        {
            var todo = vm as TodoVM;
            if (null == todo)
                throw new ArgumentException($"expected '{typeof(TodoVM)}' but received '{vm.GetType()}'", nameof(vm));

            return this.FromItem(todo, null);
        }

        private TRepresentation CollectionVmBridge(object vm)
        {
            var collection = vm as TodoCollectionVm;
            if (null == collection)
                throw new ArgumentException($"expected '{typeof(TodoCollectionVm)}' but received '{vm.GetType()}'", nameof(vm));

            return this.FromCollection(collection);
        }

        protected abstract TRepresentation FromItem(TodoVM vm, string[] additionalRels);
        protected abstract TRepresentation FromCollection(TodoCollectionVm vm);
    }
}