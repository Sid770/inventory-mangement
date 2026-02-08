import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { DashboardStats } from '../../models/inventory.model';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private readonly inventoryService = inject(InventoryService);
  
  protected readonly stats = signal<DashboardStats | null>(null);
  protected readonly alerts = signal<any>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly hasAlerts = computed(() => {
    const alertData = this.alerts();
    return alertData && alertData.totalAlerts > 0;
  });

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading.set(true);
    this.error.set(null);

    this.inventoryService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load dashboard data');
        this.loading.set(false);
        console.error('Dashboard error:', err);
      }
    });

    this.inventoryService.getAlerts().subscribe({
      next: (data) => {
        this.alerts.set(data);
      },
      error: (err) => {
        console.error('Alerts error:', err);
      }
    });
  }

  getAlertClass(level: string): string {
    switch (level) {
      case 'critical': return 'alert-critical';
      case 'high': return 'alert-high';
      case 'medium': return 'alert-medium';
      default: return '';
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }
}
