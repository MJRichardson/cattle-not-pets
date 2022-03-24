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

// Get space
var space = repository.Spaces.FindByName(spaceName);
var repositoryForSpace = client.ForSpace(space);

switch (commandLineArgs.First().ToLower())
{
    case "create": Commands.CreateBranchEnvironment(repositoryForSpace, branch, projectName);
        break;
    default: throw new Exception($"Unknown command: {commandLineArgs.First()}");
}



