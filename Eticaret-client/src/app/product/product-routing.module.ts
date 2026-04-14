import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductAdd } from './pages/product-add/product-add';
import { Offer } from './pages/offer/offer';


const routes: Routes = [
 
  { 
    path:'add',component:ProductAdd
  },
  { path: 'offers', component: Offer },
];

@NgModule({
  imports: [RouterModule.forChild(routes)], 
  exports: [RouterModule]
})
export class ProductRoutingModule { 
  
}