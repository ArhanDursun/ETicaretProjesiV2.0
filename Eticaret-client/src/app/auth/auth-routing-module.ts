import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { AuthLayout } from './components/auth-layout/auth-layout';
import { Register } from './pages/register/register';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

const routes: Routes = [
  {
    path: '',
    component: AuthLayout,
    children: [
      { path: 'login', component: Login },
      { path: 'register', component: Register },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AuthRoutingModule {}
