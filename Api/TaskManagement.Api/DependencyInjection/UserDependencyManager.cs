using Autofac;
using TaskManagement.Business.TaskDetail;
using TaskManagement.Business.Tasks;
using TaskManagement.Business.User;
using TaskManagement.Data.Repository;

namespace TaskManagement.Api.DependencyInjection;

public class UserDependencyManager : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UserManager>().As<IUserManager>().InstancePerLifetimeScope();
        builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
       
    }
}
