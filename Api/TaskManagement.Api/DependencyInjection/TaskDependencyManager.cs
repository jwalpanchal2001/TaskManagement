using Autofac;
using TaskManagement.Business.TaskDetail;
using TaskManagement.Business.Tasks;
using TaskManagement.Data.Repository;

namespace TaskManagement.Api.DependencyInjection;

public class TaskDependencyManager : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TaskManager>().As<ITaskManager>().InstancePerLifetimeScope();
        builder.RegisterType<TaskRepository>().As<ITaskRepository>().InstancePerLifetimeScope();
        builder.RegisterType<TaskDetailManager>().As<ITaskDetailManager>().InstancePerLifetimeScope();
        builder.RegisterType<TaskDetailRepository>().As<ITaskDetailRepository>().InstancePerLifetimeScope();

    }
}