export interface InventoryItem {
  id: number;
  name: string;
  description?: string;
  sku: string;
  category: string;
  price: number;
  quantity: number;
  minimumStock: number;
  location?: string;
  createdAt: Date;
  lastUpdated?: Date;
  isLowStock: boolean;
}

export interface CreateInventoryItemDto {
  name: string;
  description?: string;
  sku: string;
  category: string;
  price: number;
  quantity: number;
  minimumStock: number;
  location?: string;
}

export interface UpdateInventoryItemDto {
  name?: string;
  description?: string;
  category?: string;
  price?: number;
  minimumStock?: number;
  location?: string;
}

export interface StockTransactionDto {
  inventoryItemId: number;
  type: 'StockIn' | 'StockOut' | 'Adjustment';
  quantity: number;
  notes?: string;
  performedBy: string;
}

export interface StockTransaction {
  id: number;
  inventoryItemId: number;
  type: string;
  quantity: number;
  notes?: string;
  transactionDate: Date;
  performedBy: string;
  previousQuantity: number;
  newQuantity: number;
}

export interface DashboardStats {
  totalItems: number;
  lowStockItems: number;
  totalInventoryValue: number;
  outOfStockItems: number;
  categoryBreakdown: CategorySummary[];
  recentTransactions: RecentActivity[];
}

export interface CategorySummary {
  category: string;
  itemCount: number;
  totalValue: number;
}

export interface RecentActivity {
  id: number;
  itemName: string;
  type: string;
  quantity: number;
  date: Date;
  performedBy: string;
}

export interface Alert {
  id: number;
  name: string;
  sku: string;
  quantity: number;
  minimumStock: number;
  category: string;
  alertLevel: 'critical' | 'high' | 'medium';
}
