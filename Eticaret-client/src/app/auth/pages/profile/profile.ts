import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, CUSTOM_ELEMENTS_SCHEMA, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Auth } from '../../services/auth';
import { Product } from '../../../product/services/product';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class Profile implements OnInit {
  profileData = {
    firstName: '',
    lastName: '',
    userName: '',
    email: '',
    balance: 0,
    profileImageUrl: '',
    description: '',
    phoneNumber: '',
  };

  passwordData = {
    currentPassword: '',
    newPassword: '',
    confirmedNewPassword: '',
  };

  sellerId: string | null = null;
  myProducts: any[] = [];
  showProfileModal: boolean = false;
  showPasswordModal: boolean = false;
  isLoading: boolean = false;
  isUpdatingProfile: boolean = false;
  isUpdatingPassword: boolean = false;
  isUploadingImage: boolean = false;
  isOwnProfile: boolean = true;

  constructor(
    private authService: Auth,
    private productService: Product,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.sellerId = params.get('id');
      const myId = this.getMyIdFromToken();

      if (this.sellerId && this.sellerId !== myId) {
        this.isOwnProfile = false;
        this.loadPublicProfile(this.sellerId);
        this.loadSellerShowcase(this.sellerId);
      } else {
        this.isOwnProfile = true;
        this.loadProfile();
        this.loadMyShowcase();
      }
    });
  }

  openProfileModal() {
    this.showProfileModal = true;
    this.cdr.detectChanges();
  }

  closeProfileModal() {
    this.showProfileModal = false;
    this.cdr.detectChanges();
  }

  openPasswordModal() {
    this.showPasswordModal = true;
    this.cdr.detectChanges();
  }

  closePasswordModal() {
    this.showPasswordModal = false;
    this.cdr.detectChanges();
  }

  loadPublicProfile(id: string) {
    this.isLoading = true;
    this.authService.getPublicProfile(id).subscribe({
      next: (res) => {
        this.profileData = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  loadProfile() {
    this.isLoading = true;
    this.authService.getMyProfile().subscribe({
      next: (res) => {
        this.profileData = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  updateProfile() {
    if (!this.profileData.firstName || !this.profileData.email) {
      alert(this.translate.instant('PROFILE.MESSAGES.REQUIRED_FIELDS'));
      return;
    }
    this.isUpdatingProfile = true;
    this.authService.updateMyProfile(this.profileData).subscribe({
      next: (res) => {
        alert(this.translate.instant('PROFILE.MESSAGES.UPDATE_SUCCESS'));
        this.isUpdatingProfile = false;
        this.showProfileModal = false;
        this.loadProfile();
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert(`${this.translate.instant('WALLET.TOPUP.MESSAGES.ERROR_PREFIX')} ${err.error?.message || ''}`);
        this.isUpdatingProfile = false;
        this.cdr.detectChanges();
      },
    });
  }

  changePassword() {
    if (this.passwordData.newPassword !== this.passwordData.confirmedNewPassword) {
      alert(this.translate.instant('PROFILE.MESSAGES.PASS_MISMATCH'));
      return;
    }
    this.isUpdatingPassword = true;
    this.authService.changeMyPassword(this.passwordData).subscribe({
      next: (res) => {
        alert(this.translate.instant('PROFILE.MESSAGES.PASS_SUCCESS'));
        this.isUpdatingPassword = false;
        this.showPasswordModal = false;
        this.passwordData = { currentPassword: '', newPassword: '', confirmedNewPassword: '' };
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert(`${this.translate.instant('WALLET.TOPUP.MESSAGES.ERROR_PREFIX')} ${err.error?.message || ''}`);
        this.isUpdatingPassword = false;
        this.cdr.detectChanges();
      },
    });
  }

  loadMyShowcase() {
    this.productService.getMyProduct().subscribe({
      next: (res) => {
        this.myProducts = res;
        this.cdr.detectChanges();
      },
      error: (err) => {},
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (!file.type.match(/image\/*/)) {
        alert(this.translate.instant('PROFILE.MESSAGES.ONLY_IMAGE'));
        return;
      }
      this.isUploadingImage = true;
      this.authService.uploadProfileImage(file).subscribe({
        next: (res: any) => {
          this.profileData.profileImageUrl = res.imageUrl;
          this.isUploadingImage = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          alert(this.translate.instant('PROFILE.MESSAGES.UPLOAD_ERROR'));
          this.isUploadingImage = false;
          this.cdr.detectChanges();
        },
      });
    }
  }

  onImageError(event: any) {
    event.target.src = '/user.png';
  }

  loadSellerShowcase(sellerId: string) {
    this.productService.getSellerProducts(sellerId).subscribe({
      next: (res) => {
        this.myProducts = res;
        this.cdr.detectChanges();
      },
      error: (err) => {},
    });
  }

  getMyIdFromToken(): string | null {
    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));
      return (
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        decoded['nameid'] ||
        null
      );
    } catch (error) {
      return null;
    }
  }

  formatPhone(value: string) {
    if (!value) {
      this.profileData.phoneNumber = '+90 ';
      return;
    }
    let digits = value.replace(/\D/g, '');
    if (digits.startsWith('0')) {
      digits = digits.substring(1);
    }
    if (!digits.startsWith('90')) {
      digits = '90' + digits;
    }
    if (digits.length > 12) {
      digits = digits.substring(0, 12);
    }
    let formatted = '+90';
    if (digits.length > 2) formatted += ' ' + digits.substring(2, 5);
    if (digits.length > 5) formatted += ' ' + digits.substring(5, 8);
    if (digits.length > 8) formatted += ' ' + digits.substring(8, 10);
    if (digits.length > 10) formatted += ' ' + digits.substring(10, 12);
    this.profileData.phoneNumber = formatted;
  }
}
