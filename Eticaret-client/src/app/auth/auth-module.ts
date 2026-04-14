import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AuthRoutingModule } from './auth-routing-module';
import { AuthLayout } from './components/auth-layout/auth-layout';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [AuthLayout, Login, Register],
  imports: [CommonModule, AuthRoutingModule, ReactiveFormsModule, FormsModule, TranslateModule],
})
export class AuthModule {}
