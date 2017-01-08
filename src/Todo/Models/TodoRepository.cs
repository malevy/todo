using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Optional;

namespace Todo.Models
{
    public class TodoRepository 
    {
        private readonly ConcurrentDictionary<int, Todo> _items = new ConcurrentDictionary<int, Todo>();
        private int _nextId;

        private int _getNextId() => Interlocked.Increment(ref _nextId);
        
        public IQueryable<Todo> Items => _items.Values.AsQueryable();
        public int Count => _items.Count;

        public Option<Todo> Get(int id)
        {
            return this._items.TryGetValue(id, out Todo t) ? t.Some() : Option.None<Todo>();
        }

        public Todo Add(string description, bool important = false)
        {
            var newItem = new Todo(_getNextId(), description, important);
            _items[newItem.Id] = newItem;
            return newItem;
        }

        public bool Remove(int id) => _items.TryRemove(id, out Todo t);

        public IList<PagingInfo> BuildPagingFor(int skip, int take)
        {
            var pages = new List<PagingInfo>();

            var offset = skip - take;
            if (offset >= 0) pages.Add(new PagingInfo("previous", offset, take));

            offset = skip + take;
            if (offset < this.Count) pages.Add(new PagingInfo("next", offset, take));

            return pages;
        }
    }

    public class PagingInfo
    {
        public PagingInfo(string direction, int offset, int pageSize)
        {
            Direction = direction;
            Offset = offset;
            PageSize = pageSize;
        }

        public string Direction { get; private set; }
        public int Offset { get; private set; }
        public int PageSize { get; private set; }
    }

    public static class TodoRepositoryFactory
    {
        /// <summary>
        /// Instantiate a repo and populate it with some tasks
        /// </summary>
        /// <returns></returns>
        public static TodoRepository Build(int initialTaskCount = 4)
        {
            var repo = new TodoRepository();
            Enumerable.Range(0, initialTaskCount).ToList().ForEach(i => repo.Add($"Task ({i})", i%2 == 2));
            return repo;
        }
    }
}