using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Business.Mapping;
public class MapsterConfig
{
    public static void Configure()
    {
        // User mappings
        TypeAdapterConfig<TaskManagement.Entity.Model.User, UserDto>
            .NewConfig()
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.IsAdmin, src => src.IsAdmin);

        TypeAdapterConfig<CreateUserDto, TaskManagement.Entity.Model.User>
            .NewConfig()
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.IsAdmin, src => src.IsAdmin)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.PasswordHash); // You'll set this separately

        // Task mappings
        TypeAdapterConfig<TaskManagement.Entity.Model.Tasks, TaskDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.DueDate, src => src.DueDate)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.UserName, src => src.User != null ? src.User.FullName : null)
            .Map(dest => dest.TaskStatusId, src => src.TaskStatusId)
            .Map(dest => dest.CreatedById, src => src.CreatedById)
            .Map(dest => dest.CreatedByName, src => src.CreatedBy != null ? src.CreatedBy.FullName : null)
            .Map(dest => dest.TaskStatus, src => src.TaskStatus != null ? src.TaskStatus.Name : null);
            

        TypeAdapterConfig<CreateTaskDto, TaskManagement.Entity.Model.Tasks>
            .NewConfig()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.DueDate, src => src.DueDate)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.TaskStatusId, src => src.TaskStatusId)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.CreatedAt); // Will be set manually

        // Task state mappings
        TypeAdapterConfig<TaskState, TaskStateDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        TypeAdapterConfig<TaskManagement.Entity.Model.TaskDetail, TaskDetails>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.IsCompleted, src => src.IsCompleted)
            .Map(dest => dest.TaskId, src => src.TaskId);

        TypeAdapterConfig<CreateTaskDetailDto, TaskManagement.Entity.Model.TaskDetail>
            .NewConfig()
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.TaskId, src => src.TaskId)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.IsCompleted);
    }
}
