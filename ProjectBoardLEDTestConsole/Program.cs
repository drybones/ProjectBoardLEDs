using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

using ProjectBoardLEDs;

namespace ProjectBoardLEDTestConsole
{
    class Program
    {
        static Random random = new Random();

        static void Main(string[] args)
        {
            var dummyProjectMapping = SetUpDummyProjectMapping();
            var animator = new LEDStripAnimator(dummyProjectMapping);

            using (var ws = new WebSocket("ws://localhost:7890/"))
            {
                ws.OnMessage += (sender, e) =>
                  Console.WriteLine(e.Data);

                ws.Connect();
                ws.Send(@"{ ""type"": ""server_info"" }");

                while (true)
                {
                    animator.Animate();
                    ws.Send(animator.Data);
                    //MaybeChangeBuildStatus(dummyProjectMapping);
                }
            }
        }
        
        private static IList<MappedProject> SetUpDummyProjectMapping()
        {
            var mappedProjects = new List<MappedProject>();

            var numTestProjects = 8;
            var stripLength = 64;
            var projectLength = stripLength / numTestProjects;

            for (var i = 0; i < numTestProjects; i++)
            {
                var mappedProject = new MappedProject() { 
                    Name = "Project " + i.ToString(), 
                    Status = (ProjectStatus)(i % Enum.GetValues(typeof(ProjectStatus)).Length), 
                    FirstLED = i*projectLength, 
                    LastLED = (i+1)*projectLength - 2 
                };
                mappedProjects.Add(mappedProject);
            }

            return mappedProjects;
        }

        private static void MaybeChangeBuildStatus(IList<MappedProject> mappedProjects)
        {
            if (random.Next(100000) <= 1)
            {
                var randomIndex = random.Next(mappedProjects.Count());
                mappedProjects[randomIndex].Status = RandomStatus();
            }
        }

        private static ProjectStatus RandomStatus()
        {
            var values = Enum.GetValues(typeof(ProjectStatus));
            return (ProjectStatus)values.GetValue(random.Next(values.Length));
        }
    }
}
