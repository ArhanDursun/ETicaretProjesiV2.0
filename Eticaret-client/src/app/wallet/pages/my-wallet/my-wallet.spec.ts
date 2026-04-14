import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyWallet } from './my-wallet';

describe('MyWallet', () => {
  let component: MyWallet;
  let fixture: ComponentFixture<MyWallet>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyWallet]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyWallet);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
