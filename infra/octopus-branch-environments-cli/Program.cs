using Octopus.Client;
using OctopusLabs.BranchEnvironmentsCli;

const string projectName = "cattle not pets";

var octopusURL = Environment.GetEnvironmentVariable("OCTOPUS_URL");
var octopusAPIKey = Environment.GetEnvironmentVariable("OCTOPUS_API_KEY");
var spaceName = Environment.GetEnvironmentVariable("OCTOPUS_SPACE");
var branch = Environment.GetEnvironmentVariable("GIT_BRANCH");

var endpoint = new OctopusServerEndpoint(octopusURL, octopusAPIKey);
var repository = new OctopusRepository(endpoint);
var client = new OctopusClient(endpoint);

var commandLineArgs = Environment.GetCommandLineArgs();
var command = commandLineArgs[1];

// Get space
var space = repository.Spaces.FindByName(spaceName);
var repositoryForSpace = client.ForSpace(space);

switch (command)
{
    case "create": Commands.CreateBranchEnvironment(repositoryForSpace, branch, projectName);
        break;
    case "destroy": Commands.DestroyBranchEnvironment(repositoryForSpace, branch, projectName);
        break;
    default: throw new Exception($"Unknown command: {command}");
}



