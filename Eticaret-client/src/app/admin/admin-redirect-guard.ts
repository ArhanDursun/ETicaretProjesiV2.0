import { inject } from '@angular/core';
import { Router } from '@angular/router';

export const adminRedirectGuard = () => {
  const router = inject(Router);
  const token = localStorage.getItem('token') || sessionStorage.getItem('token');

  if (token) {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        window
          .atob(base64)
          .split('')
          .map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
          })
          .join(''),
      );
      const decodedPayload = JSON.parse(jsonPayload);

      const role =
        decodedPayload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        decodedPayload.role ||
        decodedPayload.Role;

      if (role === 'admin' || role === 'Admin') {
        router.navigate(['/admin']);
        return false;
      }
    } catch (error) {
      console.error('JWT decode edilirken hata oluştu:', error);
    }
  }

  return true;
};
