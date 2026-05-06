import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';
import { email } from '@angular/forms/signals';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register implements OnInit {
  step: 1 | 2 = 1;
  isLoading: boolean = false;
  registerForm!: FormGroup;
  verifyForm!: FormGroup;

  userEmail: string = '';
  constructor(
    private fb: FormBuilder,
    private authService: Auth,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      phoneNumber: ['', [Validators.required]],
    });

    this.verifyForm = this.fb.group({
      code: ['', Validators.required],
    });
  }

  onSubmitRequest(): void {
    if (this.registerForm.valid) {
      if (this.registerForm.value.password !== this.registerForm.value.confirmPassword) {
        alert('şifreler uyuşmuyor ');
        return;
      }
      this.isLoading = true;

      this.authService.registerRequest(this.registerForm.value).subscribe({
        next: (response) => {
          this.userEmail = this.registerForm.value.email;
          this.step = 2;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          alert('Kaytı Başlatılamadı. Hata' + (err.error?.Message || err.message));
          this.isLoading = false;
          this.cdr.detectChanges();
        },
      });
    }
  }
  onVerifyCode(): void {
    if (this.verifyForm.valid) {
      const verifyDto = {
        email: this.userEmail,
        code: this.verifyForm.value.code,
      };
      this.authService.verifyRegister(verifyDto).subscribe({
        next: (response) => {
          alert('Harika! Hesabınız başarıyla onaylandı. Şimdi giriş yapabilirsiniz.');
          this.router.navigate(['/auth/login']);
        },
        error: (err) => {
          console.error('Doğrulama Hatası:', err);
          alert('Kod yanlış veya süresi dolmuş olabilir!');
        },
      });
    }
  }

  onPhoneInput(event: any) {
    let value = event.target.value;
    if (!value) {
      this.registerForm.get('phoneNumber')?.setValue('+90', { emailEvent: false });
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
    this.registerForm.get('phoneNumber')?.setValue(formatted, { emitEvent: false });
  }
}
