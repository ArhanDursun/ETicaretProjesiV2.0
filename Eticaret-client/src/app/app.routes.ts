import { Routes } from '@angular/router';
import { adminGuard } from './auth/guards/admin-guard';
import { ProductAdd } from './product/pages/product-add/product-add';
import { Home } from './user/pages/home/home';
import { ProductDetail } from './product/pages/product-detail/product-detail';
import { Offer } from './product/pages/offer/offer';
import { Basket } from './basket/pages/basket/basket';
import { MyOrders } from './order/pages/my-orders/my-orders';
import { IncomingOrders } from './order/pages/incoming-orders/incoming-orders';
import { SellerOrders } from './order/pages/seller-orders/seller-orders';
import { MyWallet } from './wallet/pages/my-wallet/my-wallet';
import { Profile } from './auth/pages/profile/profile';
import { AdminCategory } from './admin/pages/admin-category/admin-category';
import { adminRedirectGuard } from './admin/admin-redirect-guard';
import { TicketChat } from './ticket-chat/ticket-chat/ticket-chat';
import { TicketList } from './ticket-chat/ticket-list/ticket-list';
import { Messages } from './direct-message/messages/messages';
import { NotificationComponent } from './notification/notification-component/notification-component';
import { ProductUpdate } from './product/pages/product-update/product-update';

export const routes: Routes = [
  { path: 'home', component: Home, canActivate: [adminRedirectGuard] },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'auth', loadChildren: () => import('./auth/auth-module').then((m) => m.AuthModule) },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadChildren: () => import('./admin/admin-module').then((m) => m.AdminModule),
  },
  {
    path: 'product',
    loadChildren: () => import('./product/product-module').then((m) => m.ProductModule),
  },
  { path: 'product/detay/:id', component: ProductDetail },
  { path: 'add-product', component: ProductAdd },
  { path: 'offers', component: Offer },
  { path: 'basket', component: Basket },
  { path: 'my-orders', component: MyOrders },
  { path: 'incoming-orders', component: IncomingOrders },
  { path: 'seller-orders', component: SellerOrders },
  { path: 'my-wallet', component: MyWallet },
  { path: 'profile', component: Profile },
  { path: 'seller-profile/:id', component: Profile },
  { path: 'support/chat/:id', component: TicketChat },
  { path: 'support/tickets', component: TicketList },
  { path: 'messages', component: Messages },
  { path: 'notifications', component: NotificationComponent },
  { path: 'product/update/:id', component: ProductUpdate },

  { path: '**', redirectTo: 'home' },
];
