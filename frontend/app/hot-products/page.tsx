"use client";

import { useState, useEffect } from "react";
import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  TrendingUp,
  DollarSign,
  Package,
  TrendingDown,
  AlertCircle,
  CheckCircle,
  Star,
  ShoppingCart,
} from "lucide-react";
import { toast } from "sonner";
import Image from "next/image";

interface ProductViability {
  totalAcquisitionCost: number;
  suggestedSalePrice: number;
  netProfit: number;
  realMarginPercent: number;
  roi: number;
  isViable: boolean;
  viabilityScore: number;
  productPriceBrl: number;
  shippingCostBrl: number;
  importTax: number;
  totalMercadoLivreFees: number;
}

interface HotProduct {
  id: number;
  name: string;
  imageUrl: string;
  supplierPrice: number;
  rating: number;
  orders: number;
  score: number;
  viability?: ProductViability;

  // Métricas adicionais
  shopName?: string;
  shopUrl?: string;
  shippingDays?: number;
  commissionRate?: number;
  hasVideo?: boolean;
  hasPromotion?: boolean;
  supplierUrl?: string;
}

export default function HotProductsPage() {
  const [products, setProducts] = useState<HotProduct[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [filters, setFilters] = useState({
    keyword: "",
    minPrice: "",
    maxPrice: "",
    pageSize: 20,
  });

  const loadHotProducts = async () => {
    try {
      setIsLoading(true);

      const params = new URLSearchParams();
      if (filters.keyword) params.append("keyword", filters.keyword);
      if (filters.minPrice) params.append("minSalePrice", filters.minPrice);
      if (filters.maxPrice) params.append("maxSalePrice", filters.maxPrice);
      params.append("pageSize", filters.pageSize.toString());

      const response = await fetch(
        `http://localhost:5001/api/products/hot?${params.toString()}`
      );

      if (response.ok) {
        const data = await response.json();
        setProducts(data);
        toast.success(`${data.length} produtos encontrados`);
      } else {
        throw new Error("Erro ao buscar produtos");
      }
    } catch (error) {
      console.error("Erro ao carregar hot products:", error);
      toast.error("Erro ao carregar produtos");
    } finally {
      setIsLoading(false);
    }
  };

  const viableProducts = products.filter((p) => p.viability?.isViable);
  const inviableProducts = products.filter(
    (p) => p.viability && !p.viability.isViable
  );

  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <Header />
      <main className="ml-64 pt-16">
        <div className="p-8 space-y-6">
          <div>
            <h2 className="text-3xl font-bold tracking-tight">
              Produtos em Alta (Hot Products)
            </h2>
            <p className="text-muted-foreground">
              Análise de viabilidade dos produtos mais vendidos no AliExpress
            </p>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Filtros de Busca</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid gap-4 md:grid-cols-4">
                <div className="md:col-span-2">
                  <Label htmlFor="keyword">Palavra-chave</Label>
                  <Input
                    id="keyword"
                    placeholder="Ex: phone case, headphones..."
                    value={filters.keyword}
                    onChange={(e) =>
                      setFilters({ ...filters, keyword: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="minPrice">Preço Mínimo (USD)</Label>
                  <Input
                    id="minPrice"
                    type="number"
                    step="0.01"
                    placeholder="0.00"
                    value={filters.minPrice}
                    onChange={(e) =>
                      setFilters({ ...filters, minPrice: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="maxPrice">Preço Máximo (USD)</Label>
                  <Input
                    id="maxPrice"
                    type="number"
                    step="0.01"
                    placeholder="100.00"
                    value={filters.maxPrice}
                    onChange={(e) =>
                      setFilters({ ...filters, maxPrice: e.target.value })
                    }
                  />
                </div>
              </div>
              <div className="mt-4 flex gap-2">
                <Button onClick={loadHotProducts} disabled={isLoading}>
                  {isLoading ? "Buscando..." : "Buscar Produtos"}
                </Button>
                <Button
                  variant="outline"
                  onClick={() =>
                    setFilters({
                      keyword: "",
                      minPrice: "",
                      maxPrice: "",
                      pageSize: 20,
                    })
                  }
                >
                  Limpar
                </Button>
              </div>
            </CardContent>
          </Card>

          {products.length > 0 && (
            <div className="grid grid-cols-3 gap-4">
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-muted-foreground">
                        Total de Produtos
                      </p>
                      <p className="text-2xl font-bold">{products.length}</p>
                    </div>
                    <Package className="h-8 w-8 text-muted-foreground" />
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-muted-foreground">
                        Produtos Viáveis
                      </p>
                      <p className="text-2xl font-bold text-green-600">
                        {viableProducts.length}
                      </p>
                    </div>
                    <CheckCircle className="h-8 w-8 text-green-600" />
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-muted-foreground">
                        Produtos Inviáveis
                      </p>
                      <p className="text-2xl font-bold text-red-600">
                        {inviableProducts.length}
                      </p>
                    </div>
                    <AlertCircle className="h-8 w-8 text-red-600" />
                  </div>
                </CardContent>
              </Card>
            </div>
          )}

          {isLoading ? (
            <div className="flex items-center justify-center p-12">
              <div className="text-center space-y-3">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto" />
                <p className="text-muted-foreground">Analisando produtos...</p>
              </div>
            </div>
          ) : products.length === 0 ? (
            <Card>
              <CardContent className="flex items-center justify-center p-12">
                <p className="text-muted-foreground">
                  Use os filtros acima para buscar produtos em alta
                </p>
              </CardContent>
            </Card>
          ) : (
            <div className="space-y-4">
              {products.map((product) => (
                <Card
                  key={product.id}
                  className={
                    product.viability?.isViable
                      ? "border-green-500/50"
                      : "border-red-500/50"
                  }
                >
                  <CardContent className="p-6">
                    <div className="flex gap-6">
                      <div className="relative h-32 w-32 rounded overflow-hidden border border-border flex-shrink-0">
                        <Image
                          src={product.imageUrl || "/placeholder.svg"}
                          alt={product.name}
                          fill
                          className="object-cover"
                        />
                      </div>

                      <div className="flex-1 space-y-4">
                        <div>
                          <div className="flex items-start justify-between gap-4">
                            <h3 className="font-semibold text-lg">
                              {product.name}
                            </h3>
                            {product.viability && (
                              <Badge
                                variant={
                                  product.viability.isViable
                                    ? "default"
                                    : "destructive"
                                }
                              >
                                {product.viability.isViable
                                  ? "VIÁVEL"
                                  : "INVIÁVEL"}
                              </Badge>
                            )}
                          </div>
                          <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground flex-wrap">
                            <span className="flex items-center gap-1">
                              <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                              {product.rating.toFixed(1)}
                            </span>
                            <span className="flex items-center gap-1">
                              <ShoppingCart className="h-4 w-4" />
                              {product.orders.toLocaleString()} pedidos
                            </span>
                            <span className="flex items-center gap-1">
                              <TrendingUp className="h-4 w-4" />
                              Score: {product.score}
                            </span>
                            {product.shippingDays && (
                              <Badge
                                variant={
                                  product.shippingDays <= 15
                                    ? "default"
                                    : product.shippingDays <= 30
                                    ? "secondary"
                                    : "outline"
                                }
                                className="text-xs"
                              >
                                📦 {product.shippingDays}d
                              </Badge>
                            )}
                            {product.hasVideo && (
                              <Badge
                                variant="outline"
                                className="text-xs bg-blue-50"
                              >
                                📹 Vídeo
                              </Badge>
                            )}
                            {product.hasPromotion && (
                              <Badge
                                variant="outline"
                                className="text-xs bg-orange-50"
                              >
                                🎁 Promoção
                              </Badge>
                            )}
                            {product.shopName && (
                              <span className="text-xs">
                                🏪 {product.shopName}
                              </span>
                            )}
                          </div>
                          {(product.shopUrl || product.supplierUrl) && (
                            <div className="flex gap-2 mt-2">
                              {product.shopUrl && (
                                <Button
                                  size="sm"
                                  variant="outline"
                                  onClick={() =>
                                    window.open(product.shopUrl, "_blank")
                                  }
                                  className="text-xs"
                                >
                                  🏪 Loja Oficial
                                </Button>
                              )}
                              {product.supplierUrl && (
                                <Button
                                  size="sm"
                                  variant="outline"
                                  onClick={() =>
                                    window.open(product.supplierUrl, "_blank")
                                  }
                                  className="text-xs"
                                >
                                  🔗 Ver Produto
                                </Button>
                              )}
                            </div>
                          )}
                        </div>

                        {product.viability && (
                          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                            <div className="space-y-1">
                              <p className="text-xs text-muted-foreground">
                                Custo Total Aquisição
                              </p>
                              <p className="text-lg font-bold text-red-600">
                                R${" "}
                                {product.viability.totalAcquisitionCost.toFixed(
                                  2
                                )}
                              </p>
                              <div className="text-xs space-y-0.5">
                                <p>
                                  Produto: R${" "}
                                  {product.viability.productPriceBrl.toFixed(2)}
                                </p>
                                <p>
                                  Frete: R${" "}
                                  {product.viability.shippingCostBrl.toFixed(2)}
                                </p>
                                <p>
                                  Import: R${" "}
                                  {product.viability.importTax.toFixed(2)}
                                </p>
                              </div>
                            </div>

                            <div className="space-y-1">
                              <p className="text-xs text-muted-foreground">
                                Preço Venda Sugerido
                              </p>
                              <p className="text-lg font-bold text-blue-600">
                                R${" "}
                                {product.viability.suggestedSalePrice.toFixed(
                                  2
                                )}
                              </p>
                              <p className="text-xs">
                                Taxas ML: R${" "}
                                {product.viability.totalMercadoLivreFees.toFixed(
                                  2
                                )}
                              </p>
                            </div>

                            <div className="space-y-1">
                              <p className="text-xs text-muted-foreground">
                                Lucro Líquido
                              </p>
                              <p
                                className={`text-lg font-bold ${
                                  product.viability.netProfit > 0
                                    ? "text-green-600"
                                    : "text-red-600"
                                }`}
                              >
                                R$ {product.viability.netProfit.toFixed(2)}
                              </p>
                              <p className="text-xs">
                                Margem:{" "}
                                {product.viability.realMarginPercent.toFixed(1)}
                                %
                              </p>
                            </div>

                            <div className="space-y-1">
                              <p className="text-xs text-muted-foreground">
                                ROI
                              </p>
                              <p
                                className={`text-lg font-bold ${
                                  product.viability.roi > 0
                                    ? "text-green-600"
                                    : "text-red-600"
                                }`}
                              >
                                {product.viability.roi.toFixed(1)}%
                              </p>
                              <div className="flex items-center gap-1">
                                <div className="flex-1 bg-muted rounded-full h-2 overflow-hidden">
                                  <div
                                    className={`h-full ${
                                      product.viability.isViable
                                        ? "bg-green-500"
                                        : "bg-red-500"
                                    }`}
                                    style={{
                                      width: `${Math.min(
                                        product.viability.viabilityScore,
                                        100
                                      )}%`,
                                    }}
                                  />
                                </div>
                                <span className="text-xs font-medium">
                                  {product.viability.viabilityScore.toFixed(0)}
                                </span>
                              </div>
                            </div>
                          </div>
                        )}
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
