import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';
import { DirectMessageService } from '../../../direct-message/direct-message-service';
import { TranslateService } from '@ngx-translate/core';

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
  isPasswordVisible: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: Auth,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private directMessageService: DirectMessageService,
    private translate: TranslateService
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
      alert(this.translate.instant('LOGIN.MESSAGES.VALID_EMAIL_REQUIRED'));
      return;
    }
    this.isSendingReset = true;
    this.authService.forgotPassword(this.resetEmail).subscribe({
      next: () => {
        alert(this.translate.instant('FORGOT_PASSWORD.MESSAGES.CODE_SENT'));
        setTimeout(() => {
          this.isSendingReset = false;
          this.isResetCodeSend = true;
          this.cdr.detectChanges();
        }, 50);
      },
      error: (err: any) => {
        if (err.status === 200 || err.message?.includes('parsing') || err.error?.text) {
          alert(this.translate.instant('FORGOT_PASSWORD.MESSAGES.SUCCESS_GENERIC'));
          setTimeout(() => {
            this.isSendingReset = false;
            this.isResetCodeSend = true;
            this.cdr.detectChanges();
          }, 50);
          return;
        }
        alert(`${this.translate.instant('FORGOT_PASSWORD.MESSAGES.ERROR_PREFIX')}: ${err.error?.Message || err.message}`);
        this.isSendingReset = false;
        this.cdr.detectChanges();
      },
    });
  }

  onResetPassword(): void {
    if (this.resetCode.length !== 6) {
      alert(this.translate.instant('FORGOT_PASSWORD.MESSAGES.INVALID_CODE'));
      return;
    }
    if (this.newPassword.length < 6) {
      alert(this.translate.instant('FORGOT_PASSWORD.MESSAGES.PASSWORD_LENGTH'));
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      alert(this.translate.instant('FORGOT_PASSWORD.MESSAGES.PASSWORD_MISMATCH'));
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
      next: () => {
        alert(this.translate.instant('FORGOT_PASSWORD.MESSAGES.RESET_SUCCESS'));
        this.isResetting = false;
        this.toggleForgotPasswordMode(false);
      },
      error: (err) => {
        alert(`${this.translate.instant('FORGOT_PASSWORD.MESSAGES.RESET_FAILED')}: ${err.error?.Message || ''}`);
        this.isResetting = false;
        this.cdr.detectChanges();
      },
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) this.isLoading = true;
    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        const token = response.token;
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
          const userRole = decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decodedToken['role'];
          if (userRole === 'Admin' || userRole === 'admin') {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.authService.setLoginStatus(true);
            this.router.navigate(['/home']);
            this.cdr.detectChanges();
          }
        } else {
          alert(this.translate.instant('LOGIN.MESSAGES.TOKEN_ERROR'));
        }
      },
      error: (err) => {
        const errorMsg = err.error?.Message || err.error?.message || err.error || err.message || '';
        const fullText = JSON.stringify(errorMsg);
        if (fullText.match(/doğrulama/i) || fullText.match(/email/i)) {
          this.unconfirmedEmail = this.loginForm.value.email;
          this.needsVerification = true;
          this.isLoading = false;
          this.cdr.detectChanges();
        } else {
          alert(`${this.translate.instant('LOGIN.MESSAGES.LOGIN_FAILED')}: ${errorMsg}`);
          this.isLoading = false;
          this.cdr.detectChanges();
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onResendCode(): void {
    this.isResending = true;
    this.authService.resendVerificationCode(this.unconfirmedEmail).subscribe({
      next: () => {
        alert(this.translate.instant('LOGIN.MESSAGES.NEW_CODE_SENT'));
        this.isResending = false;
      },
      error: (err) => {
        alert(`${this.translate.instant('LOGIN.MESSAGES.CODE_SEND_ERROR')}: ${err.error?.Message || ''}`);
        this.isResending = false;
      },
    });
  }

  onVerifyCode(): void {
    if (!this.verificationCode || this.verificationCode.length !== 6) {
      alert(this.translate.instant('FORGOT_PASSWORD.MESSAGES.INVALID_CODE'));
      return;
    }
    const verifyDto = { email: this.unconfirmedEmail, code: this.verificationCode };
    this.authService.verifyRegister(verifyDto).subscribe({
      next: () => {
        alert(this.translate.instant('LOGIN.MESSAGES.ACCOUNT_CONFIRMED'));
        this.needsVerification = false;
        this.loginForm.patchValue({ password: '' });
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert(`${this.translate.instant('LOGIN.MESSAGES.VERIFICATION_FAILED')}: ${err.error?.Message || ''}`);
      },
    });
  }
}
