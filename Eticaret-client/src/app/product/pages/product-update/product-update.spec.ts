import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductUpdate } from './product-update';

describe('ProductUpdate', () => {
  let component: ProductUpdate;
  let fixture: ComponentFixture<ProductUpdate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductUpdate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductUpdate);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
