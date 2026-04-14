import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user';

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
  constructor(
    private userService: UserService,
    private cdr: ChangeDetectorRef,
  ) {}
  ngOnInit(): void {
    this.loadUser();
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
    if (!this.searchTerm) {
      this.filteredUsers = this.users;
    } else {
      const term = this.searchTerm.toLowerCase();

      this.filteredUsers = this.users.filter(
        (user) =>
          user.firstName.toLowerCase().includes(term) ||
          user.lastName.toLowerCase().includes(term) ||
          user.userName.toLowerCase().includes(term) ||
          user.email.toLowerCase().includes(term),
      );
    }
  }
}
