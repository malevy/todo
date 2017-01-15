using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Todo.ViewModels.Out
{
    public interface IHasLinks
    {
        List<LinkVM> Links { get; }
    }

    public interface IHasActions
    {
        List<ActionVM> Actions { get; }
    }

    public class LinkVM
    {
        public LinkVM(string rel, Uri href, string title=null)
        {
            if (string.IsNullOrWhiteSpace(rel))
                throw new ArgumentException("links must have at least one rel attribute", nameof(rel));
            if (null == href)
                throw new ArgumentException("links must have uri", nameof(href));

            this.Rel.Add(rel);
            Href = href;
            Title = title;
        }

        public LinkVM(IEnumerable<string> rels,  Uri href, string title)
        {
            if (null == rels)
                throw new ArgumentException("links must have at least one rel attribute", nameof(rels));
            if (Rel.All(string.IsNullOrWhiteSpace))
                throw new ArgumentException("links must have at least one rel attribute", nameof(rels));
            if (null == href)
                throw new ArgumentException("links must have uri", nameof(href));

            this.Rel.AddRange(rels.Where(r => !string.IsNullOrWhiteSpace(r)));

            Href = href;
            Title = title;
        }

        public List<string> Rel { get; private set; } = new List<string>();

        public Uri Href { get; set; }

        public string Title { get; set; }
    }

    public enum FieldType
    {
        Hidden=0, Text, Search, Tel, Url, Email, Password, @Datetime,
        Date, Month, Week, Time, Datetimelocal, Number, Range, Color, Checkbox, Radio, File
    }

    public static class FieldTypeExtensions
    {
        public static string AsString(this FieldType @this)
        {
            var s = @this.ToString();
            if (string.Compare(s, "Datetimelocal", true) == 0) s = "datetime-local";
            return s.ToLower();
        }
    }

    public class FieldVM
    {
        public FieldVM(string name, FieldType type)
        {
            Name = name;
            Type = type;
            Title = name;
        }

        public string Name { get; set; }
        public string Title { get; set; }
        public FieldType Type { get; set; }
        public string Value { get; set; }
    }

    public class ActionVM
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Method { get; set; }
        public Uri Href { get; set; }
        public string Accepts { get; set; }
        public List<FieldVM> Fields { get; private set; } = new List<FieldVM>();
    }

    public class TodoVM : IHasActions, IHasLinks
    {
        public List<ActionVM> Actions { get; private set; } = new List<ActionVM>();
        public List<LinkVM> Links { get; private set; } = new List<LinkVM>();

        public string Description { get; set; }
        public DateTime? CompletedOn { get; set; }
        public bool Important { get; set; }

        public static TodoVM From(Models.Todo todo)
        {
            return new TodoVM()
            {
                CompletedOn = todo.CompletedOn,
                Description = todo.Description,
                Important = todo.Important
            };
        }
    }

    public class TodoCollectionVm : IHasActions, IHasLinks
    {
        public TodoCollectionVm()
        {
            this.Items = new Collection<TodoVM>();
        }

        public TodoCollectionVm(IEnumerable<TodoVM> items )
        {
            this.Items = new Collection<TodoVM>(items.ToList());
        }

        public Collection<TodoVM> Items { get; private set; }
        public List<ActionVM> Actions { get; private set; } = new List<ActionVM>();
        public List<LinkVM> Links { get; private set; } = new List<LinkVM>();
    }
}