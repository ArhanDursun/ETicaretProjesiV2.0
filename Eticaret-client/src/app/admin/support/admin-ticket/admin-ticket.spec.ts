import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminTicket } from './admin-ticket';

describe('AdminTicket', () => {
  let component: AdminTicket;
  let fixture: ComponentFixture<AdminTicket>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminTicket]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminTicket);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
