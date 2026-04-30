import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, Signal } from '@angular/core';
import { NavigationEnd, Router, RouterModule, RouterOutlet } from '@angular/router';
import { Auth } from './auth/services/auth';
import { filter, Subject, debounceTime, distinctUntilChanged, switchMap, of, count } from 'rxjs';
import { OfferService } from './product/services/offer';
import { BasketService } from './basket/services/basket';
import { NotificationService, NotificationDto } from './services/notification';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DirectMessageService } from './direct-message/direct-message-service';
import { FactoryTarget } from '@angular/compiler';
import { NotificationSignalR } from './core/signalr/notification';
import { Signalr } from './core/signalr/signalr';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule, CommonModule, TranslateModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  title = 'E-Ticaret';

  trendMessage: string = '';
  showToast: boolean = false;
  private timeoutId: any;

  isMenuOpen: boolean = false;
  isLoggedIn: boolean = false;
  showNavbar: boolean = true;
  pendingOfferCount: number = 0;
  cartItemCount: number = 0;

  searchSubject = new Subject<string>();
  searchResults: any[] = [];
  showDropdown: boolean = false;
  isSearching: boolean = false;

  unreadNotifications: NotificationDto[] = [];
  showModal: boolean = false;

  isMessageDrawerOpen: boolean = false;
  recentChats: any[] = [];
  unreadMessageTotal: number = 0;

  currentAlertProductId: string = '';
  constructor(
    private authService: Auth,
    private router: Router,
    private offerService: OfferService,
    private cdr: ChangeDetectorRef,
    private basketService: BasketService,
    private notificationService: NotificationService,
    public translate: TranslateService,
    private directMessageService: DirectMessageService,
    private notificationSignalR: NotificationSignalR,
    private signalRService: Signalr,
  ) {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        if (
          event.urlAfterRedirects.includes('/login') ||
          event.urlAfterRedirects.includes('register') ||
          event.urlAfterRedirects.includes('/admin')
        ) {
          this.showNavbar = false;
        } else {
          this.showNavbar = true;
        }
      });

    this.translate.addLangs(['tr', 'en']);
    this.translate.setDefaultLang('tr');
    this.translate.use('tr');
  }

  ngOnInit(): void {
    this.authService.isLoggedIn$.subscribe((status) => {
      this.isLoggedIn = status;

      if (status) {
        const token = localStorage.getItem('token') || sessionStorage.getItem('token');
        if (token) {
          this.directMessageService.createHubConnection(token);
        }

        this.offerService.updatePendingOfferCount();
        this.basketService.updateCartCount();
        this.directMessageService.loadRecentChats();
        this.checkNotifications();
      } else {
        if (this.directMessageService) {
          this.directMessageService.stopHubConnection();
        }
      }
    });

    this.basketService.cartItemCount$.subscribe((count) => {
      this.cartItemCount = count;
      this.cdr.detectChanges();
    });

    this.offerService.pendingOfferCount$.subscribe((count) => {
      this.pendingOfferCount = count;
      this.cdr.detectChanges();
    });

    this.offerService.updatePendingOfferCount();

    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((term) => {
          if (!term.trim()) {
            this.showDropdown = false;
            return of([]);
          }
          this.isSearching = true;
          this.showDropdown = true;
          return this.authService.searchUsers(term);
        }),
      )
      .subscribe({
        next: (res) => {
          this.searchResults = res;
          this.isSearching = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.isSearching = false;
          this.cdr.detectChanges();
        },
      });

    this.directMessageService.recentChats$.subscribe((chats) => {
      setTimeout(() => {
        this.recentChats = chats;
        this.cdr.detectChanges();
      }, 0);
    });

    this.directMessageService.unreadTotal$.subscribe((count) => {
      setTimeout(() => {
        this.unreadMessageTotal = count;
        this.cdr.detectChanges();
      }, 0);
    });

    this.notificationSignalR.startConnection();

    this.notificationSignalR.trendUpdate$.subscribe((message: string) => {
      setTimeout(() => {
        this.trendMessage = message;
        this.showToast = true;

        if (this.timeoutId) {
          clearTimeout(this.timeoutId);
        }
        this.timeoutId = setTimeout(() => {
          this.showToast = false;
        }, 4000);
      }, 0);
    });
    this.signalRService.priceAlert$.subscribe((data: any) => {
      setTimeout(() => {
        this.trendMessage = data.message;
        this.currentAlertProductId = data.productId;
        this.showToast = true;

        if (this.timeoutId) {
          clearTimeout(this.timeoutId);
        }

        this.timeoutId = setTimeout(() => {
          this.showToast = false;
          this.cdr.detectChanges();
        }, 5000);

        this.cdr.detectChanges();
      }, 0);
    });
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  onLogout() {
    this.authService.logout();

    this.isMenuOpen = false;
    this.directMessageService.stopHubConnection();

    const logoutMessage = this.translate.instant('MESSAGES.LOGOUT_SUCCESS');
    alert(logoutMessage);

    this.router.navigate(['/auth/login']);
  }

  onSearchInput(event: any) {
    this.searchSubject.next(event.target.value);
  }

  goToProfile(userId: string) {
    this.showDropdown = false;
    this.router.navigate(['/seller-profile', userId]);
  }

  closeSearch() {
    setTimeout(() => {
      this.showDropdown = false;
      this.cdr.detectChanges();
    }, 200);
  }

  checkNotifications() {
    this.notificationService.getUnreadNotifications().subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.unreadNotifications = data;
          this.showModal = true;
          this.cdr.detectChanges();
        }
      },
      error: (err) => console.error('Bildirimler alınırken hata oluştu:', err),
    });
  }

  closeNotification(id: string) {
    this.notificationService.markAsRead(id).subscribe({
      next: () => {
        this.unreadNotifications = this.unreadNotifications.filter((n) => n.id !== id);

        if (this.unreadNotifications.length === 0) {
          this.showModal = false;
        }
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Bildirim kapatılamadı:', err),
    });
  }

  goToProductFromToast() {
    if (this.currentAlertProductId) {
      this.showToast = false;
      this.router.navigate(['/product-detail', this.currentAlertProductId]);
    }
  }
}
