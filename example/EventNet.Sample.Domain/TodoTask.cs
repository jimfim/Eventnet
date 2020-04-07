using System;

namespace EventNet.Sample.Domain
{
    public class TodoTask
    {
        public Guid Id { get; }

        public bool Complete { get; }

        public string Description { get; }

        public TodoTask(Guid id, string description)
        {
            Id = id;
            Description = description;
            Complete = false;
        }
    }
}