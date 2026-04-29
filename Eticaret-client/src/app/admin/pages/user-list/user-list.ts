import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user';
import { Signalr } from '../../../core/signalr/signalr';

@Component({
  selector: 'app-user-list',
  standalone: false,
  templateUrl: './user-list.html',
  styleUrl: './user-list.scss',
})
export class UserList implements OnInit {
  searchTerm: string = '';
  users: any[] = [];
  isLoading: boolean = true;
  filteredUsers: any[] = [];
  statusFilter: 'all' | 'online' | 'offline' = 'all';
  onlineIds: string[] = [];
  constructor(
    private userService: UserService,
    private cdr: ChangeDetectorRef,
    public signalr: Signalr,
  ) {}
  ngOnInit(): void {
    this.loadUser();
    this.signalr.onlineUsers$.subscribe((ids) => {
      this.onlineIds = ids;
      this.filterUsers();
      this.cdr.detectChanges();
    });
  }
  loadUser(): void {
    this.isLoading = true;
    this.userService.getAllUsers().subscribe({
      next: (response) => {
        this.users = response;
        this.filteredUsers = response;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Kullanıcılar çekilirken hata oluştu', err);
        alert('Kullanıcı Listesi Alınamadı');
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  deleteUser(id: string) {
    if (confirm('Bu kullanıcıyı silmek istediğinize emin misiniz?')) {
      this.userService.deleteUser(id).subscribe({
        next: (response) => {
          alert('Kullanıcı Başarıyla silindi!');
          this.loadUser();
        },
        error: (err) => {
          const gercekHata = err.error.Message || err.message;
          alert('Silme İşlemi Başarısız');
          console.error(err);
        },
      });
    }
  }
  filterUsers() {
    const term = this.searchTerm.toLowerCase().trim();

    this.filteredUsers = this.users.filter((user) => {
      const matchesSearch =
        user.firstName.toLowerCase().includes(term) ||
        user.lastName.toLowerCase().includes(term) ||
        user.userName.toLowerCase().includes(term) ||
        user.email.toLowerCase().includes(term);

      const isOnline = this.onlineIds.includes(user.id);
      let matchesStatus = true;

      if (this.statusFilter === 'online') matchesStatus = isOnline;
      if (this.statusFilter === 'offline') matchesStatus = !isOnline;

      return matchesSearch && matchesStatus;
    });
  }
  setStatusFilter(status: 'all' | 'online' | 'offline') {
    this.statusFilter = status;
    this.filterUsers();
  }
}
