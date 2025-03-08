using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Common
{
    public abstract class BaseApiController : ControllerBase
    {
        //property to access the currentuser from httpContext.Items
        protected User CurrentUser
        {
            get
            {
                if (HttpContext.Items["CurrentUser"] is User currentUser)
                {
                    return currentUser;
                }

                // Return null if the user is not authenticated
                return null;


            }
        }
    }
}