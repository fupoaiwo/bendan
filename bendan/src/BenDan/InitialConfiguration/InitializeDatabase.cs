using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenDan.InitialConfiguration
{
    public class InitializeDatabase
    {

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<DataContext>();
                db.Database.Migrate();
                //if (db.TopicNodes.Count() == 0)
                //{
                //    db.TopicNodes.AddRange(GetTopicNodes());
                //    db.SaveChanges();
                //}
            }

        }

        //IEnumerable<TopicNode> GetTopicNodes()
        //{
        //    return new List<TopicNode>()
        //    {
        //        new TopicNode() { Name=".NET Core", NodeName="", ParentId=0, Order=1, CreateOn=DateTime.Now, },
        //        new TopicNode() { Name=".NET Core", NodeName="netcore", ParentId=1, Order=1, CreateOn=DateTime.Now, },
        //        new TopicNode() { Name="ASP.NET Core", NodeName="aspnetcore", ParentId=1, Order=1, CreateOn=DateTime.Now, }
        //    };
        //}
    }
}
