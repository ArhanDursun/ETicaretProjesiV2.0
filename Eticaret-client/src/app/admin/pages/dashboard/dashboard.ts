import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { DashboardService, DashboardStats, RecentOrder } from '../../services/dashboard-service';
import { Chart, registerables } from 'chart.js';
Chart.register(...registerables);
@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  stats: DashboardStats = {
    totalPlatformVolume: 0,
    totalCommissionEarned: 0,
    activeSellers: 0,
    totalProducts: 0,
  };
  recentOrders: RecentOrder[] = [];
  isLoading: boolean = false;
  isModalOpen: boolean = false;
  public chart: any;
  public selectedTimeRange: string = '7days';

  constructor(
    private dashboardService: DashboardService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
    this.loadChartData();
  }

  loadDashboardData(): void {
    this.dashboardService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('istatistikler yüklenmedi:', err);
      },
    });

    this.dashboardService.getRecentOrders().subscribe({
      next: (res) => {
        this.recentOrders = res;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Siparişler yüklenirken hata oluştu', err);
        this.isLoading = false;
      },
    });
  }

  openModal() {
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  closeModal() {
    this.isModalOpen = false;
    this.cdr.detectChanges();
  }
  loadChartData() {
    this.dashboardService.getDailyComission(this.selectedTimeRange).subscribe({
      next: (data) => {
        const dates = data.map((item) => item.date);
        const commissions = data.map((item) => item.totalComission);

        if (this.chart) {
          this.chart.destroy();
        }

        this.createChart(dates, commissions);
      },
      error: (err) => {
        console.error('Grafik verisi alınamadı', err);
      },
    });
  }
  onFilterChange() {
    this.loadChartData();
  }

  createChart(labels: string[], dataPoints: number[]) {
    this.chart = new Chart('commissionChart', {
      type: 'line',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Komisyon Kazancı (₺)',
            data: dataPoints,
            backgroundColor: 'rgba(59, 130, 246, 0.2)',
            borderColor: '#3b82f6',
            borderWidth: 3,
            tension: 0.4,
            fill: true,
            pointBackgroundColor: '#ffffff',
            pointBorderColor: '#3b82f6',
            pointRadius: 5,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, grid: { color: '#f1f5f9' } },
          x: { grid: { display: false } },
        },
      },
    });
  }
}
