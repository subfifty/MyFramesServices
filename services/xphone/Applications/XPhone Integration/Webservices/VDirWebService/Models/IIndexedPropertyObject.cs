using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C4B.VDir.WebService.Models
{
    interface IIndexedPropertyObject
    {
        string this[string key]
        {
            get;
        }
    }
}
