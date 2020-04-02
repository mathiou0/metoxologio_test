using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace metoxologio_test.Data
{
    public class ApplicationIdentityUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
    }

    public class ApplicationRole : IdentityRole<int>
    {
    }

   
}
