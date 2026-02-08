import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  InventoryItem,
  CreateInventoryItemDto,
  UpdateInventoryItemDto,
  DashboardStats,
  StockTransactionDto,
  StockTransaction,
  Alert
} from '../models/inventory.model';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api';

  // Inventory CRUD operations
  getAllItems(category?: string, lowStockOnly?: boolean, search?: string): Observable<InventoryItem[]> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    if (lowStockOnly !== undefined) params = params.set('lowStockOnly', lowStockOnly.toString());
    if (search) params = params.set('search', search);

    return this.http.get<InventoryItem[]>(`${this.apiUrl}/inventory`, { params });
  }

  getItemById(id: number): Observable<InventoryItem> {
    return this.http.get<InventoryItem>(`${this.apiUrl}/inventory/${id}`);
  }

  createItem(item: CreateInventoryItemDto): Observable<InventoryItem> {
    return this.http.post<InventoryItem>(`${this.apiUrl}/inventory`, item);
  }

  updateItem(id: number, item: UpdateInventoryItemDto): Observable<InventoryItem> {
    return this.http.put<InventoryItem>(`${this.apiUrl}/inventory/${id}`, item);
  }

  deleteItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/inventory/${id}`);
  }

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/inventory/categories`);
  }

  getLowStockItems(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(`${this.apiUrl}/inventory/low-stock`);
  }

  // Stock transactions
  processTransaction(transaction: StockTransactionDto): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/stock/transaction`, transaction);
  }

  getItemTransactions(itemId: number, limit: number = 50): Observable<StockTransaction[]> {
    return this.http.get<StockTransaction[]>(
      `${this.apiUrl}/stock/transactions/${itemId}?limit=${limit}`
    );
  }

  getAllTransactions(limit: number = 100): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/stock/transactions?limit=${limit}`);
  }

  // Dashboard
  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard/stats`);
  }

  getAlerts(): Observable<{ totalAlerts: number; criticalAlerts: number; alerts: Alert[] }> {
    return this.http.get<{ totalAlerts: number; criticalAlerts: number; alerts: Alert[] }>(
      `${this.apiUrl}/dashboard/alerts`
    );
  }
}
