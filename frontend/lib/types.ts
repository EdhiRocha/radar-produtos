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
  keyword: string;
}

export interface PaginatedProducts {
  items: Product[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
