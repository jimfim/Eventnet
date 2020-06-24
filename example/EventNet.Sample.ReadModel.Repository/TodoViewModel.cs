using System;
using System.Collections.Generic;

namespace EventNet.Sample.ReadModel.Repository
{
    public class TodoViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Dictionary<Guid,string> Tasks { get; set; } = new Dictionary<Guid, string>();
    }
}