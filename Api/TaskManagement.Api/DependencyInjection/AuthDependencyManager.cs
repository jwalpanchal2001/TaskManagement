﻿using Autofac;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using TaskManagement.Business.Authentication;
using TaskManagement.Business.Mapping;
using TaskManagement.Business.TaskDetail;
using TaskManagement.Business.Tasks;
using TaskManagement.Business.User;
using TaskManagement.Data.Repository;
using TaskManagement.Data.Repository.Authentication;

namespace TaskManagement.Api.DependencyInjection;

public class AuthDependencyManager : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthManager>().As<IAuthManager>().InstancePerLifetimeScope();
        builder.RegisterType<AuthRepository>().As<IAuthRepository>().InstancePerLifetimeScope();
        builder.RegisterType<LoginManager>().As<ILoginManager>().InstancePerLifetimeScope();
      }
}

