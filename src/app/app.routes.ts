import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { InventoryListComponent } from './components/inventory-list/inventory-list.component';
import { InventoryFormComponent } from './components/inventory-form/inventory-form.component';
import { StockTransactionComponent } from './components/stock-transaction/stock-transaction.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'inventory', component: InventoryListComponent },
  { path: 'inventory/new', component: InventoryFormComponent },
  { path: 'inventory/:id', component: InventoryFormComponent },
  { path: 'inventory/:id/edit', component: InventoryFormComponent },
  { path: 'stock/:id', component: StockTransactionComponent },
  { path: '**', redirectTo: '/dashboard' }
];
