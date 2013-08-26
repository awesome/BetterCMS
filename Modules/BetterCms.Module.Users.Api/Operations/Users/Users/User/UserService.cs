﻿using System.Linq;

using BetterCms.Core.DataAccess;
using BetterCms.Core.DataAccess.DataContext;
using BetterCms.Module.Api.Operations.Users.Users.User;

using ServiceStack.ServiceInterface;

namespace BetterCms.Module.Users.Api.Operations.Users.Users.User
{
    public class UserService : Service, IUserService
    {
        private readonly IRepository repository;

        public UserService(IRepository repository)
        {
            this.repository = repository;
        }

        public GetUserResponse Get(GetUserRequest request)
        {
            var query = repository
                .AsQueryable<Models.User>();

            if (request.UserId.HasValue)
            {
                query = query.Where(user => user.Id == request.UserId);
            }
            else
            {
                query = query.Where(user => user.UserName == request.UserName);
            }
            
            var model = query
                .Select(user => new UserModel
                    {
                        Id = user.Id,
                        Version = user.Version,
                        CreatedBy = user.CreatedByUser,
                        CreatedOn = user.CreatedOn,
                        LastModifiedBy = user.ModifiedByUser,
                        LastModifiedOn = user.ModifiedOn,

                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        ImageId = user.Image != null ? user.Image.Id : (System.Guid?)null,
                        ImageCaption = user.Image != null ? user.Image.Caption : null,
                        ImageThumbnailUrl = user.Image != null ? user.Image.PublicThumbnailUrl : null,
                        ImageUrl = user.Image != null ? user.Image.PublicUrl : null
                    })
                .FirstOne();

            var response = new GetUserResponse { Data = model };

            if (request.Data.IncludeRoles)
            {
                response.Roles = repository
                    .AsQueryable<Models.UserRole>(role => role.User.Id == model.Id)
                    .OrderBy(role => role.Role.Name)
                    .Select(role => new RoleModel
                        {
                            Id = role.Role.Id,
                            Version = role.Role.Version,
                            CreatedBy = role.Role.CreatedByUser,
                            CreatedOn = role.Role.CreatedOn,
                            LastModifiedBy = role.Role.ModifiedByUser,
                            LastModifiedOn = role.Role.ModifiedOn,

                            Name = role.Role.DisplayName ?? role.Role.Name,
                            IsSystematic = role.Role.IsSystematic
                        })
                    .ToList();
            }

            return response;
        }
    }
}