using DevEnv.Execute.Service.Execution.Dotnet;
using DevEnv.Execute.Service.Execution.Java;

namespace DevEnv.Execute.Service.Execution
{
    public class ExecutorProvider : IExecutorProvider
    {
        public IExecute GetExecutor(GrpcExecute.Environment env)
        {
            switch (env)
            {
                case GrpcExecute.Environment.DotnetExe:
                    return new DotnetExeExecutor();
                case GrpcExecute.Environment.JavaJar:
                    return new JavaJarExecutor();
                default:
                    throw new NotSupportedException($"The environment {env} is not supported.");
            }
        }
    }
}
