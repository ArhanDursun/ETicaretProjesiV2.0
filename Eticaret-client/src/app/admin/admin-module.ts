import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminRoutingModule } from './admin-routing-module';
import { AdminLayout } from './components/admin-layout/admin-layout';
import { Dashboard } from './pages/dashboard/dashboard';
import { UserList } from './pages/user-list/user-list';
import { AdminCategory } from './pages/admin-category/admin-category';
import { FormsModule } from '@angular/forms';
import { Products } from './pages/products/products';
import { UserDetail } from './pages/user-detail/user-detail';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [AdminLayout, Dashboard, AdminCategory, UserList, UserDetail],
  imports: [CommonModule, AdminRoutingModule, FormsModule, Products, TranslateModule.forChild()],
})
export class AdminModule {}
