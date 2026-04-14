import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Walletservice } from '../../services/walletservice';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-my-wallet',
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './my-wallet.html',
  styleUrls: ['./my-wallet.scss'],
})
export class MyWallet implements OnInit {
  currentBalance: number = 0;
  transactions: any[] = [];
  isLoading: boolean = true;

  constructor(
    private walletService: Walletservice,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadWalletData();
  }

  loadWalletData() {
    this.isLoading = true;
    this.walletService.getBalance().subscribe({
      next: (res) => {
        this.currentBalance = res.currentBalance;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Hata' + err),
    });
    this.walletService.getTransactions().subscribe({
      next: (res) => {
        this.transactions = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Hata Dekontlar Getirilemedi' + err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }
}
