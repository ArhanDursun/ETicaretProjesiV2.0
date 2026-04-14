import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductAdd } from './pages/product-add/product-add';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ProductRoutingModule } from './product-routing.module';

import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [ProductAdd],
  imports: [CommonModule, ReactiveFormsModule, ProductRoutingModule, FormsModule, TranslateModule],
})
export class ProductModule {}
