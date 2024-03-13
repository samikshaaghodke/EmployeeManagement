using Haley.Models;
using System;


namespace OrgHierarchy.Models 
{
    public abstract class OrgBaseModel : ChangeNotifier, ICloneable 
    {
        public string Id { get; set; }

        public OrgBaseModel()
        {
            Id = Guid.NewGuid().ToString(); 
        }
        public abstract object Clone();
    }
}
