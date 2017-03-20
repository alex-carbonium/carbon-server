using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Carbon.Business.Services;
using Carbon.Services;
using Carbon.Services.IdentityServer;
using Carbon.Test.Common;
using Microsoft.AspNet.Identity;

namespace Carbon.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataService = new DataService();
            Console.WriteLine(dataService.Generate("FirstName,LastName,FullName", 1)[0][DataField.FirstName]);
            Console.ReadLine();
        }
    }
}
