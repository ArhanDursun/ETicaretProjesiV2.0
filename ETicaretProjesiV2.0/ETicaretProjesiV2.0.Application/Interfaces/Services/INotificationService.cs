using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(UserNotificationDto dto);
    }
}
