import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { InventoryService } from '../../services/inventory.service';
import { InventoryItem } from '../../models/inventory.model';

@Component({
  selector: 'app-stock-transaction',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './stock-transaction.component.html',
  styleUrl: './stock-transaction.component.css'
})
export class StockTransactionComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly inventoryService = inject(InventoryService);
  
  protected readonly item = signal<InventoryItem | null>(null);
  protected readonly loading = signal(false);
  protected readonly processing = signal(false);
  protected readonly alert = signal<any>(null);
  
  protected readonly form: FormGroup = this.fb.group({
    type: ['StockIn', Validators.required],
    quantity: [1, [Validators.required, Validators.min(1)]],
    notes: [''],
    performedBy: ['Admin', [Validators.required]]
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadItem(+id);
    }
  }

  loadItem(id: number): void {
    this.loading.set(true);
    this.inventoryService.getItemById(id).subscribe({
      next: (data) => {
        this.item.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading item:', err);
        alert('Failed to load item');
        this.router.navigate(['/inventory']);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid || !this.item()) {
      this.form.markAllAsTouched();
      return;
    }

    this.processing.set(true);
    this.alert.set(null);

    const transaction = {
      inventoryItemId: this.item()!.id,
      type: this.form.value.type,
      quantity: +this.form.value.quantity,
      notes: this.form.value.notes,
      performedBy: this.form.value.performedBy
    };

    this.inventoryService.processTransaction(transaction).subscribe({
      next: (response) => {
        this.processing.set(false);
        this.alert.set(response.alert);
        
        // Update item with new values
        this.item.set(response.item);
        
        // Reset form
        this.form.patchValue({
          quantity: 1,
          notes: ''
        });

        alert('Transaction completed successfully!');
      },
      error: (err) => {
        this.processing.set(false);
        const errorMsg = err.error?.message || 'Failed to process transaction';
        alert(errorMsg);
        console.error('Transaction error:', err);
      }
    });
  }

  getStockStatusClass(): string {
    const currentItem = this.item();
    if (!currentItem) return '';
    
    if (currentItem.quantity === 0) return 'status-critical';
    if (currentItem.isLowStock) return 'status-warning';
    return 'status-good';
  }

  calculateNewQuantity(): number {
    const currentItem = this.item();
    if (!currentItem) return 0;

    const type = this.form.get('type')?.value;
    const quantity = +this.form.get('quantity')?.value || 0;

    if (type === 'StockOut') {
      return Math.max(0, currentItem.quantity - quantity);
    } else if (type === 'Adjustment') {
      return quantity;
    }
    return currentItem.quantity + quantity;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  }
}
