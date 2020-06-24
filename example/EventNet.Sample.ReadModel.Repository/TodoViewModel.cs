using System;
using System.Collections.Generic;

namespace EventNet.Sample.ReadModel.Repository
{
    public class TodoViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<TodoTasksViewModel> Tasks { get; set; }
    }
}