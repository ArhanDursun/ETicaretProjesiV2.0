using ETicaretProjesiV2._0.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateTokenAsync(AppUser user,AppRole role,bool rememberMe);
    }
}
