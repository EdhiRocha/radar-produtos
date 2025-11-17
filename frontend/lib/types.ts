// Types baseados na API .NET RadarProdutos

export interface Product {
  id: number;
  externalId: string;
  name: string;
  supplier: string;
  supplierPrice: number;
  estimatedSalePrice: number;
  marginPercent: number;
  imageUrl?: string;
  supplierUrl?: string;
  rating: number;
  orders: number;
  competitionLevel: string;
  sentiment: string;
  score: number;
  productAnalysisId: number;

  // Métricas adicionais
  shopName?: string;
  shopUrl?: string;
  shippingDays?: number;
  commissionRate?: number;
  hasVideo?: boolean;
  hasPromotion?: boolean;

  // Campos importantes adicionados
  originalPrice?: number;
  discount?: string; // Ex: "50%"
  promotionLink?: string;
  productDetailUrl?: string;
  firstLevelCategoryId?: string;
  firstLevelCategoryName?: string;
}

export interface ProductAnalysis {
  id: number;
  keyword: string;
  analyzedAt: string;
  totalProducts: number;
  averageScore: number;
  products: Product[];
}

export interface AnalysisConfig {
  id: number;
  minMarginPercent: number;
  maxMarginPercent: number;
  weightSales: number;
  weightCompetition: number;
  weightSentiment: number;
  weightMargin: number;
}

export interface RunAnalysisRequest {
  keyword?: string;
  categoryIds?: string; // Ex: "111,222,333"
  sort?:
    | "SALE_PRICE_ASC"
    | "SALE_PRICE_DESC"
    | "LAST_VOLUME_ASC"
    | "LAST_VOLUME_DESC";
  maxSalePrice?: number; // em centavos
  minSalePrice?: number; // em centavos
  pageNo?: number;
  pageSize?: number;
}

export interface PaginatedProducts {
  items: Product[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Category {
  categoryId: string;
  categoryName: string;
  parentCategoryId?: string;
}
