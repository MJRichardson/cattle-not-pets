namespace OctopusLabs.BranchEnvironmentsCli;

public class Naming
{
    internal static string EnvironmentName(string branch, string projectName) => $"{projectName} - {branch}";
    internal static string LifecycleName(string branch, string projectName) => EnvironmentName(branch, projectName);
    internal static string ChannelName(string branch) => $"branch - {branch}";
}