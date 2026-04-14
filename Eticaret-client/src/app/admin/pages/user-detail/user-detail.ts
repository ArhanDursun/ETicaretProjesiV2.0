import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Admin, AdminProduct, UserDetailDto } from '../../services/admin';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-user-detail',
  standalone: false,
  templateUrl: './user-detail.html',
  styleUrl: './user-detail.scss',
})
export class UserDetail implements OnInit {
  userId: string | null = null;
  userInfo: UserDetailDto | null = null;
  userProducts: AdminProduct[] = [];
  isLoading: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private adminService: Admin,
    private cdr: ChangeDetectorRef,
  ) {}
  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.userId = params.get('id');
      if (this.userId) {
        this.loadUserData(this.userId);
      }
    });
  }

  loadUserData(id: string) {
    this.isLoading = true;
    this.adminService.getUserInfo(id).subscribe({
      next: (data) => {
        this.userInfo = data;
        this.cdr.detectChanges();
        this.adminService.getUserProducts(id).subscribe({
          next: (products) => {
            this.userProducts = products;
            this.isLoading = false;
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('Kullanıcı ürünleri çekilemedi:', err);
            this.isLoading = false;
          },
        });
      },
      error: (err) => {
        console.error('Kullanıcı bilgileri çekilemedi:', err);
        this.isLoading = false;
      },
    });
  }
}
