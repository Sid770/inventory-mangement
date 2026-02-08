import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InventoryService } from '../../services/inventory.service';
import { InventoryItem } from '../../models/inventory.model';

@Component({
  selector: 'app-inventory-list',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './inventory-list.component.html',
  styleUrl: './inventory-list.component.css'
})
export class InventoryListComponent implements OnInit {
  private readonly inventoryService = inject(InventoryService);
  
  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly categories = signal<string[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  
  protected readonly searchTerm = signal('');
  protected readonly selectedCategory = signal('');
  protected readonly showLowStockOnly = signal(false);

  protected readonly filteredItems = computed(() => {
    return this.items();
  });

  ngOnInit(): void {
    this.loadCategories();
    this.loadItems();
  }

  loadCategories(): void {
    this.inventoryService.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: (err) => console.error('Error loading categories:', err)
    });
  }

  loadItems(): void {
    this.loading.set(true);
    this.error.set(null);

    this.inventoryService.getAllItems(
      this.selectedCategory() || undefined,
      this.showLowStockOnly() || undefined,
      this.searchTerm() || undefined
    ).subscribe({
      next: (data) => {
        this.items.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load inventory items');
        this.loading.set(false);
        console.error('Error loading items:', err);
      }
    });
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.loadItems();
  }

  onCategoryChange(value: string): void {
    this.selectedCategory.set(value);
    this.loadItems();
  }

  onLowStockToggle(checked: boolean): void {
    this.showLowStockOnly.set(checked);
    this.loadItems();
  }

  deleteItem(id: number, name: string): void {
    if (confirm(`Are you sure you want to delete "${name}"?`)) {
      this.inventoryService.deleteItem(id).subscribe({
        next: () => {
          this.loadItems();
        },
        error: (err) => {
          alert('Failed to delete item');
          console.error('Delete error:', err);
        }
      });
    }
  }

  getStockStatusClass(item: InventoryItem): string {
    if (item.quantity === 0) return 'out-of-stock';
    if (item.isLowStock) return 'low-stock';
    return 'in-stock';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  }
}
