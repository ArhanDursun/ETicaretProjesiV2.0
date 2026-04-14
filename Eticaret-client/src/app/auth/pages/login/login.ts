import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { email } from '@angular/forms/signals';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';
import { fakeAsync } from '@angular/core/testing';
import { DirectMessageService } from '../../../direct-message/direct-message-service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  loginForm!: FormGroup;
  needsVerification: boolean = false;
  unconfirmedEmail: string = '';
  verificationCode: string = '';
  isResending: boolean = false;
  isLoading: boolean = false;
  isForgotPasswordMode: boolean = false;
  isResetCodeSend: boolean = false;
  resetEmail: string = '';
  resetCode: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  isSendingReset: boolean = false;
  isResetting: boolean = false;
  constructor(
    private fb: FormBuilder,
    private authService: Auth,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private directMessageService: DirectMessageService,
  ) {}
  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false],
    });
  }
  toggleForgotPasswordMode(state: boolean): void {
    this.isForgotPasswordMode = state;
    this.isResetCodeSend = false;
    this.needsVerification = false;
    this.resetEmail = '';
    this.resetCode = '';
    this.newPassword = '';
    this.confirmPassword = '';
    this.cdr.detectChanges();
  }
  onSendResetLink(): void {
    if (!this.resetEmail || !this.resetEmail.includes('@')) {
      alert('Lütfen geçerli bir e-posta giriniz');
      return;
    }
    this.isSendingReset = true;
    this.authService.forgotPassword(this.resetEmail).subscribe({
      next: (response) => {
        alert('Doğrulama kodu mailinize gönderildi');

        setTimeout(() => {
          this.isSendingReset = false;
          this.isResetCodeSend = true;
          this.cdr.detectChanges();
        }, 50);
      },
      error: (err: any) => {
        if (err.status === 200 || err.message.includes('parsing') || err.error?.text) {
          alert('Kod başarıyla gönderildi! (Metin uyarısı aşıldı)');

          setTimeout(() => {
            this.isSendingReset = false;
            this.isResetCodeSend = true;
            this.cdr.detectChanges();
          }, 50);
          return;
        }

        alert('Bir sorun oluştu: ' + (err.error?.Message || err.message));
        this.isSendingReset = false;
        this.cdr.detectChanges();
      },
    });
  }
  onResetPassword(): void {
    if (this.resetCode.length !== 6) {
      alert('Lütfen 6 haneli kodu eksiksiz girin.');
      return;
    }
    if (this.newPassword.length < 6) {
      alert('Yeni şifreniz en az 6 karakter olmalıdır.');
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      alert('Şifreler birbiriyle eşleşmiyor!');
      return;
    }

    this.isResetting = true;
    const resetDto = {
      email: this.resetEmail,
      token: this.resetCode,
      newPassword: this.newPassword,
      confirmNewPassword: this.confirmPassword,
    };
    this.authService.resetPassword(resetDto).subscribe({
      next: (response: any) => {
        alert('Harika! Şifreniz başarıyla değiştirildi. Şimdi giriş yapabilirsiniz.');
        this.isResetting = false;
        this.toggleForgotPasswordMode(false);
      },
      error: (err) => {
        alert('İşlem başarısız: ' + (err.error?.Message || 'Kod hatalı veya süresi dolmuş.'));
        this.isResetting = false;
        this.cdr.detectChanges();
      },
    });
  }
  onSubmit(): void {
    if (this.loginForm.valid) this.isLoading = true;
    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        var token = response.token;
        if (token) {
          const isRememberMe = this.loginForm.value.rememberMe;
          if (isRememberMe) {
            localStorage.setItem('token', token);
          } else {
            sessionStorage.setItem('token', token);
          }
          this.isLoading = false;

          this.directMessageService.createHubConnection(token);
          this.directMessageService.loadRecentChats();
          const decodedToken = this.authService.getDecodedToken(token);

          const userRole =
            decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
            decodedToken['role'];
          if (userRole === 'Admin' || userRole === 'admin') {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.authService.setLoginStatus(true);
            this.router.navigate(['/home']);
            this.cdr.detectChanges();
          }
        } else {
          alert('Giriş yapılıyor ama token alınamadı');
        }
      },
      error: (err) => {
        this.isLoading = false;
        const errorMsg = err.error?.Message || err.error?.message || err.error || err.message || '';
        const tamMetin = JSON.stringify(errorMsg);

        if (tamMetin.match(/doğrulama/i) || tamMetin.match(/email/i)) {
          this.unconfirmedEmail = this.loginForm.value.email;
          this.needsVerification = true;
          this.cdr.detectChanges();
        } else {
          alert('Giriş başarısız: ' + errorMsg);
        }
      },
    });
  }

  onResendCode(): void {
    this.isResending = true;
    this.authService.resendVerificationCode(this.unconfirmedEmail).subscribe({
      next: (response) => {
        alert('Yeni kod başarıyla gönderildi! Lütfen mailinizi kontrol edin.');
        this.isResending = false;
      },
      error: (err) => {
        alert('Kod gönderilemedi: ' + (err.error?.Message || 'Bir hata oluştu.'));
        this.isResending = false;
      },
    });
  }
  onVerifyCode(): void {
    if (!this.verificationCode || this.verificationCode.length !== 6) {
      alert('Lütfen 6 haneli kodu eksiksiz girin.');
      return;
    }

    const verifyDto = { email: this.unconfirmedEmail, code: this.verificationCode };

    this.authService.verifyRegister(verifyDto).subscribe({
      next: (res) => {
        alert('Harika! Hesabınız onaylandı. Şimdi giriş yapabilirsiniz.');
        this.needsVerification = false;
        this.loginForm.patchValue({ password: '' });
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert('Doğrulama başarısız: ' + (err.error?.Message || 'Hatalı kod.'));
      },
    });
  }
}
