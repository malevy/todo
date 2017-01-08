using System;

namespace Todo.Models
{
    public class Todo
    {
        public int Id { get; private set; }
        public string Description { get; private set; }
        public DateTime? CompletedOn { get; private set; }
        public bool Important { get; private set; }

        public Todo(int id, string description, bool important = false)
        {
            this.Id = id;
            this.Description = description;
            this.Important = important;
        }

        public void MarkComplete(DateTime dateCompleted) => this.CompletedOn = dateCompleted;
        public void MarkComplete() => this.MarkComplete(DateTime.Today);
        public bool IsComplete => CompletedOn.HasValue;

        public void Change(string description, bool important)
        {
            this.Description = description;
            this.Important = important;
        }
    }
}