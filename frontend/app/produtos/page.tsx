"use client";

import { useState, useEffect } from "react";
import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { Product } from "@/lib/types";
import { productsApi } from "@/lib/api";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import Image from "next/image";
import { ScoreBadge } from "@/components/products/score-badge";
import { SentimentBadge } from "@/components/products/sentiment-badge";
import { CompetitionBadge } from "@/components/products/competition-badge";
import { ProductModal } from "@/components/products/product-modal";
import { Star } from "lucide-react";

export default function ProductsPage() {
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [filters, setFilters] = useState({
    minRating: "",
    minOrders: "",
    minMargin: "",
    competition: "all",
  });

  useEffect(() => {
    loadProducts();
  }, []);

  const loadProducts = async () => {
    try {
      setIsLoading(true);

      console.log("=== Iniciando carregamento de produtos ===");

      // Tentar carregar do localStorage primeiro
      const localProducts = localStorage.getItem("products");
      console.log("localStorage raw:", localProducts);

      if (localProducts) {
        const parsedProducts = JSON.parse(localProducts);
        console.log("Produtos parseados do localStorage:", parsedProducts);
        console.log("Quantidade de produtos:", parsedProducts.length);
        setProducts(parsedProducts);
        console.log(
          "Produtos carregados do localStorage:",
          parsedProducts.length
        );
      } else {
        console.log("localStorage vazio, tentando API...");
        // Se não tiver no localStorage, tentar da API
        const response = await productsApi.getAll();
        console.log("Resposta da API getAll:", response);
        setProducts(response.items || []);
      }
    } catch (error) {
      console.error("Erro ao carregar produtos:", error);
      // Se der erro na API, tentar localStorage como fallback
      const localProducts = localStorage.getItem("products");
      if (localProducts) {
        setProducts(JSON.parse(localProducts));
      } else {
        toast.error("Erro ao carregar produtos");
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleProductClick = (product: Product) => {
    setSelectedProduct(product);
    setModalOpen(true);
  };

  const filteredProducts = products.filter((product) => {
    if (filters.minRating && product.rating < parseFloat(filters.minRating))
      return false;
    if (filters.minOrders && product.orders < parseInt(filters.minOrders))
      return false;
    if (
      filters.minMargin &&
      product.marginPercent < parseFloat(filters.minMargin)
    )
      return false;
    if (
      filters.competition !== "all" &&
      product.competitionLevel.toLowerCase() !== filters.competition
    )
      return false;
    return true;
  });

  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <Header />
      <main className="ml-64 pt-16">
        <div className="p-8 space-y-6">
          <div>
            <h2 className="text-3xl font-bold tracking-tight">Produtos</h2>
            <p className="text-muted-foreground">
              Lista completa de produtos analisados
            </p>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Filtros</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid gap-4 md:grid-cols-4">
                <div>
                  <label className="text-sm font-medium mb-2 block">
                    Rating Mínimo
                  </label>
                  <Input
                    type="number"
                    placeholder="4.5"
                    step="0.1"
                    value={filters.minRating}
                    onChange={(e) =>
                      setFilters({ ...filters, minRating: e.target.value })
                    }
                  />
                </div>
                <div>
                  <label className="text-sm font-medium mb-2 block">
                    Pedidos Mínimos
                  </label>
                  <Input
                    type="number"
                    placeholder="100"
                    value={filters.minOrders}
                    onChange={(e) =>
                      setFilters({ ...filters, minOrders: e.target.value })
                    }
                  />
                </div>
                <div>
                  <label className="text-sm font-medium mb-2 block">
                    Margem Mínima (%)
                  </label>
                  <Input
                    type="number"
                    placeholder="60"
                    value={filters.minMargin}
                    onChange={(e) =>
                      setFilters({ ...filters, minMargin: e.target.value })
                    }
                  />
                </div>
                <div>
                  <label className="text-sm font-medium mb-2 block">
                    Concorrência
                  </label>
                  <Select
                    value={filters.competition}
                    onValueChange={(value) =>
                      setFilters({ ...filters, competition: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">Todas</SelectItem>
                      <SelectItem value="baixa">Baixa</SelectItem>
                      <SelectItem value="media">Média</SelectItem>
                      <SelectItem value="alta">Alta</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="mt-4 flex gap-2">
                <Button
                  variant="outline"
                  onClick={() =>
                    setFilters({
                      minRating: "",
                      minOrders: "",
                      minMargin: "",
                      competition: "all",
                    })
                  }
                >
                  Limpar Filtros
                </Button>
                <span className="text-sm text-muted-foreground flex items-center ml-auto">
                  {filteredProducts.length} produto(s) encontrado(s)
                </span>
              </div>
            </CardContent>
          </Card>

          <div className="rounded-lg border border-border bg-card overflow-hidden">
            {isLoading ? (
              <div className="flex items-center justify-center p-12">
                <div className="text-center space-y-3">
                  <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto" />
                  <p className="text-muted-foreground">
                    Carregando produtos...
                  </p>
                </div>
              </div>
            ) : filteredProducts.length === 0 ? (
              <div className="flex items-center justify-center p-12">
                <p className="text-muted-foreground">
                  Nenhum produto encontrado
                </p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="border-b border-border bg-muted/50">
                    <tr>
                      <th className="text-left p-4 font-medium text-sm">
                        Produto
                      </th>
                      <th className="text-left p-4 font-medium text-sm">
                        Preço Fornecedor
                      </th>
                      <th className="text-left p-4 font-medium text-sm">
                        Preço Venda
                      </th>
                      <th className="text-left p-4 font-medium text-sm">
                        Margem
                      </th>
                      <th className="text-left p-4 font-medium text-sm">
                        Concorrência
                      </th>
                      <th className="text-left p-4 font-medium text-sm">
                        Sentimento
                      </th>
                      <th className="text-left p-4 font-medium text-sm">
                        Score
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredProducts.map((product) => (
                      <tr
                        key={product.id}
                        className="border-b border-border hover:bg-muted/50 cursor-pointer transition-colors"
                        onClick={() => handleProductClick(product)}
                      >
                        <td className="p-4">
                          <div className="flex items-center gap-3">
                            <div className="relative h-12 w-12 rounded overflow-hidden border border-border flex-shrink-0">
                              <Image
                                src={product.imageUrl || "/placeholder.svg"}
                                alt={product.name}
                                fill
                                className="object-cover"
                              />
                            </div>
                            <div className="min-w-0 max-w-md">
                              <TooltipProvider>
                                <Tooltip>
                                  <TooltipTrigger asChild>
                                    <p className="font-medium text-sm truncate cursor-pointer">
                                      {product.name}
                                    </p>
                                  </TooltipTrigger>
                                  <TooltipContent
                                    side="top"
                                    className="max-w-sm"
                                  >
                                    <p className="text-sm">{product.name}</p>
                                  </TooltipContent>
                                </Tooltip>
                              </TooltipProvider>
                              <p className="text-xs text-muted-foreground flex items-center gap-2">
                                <span className="flex items-center gap-1">
                                  <Star className="h-3 w-3 fill-yellow-400 text-yellow-400" />
                                  {product.rating}
                                </span>
                                <span>•</span>
                                <span>{product.orders} pedidos</span>
                              </p>
                            </div>
                          </div>
                        </td>
                        <td className="p-4 text-sm">
                          R$ {product.supplierPrice.toFixed(2)}
                        </td>
                        <td className="p-4 text-sm font-medium text-green-600">
                          R$ {product.estimatedSalePrice.toFixed(2)}
                        </td>
                        <td className="p-4">
                          <span className="font-semibold text-primary text-sm">
                            {product.marginPercent.toFixed(1)}%
                          </span>
                        </td>
                        <td className="p-4">
                          <CompetitionBadge
                            level={
                              product.competitionLevel.toLowerCase() as any
                            }
                          />
                        </td>
                        <td className="p-4">
                          <SentimentBadge
                            sentiment={product.sentiment.toLowerCase() as any}
                          />
                        </td>
                        <td className="p-4">
                          <ScoreBadge score={product.score} />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">
              Mostrando {filteredProducts.length} de {products.length} produtos
            </p>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" disabled>
                Anterior
              </Button>
              <Button variant="outline" size="sm" disabled>
                Próxima
              </Button>
            </div>
          </div>
        </div>
      </main>

      <ProductModal
        product={selectedProduct}
        open={modalOpen}
        onClose={() => setModalOpen(false)}
      />
    </div>
  );
}
