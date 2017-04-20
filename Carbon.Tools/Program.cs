using System;
using Carbon.Business.Services;

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
