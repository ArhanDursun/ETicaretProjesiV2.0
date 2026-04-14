import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayout } from './components/admin-layout/admin-layout';
import { Dashboard } from './pages/dashboard/dashboard';
import { UserList } from './pages/user-list/user-list';
import { AdminCategory } from './pages/admin-category/admin-category';
import { UserDetail } from './pages/user-detail/user-detail';
import { Products } from './pages/products/products';
import { AdminTicket } from './support/admin-ticket/admin-ticket';
import { TicketChat } from '../ticket-chat/ticket-chat/ticket-chat';

const routes: Routes = [
  {
    path: '',
    component: AdminLayout,
    children: [
      { path: 'dashboard', component: Dashboard },
      { path: 'user-detail/:id', component: UserDetail },
      { path: 'products', component: Products },
      { path: 'users', component: UserList },
      { path: 'categories', component: AdminCategory },
      { path: 'support-tickets', component: AdminTicket },
      { path: 'support/chat/:id', component: TicketChat },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
