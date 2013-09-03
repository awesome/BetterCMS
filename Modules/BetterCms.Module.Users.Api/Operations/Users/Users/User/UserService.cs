﻿using System.Linq;

using BetterCms.Core.DataAccess;
using BetterCms.Core.DataAccess.DataContext;
using BetterCms.Module.Api.Operations.Users.Users.User;
using BetterCms.Module.Api.Operations.Users.Users.User.ValidateUser;

using ServiceStack.ServiceInterface;

namespace BetterCms.Module.Users.Api.Operations.Users.Users.User
{
    public class UserService : Service, IUserService
    {
        private readonly IRepository repository;
        
        private readonly IValidateUserService validateUserService;

        public UserService(IRepository repository, IValidateUserService validateUserService)
        {
            this.repository = repository;
            this.validateUserService = validateUserService;
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
                        ImageId = user.Image != null && !user.Image.IsDeleted ? user.Image.Id : (System.Guid?)null,
                        ImageCaption = user.Image != null && !user.Image.IsDeleted ? user.Image.Caption : null,
                        ImageThumbnailUrl = user.Image != null && !user.Image.IsDeleted ? user.Image.PublicThumbnailUrl : null,
                        ImageUrl = user.Image != null && !user.Image.IsDeleted ? user.Image.PublicUrl : null
                    })
                .FirstOne();

            var response = new GetUserResponse { Data = model };

            if (request.Data.IncludeRoles)
            {
                response.Roles = repository
                    .AsQueryable<Models.UserRole>(userRole => userRole.User.Id == model.Id && !userRole.Role.IsDeleted)
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

        IValidateUserService IUserService.Validate
        {
            get { return validateUserService; }
        }
    }
}