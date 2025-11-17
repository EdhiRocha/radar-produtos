import type {
  Product,
  ProductAnalysis,
  AnalysisConfig,
  RunAnalysisRequest,
  PaginatedProducts,
  Category,
} from "./types";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5001";

class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = "ApiError";
  }
}

async function fetchApi<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  const url = `${API_URL}${endpoint}`;

  const response = await fetch(url, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options?.headers,
    },
  });

  if (!response.ok) {
    const error = await response.text();
    throw new ApiError(response.status, error || response.statusText);
  }

  return response.json();
}

// Analysis endpoints
export const analysisApi = {
  run: async (data: RunAnalysisRequest): Promise<ProductAnalysis> => {
    return fetchApi<ProductAnalysis>("/api/analysis/run", {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  getLatest: async (): Promise<ProductAnalysis> => {
    return fetchApi<ProductAnalysis>("/api/analysis/latest");
  },
};

// Products endpoints
export const productsApi = {
  getAll: async (params?: {
    pageNumber?: number;
    pageSize?: number;
    minScore?: number;
    competitionLevel?: string;
  }): Promise<PaginatedProducts> => {
    const searchParams = new URLSearchParams();
    if (params?.pageNumber)
      searchParams.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize)
      searchParams.append("pageSize", params.pageSize.toString());
    if (params?.minScore)
      searchParams.append("minScore", params.minScore.toString());
    if (params?.competitionLevel)
      searchParams.append("competitionLevel", params.competitionLevel);

    const query = searchParams.toString();
    return fetchApi<PaginatedProducts>(
      `/api/products${query ? `?${query}` : ""}`
    );
  },

  getById: async (id: number): Promise<Product> => {
    return fetchApi<Product>(`/api/products/${id}`);
  },
};

// Config endpoints
export const configApi = {
  get: async (): Promise<AnalysisConfig> => {
    return fetchApi<AnalysisConfig>("/api/config");
  },

  update: async (data: Partial<AnalysisConfig>): Promise<AnalysisConfig> => {
    return fetchApi<AnalysisConfig>("/api/config", {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },
};

// Category endpoints
export const categoryApi = {
  getAll: async (): Promise<Category[]> => {
    return fetchApi<Category[]>("/api/categories");
  },
};

export { ApiError };
