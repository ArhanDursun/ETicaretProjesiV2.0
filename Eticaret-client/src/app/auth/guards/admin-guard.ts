import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../services/auth';

export const adminGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(Auth);

  const token = localStorage.getItem('token') || sessionStorage.getItem('token');

  if (!token) {
    console.warn('Token Yok');
    router.navigate(['/auth/login']);
    return false;
  }

  const decodedToken = authService.getDecodedToken(token);
  if (!decodedToken) {
    router.navigate(['/auth/login']);
    return false;
  }
  const userRole =
    decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
    decodedToken['role'];
  if (userRole === 'Admin' || userRole === 'admin') {
    return true;
  } else {
    console.warn('Yetkisiz Kullanıcı');
    alert('Bu sayfayı görüntüleme izniniz yok');
    router.navigate(['/']);
    return false;
  }
};
