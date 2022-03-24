using Octopus.Client;
using Octopus.Client.Model;

namespace OctopusLabs.BranchEnvironmentsCli;

public class Commands
{
    internal static void CreateBranchEnvironment(IOctopusSpaceRepository repositoryForSpace,
        string branch, string projectName)
    {
        var environmentName = Naming.EnvironmentName(branch, projectName);
        var lifecycleName = Naming.LifecycleName(branch, projectName);
        var channelName = Naming.ChannelName(branch);


        // Get project
        Console.WriteLine($"Finding project {projectName}");
        var project = repositoryForSpace.Projects.FindByName(projectName);
        if (project == null)
        {
            throw new Exception($"Could not find project {projectName}");
        }

        // Check for existing environment
        var environment = repositoryForSpace.Environments.FindByName(environmentName);
        if (environment != null)
        {
            Console.WriteLine("Environment '{0}' already exists. Nothing to create :)", environmentName);
        }
        else
        {
            Console.WriteLine("Creating environment '{0}'", environmentName);
            var environmentResource = new EnvironmentResource { Name = environmentName };
            environment = repositoryForSpace.Environments.Create(environmentResource);
            Console.WriteLine("EnvironmentId: {0}", environment.Id);

            // Create lifecycle
            Console.WriteLine($"Creating lifecycle {lifecycleName}");
            var lifecycle = repositoryForSpace.Lifecycles.FindByName(lifecycleName);
            if (lifecycle != null)
            {
                Console.WriteLine($"Lifecycle {lifecycle.Name} already exists");
            }
            else
            {
                lifecycle = new LifecycleResource
                {
                    Name = lifecycleName,
                    Phases =
                    {
                        new PhaseResource
                        {
                            Name = environmentName,
                            AutomaticDeploymentTargets = new ReferenceCollection { environment.Id }
                        }
                    }
                };

                lifecycle = repositoryForSpace.Lifecycles.Create(lifecycle);

                // Create channel using lifecycle
                Console.WriteLine($"Creating channel {channelName}");
                var channel = repositoryForSpace.Channels.FindByName(project, channelName);

                if (channel != null)
                {
                    Console.WriteLine("Channel already exists");
                }
                else
                {
                    channel = new ChannelResource
                    {
                        Name = channelName,
                        ProjectId = project.Id,
                        LifecycleId = lifecycle.Id,
                        Rules = new List<ChannelVersionRuleResource>
                        {
                            new()
                            {
                                Tag = branch
                            }
                        }
                    };

                    repositoryForSpace.Channels.Create(channel);
                }

            }
        }
    }
}