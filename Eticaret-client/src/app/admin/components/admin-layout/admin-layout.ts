import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-admin-layout',
  standalone: false,
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout implements OnInit {
  currentLang: string;

  constructor(
    private translate: TranslateService,
    private router: Router,
  ) {
    this.currentLang = this.translate.currentLang || 'tr';
  }
  ngOnInit(): void {
    this.translate.onLangChange.subscribe((event) => {
      this.currentLang = event.lang;
    });
  }

  public changeLanguage(lang: string) {
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
  }
  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/auth/login']);
  }
}
