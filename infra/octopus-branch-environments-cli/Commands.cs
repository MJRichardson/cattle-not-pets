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
            Console.WriteLine($"##[set-output name=environment;]{environment.Name}");

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
                    },
                    ReleaseRetentionPolicy = new RetentionPeriod(30, RetentionUnit.Days)
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
                        LifecycleId = lifecycle.Id
                        /* can't create rule without including each step/package
                        Rules = new List<ChannelVersionRuleResource>
                        {
                            new()
                            {
                                Tag = branch
                            }
                        }
                        */
                    };

                    repositoryForSpace.Channels.Create(channel);
                }
            }
        }
    }

    internal static void DestroyBranchEnvironment(IOctopusSpaceRepository repositoryForSpace,
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
        
        // Delete the channel first
        var channel = repositoryForSpace.Channels.FindByName(project, channelName);
        if (channel != null)
        {
            repositoryForSpace.Channels.Delete(channel);
            Console.WriteLine($"Channel {channelName} deleted");
        }
        else
        {
            Console.WriteLine($"Channel {channelName} was not found");
        }
        
        // Then the lifecycle
        var lifecycle = repositoryForSpace.Lifecycles.FindByName(lifecycleName);
        if (lifecycle != null)
        {
            repositoryForSpace.Lifecycles.Delete(lifecycle);
            Console.WriteLine($"Lifecycle {lifecycleName} deleted");
        }
        else
        {
            Console.WriteLine($"Lifecycle {lifecycleName} was not found");
        }

        // And finally delete the environment
        var environment = repositoryForSpace.Environments.FindByName(environmentName);
        if (environment != null)
        {
            repositoryForSpace.Environments.Delete(environment);
            Console.WriteLine($"Environment {environmentName} deleted");
        }
        else
        {
            Console.WriteLine($"Environment {environmentName} was not found");
        }
    }
}