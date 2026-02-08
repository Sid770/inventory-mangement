import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { InventoryService } from '../../services/inventory.service';
import { InventoryItem } from '../../models/inventory.model';

@Component({
  selector: 'app-inventory-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './inventory-form.component.html',
  styleUrl: './inventory-form.component.css'
})
export class InventoryFormComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly inventoryService = inject(InventoryService);
  
  protected readonly itemId = signal<number | null>(null);
  protected readonly isEditMode = signal(false);
  protected readonly loading = signal(false);
  protected readonly categories = signal<string[]>([]);
  
  protected readonly form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', Validators.maxLength(500)],
    sku: ['', [Validators.required, Validators.maxLength(50)]],
    category: ['', [Validators.required, Validators.maxLength(50)]],
    price: [0, [Validators.required, Validators.min(0)]],
    quantity: [0, [Validators.required, Validators.min(0)]],
    minimumStock: [0, [Validators.required, Validators.min(0)]],
    location: ['']
  });

  ngOnInit(): void {
    this.loadCategories();
    
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.itemId.set(+id);
      this.isEditMode.set(true);
      this.loadItem(+id);
    }
  }

  loadCategories(): void {
    this.inventoryService.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: (err) => console.error('Error loading categories:', err)
    });
  }

  loadItem(id: number): void {
    this.loading.set(true);
    this.inventoryService.getItemById(id).subscribe({
      next: (item) => {
        this.form.patchValue({
          name: item.name,
          description: item.description,
          sku: item.sku,
          category: item.category,
          price: item.price,
          quantity: item.quantity,
          minimumStock: item.minimumStock,
          location: item.location
        });
        
        // Disable SKU field in edit mode
        this.form.get('sku')?.disable();
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
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const formValue = this.form.getRawValue();

    if (this.isEditMode() && this.itemId()) {
      // Update existing item
      const updateDto = {
        name: formValue.name,
        description: formValue.description,
        category: formValue.category,
        price: formValue.price,
        minimumStock: formValue.minimumStock,
        location: formValue.location
      };

      this.inventoryService.updateItem(this.itemId()!, updateDto).subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigate(['/inventory']);
        },
        error: (err) => {
          this.loading.set(false);
          alert('Failed to update item');
          console.error('Update error:', err);
        }
      });
    } else {
      // Create new item
      this.inventoryService.createItem(formValue).subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigate(['/inventory']);
        },
        error: (err) => {
          this.loading.set(false);
          alert('Failed to create item. SKU might already exist.');
          console.error('Create error:', err);
        }
      });
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getErrorMessage(fieldName: string): string {
    const field = this.form.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) return 'This field is required';
    if (field.errors['min']) return `Minimum value is ${field.errors['min'].min}`;
    if (field.errors['maxLength']) return `Maximum length is ${field.errors['maxLength'].max}`;
    
    return 'Invalid value';
  }
}
