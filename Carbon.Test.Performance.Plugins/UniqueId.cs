using System;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace Carbon.Test.Performance.Plugins
{
    public class UniqueId : WebTestPlugin
    {
        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            base.PreWebTest(sender, e);

            e.WebTest.Context.Add("UniqueId", Guid.NewGuid());

            for (var i = 1; i < 10; ++i)
            {
                e.WebTest.Context.Add("UniqueId" + i, Guid.NewGuid());
            }
        }    
    }
}