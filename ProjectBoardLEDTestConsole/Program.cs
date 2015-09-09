using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectBoardLEDs;

namespace ProjectBoardLEDTestConsole
{
    class Program
    {
        static Random random = new Random();

        static void Main(string[] args)
        {
            var dummyProjectMapping = SetUpDummyProjectMapping();

            using (var opc = new OPC("localhost", 7890))
            {
                var animator = new LEDStripAnimator(dummyProjectMapping, opc);
                opc.statusLedAuto();

                var lastChange = DateTime.Now.Ticks;
                while (true)
                {
                    animator.Animate();
                    opc.writePixels();
                    var now = DateTime.Now.Ticks;
                    if (now - lastChange > 10000000*5) // 5 seconds
                    {
                        IncrementBuildStatus(dummyProjectMapping);
                        lastChange = now;
                    }
                }
            }
            
        }
        
        private static IList<MappedProject> SetUpDummyProjectMapping()
        {
            var mappedProjects = new List<MappedProject>();

            var numTestProjects = 1;
            var stripLength = 13;
            var projectLength = stripLength / numTestProjects;

            for (var i = 0; i < numTestProjects; i++)
            {
                var mappedProject = new MappedProject(
                    new Project() {
                        Name = "Project " + i.ToString(),
                        Status = (ProjectStatus)(i % Enum.GetValues(typeof(ProjectStatus)).Length)
                    },
                    new BoardLocation() {
                        FirstLED = i * projectLength,
                        LastLED = (i + 1) * projectLength - 2
                    }
                );
                mappedProjects.Add(mappedProject);
            }

            return mappedProjects;
          }

        private static void MaybeChangeBuildStatus(IList<MappedProject> mappedProjects)
        {
            if (random.Next(1000000) <= 1)
            {
                var randomIndex = random.Next(mappedProjects.Count());
                mappedProjects[randomIndex].Project.Status = RandomStatus();
            }
        }

        private static ProjectStatus RandomStatus()
        {
            var values = Enum.GetValues(typeof(ProjectStatus));
            return (ProjectStatus)values.GetValue(random.Next(values.Length));
        }

        private static void IncrementBuildStatus(IList<MappedProject> mappedProjects)
        {
            foreach (var mappedProject in mappedProjects)
            {
                mappedProject.Project.Status = NextStatus(mappedProject.Project.Status);
            }
        }

        private static ProjectStatus NextStatus(ProjectStatus currentStatus)
        {
            return (ProjectStatus) ((((int) currentStatus) + 1) % Enum.GetValues(typeof (ProjectStatus)).Length);
        }
    }
}
