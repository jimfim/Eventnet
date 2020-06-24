using System;

namespace EventNet.Sample.ReadModel.Repository
{
    public class TodoTasksViewModel
    {
        public Guid Id { get; set; }
        public string Task { get; set; }
        public bool Done { get; set; }
    }
}